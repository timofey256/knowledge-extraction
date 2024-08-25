using KnowledgeExtractionTool.Core.Domain;

namespace KnowledgeExtractionTool.Utils;

public static class GraphAlgorithms {
    public static double CalculateNodeDistanceBFS(KnowledgeGraph graph,
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

            foreach (var neighborId in currentNode.NeighborsIds) {
                if (neighborId == node2.Id)
                    return currentDistance + 1;

                if (!visited.Contains(neighborId)) {
                    var neighborNode = graph.Nodes.First(node => node.Id == neighborId);
                    queue.Enqueue((neighborNode, currentDistance + 1));
                    visited.Add(neighborId);
                }
            }
        }

        return double.MaxValue;
    }

} 