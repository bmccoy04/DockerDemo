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

namespace NetworkScanner
{
    class Program
    {
        private static List<string> output = new List<string>{"Starting Nmap 7.40 ( https://nmap.org ) at 2018-07-25 01:27 UTC",
            "Nmap scan report for 172.17.0.1",
            "Host is up (0.000043s latency)",
            "MAC Address: 02:42:31:20:D2:3A (Unknown)",
            "Nmap scan report for 172.17.0.2",
            "MAC Address: 02:42:31:20:D2:3A (Unknown)",
            "Host is up.",
            "Nmap done: 256 IP addresses (2 hosts up) scanned in 5.91 seconds"
        };

        static void Main(string[] args)
        {
            Console.WriteLine("start");
            var ipAddressRange = GetIpAddressRange();
            
            Console.WriteLine("My Ip Address: " + ipAddressRange);
            var readOut = GetConsoleReadOut(ipAddressRange);
            
            var hostList = ProcessOutPut(readOut);
            foreach(var host in hostList) {
                Console.WriteLine(">> " + host.IpAddress + "  " 
                + host.Latency + " " 
                + host.MacAddress + " "
                + host.IsUp);
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
                Console.WriteLine(line);
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

        private static void RedisTest()
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

            var host = new List<Host>() {
                new Host() {
                    IsUp = true,
                    MacAddress = "38:EA:A7:55:C4:E0",
                    IpAddress = "192.168.7.15",
                },
                new Host() {
                    IsUp = false,
                    MacAddress = "6C:33:A9:72:22:22",
                    IpAddress = "192.168.7.121",
                }
            };

            Console.WriteLine("Adding Hosts...");

            cacheClient.Add("myhost", host);

            Console.WriteLine("Hosts Added");
            var s = Console.ReadLine();
            Console.WriteLine(s);
        }

        private static List<string> GetConsoleReadOut(string ipAddressRange) {
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
                    Console.WriteLine(e.Data);
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
