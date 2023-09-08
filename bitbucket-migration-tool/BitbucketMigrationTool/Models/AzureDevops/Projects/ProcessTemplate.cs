namespace BitbucketMigrationTool.Models.AzureDevops
{
    public class ProcessTemplate
    {
        public const string SCRUM = "6b724908-ef14-45cf-84f8-768b5384da45";
        public const string CMMI = "27450541-8e31-4150-9947-dc59f998fc01";

        public string TemplateTypeId { get; set; } = SCRUM;
    }

}
