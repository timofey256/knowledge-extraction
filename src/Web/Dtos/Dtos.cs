namespace KnowledgeExtractionTool.Controllers.DTOs;

public record class DirectedKnowledgeEdgeDto { 
    public string Node1 { get; init; }
    public int Node1Importance { get; init; }
    public string Node2 { get; init; }
    public int Node2Importance { get; init; }
    public string EdgeDescription { get; init; }

    private const int _minImportance = 0;
    private const int _maxImportance = 3;


    public DirectedKnowledgeEdgeDto(string node1, int node1Importance, string node2, int node2Importance, string desc) {
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

public record class KnowledgeGraphDto { 
    public string GraphId { get; init; }
    public DirectedKnowledgeEdgeDto[] Edges { get; init; }
    
    public KnowledgeGraphDto(string id, DirectedKnowledgeEdgeDto[] edges) {
        GraphId = id;
        Edges = edges;
    }
}