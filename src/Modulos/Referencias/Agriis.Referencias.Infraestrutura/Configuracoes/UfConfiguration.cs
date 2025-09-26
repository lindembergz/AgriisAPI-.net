using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Referencias.Dominio.Entidades;

namespace Agriis.Referencias.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Uf
/// </summary>
public class UfConfiguration : IEntityTypeConfiguration<Uf>
{
    public void Configure(EntityTypeBuilder<Uf> builder)
    {
        // Tabela - mapear para a tabela estados existente
        builder.ToTable("estados", "public");

        // Chave primária
        builder.HasKey(u => u.Id);

        // Propriedades básicas
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Codigo)
            .HasColumnName("uf")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(u => u.Nome)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.PaisId)
            .HasColumnName("pais_id")
            .IsRequired()
            .HasDefaultValue(1); // Assumindo que Brasil tem ID 1

        builder.Property(u => u.Ativo)
            .HasColumnName("ativo")
            .IsRequired()
            .HasDefaultValue(true);

        // Propriedades de auditoria
        builder.Property(u => u.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.DataAtualizacao)
            .HasColumnName("data_atualizacao");

        // Relacionamentos
        builder.HasOne(u => u.Pais)
            .WithMany()
            .HasForeignKey(u => u.PaisId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Municipios)
            .WithOne(m => m.Uf)
            .HasForeignKey(m => m.UfId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(u => u.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_estados_uf");

        builder.HasIndex(u => u.Nome)
            .HasDatabaseName("IX_estados_nome");

        builder.HasIndex(u => u.Ativo)
            .HasDatabaseName("IX_estados_ativo");

        builder.HasIndex(u => u.DataCriacao)
            .HasDatabaseName("IX_Estados_DataCriacao");

        // Configurações adicionais
        builder.Navigation(u => u.Municipios)
            .EnableLazyLoading(false);
    }
}