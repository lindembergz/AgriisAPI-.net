using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Referencias.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade UnidadeMedida
/// </summary>
public class UnidadeMedidaConfiguration : IEntityTypeConfiguration<UnidadeMedida>
{
    public void Configure(EntityTypeBuilder<UnidadeMedida> builder)
    {
        builder.ToTable("UnidadesMedida");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Simbolo)
            .IsRequired()
            .HasMaxLength(10);
            
        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Tipo)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(x => x.FatorConversao)
            .HasPrecision(18, 6);
            
        builder.Property(x => x.Ativo)
            .IsRequired();
            
        builder.Property(x => x.DataCriacao)
            .IsRequired();
            
        builder.Property(x => x.DataAtualizacao);
        

        // Índices únicos
        builder.HasIndex(x => x.Simbolo)
            .IsUnique()
            .HasDatabaseName("IX_UnidadesMedida_Simbolo_Unique");
            
        // Índice para consultas por tipo
        builder.HasIndex(x => x.Tipo)
            .HasDatabaseName("IX_UnidadesMedida_Tipo");
            
        // Índice para busca por nome
        builder.HasIndex(x => x.Nome)
            .HasDatabaseName("IX_UnidadesMedida_Nome");
            
        // Relacionamento com Embalagens (cascade delete restrict para evitar exclusão acidental)
        builder.HasMany(x => x.Embalagens)
            .WithOne(x => x.UnidadeMedida)
            .HasForeignKey(x => x.UnidadeMedidaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}