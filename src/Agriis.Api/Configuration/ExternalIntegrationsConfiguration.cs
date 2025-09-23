using Agriis.Compartilhado.Infraestrutura.Integracoes;
using Amazon.S3;
using Microsoft.AspNetCore.SignalR;

namespace Agriis.Api.Configuration;

public static class ExternalIntegrationsConfiguration
{
    public static IServiceCollection AddExternalIntegrations(this IServiceCollection services, IConfiguration configuration)
    {
        // Memory Cache (required by CurrencyConverterService)
        services.AddMemoryCache();

        // AWS Services
        services.AddAWSService<IAmazonS3>();
        services.AddScoped<IAwsService, AwsService>();

        // SignalR
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        });

        // Notification Service
        services.AddScoped<INotificationService, NotificationService>();

        // Currency Converter
        services.AddHttpClient<ICurrencyConverterService, CurrencyConverterService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "Agriis-API/1.0");
        });
        services.AddScoped<ICurrencyConverterService, CurrencyConverterService>();

        // Background Services
        services.AddHostedService<CurrencyRateUpdateService>();

        return services;
    }

    public static WebApplication ConfigureExternalIntegrations(this WebApplication app)
    {
        // SignalR Hub
        app.MapHub<NotificationHub>("/hubs/notifications");

        return app;
    }
}