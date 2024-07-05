
using HeartbeatApp.RabbitMq;

namespace HeartbeatApp.Models
{
    public class CheckPostQueue : BackgroundService
    {
        private readonly string queueName = "post-queue";
        private Timer timer;
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Queue.StartListening(queueName, (message)=>SendMessagePeriodically());

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
