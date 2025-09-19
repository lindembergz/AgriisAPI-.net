using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Api.Contexto;

public class AgriisDbContext : DbContext
{
    public AgriisDbContext(DbContextOptions<AgriisDbContext> options) : base(options)
    {
    }

    // DbSets serão adicionados conforme os módulos forem implementados
    // Exemplo: public DbSet<Produtor> Produtores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar schema padrão
        modelBuilder.HasDefaultSchema("public");

        // Configurar conversores globais para JSON
        ConfigurarConversoresJson(modelBuilder);

        // Configurar auditoria automática
        ConfigurarAuditoria(modelBuilder);

        // Aplicar configurações de entidades de todos os módulos
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgriisDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Configuração de fallback - normalmente será configurado via DI
            optionsBuilder.UseNpgsql("Host=localhost;Database=DBAgriis;Username=postgres;Password=RootPassword123;Port=5432");
        }

        // Configurações específicas do PostgreSQL
        optionsBuilder.UseNpgsql(options =>
        {
            options.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        });

        // Configurações de desenvolvimento
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Aplicar auditoria automática antes de salvar
        AplicarAuditoriaAutomatica();
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        // Aplicar auditoria automática antes de salvar
        AplicarAuditoriaAutomatica();
        
        return base.SaveChanges();
    }

    private void ConfigurarConversoresJson(ModelBuilder modelBuilder)
    {
        // Configurar conversores para tipos JSON que serão usados em múltiplas entidades
        // Exemplo: para campos que armazenam listas de IDs como JSON
        
        // Conversores JSON serão configurados nas configurações específicas de cada entidade
        // quando necessário, para evitar problemas com expression trees

        // Estes conversores serão aplicados nas configurações específicas de cada entidade
    }

    private void ConfigurarAuditoria(ModelBuilder modelBuilder)
    {
        // Configurar propriedades de auditoria para todas as entidades que herdam de EntidadeBase
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(EntidadeBase).IsAssignableFrom(entityType.ClrType))
            {
                // Configurar DataCriacao como obrigatória
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(EntidadeBase.DataCriacao))
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Configurar DataAtualizacao como opcional
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(EntidadeBase.DataAtualizacao))
                    .IsRequired(false);

                // Configurar índices para otimizar consultas por data
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(EntidadeBase.DataCriacao))
                    .HasDatabaseName($"IX_{entityType.GetTableName()}_DataCriacao");
            }
        }
    }

    private void AplicarAuditoriaAutomatica()
    {
        var entries = ChangeTracker.Entries<EntidadeBase>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetDataCriacao(DateTime.UtcNow);
                    break;

                case EntityState.Modified:
                    entry.Entity.AtualizarDataModificacao();
                    break;
            }
        }
    }
}