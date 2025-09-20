using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Combos.Dominio.Entidades;

namespace Agriis.Combos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração da entidade ComboCategoriaDesconto para Entity Framework
/// </summary>
public class ComboCategoriaDescontoConfiguration : IEntityTypeConfiguration<ComboCategoriaDesconto>
{
    public void Configure(EntityTypeBuilder<ComboCategoriaDesconto> builder)
    {
        builder.ToTable("ComboCategoriaDesconto");

        builder.HasKey(ccd => ccd.Id);

        builder.Property(ccd => ccd.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(ccd => ccd.ComboId)
            .HasColumnName("ComboId")
            .IsRequired();

        builder.Property(ccd => ccd.CategoriaId)
            .HasColumnName("CategoriaId")
            .IsRequired();

        builder.Property(ccd => ccd.PercentualDesconto)
            .HasColumnName("PercentualDesconto")
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(ccd => ccd.ValorDescontoFixo)
            .HasColumnName("ValorDescontoFixo")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(ccd => ccd.DescontoPorHectare)
            .HasColumnName("DescontoPorHectare")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(ccd => ccd.TipoDesconto)
            .HasColumnName("TipoDesconto")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(ccd => ccd.HectareMinimo)
            .HasColumnName("HectareMinimo")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(ccd => ccd.HectareMaximo)
            .HasColumnName("HectareMaximo")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(ccd => ccd.Ativo)
            .HasColumnName("Ativo")
            .IsRequired();

        builder.Property(ccd => ccd.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(ccd => ccd.DataAtualizacao)
            .HasColumnName("DataAtualizacao")
            .HasColumnType("timestamp with time zone");

        // Relacionamentos
        builder.HasOne(ccd => ccd.Combo)
            .WithMany(c => c.CategoriasDesconto)
            .HasForeignKey(ccd => ccd.ComboId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(ccd => ccd.ComboId)
            .HasDatabaseName("IX_ComboCategoriaDesconto_ComboId");

        builder.HasIndex(ccd => ccd.CategoriaId)
            .HasDatabaseName("IX_ComboCategoriaDesconto_CategoriaId");

        builder.HasIndex(ccd => new { ccd.ComboId, ccd.CategoriaId })
            .HasDatabaseName("IX_ComboCategoriaDesconto_ComboCategoria")
            .IsUnique();

        builder.HasIndex(ccd => ccd.Ativo)
            .HasDatabaseName("IX_ComboCategoriaDesconto_Ativo");
    }
}