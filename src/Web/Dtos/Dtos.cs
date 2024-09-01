using KnowledgeExtractionTool.Core.Domain;

namespace KnowledgeExtractionTool.Controllers.DTOs;

public struct EdgeDto {
    // For design decision see comment for KnowledgeGraphDto.ToString()
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

    /// <summary>
    /// Converts KnowledgeGraphDto to a list of edges in the following format:
    /// { 
    ///     "node_1" : first node's label
    ///     "importance_1" : first node's importance
    ///     "node_2" : second node's label
    ///     "importance_2" : second node's importance
    ///     "edge" : edge's label
    /// }
    /// </summary>
    public override string ToString() {
        // NOTE: This implementation and format is indeed strange.
        // The reason for that is am not a very experienced frontend developer,
        // so the only way i got d3.js graph going is with this format. 
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