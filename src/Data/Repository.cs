namespace KnowledgeExtractionTool.Data;

using KnowledgeExtractionTool.Data.Types;
using KnowledgeExtractionTool.Core.Interfaces;
using MongoDB.Driver;

/*
 * The Repository<T> class is a generic repository implementation designed to handle the storage 
 * of documents in both MongoDB and an in-memory cache. The class serves as an abstraction layer 
 * that facilitates interaction with a MongoDB collection while optionally leveraging an in-memory 
 * cache for faster data retrieval. The concrete settings are specified with DatabaseSettings in constructor.
 */
public class Repository<T> where T : IHasId {
    protected readonly DatabaseSettings _settings;
    protected readonly string _mongoCollectionName;
    protected readonly IMongoCollection<T> _documents;
    protected readonly SimpleMemCache<T> _cache;
    protected readonly TimeSpan _defaultCleanupTimeSpan = new TimeSpan(hours: 0, minutes: 5, seconds: 0);
    protected readonly TimeSpan _defaultExpirationTimeSpan = new TimeSpan(hours: 0, minutes: 30, seconds: 0);

    /// <summary>
    /// Constructor initializes MongoDB collection and in-memory cache with specified settings.
    /// </summary>
    /// <param name="database">MongoDB database</param>
    /// <param name="collectionName">Name of colletion where documents will be stored</param>
    /// <param name="settings">Settings of the application storing system</param>
    public Repository(IMongoDatabase database, string collectionName, DatabaseSettings settings) {
        _mongoCollectionName = collectionName;
        _documents = database.GetCollection<T>(collectionName);
        _settings = settings;
        _cache = new SimpleMemCache<T>(_defaultCleanupTimeSpan);
    }
    
    /// <summary>
    /// Attempts to insert a document into both in-memory cache and MongoDB based on settings.
    /// </summary>
    /// <param name="doc">Document to insert</param>
    /// <returns></returns>
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

    /// <summary>
    /// Inserts a document into MongoDB and returns the result of the operation.
    /// </summary>
    /// <param name="doc">Document to delete</param>
    /// <returns></returns>
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

    /// <summary>
    /// Inserts a document into in-memory cache and returns the result of the operation.
    /// </summary>
    /// <param name="doc">Document to insert</param>
    /// <returns></returns>
    public async Task<OperationResult> TryInsertInMemcached(T doc) {
        // This memcache is very simple: we do not consider that memory can run out for example
        // or any other thing can go wrong. For simplicity now, we assume that we always can append.
        _cache.Set(doc.Id, doc, _defaultExpirationTimeSpan);
        return OperationResult.Success();
    }
}