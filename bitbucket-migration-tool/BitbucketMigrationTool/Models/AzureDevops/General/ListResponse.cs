namespace BitbucketMigrationTool.Models.AzureDevops.General
{
    internal class ListResponse<T>
    {
        public int Count { get; set; }
        public List<T> Value { get; set; } = new List<T>();
    }
}
