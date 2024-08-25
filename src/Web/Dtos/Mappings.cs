namespace KnowledgeExtractionTool.Controllers.DTOs.Mappings;

using KnowledgeExtractionTool.Core.Domain;

public static class Mappings {
    public static KnowledgeGraphDto toKnowledgeGraphDto(KnowledgeGraph graph) {
        List<DirectedKnowledgeEdgeDto> edges = new();

        foreach (var edge in graph.Edges) {
            var node1 = graph.Nodes.Find((node) => node.Id == edge.Node1Id);
            var node2 = graph.Nodes.Find((node) => node.Id == edge.Node2Id);

            edges.Add(new DirectedKnowledgeEdgeDto(node1.Label, node1.Importance, node2.Label, node2.Importance, edge.Label));
        }
        
        return new KnowledgeGraphDto(graph.Id, edges.ToArray());
    }
}