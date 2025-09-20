using Agriis.Segmentacoes.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Segmentacoes.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Grupo
/// </summary>
public class GrupoConfiguration : IEntityTypeConfiguration<Grupo>
{
    public void Configure(EntityTypeBuilder<Grupo> builder)
    {
        // Configuração da tabela
        builder.ToTable("Grupo");
        
        // Chave primária
        builder.HasKey(g => g.Id);
        
        // Configuração das propriedades
        builder.Property(g => g.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();
            
        builder.Property(g => g.Nome)
            .HasColumnName("Nome")
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(g => g.Descricao)
            .HasColumnName("Descricao")
            .HasMaxLength(500);
            
        builder.Property(g => g.AreaMinima)
            .HasColumnName("AreaMinima")
            .HasColumnType("decimal(18,4)")
            .IsRequired();
            
        builder.Property(g => g.AreaMaxima)
            .HasColumnName("AreaMaxima")
            .HasColumnType("decimal(18,4)");
            
        builder.Property(g => g.Ativo)
            .HasColumnName("Ativo")
            .IsRequired();
            
        builder.Property(g => g.SegmentacaoId)
            .HasColumnName("SegmentacaoId")
            .IsRequired();
            
        builder.Property(g => g.DataCriacao)
            .HasColumnName("DataCriacao")
            .IsRequired();
            
        builder.Property(g => g.DataAtualizacao)
            .HasColumnName("DataAtualizacao");
        
        // Relacionamentos
        builder.HasOne(g => g.Segmentacao)
            .WithMany(s => s.Grupos)
            .HasForeignKey(g => g.SegmentacaoId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(g => g.GruposSegmentacao)
            .WithOne(gs => gs.Grupo)
            .HasForeignKey(gs => gs.GrupoId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índices
        builder.HasIndex(g => g.SegmentacaoId)
            .HasDatabaseName("IX_Grupo_SegmentacaoId");
            
        builder.HasIndex(g => new { g.SegmentacaoId, g.Ativo })
            .HasDatabaseName("IX_Grupo_SegmentacaoId_Ativo");
            
        builder.HasIndex(g => new { g.SegmentacaoId, g.AreaMinima, g.AreaMaxima })
            .HasDatabaseName("IX_Grupo_SegmentacaoId_Areas");
            
        builder.HasIndex(g => g.Nome)
            .HasDatabaseName("IX_Grupo_Nome");
        
        // Constraints
        builder.HasCheckConstraint("CK_Grupo_AreaMinima_Positiva", "\"AreaMinima\" >= 0");
        builder.HasCheckConstraint("CK_Grupo_AreaMaxima_MaiorQueMinima", "\"AreaMaxima\" IS NULL OR \"AreaMaxima\" >= \"AreaMinima\"");
    }
}