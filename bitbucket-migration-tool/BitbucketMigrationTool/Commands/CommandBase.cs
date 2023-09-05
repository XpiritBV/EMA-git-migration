using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace BitbucketMigrationTool.Commands
{
    [Command(Name = "migrate", OptionsComparison = StringComparison.InvariantCultureIgnoreCase), VersionOptionFromMember("--version", MemberName = "GetVersion")]
    internal class CommandBase
    {
        internal readonly ILogger logger;
        internal readonly AppSettings appSettings;
        internal readonly BitbucketClient bitbucketClient;


        public CommandBase(ILogger logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient)
        {
            this.logger = logger;
            this.appSettings = appSettingsOptions.Value;
            this.bitbucketClient = bitbucketClient;
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
    }
}