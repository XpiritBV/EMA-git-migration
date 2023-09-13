namespace BitbucketMigrationTool.Models.AzureDevops.Repository.Threads
{
    internal enum PullRequestThreadStatus
    {
        Active,
        Fixed,
        WontFix,
        Closed,
        ByDesign,
        Pending,
        Unknown
    }
}
