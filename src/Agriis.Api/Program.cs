using Serilog;
using Agriis.Api.Configuration;
using Agriis.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with structured logging
builder.Host.ConfigureSerilogLogging();

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

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerConfiguration();

// Configure CORS
builder.Services.AddCorsConfiguration(builder.Configuration, builder.Environment);

// Configure Health Checks
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

// Configure External Integrations
builder.Services.AddExternalIntegrations(builder.Configuration);

// Configure Logging
builder.Services.AddLoggingConfiguration(builder.Configuration, builder.Environment);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline

// Global Exception Handling (deve ser o primeiro middleware)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Request Logging (antes de outros middlewares)
app.UseRequestLogging();

// Configure Swagger/OpenAPI
app.UseSwaggerConfiguration();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use Serilog for request logging (após o middleware customizado)
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// Use CORS (deve vir antes da autenticação)
var corsPolicy = CorsConfiguration.GetPolicyName(app.Environment);
app.UseCors(corsPolicy);

// Use Authentication & Authorization
app.UseAuthentication();
app.UseMiddleware<JwtAuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllers();

// Configure Health Checks
app.UseHealthChecksConfiguration();

// Configure External Integrations
app.ConfigureExternalIntegrations();

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
