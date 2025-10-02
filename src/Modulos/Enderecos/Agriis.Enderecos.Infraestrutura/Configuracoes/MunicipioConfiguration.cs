using Agriis.Enderecos.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Enderecos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Municipio
/// </summary>
public class MunicipioConfiguration : IEntityTypeConfiguration<Municipio>
{
    public void Configure(EntityTypeBuilder<Municipio> builder)
    {
        // Configuração da tabela
        builder.ToTable("municipios", "public");
        
        // Chave primária
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        // Propriedades
        builder.Property(m => m.Nome)
            .HasColumnName("nome")
            .HasMaxLength(150)
            .IsRequired();
            
        builder.Property(m => m.CodigoIbge)
            .HasColumnName("codigo_ibge")
            .IsRequired();
            
        builder.Property(m => m.CepPrincipal)
            .HasColumnName("cep_principal")
            .HasMaxLength(8)
            .IsRequired(false);
            
        builder.Property(m => m.Latitude)
            .HasColumnName("latitude")
            .HasPrecision(10, 8)
            .IsRequired(false);
            
        builder.Property(m => m.Longitude)
            .HasColumnName("longitude")
            .HasPrecision(11, 8)
            .IsRequired(false);
            
        // Configuração do campo geográfico PostGIS - temporariamente desabilitado até migração
        builder.Ignore(m => m.Localizacao);
        
        // Chave estrangeira
        builder.Property(m => m.EstadoId)
            .HasColumnName("estado_id")
            .IsRequired();
        
        // Propriedades de auditoria
        builder.Property(m => m.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(m => m.DataAtualizacao)
            .HasColumnName("data_atualizacao")
            .IsRequired(false);
        
        // Índices
        builder.HasIndex(m => m.CodigoIbge)
            .IsUnique()
            .HasDatabaseName("IX_municipios_codigo_ibge");
            
        builder.HasIndex(m => m.Nome)
            .HasDatabaseName("IX_municipios_nome");
            
        builder.HasIndex(m => m.EstadoId)
            .HasDatabaseName("IX_municipios_estado_id");
            
        builder.HasIndex(m => m.CepPrincipal)
            .HasDatabaseName("IX_municipios_cep_principal");
        
        // Índice espacial para consultas geográficas - temporariamente desabilitado até migração
        // builder.HasIndex(m => m.Localizacao)
        //     .HasDatabaseName("IX_municipios_localizacao")
        //     .HasMethod("gist");
        
        // Relacionamentos
        builder.HasOne(m => m.Estado)
            .WithMany(e => e.Municipios)
            .HasForeignKey(m => m.EstadoId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(m => m.Enderecos)
            .WithOne(e => e.Municipio)
            .HasForeignKey(e => e.MunicipioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}