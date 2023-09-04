using BitbucketMigrationTool.Models.Bitbucket.Repository;

namespace BitbucketMigrationTool.Models.Bitbucket.PullRequest
{
    public class Ref
    {
        public string Id { get; set; }
        public RefType Type { get; set; }
        public Repo Repository { get; set; }
        public string DisplayId { get; set; }
        public string LatestCommit { get; set; }
    }
}
