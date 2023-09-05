using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Models.Bitbucket.PullRequest;
using BitbucketMigrationTool.Models.Bitbucket.Repository;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.RegularExpressions;
using MarkdownLink = BitbucketMigrationTool.Models.Markdown.Link;
namespace BitbucketMigrationTool.Commands
{
    [Command(Name = "migrate", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    internal class MigrateCommand : CommandBase
    {

        [Option("-p|--project", CommandOptionType.SingleValue, Description = "Project key")]
        public string Project { get; set; }

        public MigrateCommand(ILogger<MigrateCommand> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient) :base(logger,appSettingsOptions, bitbucketClient)
        {
        }

        private async Task<int> OnExecute(CommandLineApplication app)
        {
            var repositories = await bitbucketClient.GetRepositoriesAsync(Project);
            foreach (var repository in repositories)
            {
                logger.LogInformation($"Found repository {repository.Name}");

                //TODO: add ssh credentials
                await GitAction($"clone {repository.Links.Clone.First(x => x.Name == "http").Href} tempdir");

                var branches = await bitbucketClient.GetBranchesAsync(Project, repository.Slug);
                foreach (var branch in branches.OrderBy(b => b.Default ? 1 : 0))
                {
                    logger.LogInformation($"\t-> Found branch {branch.DisplayId}");

                    await GitAction($"checkout {branch.DisplayId}", "tempdir");
                }

                await GitAction("fetch --tags", "tempdir");
                await GitAction("remote rm origin", "tempdir");

                var pullRequests = await bitbucketClient.GetPullRequests(Project, repository.Slug);
                foreach (var pullRequest in pullRequests)
                {
                    logger.LogInformation($"\t-> Found pull request {pullRequest.Title}");
                    var activities = await bitbucketClient.GetPullRequestActivities(Project, repository.Slug, pullRequest.Id);
                    foreach (var activity in activities.Where(x => x.Action == ActivityActionType.COMMENTED).OrderBy(x => x.CreatedDate))
                    {
                        logger.LogInformation($"\t\t-> Found activity {activity.Action}");
                        logger.LogInformation($"\t\t-> {activity.Comment.Author.DisplayName}: {activity.Comment.Text}");
                        var links = ScanForLinks(activity.Comment.Text);
                        foreach (var link in links)
                        {
                            logger.LogInformation($"\t\t\t-> Found link {link}, is an attachment: {link.IsAttachment}");
                        }
                    }
                }
            }
            return 0;
        }

        

        private IEnumerable<MarkdownLink> ScanForLinks(string text)
        {
            var regex = new Regex(@"\[(?<text>[^\]]+)\]\((?<url>[^\)]+)\)");
            var matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                yield return new MarkdownLink(match.Groups["text"].Value, match.Groups["url"].Value);
            }
        }
    }
}
