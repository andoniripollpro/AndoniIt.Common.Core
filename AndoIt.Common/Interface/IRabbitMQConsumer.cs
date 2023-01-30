using System;
using AndoIt.Common.Dto;
using RabbitMQ.Client.Events;

namespace AndoIt.Common.Interface
{
	public interface IRabbitMQConsumer
	{
		event EventHandler<Envelope<string>> ErrorFatalEvent;
		event EventHandler<Envelope<string>> MessageArrived;

		int InternalRestartRetrays { get; set; } 
		int RetryEachSeconds { get; set; } 

		void ConsumeMessage(BasicDeliverEventArgs ea);
		void Dispose();
		void Listen();
	}
}