using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Enums;

namespace Agriis.Pedidos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Pedido
/// </summary>
public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        // Configuração da tabela
        builder.ToTable("Pedido");
        
        // Chave primária
        builder.HasKey(p => p.Id);
        
        // Configuração das propriedades
        builder.Property(p => p.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();
            
        builder.Property(p => p.Status)
            .HasColumnName("Status")
            .HasConversion<int>()
            .IsRequired();
            
        builder.Property(p => p.StatusCarrinho)
            .HasColumnName("StatusCarrinho")
            .HasConversion<int>()
            .IsRequired();
            
        builder.Property(p => p.QuantidadeItens)
            .HasColumnName("QuantidadeItens")
            .IsRequired();
            
        builder.Property(p => p.Totais)
            .HasColumnName("Totais")
            .HasColumnType("jsonb");
            
        builder.Property(p => p.PermiteContato)
            .HasColumnName("PermiteContato")
            .IsRequired();
            
        builder.Property(p => p.NegociarPedido)
            .HasColumnName("NegociarPedido")
            .IsRequired();
            
        builder.Property(p => p.DataLimiteInteracao)
            .HasColumnName("DataLimiteInteracao")
            .IsRequired();
            
        builder.Property(p => p.FornecedorId)
            .HasColumnName("FornecedorId")
            .IsRequired();
            
        builder.Property(p => p.ProdutorId)
            .HasColumnName("ProdutorId")
            .IsRequired();
            
        builder.Property(p => p.DataCriacao)
            .HasColumnName("DataCriacao")
            .IsRequired();
            
        builder.Property(p => p.DataAtualizacao)
            .HasColumnName("DataAtualizacao");
        
        // Relacionamentos
        builder.HasMany(p => p.Itens)
            .WithOne(i => i.Pedido)
            .HasForeignKey(i => i.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índices
        builder.HasIndex(p => p.FornecedorId)
            .HasDatabaseName("IX_Pedido_FornecedorId");
            
        builder.HasIndex(p => p.ProdutorId)
            .HasDatabaseName("IX_Pedido_ProdutorId");
            
        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Pedido_Status");
            
        builder.HasIndex(p => p.DataLimiteInteracao)
            .HasDatabaseName("IX_Pedido_DataLimiteInteracao");
            
        builder.HasIndex(p => new { p.ProdutorId, p.FornecedorId })
            .HasDatabaseName("IX_Pedido_ProdutorId_FornecedorId");
    }
}