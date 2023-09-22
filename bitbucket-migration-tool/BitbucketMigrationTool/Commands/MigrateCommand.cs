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
using AZPullRequest = BitbucketMigrationTool.Models.AzureDevops.Repository.CreatePullRequest;
using BitbucketMigrationTool.Models.AzureDevops.Repository.Threads;
using BitbucketMigrationTool.Models.Bitbucket.General;

namespace BitbucketMigrationTool.Commands
{
    [Command(Name = "migrate", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    internal class MigrateCommand : CommandBase
    {
        private const string tempDir = "tempdir";

        [Argument(0, nameof(Project), Description = "* Project key")]
        public string Project { get; set; }

        [Argument(1, nameof(Repository), Description = "* Repository slug")]
        public string Repository { get; set; }

        [Option("-b|--branch", CommandOptionType.MultipleValue, Description = "[Aditional branches to migrate]")]
        public string[] Branches { get; set; } = Array.Empty<string>();

        [Option("-tp|--target-prefix", CommandOptionType.SingleOrNoValue, Description = "[Prefix for target project]")]
        public (bool HasValue, string Value) TargetPrefix { get; set; }

        [Option("-t|--target", CommandOptionType.SingleOrNoValue, Description = "[Target project slug]")]
        public (bool HasValue, string Value) TargetProject { get; set; }

        [Option("-tr|--target-repository", CommandOptionType.SingleOrNoValue, Description = "[Target repository slug]")]
        public (bool HasValue, string Value) TargetRepository { get; set; }

        [Option("-s|--skip-if-target-exists", CommandOptionType.NoValue, Description = "[Skip if target repository exists]")]
        public bool SkipIfTargetExists { get; set; } = false;

        [Option("-pr|--pull-requests", CommandOptionType.SingleValue, Description = "[Migrate pull requests]")]
        public bool DoPullRequests { get; set; } = true;

        [Option("-pc|--pull-request-comments", CommandOptionType.SingleValue, Description = "[Migrate pull request comments]")]
        public bool DoPullRequestComments { get; set; } = true;

        private string TargetProjectSlug => $"{(TargetPrefix.HasValue ? $"{TargetPrefix.Value}-" : string.Empty)}{(TargetProject.HasValue ? TargetProject.Value : Project)}";

        private string TargetRepositorySlug => TargetRepository.HasValue ? TargetRepository.Value : Repository;

        public MigrateCommand(ILogger<MigrateCommand> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient, AZDevopsClient aZDevopsClient)
            : base(logger, appSettingsOptions, bitbucketClient, aZDevopsClient)
        {
        }

        // TODO: check if rate limit is reached
        // TODO: precreate projects
        // TODO: Check rewrite of tags

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            string permissionsProjectFilenName = $"permissions-project-{Project}-{Repository}.csv";
            string permissionsRepoFilenName = $"permissions-repo-{Project}-{Repository}.csv";

            await Console.Out.WriteLineAsync($"Delete output files");
            await DeleteFolder(tempDir);
            File.Delete(permissionsProjectFilenName);
            File.Delete(permissionsRepoFilenName);

            var repository = await bitbucketClient.GetRepositoryAsync(Project, Repository);
            if (repository == null)
            {
                logger.LogError($"Repository {Repository} not found");
                return 1;
            }
            var branches = (await bitbucketClient.GetBranchesAsync(Project, repository.Slug))
                .Where(b => b.Default || Branches.Contains(b.DisplayId) || b.DisplayId.Equals("main", StringComparison.InvariantCultureIgnoreCase) || b.DisplayId.Equals("master", StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(b => b.Default ? 0 : 1)
                .ToList();
            await EnsureAZDevopsProjectisCreated();


            try
            {
                var userPermissions = await bitbucketClient.GetRepoPermissionsAsync(Project, repository.Slug);
                var projectPermissions = await bitbucketClient.GetProjectPermissionsAsync(Project);

                await File.AppendAllLinesAsync(permissionsProjectFilenName, projectPermissions.Select(s => $"{s.User}: {s.Permission}"));
                await File.AppendAllLinesAsync(permissionsRepoFilenName, userPermissions.Select(s => $"{s.User}: {s.Permission}"));


                logger.LogInformation($"-- PERMISSIONS Project {Project} --");

                foreach (var projectPermission in projectPermissions)
                {
                    logger.LogInformation($"{projectPermission.User}: {projectPermission.Permission}");

                    if (!TargetPrefix.HasValue)
                        await bitbucketClient.SetProjectPermission(Project, projectPermission.User.Name);
                }

                logger.LogInformation($"-- PERMISSIONS Repos {Repository} --");

                foreach (var userPermission in userPermissions)
                {
                    logger.LogInformation($"{userPermission.User}: {userPermission.Permission}");

                    if (!TargetPrefix.HasValue)
                        await bitbucketClient.SetRepositoryPermission(Project, repository.Slug, userPermission.User.Name);
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "could not read permissions");
            }
            
            var targetRepository = await aZDevopsClient.GetRepositoryAsync(TargetProjectSlug, TargetRepositorySlug);
            var repositoryExists = targetRepository != null;

            if ((!SkipIfTargetExists && repositoryExists) || !repositoryExists)
            {
                if (!repositoryExists)
                {
                    targetRepository = await aZDevopsClient.CreateRepositoryAsync(TargetProjectSlug, TargetRepositorySlug);
                }

                await CloneRepo(repository);
                await CheckoutBranches(branches);
                await SwitchGitRemote(targetRepository);
                await FixDefaultBranch(branches, targetRepository);
                await DeleteFolder(tempDir);
            }
            if (DoPullRequests)
            {
                await HandlePullRequests(repository, targetRepository, branches);
            }

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
                if (result == null)
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
            await GitAction("push origin --all --force", tempDir);
            await GitAction("push --tags", tempDir);
        }

        private async Task HandlePullRequests(Repo repository, AZRepo repo, IEnumerable<Branch> branches)
        {
            var pullRequests = await bitbucketClient.GetPullRequests(Project, repository.Slug);
            foreach (var pullRequest in pullRequests.Where(p => branches.Any(b => b.DisplayId == p.FromRef.DisplayId) && branches.Any(b => b.DisplayId == p.ToRef.DisplayId)))
            {
                var response = await aZDevopsClient.CreatePullRequest(TargetProjectSlug, repo.Id, FromPullRequest(pullRequest));

                var activities = await bitbucketClient.GetPullRequestActivities(Project, repository.Slug, pullRequest.Id);
                foreach (var activity in activities.Where(x => x.Action == ActivityActionType.COMMENTED).OrderBy(x => x.CreatedDate))
                {

                    var thread = await aZDevopsClient.CreatePullRequestThread(TargetProjectSlug, repo.Id, response.PullRequestId, FromActivity(activity));
                    await ReplaceAttachments(repo.Id, response.PullRequestId, thread.Id, activity.Comment);

                    if (DoPullRequestComments)
                    {
                        foreach (var comment in activity.Comment.Comments)
                        {
                            await ReplaceAttachments(repo.Id, response.PullRequestId, thread.Id, comment);
                            await aZDevopsClient.CreatePullRequestThreadComment(TargetProjectSlug, repo.Id, response.PullRequestId, thread.Id, new PullRequestThreadComment
                            {
                                ParentCommentId = 1,
                                Content = $"{comment.Author}: {comment.Text}"
                            });
                        }
                    }

                }
            }
        }

        private async Task ReplaceAttachments(Guid repoId, int prId, int threadId, Comment comment)
        {
            var links = ScanForLinks(comment.Text);
            foreach (var link in links)
            {
                if (link.IsAttachment)
                {
                    var attachmentId = int.Parse(link.Target.Split('/').Last());
                    var attachment = await bitbucketClient.GetAttachment(Project, Repository, attachmentId);
                    if (attachment != null)
                    {
                        var azAttachment = await aZDevopsClient.UploadAttachment(TargetProjectSlug, repoId, prId, threadId, link.Text.Replace(' ', '_'), attachment);
                        comment.Text = comment.Text.Replace(link.ToString(), link.WithNewTarget(azAttachment.Url).ToString());
                    }
                }
            }
        }

        private async Task CheckoutBranches(IEnumerable<Branch> branches)
        {
            foreach (var branch in branches)
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

        private static CreatePullRequestThreadRequest FromActivity(Activity activity)
        {
            return new CreatePullRequestThreadRequest
            {
                Comments = new[]
                {
                    new PullRequestThreadComment
                    {
                        Content = $"{activity.Comment.Author}:{activity.Comment.Text}"
                    }
                },
                Status = activity.Comment.ThreadResolved ? PullRequestThreadStatus.Fixed : PullRequestThreadStatus.Active,
                ThreadContext = activity.CommentAnchor != null ? PullRequestThreadContext.Create(activity.CommentAnchor.Path, activity.CommentAnchor.Line, 1) : null,
            };
        }
    }
}
