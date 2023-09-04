using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitbucketMigrationTool.Commands
{
    [Command(Name = "largefile", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    internal class LargeFileCommand
    {

        [Option("-s|--size", CommandOptionType.SingleValue, Description = "size in B")]
        public long Size { get; set; }

        private async Task<int> OnExecute(CommandLineApplication app)
        {
            // Loop over all projects
            // Loop over all repositories
            // pull all branches
            // check for large files
            // report project, repository, branch, file, size
            return 0;
        }
    }
}
