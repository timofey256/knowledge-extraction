using KnowledgeExtractionTool.Core.Domain;

namespace KnowledgeExtractionTool.Utils;

public class Result<T> {
    public T? Value { get; }
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }

    protected Result(T value) {
        IsSuccess = true;
        Value = value;
    }

    protected Result(string errorMessage) {
        IsSuccess = false;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value) {
        return new Result<T>(value);
    }

    public static Result<T> Failure(string errorMessage) {
        return new Result<T>(errorMessage);
    }
}

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
