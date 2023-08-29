using BitbucketMigrationTool.Models.Bitbucket.General;

namespace BitbucketMigrationTool.Models.Bitbucket.Repository
{
    public class Repo
    {
        public string Slug { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string HierarchyId { get; set; }
        public string ScmId { get; set; }
        public string State { get; set; }
        public string StatusMessage { get; set; }
        public bool Forkable { get; set; }
        public Project Project { get; set; }
        public bool Public { get; set; }
        public bool Archived { get; set; }
    }
}
