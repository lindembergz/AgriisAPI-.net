using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Propriedades.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Agriis.Propriedades.Infraestrutura.Configuracoes;

public class PropriedadeConfiguration : IEntityTypeConfiguration<Propriedade>
{
    public void Configure(EntityTypeBuilder<Propriedade> builder)
    {
        builder.ToTable("Propriedade");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Nome)
            .HasColumnName("Nome")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Nirf)
            .HasColumnName("Nirf")
            .HasMaxLength(50);

        builder.Property(p => p.InscricaoEstadual)
            .HasColumnName("InscricaoEstadual")
            .HasMaxLength(50);

        builder.Property(p => p.AreaTotal)
            .HasColumnName("AreaTotal")
            .HasColumnType("decimal(18,4)")
            .HasConversion(
                v => v.Valor,
                v => new AreaPlantio(v))
            .IsRequired();

        builder.Property(p => p.ProdutorId)
            .HasColumnName("ProdutorId")
            .IsRequired();

        builder.Property(p => p.EnderecoId)
            .HasColumnName("EnderecoId");

        builder.Property(p => p.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(p => p.DataAtualizacao)
            .HasColumnName("DataAtualizacao")
            .HasColumnType("timestamp with time zone");

        builder.Property(p => p.DadosAdicionais)
            .HasColumnName("DadosAdicionais")
            .HasColumnType("jsonb");

        // Relacionamentos
        builder.HasMany(p => p.Talhoes)
            .WithOne(t => t.Propriedade)
            .HasForeignKey(t => t.PropriedadeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.PropriedadeCulturas)
            .WithOne(pc => pc.Propriedade)
            .HasForeignKey(pc => pc.PropriedadeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(p => p.ProdutorId)
            .HasDatabaseName("IX_Propriedade_ProdutorId");

        builder.HasIndex(p => p.EnderecoId)
            .HasDatabaseName("IX_Propriedade_EnderecoId");

        builder.HasIndex(p => p.Nome)
            .HasDatabaseName("IX_Propriedade_Nome");
    }
}