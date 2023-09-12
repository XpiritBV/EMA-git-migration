using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Models.Bitbucket.PullRequest;
using BitbucketMigrationTool.Models.Bitbucket.Repository;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using MarkdownLink = BitbucketMigrationTool.Models.Markdown.Link;
using AZRepo = BitbucketMigrationTool.Models.AzureDevops.Repository.Repo;
using AZPullRequest = BitbucketMigrationTool.Models.AzureDevops.Repository.PullRequest;

namespace BitbucketMigrationTool.Commands
{
    [Command(Name = "migrate", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    internal class MigrateCommand : CommandBase
    {
        private const string tempDir = "tempdir";

        [Option("-p|--project", CommandOptionType.SingleValue, Description = "Project key")]
        public string Project { get; set; }

        [Option("-r|--repository", CommandOptionType.SingleValue, Description = "Repository slug")]
        public string Repository { get; set; }

        [Option("-b|--branch", CommandOptionType.MultipleValue, Description = "Aditional branches to migrate")]
        public string[] Branches { get; set; } = Array.Empty<string>();

        [Option("-tp|--target-prefix", CommandOptionType.SingleOrNoValue, Description = "Prefix for target project")]
        public (bool HasValue, string Value) TargetPrefix { get; set; }

        [Option("-t|--target", CommandOptionType.SingleOrNoValue, Description = "Target project slug")]
        public (bool HasValue, string Value) TargetProject { get; set; }

        [Option("-tr|--target-repository", CommandOptionType.SingleOrNoValue, Description = "Target repository slug")]
        public (bool HasValue, string Value) TargetRepository { get; set; }

        private string TargetProjectSlug => $"{(TargetPrefix.HasValue ? $"{TargetPrefix.Value}-" : string.Empty)}{(TargetProject.HasValue ? TargetProject.Value : Project)}";

        private string TargetRepositorySlug => TargetRepository.HasValue ? TargetRepository.Value : Repository;

        public MigrateCommand(ILogger<MigrateCommand> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient, AZDevopsClient aZDevopsClient) 
            : base(logger, appSettingsOptions, bitbucketClient, aZDevopsClient)
        {
        }

        /*
            Create project if not exists (Original name or new name)
	            Create repository if not exists (Original name or new name)
		            Create branches if not exist, full history
		            Create PR if not exists and if necessary (author!)
			            Create Comment if not exists (author!)
			            Create Attachment if not exists and if necessary
        */

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            await DeleteFolder(tempDir);

            var repository = await bitbucketClient.GetRepositoryAsync(Project, Repository);
            if (repository == null)
            {
                logger.LogError($"Repository {Repository} not found");
                return 1;
            }
            var branches = await bitbucketClient.GetBranchesAsync(Project, repository.Slug);
            await EnsureAZDevopsProjectisCreated();

            var targetRepository = await aZDevopsClient.GetRepositoryAsync(TargetProjectSlug, TargetRepositorySlug)
                ?? (await aZDevopsClient.CreateRepositoryAsync(TargetProjectSlug, TargetRepositorySlug));

            await CloneRepo(repository);
            await CheckoutBranches(branches);
            await SwitchGitRemote(targetRepository);
            await HandlePullRequests(repository, targetRepository);
            await FixDefaultBranch(branches, targetRepository);
            await DeleteFolder(tempDir);

            return 0;
        }

        private async Task FixDefaultBranch(IEnumerable<Branch> branches, AZRepo targetRepository)
        {
            var defaultBranch = branches.FirstOrDefault(x => x.Default || x.DisplayId.Equals("main", StringComparison.InvariantCultureIgnoreCase) || x.DisplayId.Equals("master", StringComparison.InvariantCultureIgnoreCase));
            await aZDevopsClient.SetMainBranch(TargetProjectSlug, targetRepository.Id, defaultBranch.DisplayId);
        }

        private async Task EnsureAZDevopsProjectisCreated()
        {
            var projects = await aZDevopsClient.GetProjectsAsync();

            var targerProject = projects.FirstOrDefault(x => x.Name == TargetProjectSlug);
            if (targerProject == null)
            {
                var result = await aZDevopsClient.CreateProjectAsync(new Models.AzureDevops.CreateProjectRequest { Name = TargetProjectSlug, Description = "", Visibility = Models.AzureDevops.ProjectVisibility.Private });
                if(result == null)
                {
                    logger.LogError($"Failed to create project {TargetProjectSlug}");
                    throw new Exception($"Failed to create project {TargetProjectSlug}");
                }

                while (!result.Status.Equals("succeeded"))
                {
                    logger.LogDebug($"Waiting for project to be created");
                    await Task.Delay(100);
                    result = await aZDevopsClient.GetOperationLinkAsync(result.Id);
                }
            }
        }

        private async Task CloneRepo(Repo repository)
        {
            logger.LogInformation($"Cloning repository {repository.Name}");

            await GitAction($"clone {repository.Links.Clone.First(x => x.Name == "http").Href} {tempDir}");
        }

        private async Task SwitchGitRemote(AZRepo repo)
        {
            await GitAction("fetch --tags", tempDir);
            await GitAction("remote rm origin", tempDir);
            await GitAction($"remote add origin {repo.RemoteUrl}", tempDir);
            await GitAction("push origin --all", tempDir);
            await GitAction("push --tags", tempDir);
        }

        private async Task HandlePullRequests(Repo repository, AZRepo repo)
        {
            var pullRequests = await bitbucketClient.GetPullRequests(Project, repository.Slug);
            foreach (var pullRequest in pullRequests)
            {
                await aZDevopsClient.CreatePullRequest(TargetProjectSlug, repo.Id, FromPullRequest(pullRequest); ;


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

        private async Task CheckoutBranches(IEnumerable<Branch> branches)
        {
            foreach (var branch in branches.Where(b => b.Default || Branches.Contains(b.DisplayId)).OrderBy(b => b.Default ? 0 : 1))
            {
                logger.LogInformation($"\t-> Found branch {branch.DisplayId}");

                await GitAction($"checkout {branch.DisplayId}", tempDir);
            }
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

        private static AZPullRequest FromPullRequest(PR pullRequest)
            => new AZPullRequest
            {
                Title = pullRequest.Title,
                Description = $"{pullRequest.Author.User}: {pullRequest.Description}",
                SourceRefName = $"refs/heads/{pullRequest.FromRef.DisplayId}",
                TargetRefName = $"refs/heads/{pullRequest.ToRef.DisplayId}",
            };
    }
}
