namespace KnowledgeExtractionTool.Core.Domain;

public class Prompts { 
    public string SystemPrompt { get; set; }

    public string ConstructFinalPrompt(string context) {
        return SystemPrompt + $"\n```\n{context}\n```\n";
    }
}

public class PromptsCollection { 
    public Prompts Default { get; set; }
}

class Graph { 

}

public class User
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Salt { get; set; }
}