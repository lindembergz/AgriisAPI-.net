using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Culturas.Dominio.Entidades;

namespace Agriis.Culturas.Infraestrutura.Configuracoes;

public class CulturaConfiguration : IEntityTypeConfiguration<Cultura>
{
    public void Configure(EntityTypeBuilder<Cultura> builder)
    {
        builder.ToTable("Cultura");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Nome)
            .HasColumnName("Nome")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Descricao)
            .HasColumnName("Descricao")
            .HasMaxLength(500);

        builder.Property(c => c.Ativo)
            .HasColumnName("Ativo")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(c => c.DataAtualizacao)
            .HasColumnName("DataAtualizacao")
            .HasColumnType("timestamptz");

        // Índices
        builder.HasIndex(c => c.Nome)
            .HasDatabaseName("IX_Cultura_Nome")
            .IsUnique();

        builder.HasIndex(c => c.Ativo)
            .HasDatabaseName("IX_Cultura_Ativo");
    }
}