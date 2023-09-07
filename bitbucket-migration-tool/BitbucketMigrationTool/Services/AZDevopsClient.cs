using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitbucketMigrationTool.Services
{
    internal class AZDevopsClient
    {
        private readonly HttpClient httpClient;
        
        public AZDevopsClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
    }
}
