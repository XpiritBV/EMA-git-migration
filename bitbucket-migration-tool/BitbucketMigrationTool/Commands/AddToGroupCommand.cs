using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Models.AzureDevops.Graph;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;

namespace BitbucketMigrationTool.Commands
{
    [Command(Name = "addtogroup", OptionsComparison = StringComparison.InvariantCultureIgnoreCase, Description = "Add AAD groups to the Azure devops groups")]
    internal class AddToGroupCommand : CommandBase
    {
        [Argument(0, nameof(Project), Description = "* Project key")]
        public string Project { get; set; }

        [Argument(1, nameof(AdminGroupId), Description = "* AAD id for the admin group")]
        public string AdminGroupId { get; set; }

        [Argument(2, nameof(TeamGroupId), Description = "* AAD id for the team group")]
        public string TeamGroupId { get; set; }

        public AddToGroupCommand(ILogger<AddToGroupCommand> logger, IOptions<AppSettings> appSettingsOptions, AZDevopsClient devopsClient, AzDoGraphClient azDoGraphClient)
            : base(logger, appSettingsOptions, devopsClient, azDoGraphClient)
        {
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            var project = await aZDevopsClient.GetProjectAsync(Project);
            if (project == null)
            {
                logger.LogError($"Project {Project} not found");
                return 1;
            }

            var groups = await FindGroups();

            if (!string.IsNullOrEmpty(groups.Admin?.Descriptor))
            {
                await azDoGraphClient.AddToGroup(groups.Admin, AdminGroupId);
            }
            else
            {
                logger.LogError($"Group 'Project Administrators' not found");
            }
            if (!string.IsNullOrEmpty(groups.Team?.Descriptor))
            {
                await azDoGraphClient.AddToGroup(groups.Team, TeamGroupId);
            }
            else
            {
                logger.LogError($"Group '{Project} Team' not found");
            }

            return 0;
        }

        private async Task<(GraphGroup? Admin, GraphGroup? Team)> FindGroups()
        {
            GraphGroup? teamGroup = await azDoGraphClient.GetGroupAsync(Project, $"{Project} Team");
            GraphGroup? adminGroup = await azDoGraphClient.GetGroupAsync(Project, $"Project Administrators");
            return (adminGroup, teamGroup);
        }

    }
}
