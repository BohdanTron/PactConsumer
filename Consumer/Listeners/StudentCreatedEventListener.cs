using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer.Listeners
{
    public class StudentCreatedEventListener : BackgroundService
    {
        private readonly IModel _channel;

        public StudentCreatedEventListener()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var resultCreatedEventHandler = new EventHandler<BasicDeliverEventArgs>((model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Message received: {message}");
                var resultEvent = JsonSerializer.Deserialize<StudentCreatedEvent>(message);
            });

            await ListenEvent("student-created", resultCreatedEventHandler);
        }

        private async Task ListenEvent(string queueName, EventHandler<BasicDeliverEventArgs> eventHandler)
        {
            _channel.QueueDeclare(queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Message received: {message}");
                var resultEvent = JsonSerializer.Deserialize<StudentCreatedEvent>(message, 
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                Console.WriteLine(resultEvent);
            };
            _channel.BasicConsume(queue: queueName,
                autoAck: true,
                consumer: consumer);
            await Task.CompletedTask;
        }
    }
}
