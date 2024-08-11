using KnowledgeExtractionTool.Core.Interfaces;

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

// ======

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

public record class KnowledgeGraph : IHasId { 
    public string Id { get; init; }
    public DirectedKnowledgeEdge[] Edges { get; init; }
    public DateTime CreatedAtUTC { get; init; }
    
    public KnowledgeGraph(DirectedKnowledgeEdge[] edges, DateTime createdAt) {
        Id = Guid.NewGuid().ToString();
        Edges = edges;
        CreatedAtUTC = createdAt;
    }

    public override string ToString()
    {
        // TODO: rewrite:
        return base.ToString();
    }
}

/// <summary>
/// Type storing texts which users send us. We call them contexts
/// </summary>
public record class Context : IHasId {
    public string Id { get; init; }
    public string Description { get; init; }
    public string FullText { get; init; }

    public Context(string context) {
        Id = Guid.NewGuid().ToString();
        FullText = context;
        Description = ""; // Maybe add summarization of the context in the future???
    }
}