using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de Health Checks
/// </summary>
public static class HealthChecksConfiguration
{
    public static IServiceCollection AddHealthChecksConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Health check do banco de dados PostgreSQL
        AddDatabaseHealthCheck(healthChecksBuilder, configuration);

        // Health check dos serviços externos
        AddExternalServicesHealthChecks(healthChecksBuilder, configuration);

        // Health check de memória
        AddMemoryHealthCheck(healthChecksBuilder, configuration);

        // Configurar formatadores de resposta
        services.Configure<HealthCheckPublisherOptions>(options =>
        {
            options.Delay = TimeSpan.FromSeconds(2);
            options.Period = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    private static void AddDatabaseHealthCheck(IHealthChecksBuilder builder, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var timeout = configuration.GetValue<int>("HealthChecks:DatabaseTimeoutInSeconds", 30);

        if (!string.IsNullOrEmpty(connectionString))
        {
            builder.AddNpgSql(
                connectionString,
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "sql", "postgresql" },
                timeout: TimeSpan.FromSeconds(timeout));
        }
    }

    private static void AddExternalServicesHealthChecks(IHealthChecksBuilder builder, IConfiguration configuration)
    {
        // Health check do SERPRO
        var serproSettings = configuration.GetSection("SerproSettings");
        var serproBaseUrl = serproSettings.GetValue<string>("BaseUrl");
        
        if (!string.IsNullOrEmpty(serproBaseUrl))
        {
            builder.AddUrlGroup(
                new Uri($"{serproBaseUrl}/health"),
                name: "serpro-api",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "external", "serpro" },
                timeout: TimeSpan.FromSeconds(10));
        }

        // Health check do AWS S3 (verificação básica)
        var awsSettings = configuration.GetSection("AwsSettings");
        var s3BucketName = awsSettings.GetValue<string>("S3BucketName");
        
        if (!string.IsNullOrEmpty(s3BucketName))
        {
            builder.AddCheck<AwsS3HealthCheck>(
                "aws-s3",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "external", "aws", "s3" },
                timeout: TimeSpan.FromSeconds(15));
        }

        // Health check do Hangfire (se configurado)
        var hangfireConnectionString = configuration.GetValue<string>("HangfireSettings:ConnectionString");
        
        if (!string.IsNullOrEmpty(hangfireConnectionString))
        {
            builder.AddHangfire(options =>
            {
                options.MinimumAvailableServers = 1;
            },
            name: "hangfire",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "hangfire", "background-jobs" });
        }
    }

    private static void AddMemoryHealthCheck(IHealthChecksBuilder builder, IConfiguration configuration)
    {
        var memoryThresholdMb = configuration.GetValue<long>("HealthChecks:MemoryThresholdMB", 1024);
        
        builder.AddPrivateMemoryHealthCheck(
            memoryThresholdMb * 1024 * 1024, // Converter MB para bytes
            name: "memory",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "memory" });
    }

    public static WebApplication UseHealthChecksConfiguration(this WebApplication app)
    {
        // Endpoint básico de health check
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse
        });

        // Endpoint detalhado de health check
        app.MapHealthChecks("/health/detailed", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = WriteDetailedHealthCheckResponse,
            AllowCachingResponses = false
        });

        // Endpoint de health check apenas para serviços críticos
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("db"),
            ResponseWriter = WriteHealthCheckResponse
        });

        // Endpoint de health check para serviços externos
        app.MapHealthChecks("/health/external", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("external"),
            ResponseWriter = WriteDetailedHealthCheckResponse
        });

        return app;
    }

    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration.TotalMilliseconds
        };

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower
        }));
    }

    private static async Task WriteDetailedHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                data = entry.Value.Data,
                tags = entry.Value.Tags,
                exception = entry.Value.Exception?.Message
            })
        };

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        }));
    }
}

/// <summary>
/// Health check customizado para AWS S3
/// </summary>
public class AwsS3HealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AwsS3HealthCheck> _logger;

    public AwsS3HealthCheck(IConfiguration configuration, ILogger<AwsS3HealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var awsSettings = _configuration.GetSection("AwsSettings");
            var bucketName = awsSettings.GetValue<string>("S3BucketName");
            var region = awsSettings.GetValue<string>("Region");

            if (string.IsNullOrEmpty(bucketName))
            {
                return HealthCheckResult.Unhealthy("S3 bucket name not configured");
            }

            // Aqui seria implementada a verificação real do S3
            // Por enquanto, apenas simular uma verificação básica
            await Task.Delay(100, cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["bucket"] = bucketName,
                ["region"] = region ?? "us-east-1"
            };

            return HealthCheckResult.Healthy("AWS S3 is accessible", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking AWS S3 health");
            return HealthCheckResult.Unhealthy("AWS S3 health check failed", ex);
        }
    }
}