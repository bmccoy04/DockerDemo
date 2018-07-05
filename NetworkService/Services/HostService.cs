using System;
using System.Collections.Generic;
using NetworkService.Models;

namespace NetworkService.Services
{
    public class HostService : IHostService
    {
        public IEnumerable<Host> GetAll()
        {
            return new List<Host>() {
                new Host() {
                    IsUp = true,
                    Latency = 0.00041,
                    MacAddress = "38:EA:A7:55:C4:E0",
                    IpAddress = "192.168.7.115",
                    Manufacturer = "Hewlett Packard"
                },
                new Host() {
                    IsUp = false,
                    Latency = 0.00,
                    MacAddress = "6C:33:A9:72:F1:E7",
                    IpAddress = "192.168.7.113",
                    Manufacturer = "Magic Jack"
                }
            };
        }
    }
}