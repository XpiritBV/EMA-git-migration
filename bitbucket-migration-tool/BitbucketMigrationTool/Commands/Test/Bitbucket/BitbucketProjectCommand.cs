using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitbucketMigrationTool.Commands.Test.Bitbucket
{
    [Command(Name = "project", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    internal class BitbucketProjectCommand : CommandBase
    {
        public BitbucketProjectCommand(ILogger<LargeFileCommand> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient, AZDevopsClient azdevopsClient) : base(logger, appSettingsOptions, bitbucketClient, azdevopsClient)
        {
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            // Bitbucket Projects
            var projectsBB = await bitbucketClient.GetProjectsAsync();

            await Console.Out.WriteLineAsync($"Number of Bitbucket Projects: - {projectsBB.Count()}");

            foreach (var project in projectsBB)
            {
                var repos = await bitbucketClient.GetRepositoriesAsync(project.Key);

                await Console.Out.WriteLineAsync($"Number of Repos: - {repos.Count()}");

                foreach (var repo in repos)
                {
                    var branches = await bitbucketClient.GetBranchesAsync(project.Key, repo.Slug);
                    await Console.Out.WriteLineAsync($"Number of Branches: - {branches.Count()}");

                    var prs = await bitbucketClient.GetPullRequests(project.Key, repo.Slug);
                    await Console.Out.WriteLineAsync($"Number of Prs: - {prs.Count()}");
                }
            }
            return 0;
        }
    }
}
