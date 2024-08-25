using KnowledgeExtractionTool.Core.Domain;

public class AppSettings {
    public required PromptsCollection PromptsCollection {get; set;}
    public required string Endpoint { get; set; }
    public required string ModelName { get; set; }
    public required string ApiKey { get; set; }
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
}

public class JwtOptions { 
    public required string SecretKey { get; set; }
    public required string Audience { get; set; }
    public required string Issuer { get; set; }
    public required double ExpiresInHours { get; set; }
    public required string CookiesKey { get; set; }
}