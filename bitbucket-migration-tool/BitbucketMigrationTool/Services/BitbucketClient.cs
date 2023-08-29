using BitbucketMigrationTool.Models.Bitbucket;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

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


        public Task<IEnumerable<Repository>> GetRepositoriesAsync(string projectKey)
            => GetPagedResultAsync<Repository>($"projects/{projectKey}/repos");

        public Task<IEnumerable<Branch>> GetBranchesAsync(string projectKey, string repositorySlug)
            => GetPagedResultAsync<Branch>($"projects/{projectKey}/repos/{repositorySlug}/branches");

        private async Task<IEnumerable<T>> GetPagedResultAsync<T>(string url)
        {
            var start = 0;
            string pagedUrl() => $"{url}?start={start}";

            var result = new List<T>();
            PagedResult<T> pagedResult;

            try
            {
                do
                {
                    pagedResult = await httpClient.GetFromJsonAsync<PagedResult<T>>(pagedUrl());
                    result.AddRange(pagedResult.Values);
                    start = pagedResult.NextPageStart ?? 0;
                } while (!pagedResult.IsLastPage);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failed to get paged result from {pagedUrl()}");
                throw;
            }

            return result;
        }
    }
}
