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