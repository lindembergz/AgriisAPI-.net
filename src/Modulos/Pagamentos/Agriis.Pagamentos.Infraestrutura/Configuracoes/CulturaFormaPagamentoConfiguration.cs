using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Pagamentos.Dominio.Entidades;

namespace Agriis.Pagamentos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade CulturaFormaPagamento
/// </summary>
public class CulturaFormaPagamentoConfiguration : IEntityTypeConfiguration<CulturaFormaPagamento>
{
    public void Configure(EntityTypeBuilder<CulturaFormaPagamento> builder)
    {
        // Tabela
        builder.ToTable("cultura_forma_pagamento");
        
        // Chave primária
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        // Propriedades
        builder.Property(x => x.FornecedorId)
            .HasColumnName("fornecedor_id")
            .IsRequired();
            
        builder.Property(x => x.CulturaId)
            .HasColumnName("cultura_id")
            .IsRequired();
            
        builder.Property(x => x.FormaPagamentoId)
            .HasColumnName("forma_pagamento_id")
            .IsRequired();
            
        builder.Property(x => x.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true);
        
        // Campos de auditoria
        builder.Property(x => x.DataCriacao)
            .HasColumnName("data_criacao")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(x => x.DataAtualizacao)
            .HasColumnName("data_atualizacao");
        
        // Relacionamentos
        builder.HasOne(x => x.FormaPagamento)
            .WithMany(x => x.CulturaFormasPagamento)
            .HasForeignKey(x => x.FormaPagamentoId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Índices
        builder.HasIndex(x => x.FornecedorId)
            .HasDatabaseName("ix_cultura_forma_pagamento_fornecedor_id");
            
        builder.HasIndex(x => x.CulturaId)
            .HasDatabaseName("ix_cultura_forma_pagamento_cultura_id");
            
        builder.HasIndex(x => x.FormaPagamentoId)
            .HasDatabaseName("ix_cultura_forma_pagamento_forma_pagamento_id");
            
        builder.HasIndex(x => new { x.FornecedorId, x.CulturaId, x.FormaPagamentoId })
            .HasDatabaseName("ix_cultura_forma_pagamento_unique")
            .IsUnique();
            
        builder.HasIndex(x => x.Ativo)
            .HasDatabaseName("ix_cultura_forma_pagamento_ativo");
    }
}