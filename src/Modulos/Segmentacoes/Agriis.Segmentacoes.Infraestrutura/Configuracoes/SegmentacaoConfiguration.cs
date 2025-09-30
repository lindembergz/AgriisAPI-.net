using Agriis.Segmentacoes.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Segmentacoes.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Segmentacao
/// </summary>
public class SegmentacaoConfiguration : IEntityTypeConfiguration<Segmentacao>
{
    public void Configure(EntityTypeBuilder<Segmentacao> builder)
    {
        // Configuração da tabela
        builder.ToTable("Segmentacao");
        
        // Chave primária
        builder.HasKey(s => s.Id);
        
        // Configuração das propriedades
        builder.Property(s => s.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();
            
        builder.Property(s => s.Nome)
            .HasColumnName("Nome")
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(s => s.Descricao)
            .HasColumnName("Descricao")
            .HasMaxLength(500);
            
        builder.Property(s => s.Ativo)
            .HasColumnName("Ativo")
            .IsRequired();
            
        builder.Property(s => s.FornecedorId)
            .HasColumnName("FornecedorId")
            .IsRequired();
            
        builder.Property(s => s.ConfiguracaoTerritorial)
            .HasColumnName("ConfiguracaoTerritorial")
            .HasColumnType("jsonb");
            
        builder.Property(s => s.EhPadrao)
            .HasColumnName("EhPadrao")
            .IsRequired();
            
        builder.Property(s => s.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamptz")
            .IsRequired();
            
        builder.Property(s => s.DataAtualizacao)
            .HasColumnName("DataAtualizacao")
            .HasColumnType("timestamptz");
        
        // Relacionamentos
        builder.HasMany(s => s.Grupos)
            .WithOne(g => g.Segmentacao)
            .HasForeignKey(g => g.SegmentacaoId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índices
        builder.HasIndex(s => s.FornecedorId)
            .HasDatabaseName("IX_Segmentacao_FornecedorId");
            
        builder.HasIndex(s => new { s.FornecedorId, s.EhPadrao })
            .HasDatabaseName("IX_Segmentacao_FornecedorId_EhPadrao");
            
        builder.HasIndex(s => new { s.FornecedorId, s.Ativo })
            .HasDatabaseName("IX_Segmentacao_FornecedorId_Ativo");
            
        builder.HasIndex(s => s.Nome)
            .HasDatabaseName("IX_Segmentacao_Nome");
    }
}