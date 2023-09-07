﻿using BitbucketMigrationTool.Models;
using BitbucketMigrationTool.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitbucketMigrationTool.Commands
{
    [Command(Name = "bitbucketmigration", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [Subcommand(typeof(LargeFileCommand), typeof(MigrateCommand))]
    internal class DummyCommand : CommandBase
    {
        public DummyCommand(ILogger<CommandBase> logger, IOptions<AppSettings> appSettingsOptions, BitbucketClient bitbucketClient) : base(logger, appSettingsOptions, bitbucketClient)
        {
        }
    }
}