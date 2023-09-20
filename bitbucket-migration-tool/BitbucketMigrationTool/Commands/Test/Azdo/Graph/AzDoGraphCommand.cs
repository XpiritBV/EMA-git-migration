using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitbucketMigrationTool.Commands.Test.Azdo.Graph
{
    [Command(Name = "graph", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [Subcommand(typeof(AzDoGraphTestCommand))]
    internal class AzDoGraphCommand : CommandBase
    {

        public AzDoGraphCommand(ILogger<AzDoGraphCommand> logger, IOptions<AppSettings> appSettingsOptions, AzDoGraphClient azDoGraphClient) : base(logger, appSettingsOptions, azDoGraphClient)
        {
        }
    }
}