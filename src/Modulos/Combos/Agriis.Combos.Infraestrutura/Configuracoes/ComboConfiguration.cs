using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Combos.Dominio.Entidades;
using Agriis.Combos.Dominio.Enums;

namespace Agriis.Combos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração da entidade Combo para Entity Framework
/// </summary>
public class ComboConfiguration : IEntityTypeConfiguration<Combo>
{
    public void Configure(EntityTypeBuilder<Combo> builder)
    {
        builder.ToTable("Combo");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Nome)
            .HasColumnName("Nome")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Descricao)
            .HasColumnName("Descricao")
            .HasMaxLength(1000);

        builder.Property(c => c.HectareMinimo)
            .HasColumnName("HectareMinimo")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(c => c.HectareMaximo)
            .HasColumnName("HectareMaximo")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(c => c.DataInicio)
            .HasColumnName("DataInicio")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(c => c.DataFim)
            .HasColumnName("DataFim")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(c => c.ModalidadePagamento)
            .HasColumnName("ModalidadePagamento")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("Status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.RestricoesMunicipios)
            .HasColumnName("RestricoesMunicipios")
            .HasColumnType("jsonb");

        builder.Property(c => c.PermiteAlteracaoItem)
            .HasColumnName("PermiteAlteracaoItem")
            .IsRequired();

        builder.Property(c => c.PermiteExclusaoItem)
            .HasColumnName("PermiteExclusaoItem")
            .IsRequired();

        builder.Property(c => c.FornecedorId)
            .HasColumnName("FornecedorId")
            .IsRequired();

        builder.Property(c => c.SafraId)
            .HasColumnName("SafraId")
            .IsRequired();

        builder.Property(c => c.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(c => c.DataAtualizacao)
            .HasColumnName("DataAtualizacao")
            .HasColumnType("timestamp with time zone");

        // Relacionamentos
        builder.HasMany(c => c.Itens)
            .WithOne(i => i.Combo)
            .HasForeignKey(i => i.ComboId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.LocaisRecebimento)
            .WithOne(l => l.Combo)
            .HasForeignKey(l => l.ComboId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.CategoriasDesconto)
            .WithOne(cd => cd.Combo)
            .HasForeignKey(cd => cd.ComboId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(c => c.FornecedorId)
            .HasDatabaseName("IX_Combo_FornecedorId");

        builder.HasIndex(c => c.SafraId)
            .HasDatabaseName("IX_Combo_SafraId");

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("IX_Combo_Status");

        builder.HasIndex(c => new { c.DataInicio, c.DataFim })
            .HasDatabaseName("IX_Combo_Periodo");

        builder.HasIndex(c => new { c.FornecedorId, c.SafraId, c.Nome })
            .HasDatabaseName("IX_Combo_FornecedorSafraNome")
            .IsUnique();
    }
}