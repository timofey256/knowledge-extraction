namespace KnowledgeExtractionTool.Data;

using MongoDB.Driver;
using KnowledgeExtractionTool.Core.Domain;
using KnowledgeExtractionTool.Data.Types;

public class GraphRepository : Repository<KnowledgeGraph> {
    public GraphRepository(IMongoDatabase database, string collectionName, DatabaseSettings settings) : base(database, collectionName, settings) { }
}