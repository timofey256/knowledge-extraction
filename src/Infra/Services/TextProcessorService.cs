namespace KnowledgeExtractionTool.Infra.Services;

using KnowledgeExtractionTool.Core.Logic;
using KnowledgeExtractionTool.Infra.Services.Interfaces;

/// <summary>
/// Processes given text using Process(string).
/// What does processing involve? 
/// 1. Removing stop words.
/// 2. Lematization.
/// </summary>
public class TextProcessorService : ITextProcessorService {

    private readonly KnowledgeExtractorService _knowledgeExtractorService;
    public TextProcessorService(KnowledgeExtractorService knowledgeExtractorService) {
        _knowledgeExtractorService = knowledgeExtractorService;
    }    
    
    public string Process(string text) {
        return _knowledgeExtractorService.ExtractKnowledge(text);
    }
}