using KnowledgeExtractionTool.Core.Domain;

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

public struct EdgeDto {
    public string node_1 { get; init; }
    public int importance_1 { get; init; }
    public string node_2 { get; init; }
    public int importance_2 { get; init; }
    public string edge { get; init; }

    public EdgeDto(string _node1, string _node2, int _importance1, int _importance2, string label) {
        node_1 = _node1;
        node_2 = _node2;
        importance_1 = _importance1;
        importance_2 = _importance2;
        edge = label;
    }
}

public record class KnowledgeGraphDto { 
    public List<EdgeDto> edges = new(); 

    public KnowledgeGraphDto(KnowledgeGraph graph) {
        Build(graph);
    }

    private void Build(KnowledgeGraph graph) {
        foreach (var edge in graph.Edges) {
            var node1 = GetNodeById(edge.Node1Id, graph);
            var node2 = GetNodeById(edge.Node2Id, graph);

            edges.Add(new EdgeDto(node1.Label, node2.Label, node1.Importance, node2.Importance, edge.Label));
        }
    } 

    private KnowledgeNode? GetNodeById(string id, KnowledgeGraph graph) {
        return graph.Nodes.Find(node => node.Id == id);
    }

    public override string ToString() {
        string result = "";
        result += "[\n";
        for (int i=0; i < edges.Count; i++) {
            var edge = edges[i];
            result += "\t{\n";
            result += $"\t\t\"node_1\": \"{edge.node_1}\",\n";
            result += $"\t\t\"importance_1\": {edge.importance_1},\n";
            result += $"\t\t\"node_2\": \"{edge.node_2}\",\n";
            result += $"\t\t\"importance_2\": {edge.importance_2},\n";
            result += $"\t\t\"edge\": \"{edge.edge}\"\n";
            result += (i == edges.Count-1 ) ? "\t}\n" : "\t},\n";
        }
        result += "]\n";
        return result;
    }
}