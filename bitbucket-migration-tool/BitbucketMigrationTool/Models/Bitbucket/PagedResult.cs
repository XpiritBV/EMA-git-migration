namespace BitbucketMigrationTool.Models.Bitbucket
{
    internal class PagedResult<T>
    {
        public int Size { get; set; }
        public int Limit { get; set; }
        public bool IsLastPage { get; set; }
        public int Start { get; set; }
        public int? NextPageStart { get; set; }
        public IEnumerable<T> Values { get; set; }
    }
}
