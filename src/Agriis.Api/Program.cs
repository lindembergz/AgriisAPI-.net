using Serilog;
using Agriis.Api.Configuration;
using Agriis.Api.Middleware;
using Agriis.Referencias.Aplicacao.Interfaces;
using Agriis.Referencias.Aplicacao.Servicos;
using Agriis.Referencias.Dominio.Interfaces;
using Agriis.Referencias.Infraestrutura.Repositorios;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with structured logging - Comentado temporariamente
// builder.Host.ConfigureSerilogLogging();

// Configure Database
builder.Services.AddDatabaseConfiguration(builder.Configuration, builder.Environment);

// Configure Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

// Configure Modules essenciais
builder.Services.AddAutenticacaoServices();
builder.Services.AddUsuariosModule();

builder.Services.AddScoped<IMoedaRepository, MoedaRepository>();
builder.Services.AddScoped<IMoedaService, MoedaService>();

builder.Services.AddScoped<IAtividadeAgropecuariaService, AtividadeAgropecuariaService>();
builder.Services.AddScoped<IAtividadeAgropecuariaRepository, AtividadeAgropecuariaRepository>();

builder.Services.AddScoped<IUnidadeMedidaService, UnidadeMedidaService>();
builder.Services.AddScoped<IUnidadeMedidaRepository, UnidadeMedidaRepository>();

builder.Services.AddScoped<IEmbalagemService, EmbalagemService>();
builder.Services.AddScoped<IEmbalagemRepository, EmbalagemRepository>();

// Outros módulos - Reabilitar gradualmente conforme necessário
builder.Services.AddEnderecosModule();
 builder.Services.AddReferenciasModule(); // Reabilitado para resolver dependências críticas
builder.Services.AddCulturasModule();
builder.Services.AddProdutoresModule();
builder.Services.AddPropriedadesModule();



// Módulos temporariamente desabilitados devido à dependência do módulo Referencias
 builder.Services.AddFornecedoresModule();
// builder.Services.AddPontosDistribuicaoModule();
builder.Services.AddSafrasModule();
builder.Services.AddCatalogosModule();
// builder.Services.AddPagamentosModule();
 builder.Services.AddProdutosModule();
// builder.Services.AddSegmentacoesModule();
// builder.Services.AddPedidosModule();
// builder.Services.AddCombosModule();

// Configure AutoMapper
builder.Services.AddAutoMapperConfiguration();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();



// Configure CORS
builder.Services.AddCorsConfiguration(builder.Configuration, builder.Environment);

// Configure Health Checks
//builder.Services.AddHealthChecksConfiguration(builder.Configuration);

// Configure External Integrations
//builder.Services.AddExternalIntegrations(builder.Configuration);

// Configure Logging - Comentado temporariamente
// builder.Services.AddLoggingConfiguration(builder.Configuration, builder.Environment);

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

// Configure the HTTP request pipeline - Configuração mínima que funcionava

app.UseHttpsRedirection();

// Configure Swagger/OpenAPI
app.UseSwaggerConfiguration();

// Use CORS
var corsPolicy = CorsConfiguration.GetPolicyName(app.Environment);
app.UseCors(corsPolicy);

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Configure Health Checks
//app.UseHealthChecksConfiguration();

// Configure External Integrations
//app.ConfigureExternalIntegrations();

// Basic API info endpoint
app.MapGet("/", () => new
{
    Application = "Agriis API",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow
})
.WithName("GetApiInfo");



try
{
    Console.WriteLine("Starting Agriis API");
    
    // Apply database migrations and seed data
    // await app.ApplyDatabaseMigrationsAsync(); // Aplicar manualmente via CLI
    // await app.SeedDatabaseAsync(); // Dados iniciais
    
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application terminated unexpectedly: {ex}");
}

// Make Program class accessible for testing
public partial class Program { }
