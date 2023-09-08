using BitbucketMigrationTool.Models.AzureDevops;
using BitbucketMigrationTool.Models.AzureDevops.General;
using BitbucketMigrationTool.Models.AzureDevops.Repository;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BitbucketMigrationTool.Services
{
    internal class AZDevopsClient
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public AZDevopsClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
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
    }
}
