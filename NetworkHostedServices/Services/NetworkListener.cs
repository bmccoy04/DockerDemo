using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;

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
            
            System.Threading.Thread.Sleep(25000);
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
                SendToRedis(message);
                Console.WriteLine(" [x] Received {0}", message);
            };
            _channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);
             
            return Task.CompletedTask;
            
        }

        private void SendToRedis(string message)
        {
            var hosts = JsonConvert.DeserializeObject<List<Host>>(message);
            var config = new RedisConfiguration()
            {
                AbortOnConnectFail = false,
                KeyPrefix = "MyApp",
                Hosts = new RedisHost[]{
                    new RedisHost(){Host = "redis", Port = 6379}
                },
            };

            using(var cacheClient = new StackExchangeRedisCacheClient(new NewtonsoftSerializer(), config))
            {
                Console.WriteLine("Adding Hosts...");

                cacheClient.Add("myhost", hosts);
                
                Console.WriteLine("Hosts Added");
            }

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Network Network listener has stopped");
            _connection.Dispose();
            _channel.Dispose();
            return Task.CompletedTask;
        }
    }

    public class Host
    {
        public string Latency { get; set; }
        public bool IsUp { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
    }
}
