namespace KnowledgeExtractionTool.Data;

using KnowledgeExtractionTool.Data.Types;
using MongoDB.Driver;

public class UsersRepository : Repository<User> {
    public UsersRepository(IMongoDatabase database, string collectionName, DatabaseSettings settings) : base(database, collectionName, settings) { }

    public async Task<bool> ExistsEmail(string email) {
        var filter = Builders<User>.Filter.Eq("email", email);
        var result = await base._documents.CountDocumentsAsync(filter);

        return result > 0;
    }
}