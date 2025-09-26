using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Referencias.Dominio.Entidades;

namespace Agriis.Referencias.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Municipio
/// </summary>
public class MunicipioConfiguration : IEntityTypeConfiguration<Municipio>
{
    public void Configure(EntityTypeBuilder<Municipio> builder)
    {
        // Tabela - criar nova tabela para municípios de referência
        // Nota: Esta é diferente da tabela municipios do módulo Enderecos
        builder.ToTable("municipios_referencia", "public");

        // Chave primária
        builder.HasKey(m => m.Id);

        // Propriedades básicas
        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(m => m.Nome)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.CodigoIbge)
            .HasColumnName("codigo_ibge")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(m => m.UfId)
            .HasColumnName("uf_id")
            .IsRequired();

        builder.Property(m => m.Ativo)
            .HasColumnName("ativo")
            .IsRequired()
            .HasDefaultValue(true);

        // Propriedades de auditoria
        builder.Property(m => m.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(m => m.DataAtualizacao)
            .HasColumnName("data_atualizacao");

        // Relacionamentos
        builder.HasOne(m => m.Uf)
            .WithMany(u => u.Municipios)
            .HasForeignKey(m => m.UfId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(m => m.CodigoIbge)
            .IsUnique()
            .HasDatabaseName("IX_municipios_referencia_codigo_ibge");

        builder.HasIndex(m => m.Nome)
            .HasDatabaseName("IX_municipios_referencia_nome");

        builder.HasIndex(m => m.UfId)
            .HasDatabaseName("IX_municipios_referencia_uf_id");

        builder.HasIndex(m => m.Ativo)
            .HasDatabaseName("IX_municipios_referencia_ativo");

        builder.HasIndex(m => m.DataCriacao)
            .HasDatabaseName("IX_municipios_referencia_DataCriacao");

        // Índice composto para consultas por UF + Nome
        builder.HasIndex(m => new { m.UfId, m.Nome })
            .HasDatabaseName("IX_municipios_referencia_uf_nome");
    }
}