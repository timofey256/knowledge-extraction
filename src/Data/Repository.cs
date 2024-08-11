namespace KnowledgeExtractionTool.Data;

using KnowledgeExtractionTool.Data.Types;
using KnowledgeExtractionTool.Core.Interfaces;
using MongoDB.Driver;

public class Repository<T> where T : IHasId {
    protected readonly DatabaseSettings _settings;


    protected readonly string _mongoCollectionName;
    protected readonly IMongoCollection<T> _documents;


    protected readonly SimpleMemCache<T> _cache;
    protected readonly TimeSpan _defaultCleanupTimeSpan = new TimeSpan(hours: 0, minutes: 5, seconds: 0);
    protected readonly TimeSpan _defaultExpirationTimeSpan = new TimeSpan(hours: 0, minutes: 30, seconds: 0);

    public Repository(IMongoDatabase database, string collectionName, DatabaseSettings settings) {
        _mongoCollectionName = collectionName;
        _documents = database.GetCollection<T>(collectionName);
        _settings = settings;
        _cache = new SimpleMemCache<T>(_defaultCleanupTimeSpan);
    }

    public async Task<StorageResult> TryInsert(T doc) {
        OperationResult? memcachedResult = null;
        OperationResult? mongoResult = null;
        if (_settings.useInMemory) {
            memcachedResult = await TryInsertInMemcached(doc);
        }

        if (!_settings.inMemoryOnly) {
            mongoResult = await TryInsertInMongo(doc);
        }

        return new StorageResult(memcached: memcachedResult, mongo: mongoResult);
    }

    public async Task<OperationResult> TryInsertInMongo(T doc) {
        try {
            await _documents.InsertOneAsync(doc);
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Error(ex.Message);
        }
    }

    public async Task<OperationResult> TryInsertInMemcached(T element) {
        // This memcache is very simple: we do not consider that memory can run out for example
        // or any other thing can go wrong. For simplicity now, we assume that we always can append.
        _cache.Set(element.Id, element, _defaultExpirationTimeSpan);
        return OperationResult.Success();
    }
}