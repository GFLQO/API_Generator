using ArchiForge.Utils;
using System.IO;

namespace ArchiForge.Generators
{
    public static class MessagingGenerator
    {
        public static void Generate(string root, string name, string bus)
        {
            if (bus == "None") return;

            if (bus == "RabbitMQ")
            {
                ShellRunner.Run($"dotnet add {name}.Infrastructure/{name}.Infrastructure.csproj package RabbitMQ.Client", root);
                var code = $@"
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;

namespace MyApp.Infrastructure.EventBus
{{
    public class RabbitMqPublisher : IAsyncDisposable
    {{
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public RabbitMqPublisher(string hostName = ""localhost"")
        {{
                    var factory = new ConnectionFactory() {{ HostName = hostName }};

                    // Crée la connexion async
                    _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();

                    // Crée un channel async
                    _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
                }}

        public async Task PublishAsync(string queue, string message)
        {{
            await _channel.QueueDeclareAsync(queue, durable: false, exclusive: false, autoDelete: false);
            var body = Encoding.UTF8.GetBytes(message);
            await _channel.BasicPublishAsync(exchange: "", routingKey: queue, mandatory: false, body: body);
        }}

        public async ValueTask DisposeAsync()
        {{
            if (_channel != null) await _channel.CloseAsync();
            _connection?.Dispose();
        }}
    }}
}}";
var path = Path.Combine(root, $"{name}.Infrastructure", "EventBus", "RabbitMqPublisher.cs");
                FileHelper.WriteAllText(path, code);
            }

            if (bus == "Kafka")
            {
                ShellRunner.Run($"dotnet add {name}.Infrastructure/{name}.Infrastructure.csproj package Confluent.Kafka", root);
                var code = $@"
using Confluent.Kafka;
using System.Threading.Tasks;

namespace {name}.Infrastructure.EventBus
{{
    public class KafkaProducer
    {{
        private readonly IProducer<Null, string> _producer;
        public KafkaProducer(string bootstrapServers)
        {{
            var config = new ProducerConfig {{ BootstrapServers = bootstrapServers }};
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }}
        public Task Publish(string topic, string message)
        {{
            return _producer.ProduceAsync(topic, new Message<Null, string> {{ Value = message }});
        }}
    }}
}}";
                var path = Path.Combine(root, $"{name}.Infrastructure", "EventBus", "KafkaProducer.cs");
                FileHelper.WriteAllText(path, code);
            }

            if (bus == "Azure Service Bus")
            {
                ShellRunner.Run($"dotnet add {name}.Infrastructure/{name}.Infrastructure.csproj package Azure.Messaging.ServiceBus", root);
                var code = $@"
using Azure.Messaging.ServiceBus;
using System.Threading.Tasks;

namespace {name}.Infrastructure.EventBus
{{
    public class AzureServiceBusPublisher
    {{
        private readonly ServiceBusClient _client;
        public AzureServiceBusPublisher(string connectionString)
        {{
            _client = new ServiceBusClient(connectionString);
        }}
        public Task Publish(string queue, string message)
        {{
            var sender = _client.CreateSender(queue);
            return sender.SendMessageAsync(new ServiceBusMessage(message));
        }}
    }}
}}";
                var path = Path.Combine(root, $"{name}.Infrastructure", "EventBus", "AzureServiceBusPublisher.cs");
                FileHelper.WriteAllText(path, code);
            }
        }
    }
}
