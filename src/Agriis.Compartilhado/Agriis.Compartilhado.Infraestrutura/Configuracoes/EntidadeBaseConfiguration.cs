using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Compartilhado.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração base para todas as entidades que herdam de EntidadeBase
/// </summary>
public static class EntidadeBaseConfiguration
{
    /// <summary>
    /// Aplica configurações padrão para propriedades de auditoria
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    /// <param name="builder">Builder da entidade</param>
    /// <param name="dataCriacaoColumnName">Nome da coluna de data de criação (padrão: "DataCriacao")</param>
    /// <param name="dataAtualizacaoColumnName">Nome da coluna de data de atualização (padrão: "DataAtualizacao")</param>
    public static void ConfigurarAuditoria<T>(
        EntityTypeBuilder<T> builder,
        string dataCriacaoColumnName = "DataCriacao",
        string dataAtualizacaoColumnName = "DataAtualizacao") 
        where T : EntidadeBase
    {
        // Configuração da chave primária
        builder.HasKey(e => e.Id);
        
        // Configuração do ID
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        // Configuração da data de criação
        builder.Property(e => e.DataCriacao)
            .HasColumnName(dataCriacaoColumnName)
            .HasColumnType("timestamptz")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Configuração da data de atualização
        builder.Property(e => e.DataAtualizacao)
            .HasColumnName(dataAtualizacaoColumnName)
            .HasColumnType("timestamptz");

        // Ignorar RowVersion no PostgreSQL - usar mecanismo nativo de concorrência
        builder.Ignore(e => e.RowVersion);
    }
    
    /// <summary>
    /// Aplica configurações padrão para propriedades de auditoria com nomes de colunas em snake_case
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    /// <param name="builder">Builder da entidade</param>
    public static void ConfigurarAuditoriaSnakeCase<T>(EntityTypeBuilder<T> builder) 
        where T : EntidadeBase
    {
        ConfigurarAuditoria(builder, "data_criacao", "data_atualizacao");
    }
}