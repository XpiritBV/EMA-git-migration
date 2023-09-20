using BitbucketMigrationTool.Models.AzureDevops.General;
using BitbucketMigrationTool.Models.AzureDevops.Repository.Threads;
using BitbucketMigrationTool.Models.AzureDevops.Repository;
using BitbucketMigrationTool.Models.AzureDevops;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace BitbucketMigrationTool.Services
{
    internal class AzDoGraphClient
    {
        private readonly HttpClient httpClient;
        private readonly ILogger logger;
        private readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

        public AzDoGraphClient(HttpClient httpClient, ILogger<AzDoGraphClient> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            options.Converters.Add(new JsonStringEnumConverter());
        }

        public async Task<bool> TestConnection()
        {
            try
            {
                var response = await httpClient.GetAsync("_apis/graph/groups");
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }

            //var result = JsonSerializer.Deserialize<ListResponse<Project>>(body, options);
            //return result.Value;
        }


        //public async Task<IEnumerable<string>> GetGroupsAsync()
        //{
        //    var response = await httpClient.GetAsync("_apis/graph/groups");
        //    var body = await response.Content.ReadAsStringAsync();

        //    return Enumerable.Empty<string>();

        //    //var result = JsonSerializer.Deserialize<ListResponse<Project>>(body, options);
        //    //return result.Value;
        //}

        //public async Task<Repo?> GetRepositoryAsync(string projectKey, string repoKey)
        //{
        //    var response = await httpClient.GetAsync($"{projectKey}/_apis/git/repositories/{repoKey}");

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var body = await response.Content.ReadAsStringAsync();
        //        return JsonSerializer.Deserialize<Repo>(body, options);
        //    }
        //    return null;
        //}

        //public async Task<OperationLink?> CreateProjectAsync(CreateProjectRequest createProjectRequest)
        //{
        //    var jsonRequestBody = JsonSerializer.Serialize(createProjectRequest, options);
        //    var response = await httpClient.PostAsync("_apis/projects", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var body = await response.Content.ReadAsStringAsync();
        //        return JsonSerializer.Deserialize<OperationLink>(body, options);
        //    }
        //    return null;
        //}

        //internal Task SetMainBranch(string targetProjectSlug, Guid repoId, string branchName)
        //{
        //    var jsonRequestBody = JsonSerializer.Serialize(new
        //    {
        //        DefaultBranch = $"refs/heads/{branchName}"
        //    }, options);

        //    return httpClient.PatchAsync($"{targetProjectSlug}/_apis/git/repositories/{repoId}", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));
        //}
    }
}
