using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Propriedades.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Propriedades.Infraestrutura.Configuracoes;

public class TalhaoConfiguration : IEntityTypeConfiguration<Talhao>
{
    public void Configure(EntityTypeBuilder<Talhao> builder)
    {
        builder.ToTable("Talhao");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Nome)
            .HasColumnName("Nome")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Area)
            .HasColumnName("Area")
            .HasColumnType("decimal(18,4)")
            .HasConversion(
                v => v.Valor,
                v => new AreaPlantio(v))
            .IsRequired();

        builder.Property(t => t.Descricao)
            .HasColumnName("Descricao")
            .HasMaxLength(500);

        builder.Property(t => t.Localizacao)
            .HasColumnName("Localizacao")
            .HasColumnType("geography(POINT, 4326)");

        builder.Property(t => t.Geometria)
            .HasColumnName("Geometria")
            .HasColumnType("geography(POLYGON, 4326)");

        builder.Property(t => t.PropriedadeId)
            .HasColumnName("PropriedadeId")
            .IsRequired();

        builder.Property(t => t.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(t => t.DataAtualizacao)
            .HasColumnName("DataAtualizacao")
            .HasColumnType("timestamp with time zone");

        // Relacionamentos
        builder.HasOne(t => t.Propriedade)
            .WithMany(p => p.Talhoes)
            .HasForeignKey(t => t.PropriedadeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(t => t.PropriedadeId)
            .HasDatabaseName("IX_Talhao_PropriedadeId");

        builder.HasIndex(t => t.Nome)
            .HasDatabaseName("IX_Talhao_Nome");

        // Índice espacial para localização
        builder.HasIndex(t => t.Localizacao)
            .HasDatabaseName("IX_Talhao_Localizacao")
            .HasMethod("gist");

        // Índice espacial para geometria
        builder.HasIndex(t => t.Geometria)
            .HasDatabaseName("IX_Talhao_Geometria")
            .HasMethod("gist");
    }
}