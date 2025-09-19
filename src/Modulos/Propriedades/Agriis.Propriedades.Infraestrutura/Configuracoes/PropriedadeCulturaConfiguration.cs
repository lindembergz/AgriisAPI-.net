using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Propriedades.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Propriedades.Infraestrutura.Configuracoes;

public class PropriedadeCulturaConfiguration : IEntityTypeConfiguration<PropriedadeCultura>
{
    public void Configure(EntityTypeBuilder<PropriedadeCultura> builder)
    {
        builder.ToTable("PropriedadeCultura");

        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(pc => pc.PropriedadeId)
            .HasColumnName("PropriedadeId")
            .IsRequired();

        builder.Property(pc => pc.CulturaId)
            .HasColumnName("CulturaId")
            .IsRequired();

        builder.Property(pc => pc.Area)
            .HasColumnName("Area")
            .HasColumnType("decimal(18,4)")
            .HasConversion(
                v => v.Valor,
                v => new AreaPlantio(v))
            .IsRequired();

        builder.Property(pc => pc.SafraId)
            .HasColumnName("SafraId");

        builder.Property(pc => pc.DataPlantio)
            .HasColumnName("DataPlantio")
            .HasColumnType("timestamp with time zone");

        builder.Property(pc => pc.DataColheitaPrevista)
            .HasColumnName("DataColheitaPrevista")
            .HasColumnType("timestamp with time zone");

        builder.Property(pc => pc.Observacoes)
            .HasColumnName("Observacoes")
            .HasMaxLength(1000);

        builder.Property(pc => pc.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(pc => pc.DataAtualizacao)
            .HasColumnName("DataAtualizacao")
            .HasColumnType("timestamp with time zone");

        // Relacionamentos
        builder.HasOne(pc => pc.Propriedade)
            .WithMany(p => p.PropriedadeCulturas)
            .HasForeignKey(pc => pc.PropriedadeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(pc => pc.PropriedadeId)
            .HasDatabaseName("IX_PropriedadeCultura_PropriedadeId");

        builder.HasIndex(pc => pc.CulturaId)
            .HasDatabaseName("IX_PropriedadeCultura_CulturaId");

        builder.HasIndex(pc => pc.SafraId)
            .HasDatabaseName("IX_PropriedadeCultura_SafraId");

        // Índice único para evitar duplicação de cultura por propriedade
        builder.HasIndex(pc => new { pc.PropriedadeId, pc.CulturaId })
            .HasDatabaseName("IX_PropriedadeCultura_PropriedadeId_CulturaId")
            .IsUnique();

        // Índice para consultas por período de plantio
        builder.HasIndex(pc => new { pc.DataPlantio, pc.DataColheitaPrevista })
            .HasDatabaseName("IX_PropriedadeCultura_PeriodoPlantio");
    }
}