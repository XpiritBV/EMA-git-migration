using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitbucketMigrationTool.Commands.Test.Azdo.Graph
{


    [Command(Name = "test", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    internal class AzDoGraphTestCommand : CommandBase
    {

        public AzDoGraphTestCommand(ILogger<AzDoGraphCommand> logger, IOptions<AppSettings> appSettingsOptions, AzDoGraphClient azDoGraphClient) : base(logger, appSettingsOptions, azDoGraphClient)
        {
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            var success = await azDoGraphClient.TestConnection() ? "Successfull" : "Failed";

            await Console.Out.WriteLineAsync($"Connection {success}");

            return 0;
        }
    }

        //[Command(Name = "group", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
        //internal class AzDoGraphGroupCommand : CommandBase
        //{
        //    public AzDoGraphGroupCommand(ILogger<AzDoGraphCommand> logger, IOptions<AppSettings> appSettingsOptions, AzDoGraphClient azDoGraphClient) : base(logger, appSettingsOptions, azDoGraphClient)
        //    {
        //    }
        //    protected override async Task<int> OnExecute(CommandLineApplication app)
        //    {
        //        var groups = await azDoGraphClient.GetGroupsAsync();
        //        await Console.Out.WriteLineAsync($"Number of Azure DevOps Groups: - {groups.Count()}");
        //        return 0;
        //    }
        //}
    }