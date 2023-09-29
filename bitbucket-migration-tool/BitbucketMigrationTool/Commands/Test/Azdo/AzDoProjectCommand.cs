using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitbucketMigrationTool.Commands.Test.Azdo
{

    [Command(Name = "project", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    internal class AzDoProjectCommand : CommandBase
    {
        [Argument(0, nameof(Project), Description = "* Project key")]
        public string Project { get; set; }

        public AzDoProjectCommand(ILogger<AzDoProjectCommand> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient, AZDevopsClient azdevopsClient) : base(logger, appSettingsOptions, azdevopsClient)
        {
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            var projectsAzdo = await aZDevopsClient.GetProjectAsync(Project);

            await Console.Out.WriteLineAsync($"{projectsAzdo.Name} - {projectsAzdo.Id}");

            return 0;
        }
    }
}