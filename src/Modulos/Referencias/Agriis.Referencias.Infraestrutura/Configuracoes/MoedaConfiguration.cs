using Agriis.Referencias.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Referencias.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Moeda
/// </summary>
public class MoedaConfiguration : IEntityTypeConfiguration<Moeda>
{
    public void Configure(EntityTypeBuilder<Moeda> builder)
    {
        builder.ToTable("Moedas");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Codigo)
            .IsRequired()
            .HasMaxLength(3);
            
        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Simbolo)
            .IsRequired()
            .HasMaxLength(5);
            
        builder.Property(x => x.Ativo)
            .IsRequired();
            
        builder.Property(x => x.DataCriacao)
            .IsRequired();
            
        builder.Property(x => x.DataAtualizacao);
        

        // Índices únicos
        builder.HasIndex(x => x.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_Moedas_Codigo_Unique");
            
        builder.HasIndex(x => x.Nome)
            .IsUnique()
            .HasDatabaseName("IX_Moedas_Nome_Unique");
            
        builder.HasIndex(x => x.Simbolo)
            .IsUnique()
            .HasDatabaseName("IX_Moedas_Simbolo_Unique");
    }
}