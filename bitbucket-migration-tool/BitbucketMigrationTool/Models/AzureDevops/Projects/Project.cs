namespace BitbucketMigrationTool.Models.AzureDevops
{

    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public ProjectVisibility Visibility { get; set; }
    }

}
