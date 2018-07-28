using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;

namespace NetworkSite.Pages
{
    public class IndexModel : PageModel
    {
        private IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public IList<Host> Hosts {get;set;}

        public async Task OnGetAsync()
        {
            //var s = _networkServiceClient.GetJson();
            var client = _httpClientFactory.CreateClient("networkService");
            var res = await client.GetAsync("/api/hosts");
            Hosts = await res.Content.ReadAsAsync<List<Host>>();            
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
