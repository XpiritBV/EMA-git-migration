using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitbucketMigrationTool.Commands
{
    [Command(Name = "test", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    internal class TestCommand : CommandBase
    {

        public TestCommand(ILogger<LargeFileCommand> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient, AZDevopsClient azdevopsClient) : base(logger, appSettingsOptions, bitbucketClient, azdevopsClient)
        {
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            // Azure DevOps Projects
            var projectsAzdo = await aZDevopsClient.GetProjectsAsync();

            await Console.Out.WriteLineAsync($"Number of Azure DevOps Projects: - {projectsAzdo.Count()}");

            // Bitbucket Projects
            var projectsBB = await bitbucketClient.GetProjectsAsync();

            await Console.Out.WriteLineAsync($"Number of Bitbucket Projects: - {projectsBB.Count()}");

            // Add Other Connections

            return 0;
        }
    }
}
