
using HeartbeatApp.RabbitMq;

namespace HeartbeatApp.Models
{
    public class CheckQueue : BackgroundService
    {
        private readonly string queueName = "queue";
        private Timer timer;
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Queue.StartListening(queueName, (message) => SendMessagePeriodically());

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
