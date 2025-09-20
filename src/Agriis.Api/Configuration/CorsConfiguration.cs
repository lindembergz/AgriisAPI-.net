namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de CORS (Cross-Origin Resource Sharing)
/// </summary>
public static class CorsConfiguration
{
    public const string DefaultPolicyName = "DefaultCorsPolicy";
    public const string DevelopmentPolicyName = "DevelopmentCorsPolicy";
    public const string ProductionPolicyName = "ProductionCorsPolicy";

    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var corsSettings = configuration.GetSection("CorsSettings");
        
        services.AddCors(options =>
        {
            // Política padrão baseada na configuração
            options.AddPolicy(DefaultPolicyName, policy =>
            {
                ConfigurePolicy(policy, corsSettings);
            });

            // Política específica para desenvolvimento
            options.AddPolicy(DevelopmentPolicyName, policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });

            // Política específica para produção
            options.AddPolicy(ProductionPolicyName, policy =>
            {
                var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() 
                    ?? Array.Empty<string>();
                
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins);
                }

                policy.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                      .WithHeaders("Content-Type", "Authorization", "X-Correlation-ID", "X-Requested-With")
                      .SetIsOriginAllowedToAllowWildcardSubdomains()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));

                var allowCredentials = corsSettings.GetValue<bool>("AllowCredentials");
                if (allowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });

        return services;
    }

    private static void ConfigurePolicy(Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicyBuilder policy, IConfigurationSection corsSettings)
    {
        var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>();
        var allowedMethods = corsSettings.GetSection("AllowedMethods").Get<string[]>();
        var allowedHeaders = corsSettings.GetSection("AllowedHeaders").Get<string[]>();
        var allowCredentials = corsSettings.GetValue<bool>("AllowCredentials");

        // Configurar origens
        if (allowedOrigins?.Length > 0)
        {
            if (allowedOrigins.Contains("*"))
            {
                policy.AllowAnyOrigin();
            }
            else
            {
                policy.WithOrigins(allowedOrigins)
                      .SetIsOriginAllowedToAllowWildcardSubdomains();
            }
        }
        else
        {
            policy.AllowAnyOrigin();
        }

        // Configurar métodos
        if (allowedMethods?.Length > 0)
        {
            if (allowedMethods.Contains("*"))
            {
                policy.AllowAnyMethod();
            }
            else
            {
                policy.WithMethods(allowedMethods);
            }
        }
        else
        {
            policy.AllowAnyMethod();
        }

        // Configurar headers
        if (allowedHeaders?.Length > 0)
        {
            if (allowedHeaders.Contains("*"))
            {
                policy.AllowAnyHeader();
            }
            else
            {
                policy.WithHeaders(allowedHeaders);
            }
        }
        else
        {
            policy.AllowAnyHeader();
        }

        // Configurar credenciais
        if (allowCredentials && !allowedOrigins?.Contains("*") == true)
        {
            policy.AllowCredentials();
        }

        // Configurações adicionais para produção
        policy.SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    }

    public static string GetPolicyName(IWebHostEnvironment environment)
    {
        return environment.EnvironmentName switch
        {
            "Development" => DevelopmentPolicyName,
            "Production" => ProductionPolicyName,
            _ => DefaultPolicyName
        };
    }
}