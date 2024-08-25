namespace KnowledgeExtractionTool.Data;

using MongoDB.Driver;
using KnowledgeExtractionTool.Core.Domain;
using KnowledgeExtractionTool.Data.Types;

public class TextsRepository : Repository<Context> {
    public TextsRepository(IMongoDatabase database, string collectionName, DatabaseSettings settings) : 
                      base(               database,        collectionName,                  settings) { }
}