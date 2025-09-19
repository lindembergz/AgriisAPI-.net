using Microsoft.EntityFrameworkCore;
using Agriis.Api.Contexto;

namespace Agriis.Api.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AgriisDbContext>(options =>
        {
            ConfigureNpgsqlOptions(options, connectionString, environment);
        });

        // Registrar Unit of Work
        services.AddScoped<Agriis.Compartilhado.Dominio.Interfaces.IUnitOfWork, 
            Agriis.Compartilhado.Infraestrutura.Persistencia.UnitOfWork<AgriisDbContext>>();

        return services;
    }

    private static void ConfigureNpgsqlOptions(
        DbContextOptionsBuilder options, 
        string connectionString, 
        IWebHostEnvironment environment)
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            // Configurar retry policy
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);

            // Configurar timeout para comandos
            npgsqlOptions.CommandTimeout(30);

            // TODO: Habilitar NetTopologySuite para dados geoespaciais (PostGIS) quando a extensão estiver instalada
            // npgsqlOptions.UseNetTopologySuite();

            // Configurações específicas para produção
            if (environment.IsProduction())
            {
                // Em produção, usar configurações mais restritivas
                npgsqlOptions.CommandTimeout(60); // Timeout maior para produção
            }
        });

        // Configurações específicas por ambiente
        if (environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            options.LogTo(Console.WriteLine, LogLevel.Information);
        }
        else if (environment.IsProduction())
        {
            // Em produção, desabilitar logs sensíveis
            options.EnableServiceProviderCaching();
            options.EnableSensitiveDataLogging(false);
        }
    }

    public static IServiceCollection AddDatabaseHealthChecks(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var healthCheckTimeout = configuration.GetValue<int>("HealthChecks:DatabaseTimeoutInSeconds", 30);

        services.AddHealthChecks()
            .AddNpgSql(
                connectionString,
                name: "postgresql",
                tags: new[] { "db", "sql", "postgresql" },
                timeout: TimeSpan.FromSeconds(healthCheckTimeout));

        return services;
    }

    public static async Task<WebApplication> ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AgriisDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AgriisDbContext>>();

        try
        {
            logger.LogInformation("Verificando se há migrações pendentes...");
            
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Aplicando {Count} migrações pendentes: {Migrations}", 
                    pendingMigrations.Count(), 
                    string.Join(", ", pendingMigrations));
                
                await context.Database.MigrateAsync();
                
                logger.LogInformation("Migrações aplicadas com sucesso");
            }
            else
            {
                logger.LogInformation("Nenhuma migração pendente encontrada");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao aplicar migrações do banco de dados");
            
            // Em desenvolvimento, podemos continuar mesmo com erro de migração
            // Em produção, isso deve parar a aplicação
            if (app.Environment.IsProduction())
            {
                throw;
            }
        }

        return app;
    }

    public static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AgriisDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AgriisDbContext>>();

        try
        {
            logger.LogInformation("Verificando se é necessário popular dados iniciais...");
            
            // Aqui será implementada a lógica de seed quando necessário
            // Por exemplo, criar dados básicos como estados, culturas padrão, etc.
            
            await SeedBasicDataIfNeeded(context, logger);
            
            logger.LogInformation("Verificação de dados iniciais concluída");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao popular dados iniciais");
            
            // Seed não deve parar a aplicação, apenas logar o erro
        }

        return app;
    }

    private static async Task SeedBasicDataIfNeeded(AgriisDbContext context, ILogger logger)
    {
        // Esta função será implementada quando tivermos as entidades criadas
        // Por enquanto, apenas verificar se o banco está acessível
        
        var canConnect = await context.Database.CanConnectAsync();
        
        if (canConnect)
        {
            logger.LogInformation("Conexão com banco de dados estabelecida com sucesso");
        }
        else
        {
            logger.LogWarning("Não foi possível estabelecer conexão com o banco de dados");
        }
    }
}