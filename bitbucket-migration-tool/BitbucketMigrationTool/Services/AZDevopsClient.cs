using BitbucketMigrationTool.Models.AzureDevops;
using BitbucketMigrationTool.Models.AzureDevops.General;
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
            var response = await httpClient.GetAsync("projects");
            var body = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ListResponse<Project>>(body, options);
            return result.Value;
        }

        public async Task<Project> CreateProjectAsync(CreateProjectRequest createProjectRequest)
        {
            var jsonRequestBody = JsonSerializer.Serialize(createProjectRequest, options);
            var response = await httpClient.PostAsync("projects", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));
            var body = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Project>(body, options);
        }
    }
}
