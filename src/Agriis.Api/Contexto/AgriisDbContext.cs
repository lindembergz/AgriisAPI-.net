using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Enderecos.Dominio.Entidades;
using Agriis.Usuarios.Dominio.Entidades;
using Agriis.Autenticacao.Dominio.Entidades;
using Agriis.Culturas.Dominio.Entidades;
using Agriis.Produtores.Dominio.Entidades;
using Agriis.Propriedades.Dominio.Entidades;
using Agriis.Fornecedores.Dominio.Entidades;
using Agriis.PontosDistribuicao.Dominio.Entidades;
using Agriis.Safras.Dominio.Entidades;
using Agriis.Catalogos.Dominio.Entidades;
using Agriis.Pagamentos.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Combos.Dominio.Entidades;

namespace Agriis.Api.Contexto;

public class AgriisDbContext : DbContext
{
    public AgriisDbContext(DbContextOptions<AgriisDbContext> options) : base(options)
    {
    }

    // DbSets dos módulos implementados
    
    // Módulo de Endereços
    public DbSet<Estado> Estados { get; set; }
    public DbSet<Municipio> Municipios { get; set; }
    public DbSet<Endereco> Enderecos { get; set; }
    
    // Módulo de Usuários
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<UsuarioRole> UsuarioRoles { get; set; }
    
    // Módulo de Autenticação
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    
    // Módulo de Culturas
    public DbSet<Cultura> Culturas { get; set; }
    
    // Módulo de Produtores
    public DbSet<Produtor> Produtores { get; set; }
    public DbSet<UsuarioProdutor> UsuariosProdutores { get; set; }
    
    // Módulo de Propriedades
    public DbSet<Propriedade> Propriedades { get; set; }
    public DbSet<Talhao> Talhoes { get; set; }
    public DbSet<PropriedadeCultura> PropriedadeCulturas { get; set; }
    
    // Módulo de Fornecedores
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<UsuarioFornecedor> UsuariosFornecedores { get; set; }
    public DbSet<UsuarioFornecedorTerritorio> UsuariosFornecedoresTerritorios { get; set; }
    
    // Módulo de Pontos de Distribuição
    public DbSet<PontoDistribuicao> PontosDistribuicao { get; set; }
    
    // Módulo de Safras
    public DbSet<Safra> Safras { get; set; }
    
    // Módulo de Catálogos
    public DbSet<Catalogo> Catalogos { get; set; }
    public DbSet<CatalogoItem> CatalogoItens { get; set; }
    
    // Módulo de Pagamentos
    public DbSet<FormaPagamento> FormasPagamento { get; set; }
    public DbSet<CulturaFormaPagamento> CulturaFormasPagamento { get; set; }
    
    // Módulo de Pedidos
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<PedidoItem> PedidoItens { get; set; }
    public DbSet<PedidoItemTransporte> PedidoItensTransporte { get; set; }
    public DbSet<Proposta> Propostas { get; set; }
    
    // Módulo de Combos
    public DbSet<Combo> Combos { get; set; }
    public DbSet<ComboItem> ComboItens { get; set; }
    public DbSet<ComboLocalRecebimento> ComboLocaisRecebimento { get; set; }
    public DbSet<ComboCategoriaDesconto> ComboCategoriasDesconto { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar schema padrão
        modelBuilder.HasDefaultSchema("public");

        // Configurar conversores globais para JSON
        ConfigurarConversoresJson(modelBuilder);

        // Configurar auditoria automática
        ConfigurarAuditoria(modelBuilder);
        
        // Ignorar objetos de valor como entidades
        ConfigurarObjetosValor(modelBuilder);

        // Aplicar configurações de entidades de todos os módulos
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgriisDbContext).Assembly);
        
        // Aplicar configurações dos módulos específicos
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Agriis.Usuarios.Infraestrutura.Configuracoes.UsuarioConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Agriis.Enderecos.Infraestrutura.Configuracoes.EstadoConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Agriis.Culturas.Infraestrutura.Configuracoes.CulturaConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Agriis.Produtores.Infraestrutura.Configuracoes.ProdutorConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Agriis.Propriedades.Infraestrutura.Configuracoes.PropriedadeConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Agriis.Fornecedores.Infraestrutura.Configuracoes.FornecedorConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Agriis.PontosDistribuicao.Infraestrutura.Configuracoes.PontoDistribuicaoConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Agriis.Safras.Infraestrutura.Configuracoes.SafraConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Agriis.Catalogos.Infraestrutura.Configuracoes.CatalogoConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Agriis.Pagamentos.Infraestrutura.Configuracoes.FormaPagamentoConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Agriis.Pedidos.Infraestrutura.Configuracoes.PedidoConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Agriis.Combos.Infraestrutura.Configuracoes.ComboConfiguration).Assembly);
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
            
            // Habilitar extensão PostGIS para suporte geográfico
            options.UseNetTopologySuite();
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

    private void ConfigurarObjetosValor(ModelBuilder modelBuilder)
    {
        // Ignorar objetos de valor para que não sejam tratados como entidades
        modelBuilder.Ignore<Agriis.Compartilhado.Dominio.ObjetosValor.Cpf>();
        modelBuilder.Ignore<Agriis.Compartilhado.Dominio.ObjetosValor.Cnpj>();
        modelBuilder.Ignore<Agriis.Compartilhado.Dominio.ObjetosValor.AreaPlantio>();
        modelBuilder.Ignore<Agriis.Produtos.Dominio.ObjetosValor.DimensoesProduto>();
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