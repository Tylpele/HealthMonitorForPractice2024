using HeartbeatApp.RabbitMq;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HeartbeatApp.Models
{
    public class CheckQueue : BackgroundService
    {
        private readonly string queueName = "queue";
        private Timer sendTimer;
        private Timer listenTimer;
        private readonly string flagPath = "../../chatbot/webapp/stop.flag";

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
                Console.WriteLine("Message sent to queue.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to queue: {ex.Message}");
                CreateFlagFile();
            }
        }

        private void CheckForMessages(object state)
        {
            try
            {
                Queue.StartListening(queueName, (message) =>
                {
                    Console.WriteLine("Message received from queue: " + message);
                    DeleteFlagFileIfExists(flagPath);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting to listen to the queue: {ex.Message}");
            }
        }

        private void CreateFlagFile()
        {
            try
            {
                if (!File.Exists(flagPath))
                {
                    File.Create(flagPath).Dispose();
                    Console.WriteLine("Flag file created.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating flag file: {ex.Message}");
            }
        }

        private static void DeleteFlagFileIfExists(string filepath)
        {
            try
            {
                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                    Console.WriteLine("Flag file deleted.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting flag file: {ex.Message}");
            }
        }
    }
}
