namespace BitbucketMigrationTool.Models.AzureDevops.Repository.Threads
{
    internal class CreatePullRequestThreadRequest
    {
        public IEnumerable<PullRequestThreadComment> Comments { get; set; }

        public PullRequestThreadStatus Status { get; set; } = PullRequestThreadStatus.Unknown;

        public PullRequestThreadContext? ThreadContext { get; set; }
    }
}
