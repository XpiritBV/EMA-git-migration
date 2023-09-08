namespace BitbucketMigrationTool.Models.AzureDevops
{
    public class Capabilities
    {
        public VersionControl Versioncontrol { get; set; } = new VersionControl();
        public ProcessTemplate ProcessTemplate { get; set; } = new ProcessTemplate();
    }

}
