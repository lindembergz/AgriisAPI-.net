using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Referencias.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade AtividadeAgropecuaria
/// </summary>
public class AtividadeAgropecuariaConfiguration : IEntityTypeConfiguration<AtividadeAgropecuaria>
{
    public void Configure(EntityTypeBuilder<AtividadeAgropecuaria> builder)
    {
        builder.ToTable("AtividadesAgropecuarias");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Codigo)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(x => x.Descricao)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(x => x.Tipo)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(x => x.Ativo)
            .IsRequired();
            
        builder.Property(x => x.DataCriacao)
            .IsRequired();
            
        builder.Property(x => x.DataAtualizacao);
        

        // Índices únicos
        builder.HasIndex(x => x.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_AtividadesAgropecuarias_Codigo_Unique");
            
        // Índice para consultas por tipo
        builder.HasIndex(x => x.Tipo)
            .HasDatabaseName("IX_AtividadesAgropecuarias_Tipo");
            
        // Índice para busca por descrição
        builder.HasIndex(x => x.Descricao)
            .HasDatabaseName("IX_AtividadesAgropecuarias_Descricao");
    }
}