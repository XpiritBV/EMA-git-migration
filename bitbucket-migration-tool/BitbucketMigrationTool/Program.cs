using BitbucketMigrationTool.Commands;
using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

var configuration = new ConfigurationBuilder()
.SetBasePath(Directory.GetCurrentDirectory())
.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
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
                client.BaseAddress = new Uri(configuration.GetConnectionString("source"));
            });
        });

try
{
    return await builder.RunCommandLineApplicationAsync<MigrateCommand>(args);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    return 1;
}