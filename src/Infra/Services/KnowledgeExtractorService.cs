namespace KnowledgeExtractionTool.Infra.Services;

using KnowledgeExtractionTool.Core.Domain;
using Microsoft.Extensions.Options;

public class KnowledgeExtractorService {
    private readonly LanguageModelQueryService _llmQueryService;
    private readonly Prompts _prompts;
    private readonly ILogger<KnowledgeExtractorService> _logger;

    public KnowledgeExtractorService(IOptions<AppSettings> settings, LanguageModelQueryService llmQueryService, ILogger<KnowledgeExtractorService> logger) {
        _prompts = settings.Value.PromptsCollection.Default;
        _llmQueryService = llmQueryService;
        _logger = logger;
    }

    public string ExtractKnowledge(string context) {
        string prompt = _prompts.ConstructFinalPrompt(context);
        _logger.Log(LogLevel.Debug, $"Sent prompt: {prompt}");
        string response = _llmQueryService.GetResponseSync(prompt); 

        return response;
    }

} 