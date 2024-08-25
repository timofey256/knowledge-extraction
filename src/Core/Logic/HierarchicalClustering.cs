namespace KnowledgeExtractionTool.Core.Logic;

using System.Linq;
using System.Collections.Generic;
using KnowledgeExtractionTool.Core.Domain;
using KnowledgeExtractionTool.Extensions;

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
    private PriorityQueue<ClusterPair, double> _clusterPairs;
    private List<Cluster> _clusters;

    public HierarchicalClustering(KnowledgeGraph graph) {
        _graph = graph;
        _clusters = graph.Nodes.Select(node => new Cluster(node)).ToList();
        _clusterPairs = new PriorityQueue<ClusterPair, double>();
        InitializeClusterPairs();
    }

    private void InitializeClusterPairs() {
        for (int i = 0; i < _clusters.Count; i++) {
            for (int j = i + 1; j < _clusters.Count; j++) {
                double distance = CalculateDistance(_clusters[i], _clusters[j]);
                _clusterPairs.Enqueue(new ClusterPair(_clusters[i], _clusters[j]), distance);
            }
        }
    }

    public List<Cluster> Cluster(ClusteringCriteria criteria) {
        while (!criteria.ShouldStop(_clusters, _clusterPairs)) {
            var closestPair = _clusterPairs.Dequeue();
            MergeClusters(closestPair.Cluster1, closestPair.Cluster2);
            UpdateClusterPairs(closestPair.Cluster1, closestPair.Cluster2);
        }
        return _clusters;
    }

    private void UpdateClusterPairs(Cluster merged, Cluster removed) {
        _clusterPairs.RemoveWhere(pair => pair.Contains(removed));
        foreach (var cluster in _clusters.Where(c => c != merged)) {
            double distance = CalculateDistance(merged, cluster);
            _clusterPairs.Enqueue(new ClusterPair(merged, cluster), distance);
        }
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

        // Combine graph distance with semantic similarity
        double graphDistance = CalculateGraphDistance(node1, node2);
        double semanticSimilarity = CalculateSemanticSimilarity(node1, node2);
        
        return (graphDistance + (1 - semanticSimilarity)) / 2;
    }

    private double CalculateGraphDistance(KnowledgeNode node1, KnowledgeNode node2) {
        if (node1.NeighborsIds.Contains(node2.Id))
            return 1;
        else
            return Utils.GraphAlgorithms.CalculateNodeDistanceBFS(_graph, node1, node2);
    }

    private double CalculateSemanticSimilarity(KnowledgeNode node1, KnowledgeNode node2) {
        // TODO: Implement semantic similarity calculation here (ML.NET?).
        // This could use techniques like cosine similarity on node embeddings or TF-IDF vectors.
        // For now, we'll use a placeholder implementation.
        return 0.5;
    }

    private void MergeClusters(Cluster cluster1, Cluster cluster2) {
        var mergedNodes = cluster1.Nodes.Concat(cluster2.Nodes).ToList();
        var newCluster = new Cluster(mergedNodes);
        _clusters.Remove(cluster1);
        _clusters.Remove(cluster2);
        _clusters.Add(newCluster);
    }
}

public class ClusterPair {
    public Cluster Cluster1 { get; }
    public Cluster Cluster2 { get; }

    public ClusterPair(Cluster cluster1, Cluster cluster2) {
        Cluster1 = cluster1;
        Cluster2 = cluster2;
    }

    public bool Contains(Cluster cluster) => Cluster1 == cluster || Cluster2 == cluster;
}

public class ClusteringCriteria {
    public int MinClusters { get; set; }
    public double MaxDistance { get; set; }

    public bool ShouldStop(List<Cluster> clusters, PriorityQueue<ClusterPair, double> clusterPairs) {
        if (clusters.Count <= MinClusters)
            return true;

        clusterPairs.TryPeek(out ClusterPair val, out double priorityVal); //for int type
        if (clusterPairs.Count > 0 && priorityVal > MaxDistance)
            return true;

        return false;
    }
}