using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitbucketMigrationTool.Commands.Test.Bitbucket
{
    [Command(Name = "permissions", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    internal class BitbucketPermissionsCommand : CommandBase
    {
        public BitbucketPermissionsCommand(ILogger<LargeFileCommand> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient, AZDevopsClient azdevopsClient) : base(logger, appSettingsOptions, bitbucketClient, azdevopsClient)
        {
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            await Console.Out.WriteLineAsync($"Get projects");
            var projectsBB = await bitbucketClient.GetProjectsAsync();

            const string projectFilenName = "projects.csv";
            const string repoFilenName = "repos.csv";
            const string branchFilenName = "branchs.csv";
            const string projectUserPermissionFilenName = "projectUserPermission.csv";
            const string projectGroupPermissionFilenName = "projectGroupPermission.csv";
            const string repoUserPermissionFilenName = "repoUserPermission.csv";
            const string repoGroupPermissionFilenName = "repoGroupPermission.csv";

            await Console.Out.WriteLineAsync($"Delete output files");
            File.Delete(projectFilenName);
            File.Delete(repoFilenName);
            File.Delete(branchFilenName);
            File.Delete(projectUserPermissionFilenName);
            File.Delete(projectGroupPermissionFilenName);
            File.Delete(repoUserPermissionFilenName);
            File.Delete(repoGroupPermissionFilenName);


            await File.AppendAllLinesAsync(projectFilenName, projectsBB.Select(s => $"{s.Key}"));


            foreach (var project in projectsBB)
            {
                await Console.Out.WriteLineAsync($"Get Project User Permissions");
                var projectUserPermissions = await bitbucketClient.GetProjectPermissionsAsync(project.Key);
                await File.AppendAllLinesAsync(projectUserPermissionFilenName, projectUserPermissions.Where(w => w.User != null).Select(s => $"{project.Key};{s.User.DisplayName};{s.User.Name};{s.User.EmailAddress};{s.User.Active};{s.Permission}"));

                await Console.Out.WriteLineAsync($"Get Project Group Permissions");
                var projectGroupPermissions = await bitbucketClient.GetProjectPermissionsAsync(project.Key, "groups");
                await File.AppendAllLinesAsync(projectGroupPermissionFilenName, projectGroupPermissions.Where(w => w.User != null).Select(s => $"{project.Key};{s.User.DisplayName};{s.User.Name};{s.User.EmailAddress};{s.User.Active};{s.Permission}"));

                var repos = await bitbucketClient.GetRepositoriesAsync(project.Key);
                await File.AppendAllLinesAsync(repoFilenName, repos.Select(s => $"{project.Key};{s.Slug}"));

                foreach (var repo in repos)
                {
                    await Console.Out.WriteLineAsync($"Get Repos User Permissions");
                    var repoUserPermissions = await bitbucketClient.GetRepoPermissionsAsync(project.Key, repo.Slug);
                    await File.AppendAllLinesAsync(repoUserPermissionFilenName, repoUserPermissions.Where(w => w.User != null).Select(s => $"{project.Key};{repo.Slug};{s.User.DisplayName};{s.User.Name};{s.User.EmailAddress};{s.User.Active};{s.Permission}"));

                    await Console.Out.WriteLineAsync($"Get Repos Group Permissions");
                    var repoGroupPermissions = await bitbucketClient.GetRepoPermissionsAsync(project.Key, repo.Slug, "groups");
                    await File.AppendAllLinesAsync(repoGroupPermissionFilenName, repoGroupPermissions.Where(w => w.User != null).Select(s => $"{project.Key};{repo.Slug};{s.User.DisplayName};{s.User.Name};{s.User.EmailAddress};{s.User.Active};{s.Permission}"));

                    await Console.Out.WriteLineAsync($"Get Branches");
                    var branches = await bitbucketClient.GetBranchesAsync(project.Key, repo.Slug);
                    await File.AppendAllLinesAsync(branchFilenName, branches.Select(s => $"{project.Key};{repo.Slug};{s.DisplayId}"));
                }
            }
            return 0;
        }
    }
}
