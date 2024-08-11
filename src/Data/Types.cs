namespace KnowledgeExtractionTool.Data.Types;

using System;
using System.Collections.Concurrent;
using System.Threading;

public struct DatabaseSettings {
    public bool useInMemory; // if set to `true` we will use memcached
    public bool inMemoryOnly; // if set to `true` we won't use any external databases
}

public enum OperationResultType { Success, Error }

public class OperationResult {

    public OperationResultType Type { get; init; }
    public string? ErrorMessage { get; init; }

    private OperationResult(OperationResultType type, string? errorMessage = null)
    {
        Type = type;
        ErrorMessage = errorMessage;
    }

    public static OperationResult Success() => new OperationResult(OperationResultType.Success);
    public static OperationResult Error(string er) => new OperationResult(OperationResultType.Error, er);

}

/// <summary>
/// Storage result to all databases. In our case: cache and mongo.
/// </summary>
public class StorageResult {

    public OperationResult? MemcachedResult { get; }
    public OperationResult? MongoResult { get; }

    public StorageResult(OperationResult? memcached = null, OperationResult? mongo = null)
    {
        MemcachedResult = memcached;
        MongoResult = mongo;
    }
}

public class SimpleMemCache<T> {
    private class CacheItem
    {
        public T Value { get; set; }
        public DateTime Expiration { get; set; }
    }

    private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();
    private readonly Timer _cleanupTimer;

    public SimpleMemCache(TimeSpan cleanupInterval)
    {
        _cleanupTimer = new Timer(CleanupExpiredItems, null, cleanupInterval, cleanupInterval);
    }

    public void Set(string key, T value, TimeSpan expiration)
    {
        var expirationTime = DateTime.UtcNow.Add(expiration);
        _cache[key] = new CacheItem { Value = value, Expiration = expirationTime };
    }

    public bool TryGet(string key, out T value)
    {
        if (_cache.TryGetValue(key, out var item) && item.Expiration > DateTime.UtcNow)
        {
            value = item.Value;
            return true;
        }

        value = default;
        return false;
    }

    public void Remove(string key)
    {
        _cache.TryRemove(key, out _);
    }

    private void CleanupExpiredItems(object state)
    {
        var now = DateTime.UtcNow;
        foreach (var key in _cache.Keys)
        {
            if (_cache.TryGetValue(key, out var item) && item.Expiration <= now)
            {
                _cache.TryRemove(key, out _);
            }
        }
    }
}

public class User
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Salt { get; set; }
}