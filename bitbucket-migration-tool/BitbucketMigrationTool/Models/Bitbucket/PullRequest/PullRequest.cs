namespace BitbucketMigrationTool.Models.Bitbucket.PullRequest
{
    public class PullRequest
    {
        public int Id { get; set; }
        public bool Locker { get; set; }
        public int Version { get; set; }
        public PullRequestState State { get; set; }
        public bool Open { get; set; }
        public ulong UpdatedDate { get; set; }
        public ulong CreatedDate { get; set; }
        public PullRequestRef ToRef { get; set; }
        public PullRequestRef FromRef { get; set; }
        public string Title { get; set; }
        public bool Closed { get; set; }
        public string Description { get; set; }
        public ulong? ClosedDate { get; set; }
        public List<PullRequestParticipant> Participants { get; set; }
        public List<PullRequestParticipant> Reviewers { get; set; }
    }
}

