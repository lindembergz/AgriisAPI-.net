using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de autenticação JWT
/// </summary>
public static class AuthenticationConfiguration
{
    /// <summary>
    /// Configura autenticação JWT
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Coleção de serviços</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurada");
        var issuer = jwtSettings["Issuer"] ?? "Agriis.Api";
        var audience = jwtSettings["Audience"] ?? "Agriis.Client";

        var key = Encoding.ASCII.GetBytes(secretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Para desenvolvimento
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero, // Remove delay padrão de 5 minutos
                RequireExpirationTime = true
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("Falha na autenticação JWT: {Exception}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var userId = context.Principal?.FindFirst("user_id")?.Value;
                    logger.LogDebug("Token JWT validado para usuário: {UserId}", userId);
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("Desafio de autenticação JWT: {Error} - {ErrorDescription}", 
                        context.Error, context.ErrorDescription);
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    /// <summary>
    /// Configura políticas de autorização
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços</returns>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Política para administradores
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("RoleAdmin"));

            // Política para fornecedores (admin ou representante)
            options.AddPolicy("FornecedorOnly", policy =>
                policy.RequireRole("RoleFornecedorWebAdmin", "RoleFornecedorWebRepresentante"));

            // Política para administradores de fornecedor
            options.AddPolicy("FornecedorAdminOnly", policy =>
                policy.RequireRole("RoleFornecedorWebAdmin"));

            // Política para compradores/produtores
            options.AddPolicy("CompradorOnly", policy =>
                policy.RequireRole("RoleComprador"));

            // Política para usuários autenticados (qualquer role)
            options.AddPolicy("AuthenticatedUser", policy =>
                policy.RequireAuthenticatedUser());

            // Política para administradores ou fornecedores
            options.AddPolicy("AdminOrFornecedor", policy =>
                policy.RequireRole("RoleAdmin", "RoleFornecedorWebAdmin", "RoleFornecedorWebRepresentante"));

            // Política para administradores ou compradores
            options.AddPolicy("AdminOrComprador", policy =>
                policy.RequireRole("RoleAdmin", "RoleComprador"));
        });

        return services;
    }
}