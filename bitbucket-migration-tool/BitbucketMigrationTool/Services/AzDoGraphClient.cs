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
using BitbucketMigrationTool.Models.AzureDevops.Graph;

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

        public async IAsyncEnumerable<GraphGroup> GetGroupsAsync()
        {
            var token = string.Empty;

            do
            {
                var path = string.IsNullOrEmpty(token) ? "_apis/graph/groups" : $"_apis/graph/groups?continuationToken={token}";
                var response = await httpClient.GetAsync(path);
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ListResponse<GraphGroup>>(body, options);
                token = response.Headers.GetValues("x-ms-continuationtoken").FirstOrDefault();

                foreach (var group in result.Value)
                {
                    yield return group;
                }

            } while (!string.IsNullOrEmpty(token));
        }

        public async Task AddToGroup(GraphGroup group, string aadId)
        {
            var jsonRequestBody = JsonSerializer.Serialize(new
            {
                OriginId = aadId,
            }, options);

            var response = await httpClient.PostAsync($"_apis/graph/groups?groupDescriptors={group.Descriptor}", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                logger.LogError($"Failed to add aad group {aadId} to group {group.PrincipalName} - {body}");
            }
        }
    }
}
