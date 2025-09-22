using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Agriis.Api;
using Agriis.Api.Contexto;
using Microsoft.Extensions.Configuration;

namespace Agriis.Tests.Shared.Base;

/// <summary>
/// Factory para criar aplicação de teste com banco em memória
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove o DbContext real
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AgriisDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Adiciona banco em memória para testes
            services.AddDbContext<AgriisDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // Configura logging para testes
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
            });

            // Remove serviços de background para testes
            var backgroundServices = services.Where(s => 
                s.ImplementationType?.Name.Contains("BackgroundService") == true ||
                s.ImplementationType?.Name.Contains("HostedService") == true)
                .ToList();
            
            foreach (var service in backgroundServices)
            {
                services.Remove(service);
            }
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Usa configuração de teste
            config.AddJsonFile("appsettings.Testing.json", optional: true);
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "InMemory",
                ["Jwt:Key"] = "test-key-with-at-least-32-characters-for-security",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:ExpireMinutes"] = "60",
                ["Logging:LogLevel:Default"] = "Warning"
            });
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Cria um escopo de serviços para testes
    /// </summary>
    public IServiceScope CreateScope()
    {
        return Services.CreateScope();
    }

    /// <summary>
    /// Obtém um serviço do container de DI
    /// </summary>
    public T GetService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }

    /// <summary>
    /// Obtém o contexto do banco de dados
    /// </summary>
    public AgriisDbContext GetDbContext()
    {
        var scope = CreateScope();
        return scope.ServiceProvider.GetRequiredService<AgriisDbContext>();
    }

    /// <summary>
    /// Garante que o banco de dados está criado
    /// </summary>
    public async Task EnsureDatabaseCreatedAsync()
    {
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AgriisDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Limpa o banco de dados
    /// </summary>
    public async Task ClearDatabaseAsync()
    {
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AgriisDbContext>();
        
        // Para banco em memória, é mais fácil recriar
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }
}