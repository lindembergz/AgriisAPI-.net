using Agriis.Segmentacoes.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Segmentacoes.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade GrupoSegmentacao
/// </summary>
public class GrupoSegmentacaoConfiguration : IEntityTypeConfiguration<GrupoSegmentacao>
{
    public void Configure(EntityTypeBuilder<GrupoSegmentacao> builder)
    {
        // Configuração da tabela
        builder.ToTable("GrupoSegmentacao");
        
        // Chave primária
        builder.HasKey(gs => gs.Id);
        
        // Configuração das propriedades
        builder.Property(gs => gs.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();
            
        builder.Property(gs => gs.GrupoId)
            .HasColumnName("GrupoId")
            .IsRequired();
            
        builder.Property(gs => gs.CategoriaId)
            .HasColumnName("CategoriaId")
            .IsRequired();
            
        builder.Property(gs => gs.PercentualDesconto)
            .HasColumnName("PercentualDesconto")
            .HasColumnType("decimal(5,2)")
            .IsRequired();
            
        builder.Property(gs => gs.Ativo)
            .HasColumnName("Ativo")
            .IsRequired();
            
        builder.Property(gs => gs.Observacoes)
            .HasColumnName("Observacoes")
            .HasMaxLength(1000);
            
        builder.Property(gs => gs.DataCriacao)
            .HasColumnName("DataCriacao")
            .IsRequired();
            
        builder.Property(gs => gs.DataAtualizacao)
            .HasColumnName("DataAtualizacao");
        
        // Relacionamentos
        builder.HasOne(gs => gs.Grupo)
            .WithMany(g => g.GruposSegmentacao)
            .HasForeignKey(gs => gs.GrupoId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índices
        builder.HasIndex(gs => gs.GrupoId)
            .HasDatabaseName("IX_GrupoSegmentacao_GrupoId");
            
        builder.HasIndex(gs => gs.CategoriaId)
            .HasDatabaseName("IX_GrupoSegmentacao_CategoriaId");
            
        builder.HasIndex(gs => new { gs.GrupoId, gs.CategoriaId })
            .IsUnique()
            .HasDatabaseName("IX_GrupoSegmentacao_GrupoId_CategoriaId_Unique");
            
        builder.HasIndex(gs => new { gs.GrupoId, gs.Ativo })
            .HasDatabaseName("IX_GrupoSegmentacao_GrupoId_Ativo");
        
        // Constraints
        builder.HasCheckConstraint("CK_GrupoSegmentacao_PercentualDesconto_Valido", "\"PercentualDesconto\" >= 0 AND \"PercentualDesconto\" <= 100");
    }
}