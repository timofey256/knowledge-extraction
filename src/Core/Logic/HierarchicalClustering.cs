namespace KnowledgeExtractionTool.Core.Logic;

using System.Linq;
using System.Collections.Generic;
using KnowledgeExtractionTool.Core.Domain;
using KnowledgeExtractionTool.Extensions;
using KnowledgeExtractionTool.Utils;

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
        UpdateClusterPairs();
    }

    public List<Cluster> Cluster(ClusteringCriteria criteria) {
        while (!criteria.ShouldStop(_clusters, _clusterPairs)) {
            Console.WriteLine($"---------Clusters before: {_clusters.Count}---------");
            var closestPair = _clusterPairs.Dequeue();
            MergeClusters(closestPair.Cluster1, closestPair.Cluster2);
            UpdateClusterPairs();
            Console.WriteLine($"---------Clusters after: {_clusters.Count}---------");
        }
        return _clusters;
    }

    private void UpdateClusterPairs() {
        _clusterPairs = new PriorityQueue<ClusterPair, double>();

        for (int i = 0; i < _clusters.Count; i++) {
            for (int j = 0; j < _clusters.Count; j++) {
                if (i == j) continue;

                double? distance = CalculateDistance(_clusters[i], _clusters[j]);
                if (distance is null) 
                    continue;
                
                Console.WriteLine($"Enqueue (({_clusters[i]}, {_clusters[j]}), {distance})");
                _clusterPairs.Enqueue(new ClusterPair(_clusters[i], _clusters[j]), distance.Value);
            }
        }
    }

    private double? CalculateDistance(Cluster cluster1, Cluster cluster2) {
        double? minDistance = null;
        foreach (var node1 in cluster1.Nodes) {
            foreach (var node2 in cluster2.Nodes) {
                double? distance = CalculateNodeDistance(node1, node2);
                if (distance is null)
                    continue;
                else if (distance < minDistance || minDistance is null)
                    minDistance = distance.Value;
            }
        }
        return minDistance;
    }

    private double? CalculateNodeDistance(KnowledgeNode node1, KnowledgeNode node2) {
        if (node1.Id == node2.Id)
            return 0;

        // Combine graph distance with semantic similarity
        double? graphDistance = GraphAlgorithms.CalculateNodeDistanceBFS(_graph, node1, node2);
        if (graphDistance is null)
            return null;
        Console.WriteLine($"GraphDistance between {node1.Label} and {node2.Label} = {graphDistance}");
        double semanticSimilarity = CalculateSemanticSimilarity(node1, node2);
        
        return (graphDistance + (1 - semanticSimilarity)) / 2;
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
        Console.WriteLine($"In new cluster there are {newCluster.Nodes.Count} nodes");
    }
}

/// <summary>
/// Represents a pair of clusters in a hierarchical clustering algorithm.
/// </summary>
public class ClusterPair {
    public Cluster Cluster1 { get; }
    public Cluster Cluster2 { get; }

    public ClusterPair(Cluster cluster1, Cluster cluster2) {
        Cluster1 = cluster1;
        Cluster2 = cluster2;
    }

    public bool Contains(Cluster cluster) => Cluster1 == cluster || Cluster2 == cluster;
}

/// <summary>
/// Defines the criteria for controlling the clustering process in a hierarchical clustering algorithm.
/// </summary>
public class ClusteringCriteria {
    /// <summary>
    /// Gets or sets the minimum number of clusters required before the clustering process stops.
    /// </summary>
    public readonly int MaxClusters;

    /// <summary>
    /// Gets or sets the maximum allowable distance between clusters for the clustering process to continue.
    /// </summary>
    public readonly double MaxDistance;

    public ClusteringCriteria(int maxClusters, int maxDistance) {
        MaxClusters = maxClusters;
        MaxDistance = maxDistance;
    }

    public bool ShouldStop(List<Cluster> clusters, PriorityQueue<ClusterPair, double> clusterPairs) {
        if (clusters.Count <= MaxClusters){
            Console.WriteLine("Stopped because <= MaxClusters"); return true;
        }

        if (clusterPairs.Count <= 0) {
            Console.WriteLine("Stopped because empty queue"); return true;
        }

        clusterPairs.TryPeek(out ClusterPair val, out double priorityVal);
        if (priorityVal > MaxDistance) {
            Console.WriteLine("Stopped because > MaxDistance"); return true;
        }

        return false;
    }
}