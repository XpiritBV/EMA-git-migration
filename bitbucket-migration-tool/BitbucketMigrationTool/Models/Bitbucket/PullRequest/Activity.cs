using BitbucketMigrationTool.Models.Bitbucket.General;

namespace BitbucketMigrationTool.Models.Bitbucket.PullRequest
{
    public class Activity
    {
        public int Id { get; set; }
        public long CreatedDate { get; set; }
        public User User { get; set; }
        public ActivityActionType Action { get; set; }
        public string CommentAction { get; set; }
        public Comment Comment { get; set; }
    }
}

