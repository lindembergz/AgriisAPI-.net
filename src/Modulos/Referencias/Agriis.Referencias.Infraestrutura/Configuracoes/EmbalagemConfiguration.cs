using Agriis.Referencias.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Referencias.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Embalagem
/// </summary>
public class EmbalagemConfiguration : IEntityTypeConfiguration<Embalagem>
{
    public void Configure(EntityTypeBuilder<Embalagem> builder)
    {
        builder.ToTable("Embalagens");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Descricao)
            .HasMaxLength(500);
            
        builder.Property(x => x.UnidadeMedidaId)
            .IsRequired();
            
        builder.Property(x => x.Ativo)
            .IsRequired();
            
        builder.Property(x => x.DataCriacao)
            .IsRequired();
            
        builder.Property(x => x.DataAtualizacao);
        

        // Índices
        builder.HasIndex(x => x.Nome)
            .HasDatabaseName("IX_Embalagens_Nome");
            
        // Índice para consultas por unidade de medida
        builder.HasIndex(x => x.UnidadeMedidaId)
            .HasDatabaseName("IX_Embalagens_UnidadeMedidaId");
            
        // Índice composto para unicidade por nome e unidade de medida
        builder.HasIndex(x => new { x.Nome, x.UnidadeMedidaId })
            .IsUnique()
            .HasDatabaseName("IX_Embalagens_Nome_UnidadeMedidaId_Unique");
        
        // Relacionamento com UnidadeMedida
        builder.HasOne(x => x.UnidadeMedida)
            .WithMany(x => x.Embalagens)
            .HasForeignKey(x => x.UnidadeMedidaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}