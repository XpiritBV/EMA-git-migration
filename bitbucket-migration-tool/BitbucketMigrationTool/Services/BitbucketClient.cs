using Microsoft.Extensions.Logging;

namespace BitbucketMigrationTool.Services
{
    internal class BitbucketClient
    {
        private readonly ILogger logger;
        private readonly HttpClient httpClient;

        public BitbucketClient(ILogger<BitbucketClient> logger, HttpClient httpClient)
        {
            this.logger = logger;
            this.httpClient = httpClient;
        }


    }
}
