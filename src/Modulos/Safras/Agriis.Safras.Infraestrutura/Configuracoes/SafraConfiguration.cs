using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Safras.Dominio.Entidades;

namespace Agriis.Safras.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Safra
/// </summary>
public class SafraConfiguration : IEntityTypeConfiguration<Safra>
{
    public void Configure(EntityTypeBuilder<Safra> builder)
    {
        builder.ToTable("Safra");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.PlantioInicial)
            .HasColumnName("PlantioInicial")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(s => s.PlantioFinal)
            .HasColumnName("PlantioFinal")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(s => s.PlantioNome)
            .HasColumnName("PlantioNome")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(s => s.Descricao)
            .HasColumnName("Descricao")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(s => s.AnoColheita)
            .HasColumnName("AnoColheita")
            .IsRequired();

        builder.Property(s => s.DataCriacao)
            .HasColumnName("DataCriacao")
            .IsRequired();

        builder.Property(s => s.DataAtualizacao)
            .HasColumnName("DataAtualizacao");

        // Índices
        builder.HasIndex(s => s.PlantioInicial)
            .HasDatabaseName("IX_Safra_PlantioInicial");

        builder.HasIndex(s => s.PlantioFinal)
            .HasDatabaseName("IX_Safra_PlantioFinal");

        builder.HasIndex(s => s.AnoColheita)
            .HasDatabaseName("IX_Safra_AnoColheita");

        builder.HasIndex(s => new { s.PlantioNome, s.PlantioInicial, s.PlantioFinal })
            .HasDatabaseName("IX_Safra_Periodo")
            .IsUnique();

        // Índice para consulta de safra atual
        builder.HasIndex(s => new { s.PlantioNome, s.PlantioInicial, s.PlantioFinal })
            .HasDatabaseName("IX_Safra_Atual")
            .HasFilter("PlantioNome = 'S1'");
    }
}