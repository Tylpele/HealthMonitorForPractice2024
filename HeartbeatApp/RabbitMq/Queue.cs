using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Collections.Generic;
using System;

namespace HeartbeatApp.RabbitMq
{
    public class Queue
    {
        private static readonly ConnectionFactory _factory;
        private static readonly IConnection _connection;
        private static readonly IModel _channel;
        static Queue()
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
            try
            {
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();
            }
            catch (BrokerUnreachableException ex)
            {
                Console.WriteLine($"Error connecting to RabbitMQ: {ex.Message}");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        public static void SendMessage(string queueName)
        {
            try
            {
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var message = Encoding.UTF8.GetBytes("Hey! I'm alive");

                var properties = _channel.CreateBasicProperties();
                properties.Headers = new Dictionary<string, object>
                {
                    { "Type", "heartbeat" }
                };

                _channel.BasicPublish(exchange: "",
                                      routingKey: queueName,
                                      basicProperties: properties,
                                      body: message);

                //Console.WriteLine($"Message sent to queue '{queueName}' successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error sending message to queue '{queueName}': {ex.Message}");
                File.Create("../../ChatBot/ChatBot/stop.flag").Dispose();
                Environment.Exit(1); // Terminate the application with a non-zero exit code
            }
        }

        public static void StartListening(string queueName, Action<string> onMessageReceived)
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                if (ea.BasicProperties.Headers != null &&
                    ea.BasicProperties.Headers.TryGetValue("Type", out var typeHeader))
                {
                    var headerValue = Encoding.UTF8.GetString((byte[])typeHeader);
                    if (headerValue == "heartbeat")
                    {
                        Console.WriteLine("Received heartbeat message: " + message);
                        // Handle heartbeat message here if needed
                    }
                    else
                    {
                        Console.WriteLine("Ignoring message with type: " + headerValue);
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                }
                else
                {
                    Console.WriteLine("Received message without a valid Type header: " + message);
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );
        }

    }
}
