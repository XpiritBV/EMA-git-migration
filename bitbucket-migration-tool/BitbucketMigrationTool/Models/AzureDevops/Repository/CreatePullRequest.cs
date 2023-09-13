namespace BitbucketMigrationTool.Models.AzureDevops.Repository
{
    internal class CreatePullRequest
    {
        public string SourceRefName { get; set; }
        public string TargetRefName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
