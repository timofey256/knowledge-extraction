using KnowledgeExtractionTool.Core.Domain;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KnowledgeExtractionTool.Data;

public class UsersRepository {
    private readonly string _collectionName;
    private readonly IMongoCollection<User> _users;

    public UsersRepository(IMongoDatabase database, string collectionName) {
        _collectionName = collectionName;
        _users = database.GetCollection<User>(collectionName);
    }

    public async Task<string?> TryInsertUser(User user) {
        try {
            await _users.InsertOneAsync(user);
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public async Task<bool> ExistsEmail(string email) {
        var filter = Builders<User>.Filter.Eq("email", email);
        var result = await _users.CountDocumentsAsync(filter);

        return result > 0;
    }
}