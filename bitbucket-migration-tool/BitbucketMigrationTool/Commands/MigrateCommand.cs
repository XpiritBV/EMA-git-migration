using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitbucketMigrationTool.Commands
{
    [Command(Name = "migrate", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    internal class MigrateCommand
    {
        private readonly ILogger logger;
        private readonly AppSettings appSettings;
        private readonly BitbucketClient bitbucketClient;

        [Option("-p|--project", CommandOptionType.SingleValue, Description = "Project key")]
        public string Project { get; set; }

        public MigrateCommand(ILogger<MigrateCommand> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient)
        {
            this.logger = logger;
            this.appSettings = appSettingsOptions.Value;
            this.bitbucketClient = bitbucketClient;
        }

        private async Task<int> OnExecute(CommandLineApplication app)
        {
            var repositories = await bitbucketClient.GetRepositoriesAsync(Project);
            foreach (var repository in repositories)
            {
                logger.LogInformation($"Found repository {repository.Name}");
                var branches = await bitbucketClient.GetBranchesAsync(Project, repository.Slug);
                foreach (var branch in branches)
                {
                    logger.LogInformation($"\t-> Found branch {branch.DisplayId}");
                }

                var pullRequests = await bitbucketClient.GetPullRequests(Project, repository.Slug);
                foreach (var pullRequest in pullRequests)
                {
                    logger.LogInformation($"\t-> Found pull request {pullRequest.Title}");
                }
            }
            return 0;
        }

        private string GetVersion() => appSettings.AppVersion;
    }
}
