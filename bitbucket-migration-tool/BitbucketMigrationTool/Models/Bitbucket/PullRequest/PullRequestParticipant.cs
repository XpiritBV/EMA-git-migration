using BitbucketMigrationTool.Models.Bitbucket.General;

namespace BitbucketMigrationTool.Models.Bitbucket.PullRequest
{
    public class PullRequestParticipant
    {
        public User User { get; set; }
        public PullRequestParticipantRole Role { get; set; }
        public PullRequestParticipantStatus Status { get; set; }
        public string LastReviewedCommit { get; set; }
        public bool Approved { get; set; }
    }
}
