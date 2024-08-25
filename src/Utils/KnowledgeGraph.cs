namespace KnowledgeExtractionTool.Utils;

using KnowledgeExtractionTool.Core.Domain;

public static class GraphAlgorithms {
    public static double? CalculateNodeDistanceBFS(KnowledgeGraph graph,
                                                KnowledgeNode node1, 
                                                KnowledgeNode node2) {
        if (node1.Id == node2.Id)
            return 0;

        var queue = new Queue<(KnowledgeNode node, int distance)>();
        var visited = new HashSet<string>();

        queue.Enqueue((node1, 0));
        visited.Add(node1.Id);
        while (queue.Count > 0) {
            var (currentNode, currentDistance) = queue.Dequeue();
            var outcomingEdges = graph.Edges.FindAll(n => n.Node1Id == currentNode.Id);

            List<KnowledgeNode> neighbors = new();
            foreach (var edge in outcomingEdges) {
                neighbors.Add(graph.Nodes.Find(node => node.Id == edge.Node2Id));
            }

            foreach (var neighbor in neighbors) {
                var neighborId = neighbor.Id;
                if (neighborId == node2.Id)
                    return currentDistance + 1;

                if (!visited.Contains(neighborId)) {
                    queue.Enqueue((neighbor, currentDistance + 1));
                    visited.Add(neighborId);
                }
            }
        }

        return null;
    }

} 