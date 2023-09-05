using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Models.Bitbucket.General;
using BitbucketMigrationTool.Models.Bitbucket.Repository;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BitbucketMigrationTool.Commands
{
    [Command(Name = "largefile", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    internal class LargeFileCommand : CommandBase
    {

        public LargeFileCommand(ILogger<LargeFileCommand> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient) : base(logger, appSettingsOptions, bitbucketClient)
        {
        }

        [Option("-s|--size", CommandOptionType.SingleValue, Description = "size in B")]
        public long Size { get; set; } = 0;

        private async Task<int> OnExecute(CommandLineApplication app)
        {
            logger.LogInformation("Start");

            File.Delete("largefile.csv");

            // Loop over all projects
            var projects = await bitbucketClient.GetProjectsAsync();

            foreach (var project in projects)
            {
                logger.LogInformation($"Project: {project.Key}");
                var repositories = await bitbucketClient.GetRepositoriesAsync(project.Key);
                // Loop over all repositories
                foreach (var repository in repositories)
                {
                    logger.LogInformation($"Repository: {repository.Slug}");
                    //try
                    //{
                    //    Directory.Delete("tempdir", true);
                    //}
                    //catch (Exception)
                    //{
                    //}

                    await GitAction($"clone {repository.Links.Clone.First(x => x.Name == "http").Href} tempdir");

                    var branches = await bitbucketClient.GetBranchesAsync(project.Key, repository.Slug);
                    foreach (var branch in branches)
                    {
                        logger.LogInformation($"Branch: {branch.DisplayId}");

                        // pull all branches
                        await GitAction($"checkout {branch.DisplayId}", "tempdir");

                        // check for large files
                        // report project, repository, branch, file, size
                        await ListFiles("tempdir", project.Key, repository.Slug, branch.DisplayId, Size);

                    }
                    SetAttributesNormal("tempdir");
                    Directory.Delete("tempdir", true);

                }
            }
            logger.LogInformation("End");
            return 0;
        }


        static void SetAttributesNormal(string directoryPath)
        {
            foreach (var subDir in Directory.GetDirectories(directoryPath))
                SetAttributesNormal(subDir);
            foreach (var file in Directory.GetFiles(directoryPath).Select(s => new FileInfo(s)))
            {
                file.Attributes = FileAttributes.Normal;
            }
        }



        static Task ListFiles(string directoryPath, string project, string repository, string branch, long size)
        {
            // List the files in the current directory
            string[] files = Directory.GetFiles(directoryPath);

            var lf = files.Select(s => new FileInfo(s)).Where(w => w.Length > size).Select(fileInfo => $"{project};{repository};{branch};{fileInfo};{fileInfo.Name};{fileInfo.Length}");

            File.AppendAllLines("largefile.csv", lf);

            // Recursively list files in subdirectories
            string[] subDirectories = Directory.GetDirectories(directoryPath);
            foreach (string subDirectory in subDirectories)
            {
                ListFiles(subDirectory, project, repository, branch, size);
            }

            return Task.CompletedTask;
        }
    }
}
