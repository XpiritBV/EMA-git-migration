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
        private readonly AzDoGraphClient azDoGraphClient;
        private readonly AZDevopsClient devopsClient;

        [Argument(0, nameof(Project), Description = "* Project key")]
        public string Project { get; set; }

        [Argument(1, nameof(Project), Description = "* AAD id for the admin group")]
        public string AdminGroupId { get; set; }

        [Argument(2, nameof(Project), Description = "* AAD id for the team group")]
        public string TeamGroupId { get; set; }

        public AddToGroupCommand(ILogger<AddToGroupCommand> logger, IOptions<AppSettings> appSettingsOptions, AzDoGraphClient azDoGraphClient, AZDevopsClient devopsClient)
            : base(logger, appSettingsOptions)
        {
            this.azDoGraphClient = azDoGraphClient;
            this.devopsClient = devopsClient;
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            var project = await devopsClient.GetProjectAsync(Project);
            if (project == null)
            {
                logger.LogError($"Project {Project} not found");
                return 1;
            }

            var groups = await FindGroups();

            await azDoGraphClient.AddToGroup(groups.Admin, AdminGroupId);
            await azDoGraphClient.AddToGroup(groups.Team, TeamGroupId);
            
            return 0;
        }

        private async Task<(GraphGroup Admin, GraphGroup Team)> FindGroups()
        {
            GraphGroup teamGroup = null;
            GraphGroup adminGroup = null;

            await foreach (var group in azDoGraphClient.GetGroupsAsync())
            {
                if (!group.PrincipalName.Contains(Project))
                    continue;


                if (group.PrincipalName.Contains("Project Administrators"))
                {
                    adminGroup = group;
                }
                else if (group.PrincipalName.Contains("Team"))
                {
                    teamGroup = group;
                }

                if (teamGroup != null && adminGroup != null)
                {
                    break;
                }
            }

            return (adminGroup, teamGroup);
        }

    }
}
