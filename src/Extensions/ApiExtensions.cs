using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace KnowledgeExtractionTool.Extensions;

public static class ApiExtensions {
    public static void AddMappedEndpoints(this IEndpointRouteBuilder app) { }

    public static void AddApiAutentication(this IServiceCollection services, JwtOptions? jwtSettings) {
        if (jwtSettings is null) {
            throw new NullReferenceException("ApiExtensions: JwtOptions are null!");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => {
                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies[jwtSettings.CookiesKey];
                            return Task.CompletedTask;
                        }
                    };
                });
        
        services.AddAuthorization();
    }
} 