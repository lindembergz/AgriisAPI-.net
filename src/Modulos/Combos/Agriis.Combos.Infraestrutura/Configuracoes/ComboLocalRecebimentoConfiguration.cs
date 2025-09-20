using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Combos.Dominio.Entidades;

namespace Agriis.Combos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração da entidade ComboLocalRecebimento para Entity Framework
/// </summary>
public class ComboLocalRecebimentoConfiguration : IEntityTypeConfiguration<ComboLocalRecebimento>
{
    public void Configure(EntityTypeBuilder<ComboLocalRecebimento> builder)
    {
        builder.ToTable("ComboLocalRecebimento");

        builder.HasKey(clr => clr.Id);

        builder.Property(clr => clr.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(clr => clr.ComboId)
            .HasColumnName("ComboId")
            .IsRequired();

        builder.Property(clr => clr.PontoDistribuicaoId)
            .HasColumnName("PontoDistribuicaoId")
            .IsRequired();

        builder.Property(clr => clr.PrecoAdicional)
            .HasColumnName("PrecoAdicional")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(clr => clr.PercentualDesconto)
            .HasColumnName("PercentualDesconto")
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(clr => clr.LocalPadrao)
            .HasColumnName("LocalPadrao")
            .IsRequired();

        builder.Property(clr => clr.Observacoes)
            .HasColumnName("Observacoes")
            .HasMaxLength(500);

        builder.Property(clr => clr.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(clr => clr.DataAtualizacao)
            .HasColumnName("DataAtualizacao")
            .HasColumnType("timestamp with time zone");

        // Relacionamentos
        builder.HasOne(clr => clr.Combo)
            .WithMany(c => c.LocaisRecebimento)
            .HasForeignKey(clr => clr.ComboId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(clr => clr.ComboId)
            .HasDatabaseName("IX_ComboLocalRecebimento_ComboId");

        builder.HasIndex(clr => clr.PontoDistribuicaoId)
            .HasDatabaseName("IX_ComboLocalRecebimento_PontoDistribuicaoId");

        builder.HasIndex(clr => new { clr.ComboId, clr.PontoDistribuicaoId })
            .HasDatabaseName("IX_ComboLocalRecebimento_ComboPonto")
            .IsUnique();
    }
}