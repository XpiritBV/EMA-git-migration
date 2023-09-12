using BitbucketMigrationTool.Models.Bitbucket.General;
using System.Xml.Linq;

namespace BitbucketMigrationTool.Models.Bitbucket.PullRequest
{
    public class PR
    {
        public int Id { get; set; }
        public bool Locker { get; set; }
        public int Version { get; set; }
        public State State { get; set; }
        public bool Open { get; set; }
        public ulong UpdatedDate { get; set; }
        public ulong CreatedDate { get; set; }
        public Ref ToRef { get; set; }
        public Ref FromRef { get; set; }
        public string Title { get; set; }
        public bool Closed { get; set; }
        public string Description { get; set; }
        public ulong? ClosedDate { get; set; }
        public List<Participant> Participants { get; set; }
        public List<Participant> Reviewers { get; set; }
        public Participant Author { get; set; }
    }
}

