﻿using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitbucketMigrationTool.Commands.Test.Azdo
{
    [Command(Name = "project", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    internal class AzDoProjectCommand : CommandBase
    {

        public AzDoProjectCommand(ILogger<LargeFileCommand> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient, AZDevopsClient azdevopsClient) : base(logger, appSettingsOptions, bitbucketClient, azdevopsClient)
        {
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            var projectsAzdo = await aZDevopsClient.GetProjectsAsync();

            await Console.Out.WriteLineAsync($"Number of Azure DevOps Projects: - {projectsAzdo.Count()}");

            return 0;
        }
    }
}