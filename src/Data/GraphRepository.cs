namespace KnowledgeExtractionTool.Data;

using MongoDB.Driver;
using KnowledgeExtractionTool.Core.Domain;
using KnowledgeExtractionTool.Data.Types;

public class GraphRepository {
    private readonly DatabaseSettings _settings;


    private readonly string _mongoCollectionName;
    private readonly IMongoCollection<KnowledgeGraph> _graphs;


    private readonly SimpleMemCache<KnowledgeGraph> _cache;
    private readonly TimeSpan _defaultCleanupTimeSpan = new TimeSpan(hours: 0, minutes: 5, seconds: 0);
    private readonly TimeSpan _defaultExpirationTimeSpan = new TimeSpan(hours: 0, minutes: 30, seconds: 0);

    public GraphRepository(IMongoDatabase database, string collectionName, DatabaseSettings settings) {
        _mongoCollectionName = collectionName;
        _graphs = database.GetCollection<KnowledgeGraph>(collectionName);
        _settings = settings;
        _cache = new SimpleMemCache<KnowledgeGraph>(_defaultCleanupTimeSpan);
    }

    public async Task<StorageResult> TryInsertGraph(KnowledgeGraph graph) {
        OperationResult? memcachedResult = null;
        OperationResult? mongoResult = null;
        if (_settings.useInMemory) {
            memcachedResult = await TryInsertInMemcached(graph);
        }

        if (!_settings.inMemoryOnly) {
            mongoResult = await TryInsertInMongo(graph);
        }

        return new StorageResult(memcached: memcachedResult, mongo: mongoResult);
    }

    public async Task<OperationResult> TryInsertInMongo(KnowledgeGraph graph) {
        try {
            await _graphs.InsertOneAsync(graph);
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Error(ex.Message);
        }
    }

    public async Task<OperationResult> TryInsertInMemcached(KnowledgeGraph graph) {
        // This memcache is very simple: we do not consider that memory can run out for example
        // or any other thing can go wrong. For simplicity now, we assume that we always can append.
        _cache.Set(graph.Id, graph, _defaultExpirationTimeSpan);
        return OperationResult.Success();
    }
}