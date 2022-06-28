using AndoIt.Common.Dto;
using AndoIt.Common.Interface;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Web;

namespace AndoIt.Common.Common
{
	public class RabbitMQConsumer : IDisposable, IRabbitMQConsumer
	{
		public event EventHandler<Envelope<string>> MessageArrived;
		public event EventHandler<Envelope<string>> ErrorFatalEvent;

		private readonly ILog log;
		private readonly ConnectionFactory connectionFactory;

		private readonly string amqpUrlListen;
		private bool DisposingOnPurpouse = false;
		private IConnection connection;
		private IModel channel;

		//public RabbitMQConsumer(ConnectionFactory connectionFactory, ILog log, string amqpUrlListen)
		public RabbitMQConsumer(ILog log, string amqpUrlListen)
		{
			this.log = log ?? throw new ArgumentNullException("log");			
			this.connectionFactory = new ConnectionFactory();
			this.connectionFactory.AutomaticRecoveryEnabled = true;
			this.connectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(this.RetryEachSeconds);
			this.connectionFactory.RequestedHeartbeat = ushort.MaxValue;
			this.connectionFactory.RequestedConnectionTimeout = int.MaxValue;
			this.log.Info("this.connectionFactory.AutomaticRecoveryEnabled = true", new StackTrace());
			this.amqpUrlListen = amqpUrlListen ?? throw new ArgumentNullException("amqpUrlListen");
		}

		public int InternalRestartRetrays { get; set; } = 10;
		public int RetryEachSeconds { get; set; } = 900; // 15 min
		public bool ErrorFatalEventOnFirstFail { get; set; } = false;

		public void Listen()
		{
			this.log.InfoSafe($"Start on completeUri '{this.amqpUrlListen}'", new StackTrace());

			try
			{				
				Uri uri = new Uri(this.amqpUrlListen);
				this.connectionFactory.Uri = uri;
				string queue = HttpUtility.ParseQueryString(uri.Query).Get("queue");

				this.connection = this.connectionFactory.CreateConnection();
				this.connection.ConnectionShutdown += (obj, msg) => ConnectionShutdown(obj, msg);
				this.connection.RecoverySucceeded += (obj, msg) => RecoverySucceeded(obj, msg);
				this.connection.ConnectionBlocked += (obj, msg) => ConnectionBlocked(obj, msg);

				this.channel = this.connection.CreateModel();
				this.channel.ModelShutdown += (obj, msg) => ModelShutdown(obj, msg);

				this.channel.QueueDeclare(queue: queue,
						 durable: true,
						 exclusive: false,
						 autoDelete: false,
						 arguments: null);

				var consumer = new EventingBasicConsumer(this.channel);
				consumer.Received += (model, ea) => { ConsumeMessage(ea); };
				consumer.ConsumerCancelled += (obj, msg) => { ConsumerCancelled(obj, msg); };
				consumer.Shutdown += (obj, msg) => { ConsumerShutdown(obj, msg); };
				consumer.Unregistered += (obj, msg) => { ConsumerUnregistered(obj, msg); };				
				this.channel.BasicConsume(queue: queue,
										autoAck: false,
										consumer: consumer);
			}
			catch (Exception e)
			{
				this.log.Fatal($"No es capaz de escuchar los mensajes de la cola RabbitMQ", e, new StackTrace());
				throw;
			}
			this.log.Info($"End", new StackTrace());
		}

		public void ConsumeMessage(BasicDeliverEventArgs ea)
		{
			string message = "<Unknown>";
			string correlationId = "<Unknown>";
			try
			{
				var body = ea.Body;
				correlationId = ea.BasicProperties.CorrelationId;
				message = Encoding.UTF8.GetString(body);
				
				this.MessageArrived?.Invoke(this, new Envelope<string>() { 
					Content = message, 
					CorrelationId = ea.BasicProperties.CorrelationId,
					ReplyTo = ea.BasicProperties.ReplyTo
				});

				this.channel.BasicAck(ea.DeliveryTag, false);

				this.log.InfoSafe($"Received message '{message}', correlationId {correlationId}. Rabbit queue: {this.amqpUrlListen}", new StackTrace());
			}
			catch (Exception e)
			{
				this.log.Error($"Received message'{message}', correlationId {correlationId}", e, new StackTrace());
				this.log.InfoSafe($"Rabbit queue: {this.amqpUrlListen}", new StackTrace());
			}
		}

		public static string GetCombinedAmqp(string completeAmqp, string toBeInserted, bool queue = true)
		{
			if (toBeInserted.StartsWith("amqp:"))
			{
				return toBeInserted;
			}
			else
			{
				var replyToQuery = HttpUtility.ParseQueryString(new Uri(completeAmqp).Query);
				replyToQuery.Remove("queue");
				replyToQuery.Remove("exchange");
				replyToQuery.Add(queue ? "queue" : "exchange", toBeInserted);
				string repyToStr = new UriBuilder(completeAmqp) { Query = replyToQuery.ToString() }.ToString();
				return repyToStr;
			}
		}

		public static string SwitchAmqpToQueue(string completeAmqp)
		{			
			var replyToQuery = HttpUtility.ParseQueryString(new Uri(completeAmqp).Query);
			string exchage = replyToQuery.Get("exchange");
			replyToQuery.Remove("exchange");
			replyToQuery.Add("queue", exchage);
			string repyToStr = new UriBuilder(completeAmqp) { Query = replyToQuery.ToString() }.ToString();
			return repyToStr;
			
		}

		private void ConnectionShutdown(object obj, ShutdownEventArgs msg)
		{
			this.log.Info($"Sender: {((obj != null) ? GetObjectName(obj) : "null")} Message: {JsonConvert.SerializeObject(msg)}", new StackTrace()); 
			this.log.Info($"this.connectionFactory.AutomaticRecoveryEnabled = {this.connectionFactory.AutomaticRecoveryEnabled}; this.connectionFactory.NetworkRecoveryInterval = {this.connectionFactory.NetworkRecoveryInterval}", new StackTrace());
			RecoverIfNeeded();
		}

		private void ModelShutdown(object obj, ShutdownEventArgs msg)
		{
			this.log.Info($"Sender: {GetObjectName(obj)} Message: {JsonConvert.SerializeObject(msg)}", new StackTrace());
		}

		private void ConsumerCancelled(object obj, ConsumerEventArgs msg)
		{
			this.log.Info($"Sender: {((obj != null) ? GetObjectName(obj) : "null")} Message: {JsonConvert.SerializeObject(msg)}", new StackTrace());
			this.log.Info($"this.connectionFactory.AutomaticRecoveryEnabled = {this.connectionFactory.AutomaticRecoveryEnabled}; this.connectionFactory.NetworkRecoveryInterval = {this.connectionFactory.NetworkRecoveryInterval}", new StackTrace());
			RecoverIfNeeded();
		}

		private void ConsumerShutdown(object obj, ShutdownEventArgs msg)
		{
			this.log.Info($"Sender: {GetObjectName(obj)} Message: {JsonConvert.SerializeObject(msg)}", new StackTrace());
		}

		//	Hasta aquí serie normal cuando hay desconexión
		private void ConnectionBlocked(object obj, ConnectionBlockedEventArgs msg)
		{
			this.log.Info($"Sender: {GetObjectName(obj)} Message: {JsonConvert.SerializeObject(msg)}", new StackTrace());
		}

		private void EventConsumerShutDown(object sender, ShutdownEventArgs e)
		{
			this.log.Info($"Sender: {GetObjectName(sender)} Message: {JsonConvert.SerializeObject(e)}", new StackTrace());
		}

		private void ConsumerUnregistered(object obj, ConsumerEventArgs msg)
		{
			this.log.Info($"Sender: {GetObjectName(obj)} Message: {JsonConvert.SerializeObject(msg)}", new StackTrace());
		}

		private void RecoverySucceeded(object obj, EventArgs msg)
		{
			this.log.Info($"Sender: {GetObjectName(obj)} Message: {JsonConvert.SerializeObject(msg)}", new StackTrace());
		}

		private string GetObjectName(object obj)
		{
			return obj.GetType().Name;
		}

		/// <summary>
		/// Este método a resultado peor que dejar a RabbitMQ restaurarse a sí mismo.
		/// Lo dejo (aunque no se use) para darle una vuelta en el futuro
		/// </summary>
		private void RecoverIfNeeded()
		{
			try
			{
				if (!this.DisposingOnPurpouse && this.channel != null)
				{
					this.log.Error($"InternalRestartRetrays: {this.InternalRestartRetrays} RetryEachSeconds {this.RetryEachSeconds}", null, new StackTrace());
					if (this.ErrorFatalEventOnFirstFail)
						this?.ErrorFatalEvent(this, new Envelope<string>() { Content = "Está intentando recuperarse, pero va a pedir reiniciar el servicio" });
					Thread.Sleep(this.RetryEachSeconds * 1000);
					new Insister(this.log).Insist(new Action(() => InternalRestart()), this.InternalRestartRetrays);
					//this.log.Info($"Este thread se va a quedar zombi para que no me mate el proceso", new StackTrace());
					//while (true) Thread.Sleep(int.MaxValue);	//	ZOMBI
				}
			}
			catch (Exception ex)
			{
				string errorMessage = "Se ha intentado recuperar la conexión con RabbitMQ, pero no se ha podido";
				this.log.Fatal($"{new StackTrace().ToStringClassMethod()}: {errorMessage}", ex);
				this?.ErrorFatalEvent(this, new Envelope<string>() { Content = errorMessage, Exception = ex });
			}
		}		
		private void InternalRestart()
		{
			this.log.Info($"CloseConnection(); this.DisposingOnPurpouse = {this.DisposingOnPurpouse}", new StackTrace());						
			CloseConnection();

			Thread.Sleep(this.RetryEachSeconds * 1000);
			this.log.Info($"Listen();", new StackTrace());
			this.Listen();
		}

		public void Dispose()
		{
			this.log.Info($"Start", new StackTrace());
			this.DisposingOnPurpouse = true;
			CloseConnection();

			this.log.Info($"Connection Closed", new StackTrace());
		}

		private void CloseConnection()
		{
			this.log.Info($"Start", new StackTrace());
			this.channel?.Close();
			this.connection?.Close();
			this.channel = null;
			this.connection = null;
			GC.Collect();
			this.log.Info($"End", new StackTrace());
		}
	}
}


