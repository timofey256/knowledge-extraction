namespace KnowledgeExtractionTool.Infra.Services;

using KnowledgeExtractionTool.Core.Domain;
using KnowledgeExtractionTool.Core.Logic;
using KnowledgeExtractionTool.Core.Logic.LLMResponseParser;
using Microsoft.Extensions.Options;

/// <summary>
/// Extracts knowledge from the given text (context) in form of knowledge graph.  
/// </summary>
public class KnowledgeExtractorService {
    private readonly LanguageModelQueryService _llmQueryService;
    private readonly Prompts _prompts;
    private readonly ILogger<KnowledgeExtractorService> _logger;

    public KnowledgeExtractorService(IOptions<AppSettings> settings, 
                                    LanguageModelQueryService llmQueryService, 
                                    ILogger<KnowledgeExtractorService> logger) {
        _prompts = settings.Value.PromptsCollection.Default;
        _llmQueryService = llmQueryService;
        _logger = logger;
    }

    public KnowledgeGraph ExtractKnowledgeGraph(string ownerId, string context) {
        string prompt = _prompts.ConstructFinalPrompt(context);
        _logger.Log(LogLevel.Information, $"Sent prompt: {prompt}");
        string response = _llmQueryService.GetResponseSync(prompt);

        KnowledgeGraph graph = LLMResponseParser.ConstructGraphFromLLMResponse(ownerId, response);
        
        var clustering = new HierarchicalClustering(graph);
        var criteria = new ClusteringCriteria(maxDistance:100, maxClusters: 2);  // emperical values. TODO: should be configurable?
        var clusters = clustering.Cluster(criteria);
        graph.Clusters = clusters;

        return graph;
    }
} 