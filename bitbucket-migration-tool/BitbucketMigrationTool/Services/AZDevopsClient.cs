using BitbucketMigrationTool.Models.AzureDevops;
using BitbucketMigrationTool.Models.AzureDevops.General;
using BitbucketMigrationTool.Models.AzureDevops.Repository;
using BitbucketMigrationTool.Models.AzureDevops.Repository.Threads;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BitbucketMigrationTool.Services
{
    internal class AZDevopsClient
    {
        private readonly HttpClient httpClient;
        private readonly ILogger logger;
        private readonly JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public AZDevopsClient(HttpClient httpClient, ILogger<AZDevopsClient> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            options.Converters.Add(new JsonStringEnumConverter());
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync()
        {
            var response = await httpClient.GetAsync("_apis/projects");
            var body = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ListResponse<Project>>(body, options);
            return result.Value;
        }

        public async Task<Repo?> GetRepositoryAsync(string projectKey, string repoKey)
        {
            var response = await httpClient.GetAsync($"${projectKey}/_apis/git/repositories/${repoKey}");

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Repo>(body, options);
            }
            return null;
        }

        public async Task<OperationLink?> CreateProjectAsync(CreateProjectRequest createProjectRequest)
        {
            var jsonRequestBody = JsonSerializer.Serialize(createProjectRequest, options);
            var response = await httpClient.PostAsync("_apis/projects", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OperationLink>(body, options);
            }
            return null;
        }

        public async Task<OperationLink?> GetOperationLinkAsync(string operationId)
        {
            var response = await httpClient.GetAsync($"_apis/operations/{operationId}");
            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<OperationLink>(body, options);
        }

        internal Task SetMainBranch(string targetProjectSlug, Guid repoId, string branchName)
        {
            var jsonRequestBody = JsonSerializer.Serialize(new
            {
                DefaultBranch = $"refs/heads/{branchName}"
            }, options);

            return httpClient.PatchAsync($"{targetProjectSlug}/_apis/git/repositories/{repoId}", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));
        }

        internal async Task<Repo> CreateRepositoryAsync(string targetProjectSlug, string targetRepositorySlug)
        {
            var jsonRequestBody = JsonSerializer.Serialize(new
            {
                Name = targetRepositorySlug
            }, options);

            var response = await httpClient.PostAsync($"{targetProjectSlug}/_apis/git/repositories", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));
            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Repo>(body, options);
        }

        internal async Task<CreatePullRequestResponse> CreatePullRequest(string targetProjectSlug, Guid repoId, CreatePullRequest pullRequest)
        {
            var jsonRequestBody = JsonSerializer.Serialize(pullRequest, options);

            var response = await httpClient.PostAsync($"{targetProjectSlug}/_apis/git/repositories/{repoId}/pullrequests", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));
            var body = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<CreatePullRequestResponse>(body, options);
        }

        internal async Task<CreatePullRequestThreadResponse> CreatePullRequestThread(string targetProjectSlug, Guid repoId, int pullRequestId, CreatePullRequestThreadRequest thread)
        {
            var jsonRequestBody = JsonSerializer.Serialize(thread, options);
            logger.LogDebug(jsonRequestBody);

            var response = await httpClient.PostAsync($"{targetProjectSlug}/_apis/git/repositories/{repoId}/pullrequests/{pullRequestId}/threads", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));
            var body = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<CreatePullRequestThreadResponse>(body, options);
        }

        internal async Task CreatePullRequestThreadComment(string targetProjectSlug, Guid repoId, int pullRequestId, int threadId, PullRequestThreadComment comment)
        {
            var jsonRequestBody = JsonSerializer.Serialize(comment, options);
            var response = await httpClient.PostAsync($"{targetProjectSlug}/_apis/git/repositories/{repoId}/pullrequests/{pullRequestId}/threads/{threadId}/comments", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));
        }
    }
}
