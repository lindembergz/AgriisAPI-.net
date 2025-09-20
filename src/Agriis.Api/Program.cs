using Serilog;
using Agriis.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Configure Database
builder.Services.AddDatabaseConfiguration(builder.Configuration, builder.Environment);

// Configure Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

// Configure Modules
builder.Services.AddEnderecosModule();
builder.Services.AddUsuariosModule();
builder.Services.AddAutenticacaoServices();
builder.Services.AddCulturasModule();
builder.Services.AddProdutoresModule();
builder.Services.AddPropriedadesModule();
builder.Services.AddFornecedoresModule();
builder.Services.AddPontosDistribuicaoModule();
builder.Services.AddSafrasModule();
builder.Services.AddCatalogosModule();
builder.Services.AddPagamentosModule();
builder.Services.AddPedidosModule();
builder.Services.AddCombosModule();

// Configure AutoMapper
builder.Services.AddAutoMapperConfiguration();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var corsSettings = builder.Configuration.GetSection("CorsSettings");
        var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "*" };
        var allowedMethods = corsSettings.GetSection("AllowedMethods").Get<string[]>() ?? new[] { "*" };
        var allowedHeaders = corsSettings.GetSection("AllowedHeaders").Get<string[]>() ?? new[] { "*" };
        var allowCredentials = corsSettings.GetValue<bool>("AllowCredentials");

        policy.WithOrigins(allowedOrigins)
              .WithMethods(allowedMethods)
              .WithHeaders(allowedHeaders);

        if (allowCredentials)
            policy.AllowCredentials();
    });
});

// Add Health Checks
builder.Services.AddDatabaseHealthChecks(builder.Configuration);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

// Use Serilog for request logging
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// Use CORS
app.UseCors();

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map Health Checks
app.MapHealthChecks("/health");

// Basic API info endpoint
app.MapGet("/", () => new
{
    Application = "Agriis API",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow
})
.WithName("GetApiInfo")
.WithOpenApi();

try
{
    Log.Information("Starting Agriis API");
    
    // Apply database migrations and seed data
    await app.ApplyDatabaseMigrationsAsync();
    await app.SeedDatabaseAsync();
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
