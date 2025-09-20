using Agriis.Api.Middleware;
using Agriis.Compartilhado.Infraestrutura.Logging;
using Serilog;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração do sistema de logging
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Adiciona configuração de logging estruturado
    /// </summary>
    public static IServiceCollection AddLoggingConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IHostEnvironment environment)
    {
        // Configurar opções do middleware de logging
        services.Configure<RequestLoggingOptions>(configuration.GetSection("RequestLogging"));

        // Adicionar logging estruturado
        services.AddStructuredLogging(configuration);

        return services;
    }

    /// <summary>
    /// Configura o Serilog como provedor de logging
    /// </summary>
    public static IHostBuilder ConfigureSerilogLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, configuration) =>
        {
            configuration.ConfigureSerilog(context.Configuration, context.HostingEnvironment);
        });
    }

    /// <summary>
    /// Adiciona middleware de logging de requisições
    /// </summary>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }

    /// <summary>
    /// Configura logging de requisições do Serilog
    /// </summary>
    public static IApplicationBuilder UseSerilogRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "Unknown");
                diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
                
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.FindFirst("user_id")?.Value ?? "Unknown");
                    diagnosticContext.Set("UserEmail", httpContext.User.FindFirst("email")?.Value ?? "Unknown");
                }

                // Adicionar correlation ID se disponível
                if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
                {
                    diagnosticContext.Set("CorrelationId", correlationId.ToString());
                }

                // Adicionar informações de performance
                diagnosticContext.Set("StatusCode", httpContext.Response.StatusCode);
                
                // Marcar requisições lentas
                if (httpContext.Items.TryGetValue("RequestStartTime", out var startTimeObj) && 
                    startTimeObj is DateTime startTime)
                {
                    var elapsed = DateTime.UtcNow - startTime;
                    if (elapsed.TotalMilliseconds > 1000)
                    {
                        diagnosticContext.Set("SlowRequest", true);
                    }
                }
            };
        });
    }
}