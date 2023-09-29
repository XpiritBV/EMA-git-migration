using BitbucketMigrationTool.Commands.Test;
using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitbucketMigrationTool.Commands
{
    [Command(Name = "bitbucketmigration", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [Subcommand(typeof(MigrateCommand), typeof(TestCommand), typeof(AddToGroupCommand))]
    internal class DummyCommand : CommandBase
    {
        public DummyCommand(ILogger<DummyCommand> logger, IOptions<AppSettings> appSettingsOptions) 
            : base(logger, appSettingsOptions)
        {
        }
    }
}
