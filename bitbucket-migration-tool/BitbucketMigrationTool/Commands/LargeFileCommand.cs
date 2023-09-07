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

        [Option("-s|--size", CommandOptionType.SingleValue, Description = "Size in B")]
        public long Size { get; set; } = 0;

        [Option("-sp|--skip-projects", CommandOptionType.SingleValue, Description = "Projects To Skip")]
        public string SkipProjects { get; set; } = string.Empty;

        [Option("-sr|--skip-repositories", CommandOptionType.SingleValue, Description = "Repositories To Skip")]
        public string SkipRepositories { get; set; } = string.Empty;

        [Option("-c|--continue", CommandOptionType.SingleValue, Description = "Continue after interuption")]
        public bool Continue { get; set; } = true;


        private async Task<int> OnExecute(CommandLineApplication app)
        {
            logger.LogInformation("Start");

            const string largeFileName = "largefile.csv";
            const string reportName = "report.csv";
            const string workingFolder = "tempdir";


            var projects2beskipped = SkipProjects.Split(",").ToList();
            var repos2beskipped = SkipRepositories.Split(",").ToList();

            if (!Continue)
            {
                File.Delete(largeFileName);
                File.Delete(reportName);
            }
            else
            {
                projects2beskipped = projects2beskipped.Concat(await GetDistinctColumnValuesAsync(reportName, 0)).ToList();
                repos2beskipped = repos2beskipped.Concat(await GetDistinctColumnValuesAsync(reportName, 1)).ToList();

                var lastProject = await GetLastNonEmptyColumnValueAsync(reportName, 0);
                var lastRepo = await GetLastNonEmptyColumnValueAsync(reportName, 1);

                projects2beskipped.Remove(lastProject);
                repos2beskipped.Remove(lastRepo);
            }


            // Loop over all projects
            var projects = await bitbucketClient.GetProjectsAsync();

            foreach (var project in projects.Where(w => !projects2beskipped.Contains(w.Key)))
            {
                logger.LogInformation($"Project: {project.Key}");
                var repositories = await bitbucketClient.GetRepositoriesAsync(project.Key);
                // Loop over all repositories
                foreach (var repository in repositories.Where(w => !repos2beskipped.Contains(w.Slug)))
                {
                    logger.LogInformation($"Repository: {repository.Slug}");

                    await GitAction($"clone {repository.Links.Clone.First(x => x.Name == "http").Href} {workingFolder}");

                    var branches = await bitbucketClient.GetBranchesAsync(project.Key, repository.Slug);
                    foreach (var branch in branches)
                    {
                        logger.LogInformation($"Branch: {branch.DisplayId}");

                        // pull all branches
                        await GitAction($"checkout {branch.DisplayId}", workingFolder);

                        // check for large files
                        // report project, repository, branch, file, size
                        await ListFiles(workingFolder, project.Key, repository.Slug, branch.DisplayId, Size, largeFileName);

                        // report project, repository, branch
                        using var report = File.AppendText(reportName);
                        await report.WriteLineAsync($"{project.Key};{repository.Slug};{branch.DisplayId}");

                    }
                    await DeleteFolder(workingFolder);
                }
            }
            logger.LogInformation("End");
            return 0;
        }

        static async Task<string> GetLastNonEmptyColumnValueAsync(string filePath, int columnIndex, char separator = ';')
        {
            string lastNonEmptyValue = null;

            using (StreamReader reader = new StreamReader(filePath))
            {
                string prevLine = null;
                string line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        prevLine = line;
                    }
                }

                if (prevLine != null)
                {
                    string[] columns = prevLine.Split(separator);

                    if (columns.Length > columnIndex)
                    {
                        lastNonEmptyValue = columns[columnIndex];
                    }
                }
            }

            return lastNonEmptyValue;
        }

        static async Task<HashSet<string>> GetDistinctColumnValuesAsync(string filePath, int columnIndex, char separator = ';')
        {
            // Create a HashSet to store distinct values.
            HashSet<string> distinctValues = new HashSet<string>();

            // Open the CSV file for reading.
            using (StreamReader reader = new StreamReader(filePath))
            {
                // Read the header line if it exists (optional).
                string headerLine = await reader.ReadLineAsync();

                // Read each line from the CSV file.
                while (!reader.EndOfStream)
                {
                    string line = await reader.ReadLineAsync();

                    // Split the line into columns.
                    string[] columns = line.Split(separator);

                    // Ensure the line has enough columns.
                    if (columns.Length > columnIndex)
                    {
                        // Add the value to the HashSet (automatically removes duplicates).
                        distinctValues.Add(columns[columnIndex]);
                    }
                }
            }

            return distinctValues;
        }


        static Task DeleteFolder(string directoryPath)
        {
            foreach (var subDir in Directory.GetDirectories(directoryPath))
                DeleteFolder(subDir);
            foreach (var file in Directory.GetFiles(directoryPath).Select(s => new FileInfo(s)))
            {
                file.Attributes = FileAttributes.Normal;
            }
            Directory.Delete(directoryPath, true);
            return Task.CompletedTask;
        }



        static Task ListFiles(string directoryPath, string project, string repository, string branch, long size, string fileName)
        {
            // List the files in the current directory
            string[] files = Directory.GetFiles(directoryPath);

            var lf = files.Where(w => !w.Contains(".git")).Select(s => new FileInfo(s)).Where(w => w.Length > size).Select(fileInfo => $"{project};{repository};{branch};{fileInfo};{fileInfo.Length}");
            File.AppendAllLines(fileName, lf);

            // Recursively list files in subdirectories
            string[] subDirectories = Directory.GetDirectories(directoryPath);
            foreach (string subDirectory in subDirectories)
            {
                ListFiles(subDirectory, project, repository, branch, size, fileName);
            }

            return Task.CompletedTask;
        }
    }
}
