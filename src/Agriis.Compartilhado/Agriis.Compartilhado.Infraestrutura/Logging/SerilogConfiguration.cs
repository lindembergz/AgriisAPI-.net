using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using Serilog.Sinks.PostgreSQL;

namespace Agriis.Compartilhado.Infraestrutura.Logging;

/// <summary>
/// Configuração centralizada do Serilog para logging estruturado
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configura o Serilog com múltiplos destinos e structured logging
    /// </summary>
    public static LoggerConfiguration ConfigureSerilog(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var loggerConfig = loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("Application", "Agriis.Api")
            .Enrich.WithProperty("Version", GetApplicationVersion());

        // Configurar níveis de log por ambiente
        ConfigureLogLevels(loggerConfig, environment);

        // Configurar destinos de log
        ConfigureLogSinks(loggerConfig, configuration, environment);

        // Configurar filtros
        ConfigureLogFilters(loggerConfig);

        return loggerConfig;
    }

    /// <summary>
    /// Configura os níveis de log baseado no ambiente
    /// </summary>
    private static void ConfigureLogLevels(LoggerConfiguration loggerConfig, IHostEnvironment environment)
    {
        var minimumLevel = environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information;

        loggerConfig.MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", environment.IsDevelopment() ? LogEventLevel.Information : LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Hangfire", LogEventLevel.Information);
    }

    /// <summary>
    /// Configura os destinos de log (sinks)
    /// </summary>
    private static void ConfigureLogSinks(LoggerConfiguration loggerConfig, IConfiguration configuration, IHostEnvironment environment)
    {
        // Console Sink - sempre habilitado
        ConfigureConsoleSink(loggerConfig, environment);

        // File Sink - sempre habilitado
        ConfigureFileSink(loggerConfig, environment);

        // PostgreSQL Sink - apenas em produção ou quando configurado
        ConfigurePostgreSqlSink(loggerConfig, configuration, environment);

        // Seq Sink - para desenvolvimento local (opcional)
        ConfigureSeqSink(loggerConfig, configuration, environment);
    }

    /// <summary>
    /// Configura o sink do console
    /// </summary>
    private static void ConfigureConsoleSink(LoggerConfiguration loggerConfig, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            loggerConfig.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Debug);
        }
        else
        {
            // Em produção, usar formato JSON compacto para melhor parsing
            loggerConfig.WriteTo.Console(
                formatter: new CompactJsonFormatter(),
                restrictedToMinimumLevel: LogEventLevel.Information);
        }
    }

    /// <summary>
    /// Configura o sink de arquivo
    /// </summary>
    private static void ConfigureFileSink(LoggerConfiguration loggerConfig, IHostEnvironment environment)
    {
        var logPath = environment.IsDevelopment() ? "logs/agriis-.log" : "/app/logs/agriis-.log";
        
        loggerConfig.WriteTo.File(
            path: logPath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: environment.IsDevelopment() ? 7 : 30,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}",
            restrictedToMinimumLevel: LogEventLevel.Information,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1));

        // Arquivo separado para erros
        loggerConfig.WriteTo.File(
            path: environment.IsDevelopment() ? "logs/agriis-errors-.log" : "/app/logs/agriis-errors-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: environment.IsDevelopment() ? 30 : 90,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}",
            restrictedToMinimumLevel: LogEventLevel.Error,
            shared: true);
    }

    /// <summary>
    /// Configura o sink do PostgreSQL
    /// </summary>
    private static void ConfigurePostgreSqlSink(LoggerConfiguration loggerConfig, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("LoggingConnection") 
                              ?? configuration.GetConnectionString("DefaultConnection");

        if (!string.IsNullOrEmpty(connectionString) && !environment.IsDevelopment())
        {
            var columnOptions = new Dictionary<string, ColumnWriterBase>
            {
                { "message", new RenderedMessageColumnWriter() },
                { "message_template", new MessageTemplateColumnWriter() },
                { "level", new LevelColumnWriter() },
                { "timestamp", new TimestampColumnWriter() },
                { "exception", new ExceptionColumnWriter() },
                { "log_event", new LogEventSerializedColumnWriter() },
                { "properties", new PropertiesColumnWriter() },
                { "machine_name", new SinglePropertyColumnWriter("MachineName") },
                { "environment", new SinglePropertyColumnWriter("EnvironmentName") },
                { "application", new SinglePropertyColumnWriter("Application") },
                { "version", new SinglePropertyColumnWriter("Version") },
                { "correlation_id", new SinglePropertyColumnWriter("CorrelationId") },
                { "user_id", new SinglePropertyColumnWriter("UserId") },
                { "user_email", new SinglePropertyColumnWriter("UserEmail") },
                { "request_path", new SinglePropertyColumnWriter("RequestPath") },
                { "request_method", new SinglePropertyColumnWriter("RequestMethod") },
                { "status_code", new SinglePropertyColumnWriter("StatusCode") },
                { "elapsed_milliseconds", new SinglePropertyColumnWriter("ElapsedMilliseconds") }
            };

            loggerConfig.WriteTo.PostgreSQL(
                connectionString: connectionString,
                tableName: "logs",
                columnOptions: columnOptions,
                restrictedToMinimumLevel: LogEventLevel.Information,
                needAutoCreateTable: true,
                batchSizeLimit: 50,
                period: TimeSpan.FromSeconds(5));
        }
    }

    /// <summary>
    /// Configura o sink do Seq (opcional para desenvolvimento)
    /// </summary>
    private static void ConfigureSeqSink(LoggerConfiguration loggerConfig, IConfiguration configuration, IHostEnvironment environment)
    {
        var seqUrl = configuration["Serilog:Seq:ServerUrl"];
        var seqApiKey = configuration["Serilog:Seq:ApiKey"];

        if (!string.IsNullOrEmpty(seqUrl) && environment.IsDevelopment())
        {
            loggerConfig.WriteTo.Seq(
                serverUrl: seqUrl,
                apiKey: seqApiKey,
                restrictedToMinimumLevel: LogEventLevel.Debug);
        }
    }

    /// <summary>
    /// Configura filtros de log
    /// </summary>
    private static void ConfigureLogFilters(LoggerConfiguration loggerConfig)
    {
        // Filtrar logs de health checks para reduzir ruído
        loggerConfig.Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", path => 
            path.Contains("/health", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/metrics", StringComparison.OrdinalIgnoreCase)));

        // Filtrar logs de static files
        loggerConfig.Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", path => 
            path.Contains("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/favicon.ico", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Obtém a versão da aplicação
    /// </summary>
    private static string GetApplicationVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "1.0.0";
        }
        catch
        {
            return "1.0.0";
        }
    }

    /// <summary>
    /// Adiciona configuração de logging estruturado aos serviços
    /// </summary>
    public static IServiceCollection AddStructuredLogging(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar HttpContextAccessor (necessário para os enrichers)
        services.AddHttpContextAccessor();
        
        // Configurar contexto de logging
        services.AddScoped<ILoggingContext, LoggingContext>();
        
        // Configurar enriquecedores customizados
        services.AddSingleton<ILogEventEnricher, CorrelationIdEnricher>();
        services.AddSingleton<ILogEventEnricher, UserContextEnricher>();
        
        // Configurar logger de performance
        services.AddScoped<IPerformanceLogger, PerformanceLogger>();
        
        return services;
    }
}