using BitbucketMigrationTool.Commands;
using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System.Net.Http;
using System.Text;

var configuration = new ConfigurationBuilder()
.SetBasePath(Directory.GetCurrentDirectory())
.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
.AddUserSecrets<Program>()
.Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = new HostBuilder()
        .ConfigureServices((hostContext, services) =>
        {
            services.AddLogging(config =>
            {
                config.ClearProviders();
                config.AddProvider(new SerilogLoggerProvider(Log.Logger));
                var minimumLevel = configuration.GetSection("Serilog:MinimumLevel")?.Value;
                if (!string.IsNullOrEmpty(minimumLevel))
                {
                    config.SetMinimumLevel(Enum.Parse<LogLevel>(minimumLevel));
                }
            });

            services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));

            services.AddHttpClient<BitbucketClient>(client =>
            {
                var bitbucketConfig = configuration.GetSection("Bitbucket").Get<RepositoryConfig>();
                client.BaseAddress = new Uri($"{bitbucketConfig.Url}/rest/api/{bitbucketConfig.ApiVersion}/");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bitbucketConfig.Key}");
            });

            services.AddHttpClient<AZDevopsClient>(client =>
            {
                var azdevopsConfig = configuration.GetSection("AzureDevops").Get<RepositoryConfig>();
                client.BaseAddress = new Uri($"{azdevopsConfig.Url}/_apis/");
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{azdevopsConfig.Key}"));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
                client.DefaultRequestHeaders.Add("Accept", $"application/json; api-version={azdevopsConfig.ApiVersion}");
            });
        });

try
{
    return await builder.RunCommandLineApplicationAsync<DummyCommand>(args);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    return 1;
}