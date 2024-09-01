using KnowledgeExtractionTool.Core.Interfaces;

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

public class Cluster : IHasId {
    public string Id { get; init; }
    public List<KnowledgeNode> Nodes { get; set; }

    public Cluster(KnowledgeNode node) {
        Id = Guid.NewGuid().ToString();
        Nodes = new List<KnowledgeNode> { node };
    }

    public Cluster(List<KnowledgeNode> nodes) {
        Id = Guid.NewGuid().ToString();
        Nodes = nodes;
    }
}

public record class DirectedKnowledgeEdge : IHasId {
    public string Id { get; init; }
    public string Node1Id { get; }
    public string Node2Id { get; }
    public string? Label { get; }

    public DirectedKnowledgeEdge(string node1Id, string node2Id, string? label) {
        Id = Guid.NewGuid().ToString();
        Node1Id = node1Id;
        Node2Id = node2Id;
        Label = label;
    }
}

public record class KnowledgeNode : IHasId {
    public string Id { get; init; }
    public string Label { get; set; }
    public int Importance { get; set; }

    private const int _minImportance = 0;
    private const int _maxImportance = 3;

    public KnowledgeNode(string label, int importance) {
        ValidateNodeImportance(importance);
        
        Id = Guid.NewGuid().ToString();
        Label = label;
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
        OwnerId = string.Empty;
        Nodes = new List<KnowledgeNode>();
        Edges = new List<DirectedKnowledgeEdge>();
        CreatedAt = DateTime.UtcNow;
        Clusters = new List<Cluster>();
    }
    
    public void AddUniqueNode(KnowledgeNode node) {
        if (!Nodes.Exists(s => s.Label == node.Label))
            Nodes.Add(node);
    }

    public void AddUniqueEdge(KnowledgeNode node1, 
                              KnowledgeNode node2,
                              string? desc) {
        var sameLabelNode1 = Nodes.Find(n => n.Label == node1.Label);
        var sameLabelNode2 = Nodes.Find(n => n.Label == node2.Label);

        if (sameLabelNode1 is not null)
            node1 = sameLabelNode1;

        if (sameLabelNode2 is not null)
            node2 = sameLabelNode2;

        var edge = new DirectedKnowledgeEdge(node1.Id, node2.Id, desc);
        Edges.Add(edge);
    }

    private KnowledgeNode? GetNodeById(string id) {
        return Nodes.Find(node => node.Id == id);
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