namespace KnowledgeExtractionTool.Data;

using MongoDB.Driver;
using KnowledgeExtractionTool.Core.Domain;
using KnowledgeExtractionTool.Data.Types;

/// <summary>
/// Repository for storing texts users build knowledge graphs from. 
/// </summary>
public class TextsRepository : Repository<Context> {
    public TextsRepository(IMongoDatabase database, string collectionName, DatabaseSettings settings) : 
                      base(               database,        collectionName,                  settings) { }
}