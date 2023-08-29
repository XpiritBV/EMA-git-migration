namespace BitbucketMigrationTool.Models.Bitbucket
{
    public class Branch
    {
        public bool Default { get; set; }
        public string DisplayId { get; set; }
        public string LatestCommit { get; set; }
        public string LatestChangeset { get; set; }
        public string Id { get; set; }
    }
}
