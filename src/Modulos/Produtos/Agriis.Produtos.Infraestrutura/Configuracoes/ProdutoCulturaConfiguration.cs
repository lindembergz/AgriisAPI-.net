using Agriis.Produtos.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Produtos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade ProdutoCultura
/// </summary>
public class ProdutoCulturaConfiguration : IEntityTypeConfiguration<ProdutoCultura>
{
    public void Configure(EntityTypeBuilder<ProdutoCultura> builder)
    {
        // Tabela
        builder.ToTable("ProdutosCulturas");

        // Chave primária
        builder.HasKey(pc => pc.Id);

        // Propriedades básicas
        builder.Property(pc => pc.ProdutoId)
            .IsRequired();

        builder.Property(pc => pc.CulturaId)
            .IsRequired();

        builder.Property(pc => pc.Ativo)
            .IsRequired();

        builder.Property(pc => pc.Observacoes)
            .HasMaxLength(500);

        // Propriedades de auditoria
        builder.Property(pc => pc.DataCriacao)
            .IsRequired();

        builder.Property(pc => pc.DataAtualizacao);

        // Relacionamentos
        builder.HasOne(pc => pc.Produto)
            .WithMany(p => p.ProdutosCulturas)
            .HasForeignKey(pc => pc.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(pc => new { pc.ProdutoId, pc.CulturaId })
            .IsUnique()
            .HasDatabaseName("IX_ProdutosCulturas_ProdutoId_CulturaId");

        builder.HasIndex(pc => pc.ProdutoId)
            .HasDatabaseName("IX_ProdutosCulturas_ProdutoId");

        builder.HasIndex(pc => pc.CulturaId)
            .HasDatabaseName("IX_ProdutosCulturas_CulturaId");

        builder.HasIndex(pc => pc.Ativo)
            .HasDatabaseName("IX_ProdutosCulturas_Ativo");
    }
}