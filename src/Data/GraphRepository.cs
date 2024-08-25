namespace KnowledgeExtractionTool.Data;

using MongoDB.Driver;
using KnowledgeExtractionTool.Core.Domain;
using KnowledgeExtractionTool.Data.Types;

public class GraphRepository : Repository<KnowledgeGraph> {
    public GraphRepository(IMongoDatabase database, string collectionName, DatabaseSettings settings) : base(database, collectionName, settings) { }

    public async Task<List<KnowledgeGraph>> FindByOwnerIdAsync(string ownerId) {
        var filter = Builders<KnowledgeGraph>.Filter.Eq(kg => kg.OwnerId, ownerId);
        var results = await _documents.Find(filter).ToListAsync();
        return results;
    }
}