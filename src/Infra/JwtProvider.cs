using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Claims;
using KnowledgeExtractionTool.Data.Types;

namespace KnowledgeExtractionTool.Infra;

public class JwtProvider {

    private readonly JwtOptions _options;

    public JwtProvider(IOptions<JwtOptions> jwtOptions) {
        _options = jwtOptions.Value;
    }

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

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenValue;
    }
}