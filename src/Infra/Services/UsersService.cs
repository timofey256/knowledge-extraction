namespace KnowledgeExtractionTool.Infra.Services;

using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using KnowledgeExtractionTool.Core.Domain;

public class UserService
{
    private readonly IMongoCollection<User> _users;
    private readonly JwtProvider _jwtProvider;
    private readonly ILogger<KnowledgeExtractorService> _logger;

    public UserService(IMongoDatabase database, JwtProvider jwtProvider, ILogger<KnowledgeExtractorService> logger)
    {
        _users = database.GetCollection<User>("Users");
        _jwtProvider = jwtProvider;
        _logger = logger;
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        _logger.Log(LogLevel.Information, $"Registration request: [email='{email}', password='{password}]'");
        var salt = GenerateSalt();
        var passwordHash = HashPassword(password, salt);

        var user = new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = email,
            PasswordHash = passwordHash,
            Salt = salt
        };

        try {
            await _users.InsertOneAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, $"Failed to insert user to the collection. ErrorMessage:\n {ex.Message}");
            return false;
        }
    }

    public async Task<string?> AuthenticateAsync(string email, string password)
    {
        _logger.Log(LogLevel.Information, $"Got autentication request: [email='{email}', password='{password}]'");
        User user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null) { 
            _logger.Log(LogLevel.Warning, $"Autentication request: [email='{email}', password='{password}] doesn't match any email password in the collection!'");
            return null;
        } 

        
        var hash = HashPassword(password, user.Salt);
        if (user.PasswordHash != hash)
            _logger.Log(LogLevel.Warning, $"Autentication request: [email='{email}', password='{password}] doesn't match password for the found user!'");


        return user.PasswordHash == hash ? _jwtProvider.GenerateToken(user) : null;
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
