using BitbucketMigrationTool.Models.Bitbucket.General;

namespace BitbucketMigrationTool.Models.Bitbucket.PullRequest
{
    public class Participant
    {
        public User User { get; set; }
        public ParticipantRole Role { get; set; }
        public ParticipantStatus Status { get; set; }
        public string LastReviewedCommit { get; set; }
        public bool Approved { get; set; }
    }
}
