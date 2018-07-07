using System;
using System.Collections.Generic;
using NetworkService.Models;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace NetworkService.Services
{
    public class HostService : IHostService
    {
        public IEnumerable<Host> GetAll()
        {
            
            var config = new RedisConfiguration(){
                AbortOnConnectFail = false,
                KeyPrefix = "MyApp",
                Hosts = new RedisHost[]{
                    new RedisHost(){Host = "redis", Port = 6379}
                },
            };

            var cacheClient = new StackExchangeRedisCacheClient(new NewtonsoftSerializer(), config);

            return  cacheClient.Get<List<Host>>("myhost");
        }
    }
}