using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.PontosDistribuicao.Dominio.Entidades;

namespace Agriis.PontosDistribuicao.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade PontoDistribuicao
/// </summary>
public class PontoDistribuicaoConfiguration : IEntityTypeConfiguration<PontoDistribuicao>
{
    public void Configure(EntityTypeBuilder<PontoDistribuicao> builder)
    {
        // Configuração da tabela
        builder.ToTable("PontoDistribuicao");
        
        // Chave primária
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
               .ValueGeneratedOnAdd();
        
        // Propriedades obrigatórias
        builder.Property(p => p.Nome)
               .IsRequired()
               .HasMaxLength(200);
        
        builder.Property(p => p.FornecedorId)
               .IsRequired();
        
        builder.Property(p => p.EnderecoId)
               .IsRequired();
        
        builder.Property(p => p.Ativo)
               .IsRequired()
               .HasDefaultValue(true);
        
        // Propriedades opcionais
        builder.Property(p => p.Descricao)
               .HasMaxLength(1000);
        
        builder.Property(p => p.RaioCobertura)
               .HasPrecision(10, 2);
        
        builder.Property(p => p.CapacidadeMaxima)
               .HasPrecision(18, 4);
        
        builder.Property(p => p.UnidadeCapacidade)
               .HasMaxLength(50);
        
        builder.Property(p => p.Observacoes)
               .HasMaxLength(2000);
        
        // Campos JSON
        builder.Property(p => p.CoberturaTerritorios)
               .HasColumnType("jsonb");
        
        builder.Property(p => p.HorarioFuncionamento)
               .HasColumnType("jsonb");
        
        // Campos de auditoria
        builder.Property(p => p.DataCriacao)
               .IsRequired()
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.Property(p => p.DataAtualizacao);
        
        // Relacionamentos
        builder.HasOne(p => p.Endereco)
               .WithMany()
               .HasForeignKey(p => p.EnderecoId)
               .OnDelete(DeleteBehavior.Restrict);
        
        // Índices
        builder.HasIndex(p => p.FornecedorId)
               .HasDatabaseName("IX_PontoDistribuicao_FornecedorId");
        
        builder.HasIndex(p => p.EnderecoId)
               .HasDatabaseName("IX_PontoDistribuicao_EnderecoId");
        
        builder.HasIndex(p => p.Ativo)
               .HasDatabaseName("IX_PontoDistribuicao_Ativo");
        
        builder.HasIndex(p => new { p.FornecedorId, p.Nome })
               .HasDatabaseName("IX_PontoDistribuicao_FornecedorId_Nome")
               .IsUnique();
        
        // Índice GIN para campos JSON (para consultas eficientes)
        builder.HasIndex(p => p.CoberturaTerritorios)
               .HasDatabaseName("IX_PontoDistribuicao_CoberturaTerritorios")
               .HasMethod("gin");
        
        // Configurações específicas do PostgreSQL
        builder.Property(p => p.Nome)
               .UseCollation("pt_BR");
        
        builder.Property(p => p.Descricao)
               .UseCollation("pt_BR");
        
        builder.Property(p => p.Observacoes)
               .UseCollation("pt_BR");
    }
}