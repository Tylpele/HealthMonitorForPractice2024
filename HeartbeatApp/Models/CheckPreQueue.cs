
using HeartbeatApp.RabbitMq;
using System;

namespace HeartbeatApp.Models
{
    public class CheckPreQueue: BackgroundService
    {
        private readonly string queueName = "pre-queue";
        private Timer timer;

        protected  override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Start listening for messages
            Queue.StartListening(queueName, (message) =>SendMessagePeriodically());

            // Create a timer to send messages periodically
            timer = new Timer(state => SendMessagePeriodically(), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            // Wait indefinitely until cancellation is requested
            return Task.CompletedTask;
        }
        private void SendMessagePeriodically()
        {
            // Simulate some message content
            Queue.SendMessage(queueName);
        }
    }

}   
