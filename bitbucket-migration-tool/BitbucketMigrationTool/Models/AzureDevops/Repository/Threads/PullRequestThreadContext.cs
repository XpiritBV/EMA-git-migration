namespace BitbucketMigrationTool.Models.AzureDevops.Repository.Threads
{
    internal class PullRequestThreadContext
    {
        public string FilePath { get; set; }

        public FilePosition RightFileEnd { get; set; }
        public FilePosition RightFileStart { get; set; }

        public static PullRequestThreadContext Create(string path, int line, int offset)
            => new PullRequestThreadContext
            {
                FilePath = path.StartsWith('/') ? path : $"/{path}",
                RightFileEnd = new FilePosition { Line = line, Offset = 1 + offset },
                RightFileStart = new FilePosition { Line = line, Offset = 1 }
            };
    }
}
