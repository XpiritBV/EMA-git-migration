using BitbucketMigrationTool.Models.Markdown;

namespace BitbucketMigrationTool.Models.Bitbucket.General
{
    public class Project
    {
        public string Key { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Public { get; set; }
        public string Type { get; set; }
    }
}
