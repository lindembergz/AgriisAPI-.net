using Agriis.Enderecos.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Enderecos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Estado
/// </summary>
public class EstadoConfiguration : IEntityTypeConfiguration<Estado>
{
    public void Configure(EntityTypeBuilder<Estado> builder)
    {
        // Configuração da tabela
        builder.ToTable("estados", "public");
        
        // Chave primária
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        // Propriedades
        builder.Property(e => e.Nome)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(e => e.Uf)
            .HasColumnName("uf")
            .HasMaxLength(2)
            .IsRequired();
            
        builder.Property(e => e.CodigoIbge)
            .HasColumnName("codigo_ibge")
            .IsRequired();
            
        builder.Property(e => e.Regiao)
            .HasColumnName("regiao")
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(e => e.PaisId)
            .HasColumnName("pais_id")
            .IsRequired()
            .HasDefaultValue(1);
        
        // Propriedades de auditoria
        builder.Property(e => e.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(e => e.DataAtualizacao)
            .HasColumnName("data_atualizacao")
            .IsRequired(false);
        
        // Índices
        builder.HasIndex(e => e.Uf)
            .IsUnique()
            .HasDatabaseName("IX_estados_uf");
            
        builder.HasIndex(e => e.CodigoIbge)
            .IsUnique()
            .HasDatabaseName("IX_estados_codigo_ibge");
            
        builder.HasIndex(e => e.Nome)
            .HasDatabaseName("IX_estados_nome");
            
        builder.HasIndex(e => e.Regiao)
            .HasDatabaseName("IX_estados_regiao");
            
        builder.HasIndex(e => e.PaisId)
            .HasDatabaseName("IX_estados_pais_id");
        
        // Relacionamentos
        builder.HasMany(e => e.Municipios)
            .WithOne(m => m.Estado)
            .HasForeignKey(m => m.EstadoId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(e => e.Enderecos)
            .WithOne(en => en.Estado)
            .HasForeignKey(en => en.EstadoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}