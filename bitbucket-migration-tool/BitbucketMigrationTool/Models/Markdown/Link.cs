namespace BitbucketMigrationTool.Models.Markdown
{
    internal readonly record struct Link(string Text, string Target)
    {
        override public string ToString()
        {
            return $"[{Text}]({Target})";
        }

        public bool IsAttachment => Target.StartsWith("attachment:");

        public Link WithNewTarget(string newTarget)
        {
            return new Link(Text, newTarget);
        }
    }
}
