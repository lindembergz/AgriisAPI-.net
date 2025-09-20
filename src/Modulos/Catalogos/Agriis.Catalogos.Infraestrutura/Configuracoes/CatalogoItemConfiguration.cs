using Agriis.Catalogos.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Catalogos.Infraestrutura.Configuracoes;

public class CatalogoItemConfiguration : IEntityTypeConfiguration<CatalogoItem>
{
    public void Configure(EntityTypeBuilder<CatalogoItem> builder)
    {
        builder.ToTable("CatalogoItem");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(ci => ci.CatalogoId)
            .HasColumnName("CatalogoId")
            .IsRequired();

        builder.Property(ci => ci.ProdutoId)
            .HasColumnName("ProdutoId")
            .IsRequired();

        builder.Property(ci => ci.EstruturaPrecosJson)
            .HasColumnName("EstruturaPrecosJson")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(ci => ci.PrecoBase)
            .HasColumnName("PrecoBase")
            .HasColumnType("decimal(18,2)");

        builder.Property(ci => ci.Ativo)
            .HasColumnName("Ativo")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ci => ci.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.Property(ci => ci.DataAtualizacao)
            .HasColumnName("DataAtualizacao")
            .HasColumnType("timestamp without time zone");

        // Relacionamentos
        builder.HasOne(ci => ci.Catalogo)
            .WithMany(c => c.Itens)
            .HasForeignKey(ci => ci.CatalogoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(ci => new { ci.CatalogoId, ci.ProdutoId })
            .IsUnique()
            .HasDatabaseName("IX_CatalogoItem_CatalogoProduto");

        builder.HasIndex(ci => ci.CatalogoId)
            .HasDatabaseName("IX_CatalogoItem_CatalogoId");

        builder.HasIndex(ci => ci.ProdutoId)
            .HasDatabaseName("IX_CatalogoItem_ProdutoId");

        builder.HasIndex(ci => ci.Ativo)
            .HasDatabaseName("IX_CatalogoItem_Ativo");
    }
}