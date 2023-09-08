namespace BitbucketMigrationTool.Models.AzureDevops
{
    public class CreateProjectRequest
    {
        public string Name { get; set; }
        public string Description { get; set; } = "";
        public ProjectVisibility Visibility { get; set; }
        public Capabilities Capabilities { get; set; } = new Capabilities();
    }
}
