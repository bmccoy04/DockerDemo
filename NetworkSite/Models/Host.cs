using System;

namespace NetworkSite.Models
{
    public class Host 
    {
        public string Latency { get; set; }
        public bool IsUp { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
    
    }
}