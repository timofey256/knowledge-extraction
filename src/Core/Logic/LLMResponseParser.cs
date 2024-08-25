namespace KnowledgeExtractionTool.Core.Logic.LLMResponseParser;

using KnowledgeExtractionTool.Core.Domain;
using KnowledgeExtractionTool.Utils;

/*
 * The LLMResponseParser class is designed to handle the potential unreliability of JSON
 * output from a LLM. While we may request that the LLM outputs data in
 * a JSON format, there's no guarantee that the output will be perfectly structured and
 * deserializable. This class takes a more robust approach by parsing the output line by line.
 */
class LLMResponseParser {
    /// LLM supposingly returns JSON (as we request in prompt).
    /// But can we really trust that output will be JSON deserializable?
    /// Even one wrong character might crash the derialization, so `Json.Derializer(output)` is not sufficiently reliable.
    /// This class will read output line by line and parse LLMs output. It's more reliable.
    /// This approach hurts reusability and readability though.

    public static KnowledgeGraph ConstructGraphFromLLMResponse(string ownerId, string output) {
        var lines = output.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0);

        var graph = Parse(lines);
        graph.OwnerId = ownerId;
        return graph;
    }
    
    // TODO: ugly code, not sure now how to rewrite.
    private static KnowledgeGraph Parse(IEnumerable<string> lines) {
        KnowledgeGraph graph = new();

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