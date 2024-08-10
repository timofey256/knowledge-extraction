

using KnowledgeExtractionTool.Core.Domain;

namespace KnowledgeExtractionTool.Core.Logic.LLMResponseParser;

/// I want to have immutable edges in graph and do not make abominations from types in Domain
/// So I make this intermediate type to work with here and then, when i am finished with initializing values here in this type, return DirectedKnowledgeEdge 
/// Maybe not the best design choice tho.
class DirectedKnowledgeEdgeConstruction {
    public string? Node1 { get; set; } 
    public string? Node2 { get; set; }
    public int Node1Importance { get; set; } 
    public int Node2Importance { get; set; } 
    public string? Description { get; set; }

    public DirectedKnowledgeEdge Build() {
        if (Node1 is not null && Node2 is not null && Description is not null)
            return new DirectedKnowledgeEdge(Node1, Node1Importance, Node2, Node2Importance, Description);

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

        return Parse(lines);
    }
    
    // TODO: ugly code, not sure now how to rewrite.
    private static KnowledgeGraph Parse(IEnumerable<string> lines) {
        var edges = new List<DirectedKnowledgeEdge>();
        DirectedKnowledgeEdgeConstruction? currentEdge = null;

        foreach (var line in lines)
        {
            if (line == "{")
            {
                currentEdge = new DirectedKnowledgeEdgeConstruction();
            }
            else if (line == "},")
            {
                if (currentEdge != null)
                {
                    edges.Add(currentEdge.Build());
                    currentEdge = null;
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
                            currentEdge.Node1 = value;
                            break;
                        case "importance_1":
                            currentEdge.Node1Importance = int.Parse(value);
                            break;
                        case "node_2":
                            currentEdge.Node2 = value;
                            break;
                        case "importance_2":
                            currentEdge.Node2Importance = int.Parse(value);
                            break;
                        case "edge":
                            currentEdge.Description = value;
                            break;
                    }
                }
            }
        }

        if (currentEdge != null)
        {
            edges.Add(currentEdge.Build());
        }

        return new KnowledgeGraph(edges.ToArray());
    }
}