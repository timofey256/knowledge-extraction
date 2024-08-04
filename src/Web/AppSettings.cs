using KnowledgeExtractionTool.Core.Domain;

public class AppSettings {
    public required PromptsCollection PromptsCollection {get; set;}
    public required string ModelName { get; set; }
    public required string ApiKey { get; set; }
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
}
