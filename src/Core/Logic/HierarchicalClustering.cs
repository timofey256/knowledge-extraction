namespace KnowledgeExtractionTool.Core.Logic;

using KnowledgeExtractionTool.Core.Domain;

/*
 * The HierarchicalClustering class implements a basic hierarchical clustering algorithm
 * for organizing nodes within a KnowledgeGraph into clusters. This approach is bottom-up,
 * starting with each node as its own cluster and iteratively merging the closest clusters
 * until a specified number of clusters is reached.
 * 
 * The current implementation uses a simple distance metric between nodes, which assumes
 * directly connected nodes have a distance of 1 and unconnected nodes have a distance of 2.
 * This can be improved by considering the actual shortest path between nodes.
 */
public class HierarchicalClustering {
    private KnowledgeGraph _graph;
    private List<Cluster> _clusters;

    private const int _defaultNumOfNodesPerCluster = 7; // emperical value. just a guess from the top of my head.

    public HierarchicalClustering(KnowledgeGraph graph) {
        _graph = graph;
        _clusters = graph.Nodes.Select(node => new Cluster(node)).ToList();
    }

    public List<Cluster> Cluster() {
        int numOfClusters = _graph.Nodes.Count / _defaultNumOfNodesPerCluster;
        return Cluster(numOfClusters);
    }

    public List<Cluster> Cluster(int numClusters) {
        while (_clusters.Count > numClusters) {
            var (cluster1, cluster2) = FindClosestClusters();
            MergeClusters(cluster1, cluster2);
        }
        return _clusters;
    }

    private (Cluster, Cluster) FindClosestClusters() {
        Cluster closestCluster1 = _clusters[0], closestCluster2 = _clusters[1];
        double minDistance = double.MaxValue;
        
        // This is O(n^2). But it is OK for our scale. 
        // Most of the time, our graphs won't be larger than 100 nodes.
        for (int i = 0; i < _clusters.Count; i++) {
            for (int j = i + 1; j < _clusters.Count; j++) {
                double distance = CalculateDistance(_clusters[i], _clusters[j]);
                if (distance < minDistance) {
                    minDistance = distance;
                    closestCluster1 = _clusters[i];
                    closestCluster2 = _clusters[j];
                }
            }
        }

        return (closestCluster1, closestCluster2);
    }

    private double CalculateDistance(Cluster cluster1, Cluster cluster2) {
        double minDistance = double.MaxValue;

        foreach (var node1 in cluster1.Nodes) {
            foreach (var node2 in cluster2.Nodes) {
                double distance = CalculateNodeDistance(node1, node2);
                if (distance < minDistance)
                    minDistance = distance;
            }
        }

        return minDistance;
    }

    private double CalculateNodeDistance(KnowledgeNode node1, KnowledgeNode node2) {
        if (node1.Id == node2.Id)
            return 0;
        if (node1.NeighborsIds.Contains(node2.Id))
            return 1;
        else
            return Utils.GraphAlgorithms.CalculateNodeDistance(_graph, node1, node2); 
    }

    private void MergeClusters(Cluster cluster1, Cluster cluster2) {
        var mergedNodes = cluster1.Nodes.Concat(cluster2.Nodes).ToList();
        var newCluster = new Cluster(mergedNodes);
        _clusters.Remove(cluster1);
        _clusters.Remove(cluster2);
        _clusters.Add(newCluster);
    }
}