using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Pedidos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Proposta
/// </summary>
public class PropostaConfiguration : IEntityTypeConfiguration<Proposta>
{
    /// <summary>
    /// Configura a entidade Proposta
    /// </summary>
    /// <param name="builder">Builder de configuração</param>
    public void Configure(EntityTypeBuilder<Proposta> builder)
    {
        // Configuração da tabela
        builder.ToTable("Proposta");
        
        // Chave primária
        builder.HasKey(p => p.Id);
        
        // Configuração das propriedades
        builder.Property(p => p.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();
            
        builder.Property(p => p.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamptz")
            .IsRequired();
            
        builder.Property(p => p.DataAtualizacao)
            .HasColumnName("DataAtualizacao")
            .HasColumnType("timestamptz");
            
        builder.Property(p => p.PedidoId)
            .HasColumnName("PedidoId")
            .IsRequired();
            
        builder.Property(p => p.AcaoComprador)
            .HasColumnName("AcaoComprador")
            .HasConversion<int>()
            .IsRequired(false);
            
        builder.Property(p => p.Observacao)
            .HasColumnName("Observacao")
            .HasMaxLength(1024)
            .IsRequired(false);
            
        builder.Property(p => p.UsuarioProdutorId)
            .HasColumnName("UsuarioProdutorId")
            .IsRequired(false);
            
        builder.Property(p => p.UsuarioFornecedorId)
            .HasColumnName("UsuarioFornecedorId")
            .IsRequired(false);
        
        // Relacionamentos
        builder.HasOne(p => p.Pedido)
            .WithMany(pe => pe.Propostas)
            .HasForeignKey(p => p.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índices
        builder.HasIndex(p => p.PedidoId)
            .HasDatabaseName("IX_Proposta_PedidoId");
            
        builder.HasIndex(p => p.DataCriacao)
            .HasDatabaseName("IX_Proposta_DataCriacao");
            
        builder.HasIndex(p => p.UsuarioProdutorId)
            .HasDatabaseName("IX_Proposta_UsuarioProdutorId");
            
        builder.HasIndex(p => p.UsuarioFornecedorId)
            .HasDatabaseName("IX_Proposta_UsuarioFornecedorId");
    }
}