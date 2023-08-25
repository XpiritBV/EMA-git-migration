using BitbucketMigrationTool.Models;
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

        public MigrateCommand(ILogger<MigrateCommand> logger, IOptions<AppSettings> appSettingsOptions)
        {
            this.logger = logger;
            this.appSettings = appSettingsOptions.Value;
        }

        private Task<int> OnExecute(CommandLineApplication app)
        {
            logger.LogInformation("Migrate command executed");
            return Task.FromResult(0);
        }

        private string GetVersion() => appSettings.AppVersion;
    }
}
