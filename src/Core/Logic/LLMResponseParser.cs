namespace KnowledgeExtractionTool.Core.Logic.LLMResponseParser;

using KnowledgeExtractionTool.Core.Domain;

/// I want to have immutable edges in graph and do not make abominations from types in Domain
/// So I make this intermediate type to work with here and then, when i am finished with initializing values here in this type, return DirectedKnowledgeEdge 
/// Maybe not the best design choice tho.
class ParsedBlock {
    public string? Node1 { get; set; } 
    public string? Node2 { get; set; }
    public int Node1Importance { get; set; } 
    public int Node2Importance { get; set; } 
    public string? Description { get; set; }

    public KnowledgeNode? _node1;
    public KnowledgeNode? _node2;
    public DirectedKnowledgeEdge? _edge;

    public KnowledgeNode GetFirstNode() {
        if (Node1 is null)
            throw new NullReferenceException("Cannot build KnowledgeEdge when some properties of DirectedKnowledgeEdgeConstruction are null!s");
        
        if (_node1 is null)
            _node1 = new KnowledgeNode(Node1, Node1Importance);

        return _node1;
    }

    public KnowledgeNode GetSecondNode() {
        if (Node2 is null)
            throw new NullReferenceException("Cannot build KnowledgeEdge when some properties of DirectedKnowledgeEdgeConstruction are null!s");
    
        if (_node2 is null)
            _node2 = new KnowledgeNode(Node2, Node1Importance);

        return _node2;    
    }

    public DirectedKnowledgeEdge GetEdge() {
        if (Node1 is not null && Node2 is not null && Description is not null) {
            if (_edge is null) {
                var node1 = GetFirstNode(); 
                var node2 = GetSecondNode();
                _edge = new DirectedKnowledgeEdge(node1.Id, node2.Id, Description) { Node1Id = node1.Id, Node2Id = node2.Id, Label = Description};
            }

            return _edge;
        }

        throw new NullReferenceException("Cannot build DirectedKnowledgeEdge when some properties of DirectedKnowledgeEdgeConstruction are null!s");

    }
}

/// <summary>
/// Parses LLM's output to Knowledge Graph. 
/// </summary>
class LLMResponseParser {
    /// LLM supposingly returns JSON (as we request in prompt).
    /// But can we really trust that output will be JSON deserializable?
    /// Even one wrong character might crash the derialization, so `Json.Derializer(output)` is not an option.
    /// This class will read output line by line and parse LLMs output. It's more reliable.

    public static KnowledgeGraph ConstructGraphFromLLMResponse(string output) {
        var lines = output.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0);

        return Parse("!!!InsertOwnerID!!!", lines);
    }
    
    // TODO: ugly code, not sure now how to rewrite.
    private static KnowledgeGraph Parse(string ownerId, IEnumerable<string> lines) {
        KnowledgeGraph graph = new(ownerId);

        ParsedBlock? currentBlock = null;
        
        foreach (var line in lines)
        {
            if (line == "{")
            {
                currentBlock = new ParsedBlock();
            }
            else if (line == "},")
            {
                if (currentBlock != null)
                {
                    graph.Nodes.Add(currentBlock.GetFirstNode());
                    graph.Nodes.Add(currentBlock.GetSecondNode());
                    graph.Edges.Add(currentBlock.GetEdge());
                    currentBlock = null;
                }
            }
            else if (line.StartsWith("\""))
            {
                var parts = line.Split(':');
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim().Trim('"');
                    var value = parts[1].Trim().Trim(',').Trim('"');

                    switch (key)
                    {
                        case "node_1":
                            currentBlock.Node1 = value;
                            break;
                        case "importance_1":
                            currentBlock.Node1Importance = int.Parse(value);
                            break;
                        case "node_2":
                            currentBlock.Node2 = value;
                            break;
                        case "importance_2":
                            currentBlock.Node2Importance = int.Parse(value);
                            break;
                        case "edge":
                            currentBlock.Description = value;
                            break;
                    }
                }
            }
        }

        if (currentBlock != null)
        {
            graph.Nodes.Add(currentBlock.GetFirstNode());
            graph.Nodes.Add(currentBlock.GetSecondNode());
            graph.Edges.Add(currentBlock.GetEdge());
        }

        return graph;
    }
}