using BitbucketMigrationTool.Commands.Test.Azdo;
using BitbucketMigrationTool.Commands.Test.Azdo.Graph;
using BitbucketMigrationTool.Commands.Test.Bitbucket;
using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitbucketMigrationTool.Commands.Test
{
    [Command(Name = "test", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [Subcommand(typeof(AzDoCommand), typeof(BitBUcketCommand), typeof(LargeFileCommand))]
    internal class TestCommand : CommandBase
    {
        public TestCommand(ILogger<TestCommand> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient, AZDevopsClient azdevopsClient) : base(logger, appSettingsOptions, bitbucketClient, azdevopsClient)
        {
        }
    }

    [Command(Name = "bb", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [Subcommand(typeof(BitbucketPermissionsCommand), typeof(BitbucketProjectCommand))]
    internal class BitBUcketCommand : CommandBase
    {
        public BitBUcketCommand(ILogger<BitBUcketCommand> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient, AZDevopsClient azdevopsClient) : base(logger, appSettingsOptions, bitbucketClient, azdevopsClient)
        {
        }
    }

    [Command(Name = "azdo", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [Subcommand(typeof(AzDoProjectCommand), typeof(AzDoProjectsCommand), typeof(AzDoGraphCommand))]
    internal class AzDoCommand : CommandBase
    {
        public AzDoCommand(ILogger<AzDoCommand> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient, AZDevopsClient azdevopsClient) : base(logger, appSettingsOptions, azdevopsClient)
        {
        }
    }
}

