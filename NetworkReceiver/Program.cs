using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NetworkReceiver
{
    class Program
    {
        static void Main(string[] args)
        {            
            
            System.Threading.Thread.Sleep(15000);
            

            var factory = new ConnectionFactory() { HostName = "rabbitmq"};
            using (var connection = factory.CreateConnection())
            {
                using(var channel = connection.CreateModel())
                {
                    
                    Console.WriteLine("Channel Created ");
                    channel.QueueDeclare(queue: "myque",
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (model, ea) => 
                    {
                        Console.WriteLine("I heard something");
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine(" Reciver received message {0}", message);
                    };
                    channel.BasicConsume(queue: "myque",
                                        autoAck:true,                                        
                                        consumer:consumer);
                }


            }

            Console.WriteLine("Press \"x\" to exit:");
            
            var s = "s";
            
            
            var keys = Console.ReadKey();

            Console.WriteLine("Es no bueno: " + s);
        }
    }
}
