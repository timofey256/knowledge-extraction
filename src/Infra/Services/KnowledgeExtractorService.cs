namespace KnowledgeExtractionTool.Infra.Services;

using KnowledgeExtractionTool.Core.Domain;
using KnowledgeExtractionTool.Core.Logic.LLMResponseParser;
using Microsoft.Extensions.Options;

/// <summary>
/// Extracts knowledge from the given text (context) in form of knowledge graph.  
/// </summary>
public class KnowledgeExtractorService {
    private readonly LanguageModelQueryService _llmQueryService;
    private readonly Prompts _prompts;
    private readonly ILogger<KnowledgeExtractorService> _logger;

    public KnowledgeExtractorService(IOptions<AppSettings> settings, LanguageModelQueryService llmQueryService, ILogger<KnowledgeExtractorService> logger) {
        _prompts = settings.Value.PromptsCollection.Default;
        _llmQueryService = llmQueryService;
        _logger = logger;
    }

    public KnowledgeGraph ExtractKnowledgeGraph(string context) {
        string prompt = _prompts.ConstructFinalPrompt(context);
        _logger.Log(LogLevel.Information, $"Sent prompt: {prompt}");
        string response = _llmQueryService.GetResponseSync(prompt);

        KnowledgeGraph graph = LLMResponseParser.ConstructGraphFromLLMResponse(response);
        
        
        return graph;
    }

} 