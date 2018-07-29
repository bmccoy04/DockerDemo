using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NetworkHostedServices.Services
{
    public class NetworkListener : IHostedService    
    {
        private readonly IConnection _connection;
        private IModel _channel;
        private ILogger _logger;
        private Task _executingTask;


        public NetworkListener(ILogger<NetworkListener> logger) {
            _logger = logger;
            
            System.Threading.Thread.Sleep(20000);
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);
            };
            _channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);
             
            return Task.CompletedTask;
            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Network Network listener has stopped");
            _connection.Dispose();
            _channel.Dispose();
            return Task.CompletedTask;
        }
    }
}
