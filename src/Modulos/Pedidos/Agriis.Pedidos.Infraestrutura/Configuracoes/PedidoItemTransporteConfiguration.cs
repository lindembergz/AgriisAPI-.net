using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Pedidos.Dominio.Entidades;

namespace Agriis.Pedidos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade PedidoItemTransporte
/// </summary>
public class PedidoItemTransporteConfiguration : IEntityTypeConfiguration<PedidoItemTransporte>
{
    public void Configure(EntityTypeBuilder<PedidoItemTransporte> builder)
    {
        // Configuração da tabela
        builder.ToTable("PedidoItemTransporte");
        
        // Chave primária
        builder.HasKey(pit => pit.Id);
        
        // Configuração das propriedades
        builder.Property(pit => pit.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();
            
        builder.Property(pit => pit.PedidoItemId)
            .HasColumnName("PedidoItemId")
            .IsRequired();
            
        builder.Property(pit => pit.Quantidade)
            .HasColumnName("Quantidade")
            .HasColumnType("decimal(18,4)")
            .IsRequired();
            
        builder.Property(pit => pit.DataAgendamento)
            .HasColumnName("DataAgendamento");
            
        builder.Property(pit => pit.ValorFrete)
            .HasColumnName("ValorFrete")
            .HasColumnType("decimal(18,4)")
            .IsRequired();
            
        builder.Property(pit => pit.PesoTotal)
            .HasColumnName("PesoTotal")
            .HasColumnType("decimal(18,4)");
            
        builder.Property(pit => pit.VolumeTotal)
            .HasColumnName("VolumeTotal")
            .HasColumnType("decimal(18,4)");
            
        builder.Property(pit => pit.EnderecoOrigem)
            .HasColumnName("EnderecoOrigem")
            .HasMaxLength(500);
            
        builder.Property(pit => pit.EnderecoDestino)
            .HasColumnName("EnderecoDestino")
            .HasMaxLength(500);
            
        builder.Property(pit => pit.InformacoesTransporte)
            .HasColumnName("InformacoesTransporte")
            .HasColumnType("jsonb");
            
        builder.Property(pit => pit.Observacoes)
            .HasColumnName("Observacoes")
            .HasMaxLength(1000);
            
        builder.Property(pit => pit.DataCriacao)
            .HasColumnName("DataCriacao")
            .IsRequired();
            
        builder.Property(pit => pit.DataAtualizacao)
            .HasColumnName("DataAtualizacao");
        
        // Relacionamentos
        builder.HasOne(pit => pit.PedidoItem)
            .WithMany(pi => pi.ItensTransporte)
            .HasForeignKey(pit => pit.PedidoItemId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índices
        builder.HasIndex(pit => pit.PedidoItemId)
            .HasDatabaseName("IX_PedidoItemTransporte_PedidoItemId");
            
        builder.HasIndex(pit => pit.DataAgendamento)
            .HasDatabaseName("IX_PedidoItemTransporte_DataAgendamento");
            
        builder.HasIndex(pit => pit.ValorFrete)
            .HasDatabaseName("IX_PedidoItemTransporte_ValorFrete");
    }
}