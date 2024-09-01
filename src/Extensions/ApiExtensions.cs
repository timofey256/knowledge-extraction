using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace KnowledgeExtractionTool.Extensions;

public static class ApiExtensions {

    /// <summary>
    /// Configures JWT authentication and authorization services
    /// </summary>
    /// <param name="services">Application services</param>
    /// <param name="jwtSettings">JWT Settings</param>
    /// <exception cref="NullReferenceException">Throws NullReferenceException if JwtSettings are null.</exception>
    public static void AddApiAutentication(this IServiceCollection services, JwtOptions? jwtSettings) {
        if (jwtSettings is null) {
            throw new NullReferenceException("ApiExtensions: JwtOptions are null!");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => {
                    // Configures JWT token validation parameters.
                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                    // Configures event to read token from cookies.
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies[jwtSettings.CookiesKey];
                            return Task.CompletedTask;
                        }
                    };
                });
        
        // Adds authorization services to the container.
        services.AddAuthorization();
    }
}
