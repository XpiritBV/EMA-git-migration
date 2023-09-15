using BitbucketMigrationTool.Commands.Test;
using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitbucketMigrationTool.Commands
{
    [Command(Name = "bitbucketmigration", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [Subcommand(typeof(MigrateCommand), typeof(TestCommand))]
    internal class DummyCommand : CommandBase
    {
        public DummyCommand(ILogger<CommandBase> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient, AZDevopsClient aZDevopsClient) 
            : base(logger, appSettingsOptions, bitbucketClient, aZDevopsClient)
        {
        }
    }
}
