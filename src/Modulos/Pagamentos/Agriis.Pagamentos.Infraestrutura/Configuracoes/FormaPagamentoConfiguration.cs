using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Pagamentos.Dominio.Entidades;

namespace Agriis.Pagamentos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade FormaPagamento
/// </summary>
public class FormaPagamentoConfiguration : IEntityTypeConfiguration<FormaPagamento>
{
    public void Configure(EntityTypeBuilder<FormaPagamento> builder)
    {
        // Tabela
        builder.ToTable("forma_pagamento");
        
        // Chave primária
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        // Propriedades
        builder.Property(x => x.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(45)
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
        builder.HasMany(x => x.CulturaFormasPagamento)
            .WithOne(x => x.FormaPagamento)
            .HasForeignKey(x => x.FormaPagamentoId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Índices
        builder.HasIndex(x => x.Descricao)
            .HasDatabaseName("ix_forma_pagamento_descricao");
            
        builder.HasIndex(x => x.Ativo)
            .HasDatabaseName("ix_forma_pagamento_ativo");
    }
}