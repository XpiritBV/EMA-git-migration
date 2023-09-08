namespace BitbucketMigrationTool.Models.AzureDevops.Repository
{
    internal class Repo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string RemoteUrl { get; set; }
    }
}
