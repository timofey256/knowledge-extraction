using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using KnowledgeExtractionTool.Core.Domain;

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(IMongoDatabase database)
    {
        _users = database.GetCollection<User>("Users");
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        var salt = GenerateSalt();
        var passwordHash = HashPassword(password, salt);

        var user = new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = email,
            PasswordHash = passwordHash,
            Salt = salt
        };

        try
        {
            await _users.InsertOneAsync(user);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null) return null;

        var hash = HashPassword(password, user.Salt);
        return user.PasswordHash == hash ? user : null;
    }

    private static string GenerateSalt()
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(salt);
    }

    private static string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 32);

        return Convert.ToBase64String(hashBytes);
    }
}
