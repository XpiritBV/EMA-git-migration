using BitbucketMigrationTool.Models.Bitbucket.General;

namespace BitbucketMigrationTool.Models.Bitbucket.PullRequest
{
    public class Comment
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public string Text { get; set; }
        public User Author { get; set; }
        public long CreatedDate { get; set; }
        public long UpdatedDate { get; set; }
        public List<Comment> Comments { get; set; }
        public bool ThreadResolved { get; set; }
        public string Severity { get; set; }
    }
}

