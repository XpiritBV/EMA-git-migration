using BitbucketMigrationTool.Models.Bitbucket.Repository;

namespace BitbucketMigrationTool.Models.Bitbucket.PullRequest
{
    public class PullRequestRef
    {
        public string Id { get; set; }
        public PullRequestRefType Type { get; set; }
        public Repo Repository { get; set; }
        public string DisplayId { get; set; }
        public string LatestCommit { get; set; }
    }
}
