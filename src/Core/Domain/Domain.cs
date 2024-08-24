using System.Collections.Specialized;
using KnowledgeExtractionTool.Core.Interfaces;
using MongoDB.Driver.Core.Events;

namespace KnowledgeExtractionTool.Core.Domain;

#region CLI-mutables

public class Prompts { 
    public string SystemPrompt { get; set; }

    public string ConstructFinalPrompt(string context) {
        return SystemPrompt + $"\n```\n{context}\n```\n";
    }
}

public class PromptsCollection { 
    public Prompts Default { get; set; }
}

#endregion

#region Graph-related stuff

public record class Cluster : IHasId {
    public required string Id { get; init; }
    public required string Name { get; init; }
}

public record class DirectedKnowledgeEdge : IHasId {
    public string Id { get; init; }
    public required string Node1Id { get; init; }
    public required string Node2Id { get; init; }
    public required string Label { get; init; }

    public DirectedKnowledgeEdge(string node1Id, string node2Id, string label) {
        Id = Guid.NewGuid().ToString();
        Node1Id = node1Id;
        Node2Id = node2Id;
        Label = label;
    }
}

public record class KnowledgeNode : IHasId {
    public string Id { get; init; }
    public string Label { get; set; }
    public List<string> NeighborsIds { get; set; }
    public int Importance { get; set; }

    private const int _minImportance = 0;
    private const int _maxImportance = 3;

    public KnowledgeNode(string label, int importance) {
        ValidateNodeImportance(importance);
        
        Id = Guid.NewGuid().ToString();
        Label = label;
        NeighborsIds = new List<string>();
        Importance = importance;
    }

    private void ValidateNodeImportance(int importance) {
        if (importance > _maxImportance || importance < _minImportance)
            throw new ArgumentException($"Node imporance is not in the valid range. Valid range : [{_minImportance}, {_maxImportance}] but actual value is {importance}");
    }
}

public record class KnowledgeGraph : IHasId { 
    public string Id { get; init; }
    public string OwnerId { get; set; }
    public List<KnowledgeNode> Nodes { get; set; }
    public List<DirectedKnowledgeEdge> Edges { get; set; }
    public DateTime CreatedAt { get; init; }
    public TimeSpan TimeTakenToCreate { get; init; } // Time taken to create the graph: get response from LLM and then construction.
    public List<Cluster> Clusters { get; set; }


    public KnowledgeGraph() {
        Id = Guid.NewGuid().ToString();
        OwnerId = "NotSpecified";
        Nodes = new List<KnowledgeNode>();
        Edges = new List<DirectedKnowledgeEdge>();
        CreatedAt = DateTime.UtcNow;
        Clusters = new List<Cluster>();
    }

    public override string? ToString()
    {
        // TODO: rewrite:
        return base.ToString();
    }
}

#endregion

/// <summary>
/// Type storing texts which users send us. We call them contexts.
/// </summary>
public record class Context : IHasId {
    public string Id { get; init; }
    public string Description { get; init; }
    public string FullText { get; init; }

    public Context(string context) {
        Id = Guid.NewGuid().ToString();
        FullText = context;
        Description = ""; // TODO: add summarization of the context in the future???
    }
}