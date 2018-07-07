using System;
using System.Collections.Generic;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace NetworkScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new RedisConfiguration(){
                AbortOnConnectFail = false,
                KeyPrefix = "MyApp",
                Hosts = new RedisHost[]{
                    new RedisHost(){Host = "redis", Port = 6379}
                },
            };

            var cacheClient = new StackExchangeRedisCacheClient(new NewtonsoftSerializer(), config);
            
            var host = new List<Host>() {
                new Host() {
                    IsUp = true,
                    Latency = 0.00041,
                    MacAddress = "38:EA:A7:55:C4:E0",
                    IpAddress = "192.168.7.15",
                    Manufacturer = "Hewlett P"
                },
                new Host() {
                    IsUp = false,
                    Latency = 0.00,
                    MacAddress = "6C:33:A9:72:22:22",
                    IpAddress = "192.168.7.121",
                    Manufacturer = "Mac Book from redis cache"
                }
            };

            Console.WriteLine("Adding Hosts...");

            cacheClient.Add("myhost", host);

            Console.WriteLine("Hosts Added");
            var s = Console.ReadLine();
            Console.WriteLine(s);
        }
    }

    public class Host 
    {
        public double Latency { get; set; }
        public bool IsUp { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string Manufacturer { get; set; }
    }
}
