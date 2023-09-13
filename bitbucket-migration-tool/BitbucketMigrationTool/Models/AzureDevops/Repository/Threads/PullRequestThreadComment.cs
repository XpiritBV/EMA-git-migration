namespace BitbucketMigrationTool.Models.AzureDevops.Repository.Threads
{
    internal class PullRequestThreadComment
    {
        public int? Id { get; set; }
        public int ParentCommentId { get; set; } = 0;
        public string Content { get; set; }
    }
}
