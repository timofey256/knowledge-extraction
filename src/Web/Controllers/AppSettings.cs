using KnowledgeExtractionTool.Core.Domain;

public class AppSettings {
    public required PromptsCollection PromptsCollection {get; set;}
    public required string ModelName { get; set; }
    public required string ApiKey { get; set; }
}
