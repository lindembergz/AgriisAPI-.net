using Microsoft.Extensions.Configuration;

namespace Agriis.Tests.Shared.Configuration;

/// <summary>
/// Configurações específicas para testes
/// </summary>
public static class TestConfiguration
{
    /// <summary>
    /// Configuração padrão para testes
    /// </summary>
    public static IConfiguration GetTestConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(GetDefaultTestSettings())
            .Build();

        return configuration;
    }

    /// <summary>
    /// Configurações padrão para testes
    /// </summary>
    private static Dictionary<string, string?> GetDefaultTestSettings()
    {
        return new Dictionary<string, string?>
        {
            // Banco de dados
            ["ConnectionStrings:DefaultConnection"] = "InMemory",
            
            // JWT
            ["Jwt:Key"] = "test-key-with-at-least-32-characters-for-security",
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:ExpireMinutes"] = "60",
            ["Jwt:RefreshExpireDays"] = "7",
            
            // Logging
            ["Logging:LogLevel:Default"] = "Warning",
            ["Logging:LogLevel:Microsoft"] = "Warning",
            ["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Information",
            
            // CORS
            ["Cors:AllowedOrigins:0"] = "http://localhost:3000",
            ["Cors:AllowedOrigins:1"] = "https://localhost:3000",
            
            // Integrations (valores de teste)
            ["Integrations:Serpro:BaseUrl"] = "https://api-test.serpro.gov.br",
            ["Integrations:Serpro:Token"] = "test-token",
            ["Integrations:Serpro:Enabled"] = "false", // Desabilitado em testes
            
            ["Integrations:Aws:AccessKey"] = "test-access-key",
            ["Integrations:Aws:SecretKey"] = "test-secret-key",
            ["Integrations:Aws:Region"] = "us-east-1",
            ["Integrations:Aws:S3BucketName"] = "test-bucket",
            ["Integrations:Aws:Enabled"] = "false", // Desabilitado em testes
            
            ["Integrations:CurrencyConverter:ApiKey"] = "test-api-key",
            ["Integrations:CurrencyConverter:BaseUrl"] = "https://api.test.com",
            ["Integrations:CurrencyConverter:Enabled"] = "false", // Desabilitado em testes
            
            // SignalR
            ["SignalR:Enabled"] = "false", // Desabilitado em testes
            
            // Background Services
            ["BackgroundServices:Enabled"] = "false", // Desabilitado em testes
            
            // Health Checks
            ["HealthChecks:Enabled"] = "false", // Desabilitado em testes
            
            // Swagger
            ["Swagger:Enabled"] = "false", // Desabilitado em testes
            
            // Cache
            ["Cache:Enabled"] = "false", // Desabilitado em testes
            
            // Rate Limiting
            ["RateLimit:Enabled"] = "false", // Desabilitado em testes
            
            // Validation
            ["Validation:StrictMode"] = "true", // Habilitado em testes para detectar problemas
            
            // Test specific settings
            ["Testing:DatabaseName"] = "AgriisTestDb",
            ["Testing:ResetDatabaseBetweenTests"] = "true",
            ["Testing:EnableDetailedErrors"] = "true",
            ["Testing:EnableSensitiveDataLogging"] = "true"
        };
    }

    /// <summary>
    /// Configuração para testes de integração
    /// </summary>
    public static IConfiguration GetIntegrationTestConfiguration()
    {
        var baseConfig = GetDefaultTestSettings();
        
        // Sobrescreve configurações específicas para testes de integração
        baseConfig["Logging:LogLevel:Default"] = "Information";
        baseConfig["Testing:EnableDetailedErrors"] = "true";
        baseConfig["Testing:EnableSensitiveDataLogging"] = "true";
        
        return new ConfigurationBuilder()
            .AddInMemoryCollection(baseConfig)
            .Build();
    }

    /// <summary>
    /// Configuração para testes unitários
    /// </summary>
    public static IConfiguration GetUnitTestConfiguration()
    {
        var baseConfig = GetDefaultTestSettings();
        
        // Configurações mínimas para testes unitários
        baseConfig["Logging:LogLevel:Default"] = "Critical";
        baseConfig["Testing:ResetDatabaseBetweenTests"] = "false";
        
        return new ConfigurationBuilder()
            .AddInMemoryCollection(baseConfig)
            .Build();
    }

    /// <summary>
    /// Configuração customizada
    /// </summary>
    public static IConfiguration GetCustomConfiguration(Dictionary<string, string?> customSettings)
    {
        var baseConfig = GetDefaultTestSettings();
        
        // Aplica configurações customizadas
        foreach (var setting in customSettings)
        {
            baseConfig[setting.Key] = setting.Value;
        }
        
        return new ConfigurationBuilder()
            .AddInMemoryCollection(baseConfig)
            .Build();
    }
}