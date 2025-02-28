using AndoIt.Common.Interface;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web;

namespace AndoIt.Common.Common
{
	/// <summary>
	/// Creará el exchange definidos así:
	/// autoCreation=true o nada
    /// Y no lo creará con los definidos así
	/// autoCreation=false
	/// </summary>
	public class RabbitMQPublisher
	{
		private readonly ILog log;
		private readonly ConnectionFactory connectionFactory;
		
		public RabbitMQPublisher(ILog log, string amqpUrlPublish)
		{
			this.log.InfoSafe($"Start on amqpUrlPublish '{amqpUrlPublish}'", new StackTrace());
			this.log = log ?? throw new ArgumentNullException("log");
			this.connectionFactory = new ConnectionFactory();
			this.connectionFactory.Uri = new Uri(amqpUrlPublish ?? throw new ArgumentNullException("amqpUrlPublish"));

			if (!this.connectionFactory.Uri.ToString().Contains("autoCreation=false"))
			{
				using (IConnection connection = this.connectionFactory.CreateConnection())
				{
					using (IModel channel = connection.CreateModel())
					{
						this.log.Info($"Llama a CreateExchangeQueueAndLinkIfPossible", new StackTrace());
						Dictionary<string, object> queueArguments = new Dictionary<string, object>();
						if (HttpUtility.ParseQueryString(new Uri(amqpUrlPublish).Query).Get("quorum") == "true")
							queueArguments.Add("x-queue-type", "quorum");
						var rabbitMqCreator = new RabbitMQCreator(this.log);
						rabbitMqCreator.CreateExchangeQueueAndLinkIfPossible(this.ExchangeName, channel, queueArguments: queueArguments);
					}
				}
			}
		}

		public string AmqpUrlPublish => this.connectionFactory.Uri.ToString();
		public string ExchangeName => HttpUtility.ParseQueryString(this.connectionFactory.Uri.Query).Get("exchange");

		public enum PipeType
		{
			Exchange,
			Queue
		}

		public void Publish(string datosPublicacion, string routingKey = "", string correlationId = "", string completeConfirmationUri = "")
		{
			this.log.Debug($"Start: {datosPublicacion}", new StackTrace());
			string exchangeName = HttpUtility.ParseQueryString(this.connectionFactory.Uri.Query).Get("exchange");			
			string confirmationQueue = string.Empty;
			if (!string.IsNullOrEmpty(completeConfirmationUri))
			{
				Uri confirmationUri = new Uri(completeConfirmationUri);
				if (this.connectionFactory.Uri.AbsolutePath != confirmationUri.AbsolutePath)
					throw new ArgumentException($"La Uri de respuesta de este mensaje tiene que coincidir en virtual host con la de publicación. completeConfirmationUri = '{completeConfirmationUri}'");
				confirmationQueue = HttpUtility.ParseQueryString(confirmationUri.Query).Get("queue");
				if (confirmationQueue is null)
					throw new ArgumentException($"La Uri de respuesta de este mensaje tiene que tener una queue válida para que responda por ella. completeConfirmationUri = '{completeConfirmationUri}'");
			}
			this.log.Debug($"confirmationQueue: {confirmationQueue}", new StackTrace());

			using (IConnection connection = this.connectionFactory.CreateConnection())
			{
				using (IModel channel = connection.CreateModel())
				{
					IBasicProperties properties = channel.CreateBasicProperties();
					properties.ContentType = "application/json";
					properties.ContentEncoding = "UTF-8";
					properties.DeliveryMode = 2;
					properties.ReplyTo = confirmationQueue;
					properties.Persistent = true; // 👈 Mensajes persistentes
					if (correlationId != string.Empty)
						properties.CorrelationId = correlationId;

					if (this.connectionFactory.Uri.ToString().Contains("pipeline=true"))
						CreateExchangeAndQueueIfPossible(exchangeName, channel, routingKey);

					byte[] payload = Encoding.UTF8.GetBytes(datosPublicacion);
					channel.BasicPublish(this.ExchangeName, routingKey, properties, payload);
					this.log.Debug($"channel.BasicPublish({this.ExchangeName}, {routingKey}, {properties}, {payload})", new StackTrace());
				}
			}
		}

		private void CreateExchangeAndQueueIfPossible(string exchangeName, IModel channel, string routingKey)
		{
			try
			{
				//type: ExchangeType.Fanout
				channel.ExchangeDeclare(exchange: exchangeName,
					type: ExchangeType.Direct,
					durable: true,
					autoDelete: false,
					arguments: null);
			}
			catch (Exception ex)
			{
				this.log.Error($"Al crear el exchange donde mandamos el mensaje: '{exchangeName}'", ex);
				return;
			}
			try
			{
				channel.QueueDeclare(queue: exchangeName,
					durable: true,
					autoDelete: false,
					arguments: null);
			}
			catch (Exception ex)
			{
				this.log.Error($"Al crear una queue homonima a la exchange donde mandamos el mensaje: '{exchangeName}'", ex);
				return;
			}
			try
			{				
				channel.QueueBind(exchangeName, exchangeName, routingKey);
			}
			catch (Exception ex)
			{
				this.log.Error($"Al lincar la queue a la exchange donde mandamos el mensaje: '{exchangeName}'", ex);
			}
		}
	}
}