using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Fornecedores.Dominio.Entidades;

namespace Agriis.Fornecedores.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade UsuarioFornecedorTerritorio
/// </summary>
public class UsuarioFornecedorTerritorioConfiguration : IEntityTypeConfiguration<UsuarioFornecedorTerritorio>
{
    public void Configure(EntityTypeBuilder<UsuarioFornecedorTerritorio> builder)
    {
        // Tabela
        builder.ToTable("UsuarioFornecedorTerritorio");

        // Chave primária
        builder.HasKey(t => t.Id);

        // Propriedades básicas
        builder.Property(t => t.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.UsuarioFornecedorId)
            .HasColumnName("UsuarioFornecedorId")
            .IsRequired();

        builder.Property(t => t.TerritorioPadrao)
            .HasColumnName("TerritorioPadrao")
            .IsRequired();

        builder.Property(t => t.Ativo)
            .HasColumnName("Ativo")
            .IsRequired();

        builder.Property(t => t.DataCriacao)
            .HasColumnName("DataCriacao")
            .IsRequired();

        builder.Property(t => t.DataAtualizacao)
            .HasColumnName("DataAtualizacao");

        // Propriedades JSON
        builder.Property(t => t.Estados)
            .HasColumnName("Estados")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(t => t.Municipios)
            .HasColumnName("Municipios")
            .HasColumnType("jsonb");

        // Relacionamentos
        builder.HasOne(t => t.UsuarioFornecedor)
            .WithMany(uf => uf.Territorios)
            .HasForeignKey(t => t.UsuarioFornecedorId)
            .OnDelete(DeleteBehavior.Cascade);   
     // Índices
        builder.HasIndex(t => t.UsuarioFornecedorId)
            .HasDatabaseName("IX_UsuarioFornecedorTerritorio_UsuarioFornecedorId");

        builder.HasIndex(t => t.TerritorioPadrao)
            .HasDatabaseName("IX_UsuarioFornecedorTerritorio_TerritorioPadrao");

        builder.HasIndex(t => t.Ativo)
            .HasDatabaseName("IX_UsuarioFornecedorTerritorio_Ativo");

        builder.HasIndex(t => t.DataCriacao)
            .HasDatabaseName("IX_UsuarioFornecedorTerritorio_DataCriacao");

        // Índices GIN para consultas JSON
        builder.HasIndex(t => t.Estados)
            .HasDatabaseName("IX_UsuarioFornecedorTerritorio_Estados_GIN")
            .HasMethod("gin");

        builder.HasIndex(t => t.Municipios)
            .HasDatabaseName("IX_UsuarioFornecedorTerritorio_Municipios_GIN")
            .HasMethod("gin")
            .HasFilter("\"Municipios\" IS NOT NULL");

        // Configurações adicionais
        builder.Navigation(t => t.UsuarioFornecedor)
            .EnableLazyLoading(false);
    }
}