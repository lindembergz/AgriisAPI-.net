using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Pedidos.Dominio.Entidades;

namespace Agriis.Pedidos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade PedidoItem
/// </summary>
public class PedidoItemConfiguration : IEntityTypeConfiguration<PedidoItem>
{
    public void Configure(EntityTypeBuilder<PedidoItem> builder)
    {
        // Configuração da tabela
        builder.ToTable("PedidoItem");
        
        // Chave primária
        builder.HasKey(pi => pi.Id);
        
        // Configuração das propriedades
        builder.Property(pi => pi.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();
            
        builder.Property(pi => pi.PedidoId)
            .HasColumnName("PedidoId")
            .IsRequired();
            
        builder.Property(pi => pi.ProdutoId)
            .HasColumnName("ProdutoId")
            .IsRequired();
            
        builder.Property(pi => pi.Quantidade)
            .HasColumnName("Quantidade")
            .HasColumnType("decimal(18,4)")
            .IsRequired();
            
        builder.Property(pi => pi.PrecoUnitario)
            .HasColumnName("PrecoUnitario")
            .HasColumnType("decimal(18,4)")
            .IsRequired();
            
        builder.Property(pi => pi.ValorTotal)
            .HasColumnName("ValorTotal")
            .HasColumnType("decimal(18,4)")
            .IsRequired();
            
        builder.Property(pi => pi.PercentualDesconto)
            .HasColumnName("PercentualDesconto")
            .HasColumnType("decimal(5,2)")
            .IsRequired();
            
        builder.Property(pi => pi.ValorDesconto)
            .HasColumnName("ValorDesconto")
            .HasColumnType("decimal(18,4)")
            .IsRequired();
            
        builder.Property(pi => pi.ValorFinal)
            .HasColumnName("ValorFinal")
            .HasColumnType("decimal(18,4)")
            .IsRequired();
            
        builder.Property(pi => pi.DadosAdicionais)
            .HasColumnName("DadosAdicionais")
            .HasColumnType("jsonb");
            
        builder.Property(pi => pi.Observacoes)
            .HasColumnName("Observacoes")
            .HasMaxLength(1000);
            
        builder.Property(pi => pi.DataCriacao)
            .HasColumnName("DataCriacao")
            .IsRequired();
            
        builder.Property(pi => pi.DataAtualizacao)
            .HasColumnName("DataAtualizacao");
        
        // Relacionamentos
        builder.HasOne(pi => pi.Pedido)
            .WithMany(p => p.Itens)
            .HasForeignKey(pi => pi.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(pi => pi.ItensTransporte)
            .WithOne(pit => pit.PedidoItem)
            .HasForeignKey(pit => pit.PedidoItemId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índices
        builder.HasIndex(pi => pi.PedidoId)
            .HasDatabaseName("IX_PedidoItem_PedidoId");
            
        builder.HasIndex(pi => pi.ProdutoId)
            .HasDatabaseName("IX_PedidoItem_ProdutoId");
            
        builder.HasIndex(pi => pi.ValorFinal)
            .HasDatabaseName("IX_PedidoItem_ValorFinal");
    }
}