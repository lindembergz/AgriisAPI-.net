using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Referencias.Dominio.Entidades;
using Agriis.Compartilhado.Infraestrutura.Configuracoes;

namespace Agriis.Referencias.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Uf
/// </summary>
public class UfConfiguration : IEntityTypeConfiguration<Uf>
{
    public void Configure(EntityTypeBuilder<Uf> builder)
    {
        // Tabela - mapear para a tabela estados_referencia (diferente da tabela estados do módulo Enderecos)
        builder.ToTable("estados_referencia", "public");

        // Configuração base de auditoria
        EntidadeBaseConfiguration.ConfigurarAuditoriaSnakeCase(builder);
        
        // Override do ID para usar nome específico
        builder.Property(u => u.Id)
            .HasColumnName("id");

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



        // Relacionamentos
        // Ignorando Pais por enquanto (pode ser configurado depois se necessário)
        builder.Ignore(u => u.Pais);
        
        // Configurando relacionamento com Municipios
        builder.HasMany(u => u.Municipios)
            .WithOne(m => m.Uf)
            .HasForeignKey(m => m.UfId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(u => u.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_estados_referencia_uf");

        builder.HasIndex(u => u.Nome)
            .HasDatabaseName("IX_estados_referencia_nome");

        builder.HasIndex(u => u.Ativo)
            .HasDatabaseName("IX_estados_referencia_ativo");

        builder.HasIndex(u => u.DataCriacao)
            .HasDatabaseName("IX_estados_referencia_data_criacao");

        // Configurações adicionais removidas devido aos ignores das navegações
    }
}