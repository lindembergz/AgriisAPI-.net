using Agriis.Enderecos.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Enderecos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Endereco
/// </summary>
public class EnderecoConfiguration : IEntityTypeConfiguration<Endereco>
{
    public void Configure(EntityTypeBuilder<Endereco> builder)
    {
        // Configuração da tabela
        builder.ToTable("enderecos", "public");
        
        // Chave primária
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        // Propriedades
        builder.Property(e => e.Cep)
            .HasColumnName("cep")
            .HasMaxLength(8)
            .IsRequired();
            
        builder.Property(e => e.Logradouro)
            .HasColumnName("logradouro")
            .HasMaxLength(200)
            .IsRequired();
            
        builder.Property(e => e.Numero)
            .HasColumnName("numero")
            .HasMaxLength(20)
            .IsRequired(false);
            
        builder.Property(e => e.Complemento)
            .HasColumnName("complemento")
            .HasMaxLength(100)
            .IsRequired(false);
            
        builder.Property(e => e.Bairro)
            .HasColumnName("bairro")
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(e => e.Latitude)
            .HasColumnName("latitude")
            .HasPrecision(10, 8)
            .IsRequired(false);
            
        builder.Property(e => e.Longitude)
            .HasColumnName("longitude")
            .HasPrecision(11, 8)
            .IsRequired(false);
            
        // Configuração do campo geográfico PostGIS
        builder.Property(e => e.Localizacao)
            .HasColumnName("localizacao")
            .HasColumnType("geometry(Point,4326)")
            .IsRequired(false);
        
        // Chaves estrangeiras
        builder.Property(e => e.MunicipioId)
            .HasColumnName("municipio_id")
            .IsRequired();
            
        builder.Property(e => e.EstadoId)
            .HasColumnName("estado_id")
            .IsRequired();
        
        // Propriedades de auditoria
        builder.Property(e => e.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(e => e.DataAtualizacao)
            .HasColumnName("data_atualizacao")
            .IsRequired(false);
        
        // Índices
        builder.HasIndex(e => e.Cep)
            .HasDatabaseName("IX_enderecos_cep");
            
        builder.HasIndex(e => e.Logradouro)
            .HasDatabaseName("IX_enderecos_logradouro");
            
        builder.HasIndex(e => e.Bairro)
            .HasDatabaseName("IX_enderecos_bairro");
            
        builder.HasIndex(e => e.MunicipioId)
            .HasDatabaseName("IX_enderecos_municipio_id");
            
        builder.HasIndex(e => e.EstadoId)
            .HasDatabaseName("IX_enderecos_estado_id");
        
        // Índice espacial para consultas geográficas
        builder.HasIndex(e => e.Localizacao)
            .HasDatabaseName("IX_enderecos_localizacao")
            .HasMethod("gist");
        
        // Índice composto para verificação de duplicatas
        builder.HasIndex(e => new { e.Cep, e.Logradouro, e.Numero, e.MunicipioId })
            .HasDatabaseName("IX_enderecos_unique_address");
        
        // Relacionamentos
        builder.HasOne(e => e.Municipio)
            .WithMany(m => m.Enderecos)
            .HasForeignKey(e => e.MunicipioId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(e => e.Estado)
            .WithMany(es => es.Enderecos)
            .HasForeignKey(e => e.EstadoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}