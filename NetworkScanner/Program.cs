using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;
using RabbitMQ.Client;
using Newtonsoft.Json;

namespace NetworkScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            
            
            System.Threading.Thread.Sleep(20000);
            
            var number = 1;
            while(true){
                Console.WriteLine("Starting scan number:" + number++);
                var ipAddressRange = GetIpAddressRange();
                
                Console.WriteLine("My Ip Address Range: " + ipAddressRange);
                var readOut = GetConsoleReadOut(ipAddressRange);
                
                var hostList = ProcessOutPut(readOut);
                //PushToRedis(hostList);

                SendCommand(hostList);

                foreach(var host in hostList) {
                    Console.WriteLine(">> " + host.IpAddress + "  " 
                    + host.Latency + " " 
                    + host.MacAddress + " "
                    + host.IsUp);
                }
            }
        }

        private static void SendCommand(IList<Host> hostList)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq"};
            using (var connection = factory.CreateConnection())
            {
                using(var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "myque",
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

                    string message = "Hello World";// JsonConvert.SerializeObject(hostList);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                        routingKey: "myque",
                                        basicProperties: null,
                                        body: body);
                    Console.WriteLine("Sender sent: {0}", message);
                }
            }
        }

        private static string GetIpAddressRange()
        {
            var s = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();

            return s.Remove(s.Length -1, 1) + "*";
        }

        private static IList<Host> ProcessOutPut(List<string> readOut)
        {
            var hosts = new List<Host>();
            var currentHost = new Host();

            foreach(var line in readOut)
            {
                if(IsScanReportLine(line)) {
                    if(currentHost != null)
                        hosts.Add(currentHost);
                    currentHost = new Host();
                    currentHost.IpAddress = GetIpAddress(line);
                    currentHost.IsUp = true;
                } else if(HostIsUpLine(line)) {
                    if(currentHost != null)
                        currentHost.Latency = GetLatency(line);
                } else if(IsMacAddressLine(line))
                    if(currentHost != null)
                        currentHost.MacAddress = GetMacAddress(line);
            }

            if(currentHost != null)
                hosts.Add(currentHost);
            return hosts;
        }

        private static string GetMacAddress(string line)
        {
            return line.Substring(13, line.Length - 13);
        }

        private static bool IsMacAddressLine(string line)
        {
            return line.Contains("MAC Address:"); 
        }

        private static string GetLatency(string line)
        {
            return line.Substring(10, line.Length - 10);
        }

        private static bool HostIsUpLine(string line)
        {
            return line.Contains("Host is up ");
        }

        private static string GetIpAddress(string line)
        {
            return line.Substring(20, line.Length - 20);
        }

        private static bool NmapDoneLine(string line)
        {
            return line.Contains("Nmap done:");
        }

        private static bool IsScanReportLine(string line)
        {
            return line.Contains("Nmap scan report for");
        }

        private static void PushToRedis(IList<Host> hosts)
        {
            var config = new RedisConfiguration()
            {
                AbortOnConnectFail = false,
                KeyPrefix = "MyApp",
                Hosts = new RedisHost[]{
                    new RedisHost(){Host = "redis", Port = 6379}
                },
            };

            var cacheClient = new StackExchangeRedisCacheClient(new NewtonsoftSerializer(), config);

            Console.WriteLine("Adding Hosts...");

            cacheClient.Add("myhost", hosts);

            Console.WriteLine("Hosts Added");
        }

        private static List<string> GetConsoleReadOut(string ipAddressRange) 
        {
            int lineCount = 0;
            var readOut = new List<string>();
            StringBuilder sb = new StringBuilder();
            Process process = new Process();
            
            process.StartInfo.FileName = "nmap";
            //process.StartInfo.ArgumentList.Add("-sL");
            // -sP -PS -n 192.165.7.*
            process.StartInfo.ArgumentList.Add("-sP");
            //process.StartInfo.ArgumentList.Add("-PS");
            //process.StartInfo.ArgumentList.Add("-n");
            process.StartInfo.ArgumentList.Add(ipAddressRange);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) => 
            {
                if(!string.IsNullOrEmpty(e.Data))
                {
                    readOut.Add(e.Data);
                }
            });
            process.Start();

            process.BeginOutputReadLine();
            process.WaitForExit();
            process.Close();
            
            return readOut;
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
