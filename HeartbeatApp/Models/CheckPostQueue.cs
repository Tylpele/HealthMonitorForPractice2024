
using HeartbeatApp.RabbitMq;
using NLog;

namespace HeartbeatApp.Models
{
    public class CheckPostQueue : BackgroundService
    {
        private readonly string queueName = "post-queue";
        private Timer sendTimer;
        private Timer listenTimer;
        private readonly string flagPath = "../../chatbot/webapp/stop.flag";
        private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Timer to send messages every 5 seconds
            sendTimer = new Timer(SendMessagePeriodically, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            // Timer to check for received messages every 5 seconds
            listenTimer = new Timer(CheckForMessages, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            // Wait indefinitely until cancellation is requested
            return Task.CompletedTask;
        }

        private void SendMessagePeriodically(object state)
        {
            try
            {
                Queue.SendMessage(queueName);
                _logger.Info("Message sent to "+ queueName);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending message to queue: {ex.Message}");
                CreateFlagFile();
            }
        }

        private void CheckForMessages(object state)
        {
            try
            {
                Queue.StartListening(queueName, (message) =>
                {
                    _logger.Info("Message received from queue: " + message);
                    DeleteFlagFileIfExists(flagPath);
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Error starting to listen to the queue: {ex.Message}");
            }
        }

        private void CreateFlagFile()
        {
            try
            {
                if (!File.Exists(flagPath))
                {
                    File.Create(flagPath).Dispose();
                    _logger.Info("Flag file created.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating flag file: {ex.Message}");
            }
        }

        private void DeleteFlagFileIfExists(string filepath)
        {
            try
            {
                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                    _logger.Info("Flag file deleted.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error deleting flag file: {ex.Message}");
            }
        }
    }
}
