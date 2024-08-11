namespace KnowledgeExtractionTool.Infra.Services;

using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using KnowledgeExtractionTool.Data;
using KnowledgeExtractionTool.Data.Types;

// This whole thing is pretty insane.
// I wanted to do something like discriminated union in F# (which would be like 5 lines of code...) and came up with this abomination.
// I would like to have some clear contract of what Registration should return, so I'm gonna use it.
public enum RegistrationResultType
{
    Success,
    UsedEmailError,
    FailedInsertionError
}

public class RegistrationResult
{
    public RegistrationResultType Type { get; }
    public string? ErrorMessage { get; }

    private RegistrationResult(RegistrationResultType type, string? errorMessage = null)
    {
        Type = type;
        ErrorMessage = errorMessage;
    }

    public static RegistrationResult Success() => new RegistrationResult(RegistrationResultType.Success);
    public static RegistrationResult UsedEmailError() => new RegistrationResult(RegistrationResultType.UsedEmailError);
    public static RegistrationResult FailedInsertionError(string? errorMessage) => 
        new RegistrationResult(RegistrationResultType.FailedInsertionError, errorMessage);

    public T Match<T>(
        Func<T> success,
        Func<T> usedEmailError,
        Func<string?, T> failedInsertionError)
    {
        return Type switch
        {
            RegistrationResultType.Success => success(),
            RegistrationResultType.UsedEmailError => usedEmailError(),
            RegistrationResultType.FailedInsertionError => failedInsertionError(ErrorMessage),
            _ => throw new InvalidOperationException("Unknown registration result type")
        };
    }
}

public class UserService {
    private readonly IMongoCollection<User> _users;
    private readonly JwtProvider _jwtProvider;
    private readonly ILogger<KnowledgeExtractorService> _logger;
    private readonly UsersRepository _usersRepository;

    public UserService(IMongoDatabase database,
                        JwtProvider jwtProvider,
                        UsersRepository usersRepository,
                        ILogger<KnowledgeExtractorService> logger)
    {
        _users = database.GetCollection<User>("Users");
        _jwtProvider = jwtProvider;
        _logger = logger;
        _usersRepository = usersRepository;
    }

    public async Task<RegistrationResult> RegisterAsync(string email, string password)
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

        if (await _usersRepository.ExistsEmail(email)) {
            return RegistrationResult.UsedEmailError();
        }

        StorageResult result = await _usersRepository.TryInsert(user);
        if (result.MemcachedResult?.Type == OperationResultType.Error) {
            _logger.Log(LogLevel.Error, $"Failed to insert user to the Memcached. ErrorMessage:\n {result.MemcachedResult.ErrorMessage}");
            return RegistrationResult.FailedInsertionError(result.MemcachedResult.ErrorMessage);
        }
        if (result.MongoResult?.Type == OperationResultType.Error) {
            _logger.Log(LogLevel.Error, $"Failed to insert user to the Mongo. ErrorMessage:\n {result.MemcachedResult.ErrorMessage}");
            return RegistrationResult.FailedInsertionError(result.MongoResult.ErrorMessage);
        }

        return RegistrationResult.Success();
    }

    public async Task<string?> AuthenticateAsync(string email, string password)
    {
        _logger.Log(LogLevel.Information, $"Got autentication request: [email='{email}', password='{password}]'");
        User user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        if (user is null) { 
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
