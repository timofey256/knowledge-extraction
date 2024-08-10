namespace KnowledgeExtractionTool.Core.Domain;

// CLI Mutable
public class Prompts { 
    public string SystemPrompt { get; set; }

    public string ConstructFinalPrompt(string context) {
        return SystemPrompt + $"\n```\n{context}\n```\n";
    }
}

// CLI Mutable
public class PromptsCollection { 
    public Prompts Default { get; set; }
}

public record class DirectedKnowledgeEdge { 
    public string Node1 { get; init; }
    public int Node1Importance { get; init; }
    public string Node2 { get; init; }
    public int Node2Importance { get; init; }
    public string EdgeDescription { get; init; }

    private const int _minImportance = 0;
    private const int _maxImportance = 3;


    public DirectedKnowledgeEdge(string node1, int node1Importance, string node2, int node2Importance, string desc) {
        ValidateNodeImportance(node1Importance);
        ValidateNodeImportance(node2Importance);

        Node1 = node1;
        Node2 = node2;
        Node1Importance = node1Importance;
        Node2Importance = node2Importance;
        EdgeDescription = desc;
    }

    private void ValidateNodeImportance(int importance) {
        if (importance > _maxImportance || importance < _minImportance)
            throw new ArgumentException($"Node imporance is not in the valid range. Valid range : [{_minImportance}, {_maxImportance}] but actual value is {importance}");
    }

}

public record class KnowledgeGraph { 
    public DirectedKnowledgeEdge[] Edges { get; init; }
    
    public KnowledgeGraph(DirectedKnowledgeEdge[] edges) {
        Edges = edges;
    }

    public override string ToString()
    {
        // TODO: rewrite:
        return base.ToString();
    }
}

public class User
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Salt { get; set; }
}