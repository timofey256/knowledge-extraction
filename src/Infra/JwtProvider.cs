using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using KnowledgeExtractionTool.Data.Types;

namespace KnowledgeExtractionTool.Infra;

/// <summary>
/// Provides JWT token generation functionality.
/// </summary>
public class JwtProvider {

    private readonly JwtOptions _options;

    /// <summary>
    /// Initializes JwtProvider with options from configuration.
    /// </summary>
    /// <param name="jwtOptions"></param>
    public JwtProvider(IOptions<JwtOptions> jwtOptions) {
        _options = jwtOptions.Value;
    }

    /// <summary>
    /// Generates a JWT token for a given user.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public string GenerateToken(User user) {
        Claim[] claims = new Claim[] { new Claim("userId", user.Id.ToString()) };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: credentials,
            expires: DateTime.UtcNow.AddHours(_options.ExpiresInHours)
        );

        // Serializes the token to a string format.
        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenValue;
    }
}