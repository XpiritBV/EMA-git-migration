using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.IO;

namespace BitbucketMigrationTool.Commands
{
    [VersionOptionFromMember("--version|-v", MemberName = "GetVersion")]
    [HelpOption("--help|-h")]
    internal abstract class CommandBase
    {
        internal readonly ILogger logger;
        internal readonly AppSettings appSettings;
        internal readonly BitbucketClient bitbucketClient;
        internal readonly AZDevopsClient aZDevopsClient;


        public CommandBase(ILogger logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient, AZDevopsClient aZDevopsClient)
        {
            this.logger = logger;
            this.appSettings = appSettingsOptions.Value;
            this.bitbucketClient = bitbucketClient;
            this.aZDevopsClient = aZDevopsClient;
        }

        protected virtual Task<int> OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return Task.FromResult(0);
        }

        internal Task GitAction(string args, string workingdir = "")
        {
            var cloneProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (!string.IsNullOrWhiteSpace(workingdir))
            {
                cloneProcess.StartInfo.WorkingDirectory = workingdir;
            }

            cloneProcess.OutputDataReceived += (sender, args) => logger.LogInformation(args.Data);
            cloneProcess.ErrorDataReceived += (sender, args) => logger.LogError(args.Data);

            cloneProcess.Start();
            cloneProcess.BeginOutputReadLine();
            cloneProcess.BeginErrorReadLine();
            return cloneProcess.WaitForExitAsync();
        }

        internal string GetVersion() => appSettings.AppVersion;

        protected static Task DeleteFolder(string directoryPath)
        {
            var directory = new DirectoryInfo(directoryPath);
            return directory.Exists ? DeleteFolder(directory) : Task.CompletedTask;
        }

        protected static Task DeleteFolder(DirectoryInfo directory)
        {
            foreach (var subDir in directory.GetDirectories())
            {
                DeleteFolder(subDir);
            }
            
            foreach (var file in directory.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
            }
            
            directory.Delete(true);
            
            return Task.CompletedTask;
        }
    }
}