using BitbucketMigrationTool.Models.Bitbucket.General;
using BitbucketMigrationTool.Models.Bitbucket.PullRequest;
using BitbucketMigrationTool.Models.Bitbucket.Repository;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;

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

        public Task<IEnumerable<Project>> GetProjectsAsync()
            => GetPagedResultAsync<Project>($"projects");

        public Task<IEnumerable<Repo>> GetRepositoriesAsync(string projectKey)
            => GetPagedResultAsync<Repo>($"projects/{projectKey}/repos");

        public Task<Repo?> GetRepositoryAsync(string projectKey, string repositorySlug)
            => httpClient.GetFromJsonAsync<Repo>($"projects/{projectKey}/repos/{repositorySlug}");

        public Task<IEnumerable<Branch>> GetBranchesAsync(string projectKey, string repositorySlug)
            => GetPagedResultAsync<Branch>($"projects/{projectKey}/repos/{repositorySlug}/branches");

        public Task<IEnumerable<PR>> GetPullRequests(string projectKey, string repositorySlug)
            => GetPagedResultAsync<PR>($"projects/{projectKey}/repos/{repositorySlug}/pull-requests");

        public Task<IEnumerable<Activity>> GetPullRequestActivities(string projectKey, string repositorySlug, int pullRequestId)
            => GetPagedResultAsync<Activity>($"projects/{projectKey}/repos/{repositorySlug}/pull-requests/{pullRequestId}/activities");

        public Task<Stream> GetAttachment(string projectKey, string repositorySlug, int attachmentId)
            => httpClient.GetStreamAsync($"projects/{projectKey}/repos/{repositorySlug}/attachments/{attachmentId}");

        public Task<IEnumerable<UserPermission>> GetProjectPermissionsAsync(string projectKey, string type = "users")
        => GetPagedResultAsync<UserPermission>($"projects/{projectKey}/permissions/{type}");

        public Task<IEnumerable<UserPermission>> GetRepoPermissionsAsync(string projectKey, string repositorySlug, string type = "users")
        => GetPagedResultAsync<UserPermission>($"projects/{projectKey}/repos/{repositorySlug}/permissions/{type}");


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
                    var response = await httpClient.GetAsync(pagedUrl());
                    var body = await response.Content.ReadAsStringAsync();

                    // logger.LogDebug(body);

                    var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                    options.Converters.Add(new JsonStringEnumConverter());
                    pagedResult = JsonSerializer.Deserialize<PagedResult<T>>(body, options);
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
