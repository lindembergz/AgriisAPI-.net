using Agriis.Catalogos.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Catalogos.Infraestrutura.Configuracoes;

public class CatalogoConfiguration : IEntityTypeConfiguration<Catalogo>
{
    public void Configure(EntityTypeBuilder<Catalogo> builder)
    {
        builder.ToTable("Catalogo");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.SafraId)
            .HasColumnName("SafraId")
            .IsRequired();

        builder.Property(c => c.PontoDistribuicaoId)
            .HasColumnName("PontoDistribuicaoId")
            .IsRequired();

        builder.Property(c => c.CulturaId)
            .HasColumnName("CulturaId")
            .IsRequired();

        builder.Property(c => c.CategoriaId)
            .HasColumnName("CategoriaId")
            .IsRequired();

        builder.Property(c => c.Moeda)
            .HasColumnName("Moeda")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(c => c.DataInicio)
            .HasColumnName("DataInicio")
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.Property(c => c.DataFim)
            .HasColumnName("DataFim")
            .HasColumnType("timestamp without time zone");

        builder.Property(c => c.Ativo)
            .HasColumnName("Ativo")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.Property(c => c.DataAtualizacao)
            .HasColumnName("DataAtualizacao")
            .HasColumnType("timestamp without time zone");

        // Relacionamentos
        builder.HasMany(c => c.Itens)
            .WithOne(i => i.Catalogo)
            .HasForeignKey(i => i.CatalogoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(c => new { c.SafraId, c.PontoDistribuicaoId, c.CulturaId, c.CategoriaId })
            .IsUnique()
            .HasDatabaseName("IX_Catalogo_ChaveUnica");

        builder.HasIndex(c => c.SafraId)
            .HasDatabaseName("IX_Catalogo_SafraId");

        builder.HasIndex(c => c.PontoDistribuicaoId)
            .HasDatabaseName("IX_Catalogo_PontoDistribuicaoId");

        builder.HasIndex(c => c.CulturaId)
            .HasDatabaseName("IX_Catalogo_CulturaId");

        builder.HasIndex(c => c.CategoriaId)
            .HasDatabaseName("IX_Catalogo_CategoriaId");

        builder.HasIndex(c => new { c.DataInicio, c.DataFim, c.Ativo })
            .HasDatabaseName("IX_Catalogo_Vigencia");
    }
}