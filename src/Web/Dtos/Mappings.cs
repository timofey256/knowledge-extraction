namespace KnowledgeExtractionTool.Controllers.DTOs.Mappings;

using KnowledgeExtractionTool.Core.Domain;

public static class Mappings {
    public static KnowledgeGraphDto toKnowledgeGraphDto(KnowledgeGraph graph) {
        return new KnowledgeGraphDto(graph);
    }
}