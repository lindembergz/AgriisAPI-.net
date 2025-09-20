using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Combos.Dominio.Entidades;

namespace Agriis.Combos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração da entidade ComboItem para Entity Framework
/// </summary>
public class ComboItemConfiguration : IEntityTypeConfiguration<ComboItem>
{
    public void Configure(EntityTypeBuilder<ComboItem> builder)
    {
        builder.ToTable("ComboItem");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(ci => ci.ComboId)
            .HasColumnName("ComboId")
            .IsRequired();

        builder.Property(ci => ci.ProdutoId)
            .HasColumnName("ProdutoId")
            .IsRequired();

        builder.Property(ci => ci.Quantidade)
            .HasColumnName("Quantidade")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(ci => ci.PrecoUnitario)
            .HasColumnName("PrecoUnitario")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(ci => ci.PercentualDesconto)
            .HasColumnName("PercentualDesconto")
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(ci => ci.ProdutoObrigatorio)
            .HasColumnName("ProdutoObrigatorio")
            .IsRequired();

        builder.Property(ci => ci.Ordem)
            .HasColumnName("Ordem")
            .IsRequired();

        builder.Property(ci => ci.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(ci => ci.DataAtualizacao)
            .HasColumnName("DataAtualizacao")
            .HasColumnType("timestamp with time zone");

        // Relacionamentos
        builder.HasOne(ci => ci.Combo)
            .WithMany(c => c.Itens)
            .HasForeignKey(ci => ci.ComboId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(ci => ci.ComboId)
            .HasDatabaseName("IX_ComboItem_ComboId");

        builder.HasIndex(ci => ci.ProdutoId)
            .HasDatabaseName("IX_ComboItem_ProdutoId");

        builder.HasIndex(ci => new { ci.ComboId, ci.Ordem })
            .HasDatabaseName("IX_ComboItem_ComboOrdem");
    }
}