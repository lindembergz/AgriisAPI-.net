using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Compartilhado.Dominio.Entidades;
using System.Text.Json;

namespace Agriis.Api.Contexto.Configuracoes;

public abstract class EntidadeBaseConfiguration<T> : IEntityTypeConfiguration<T> where T : EntidadeBase
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        // Configurar chave primária
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();

        // Configurar propriedades de auditoria
        builder.Property(e => e.DataCriacao)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.DataAtualizacao)
            .IsRequired(false)
            .HasColumnType("timestamp with time zone");

        // Configurar índices para otimizar consultas por data
        builder.HasIndex(e => e.DataCriacao)
            .HasDatabaseName($"IX_{GetTableName()}_DataCriacao");

        // Configurar nome da tabela
        builder.ToTable(GetTableName());

        // Permitir configurações específicas da entidade
        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);
    protected abstract string GetTableName();

    // Métodos auxiliares para configurações comuns
    protected static void ConfigurarCampoJson<TProperty>(
        EntityTypeBuilder<T> builder,
        System.Linq.Expressions.Expression<Func<T, TProperty>> propertyExpression,
        string columnName)
    {
        builder.Property(propertyExpression)
            .HasColumnName(columnName)
            .HasColumnType("jsonb");
    }

    protected static void ConfigurarCampoJsonDocument(
        EntityTypeBuilder<T> builder,
        System.Linq.Expressions.Expression<Func<T, JsonDocument?>> propertyExpression,
        string columnName)
    {
        builder.Property(propertyExpression)
            .HasColumnName(columnName)
            .HasColumnType("jsonb");
    }

    protected static void ConfigurarListaIntComoJson(
        EntityTypeBuilder<T> builder,
        System.Linq.Expressions.Expression<Func<T, List<int>>> propertyExpression,
        string columnName)
    {
        builder.Property(propertyExpression)
            .HasColumnName(columnName)
            .HasColumnType("jsonb");
    }

    protected static void ConfigurarCampoTexto(
        EntityTypeBuilder<T> builder,
        System.Linq.Expressions.Expression<Func<T, string>> propertyExpression,
        string columnName,
        int? maxLength = null,
        bool isRequired = true)
    {
        var propertyBuilder = builder.Property(propertyExpression)
            .HasColumnName(columnName);

        if (maxLength.HasValue)
        {
            propertyBuilder.HasMaxLength(maxLength.Value);
        }

        propertyBuilder.IsRequired(isRequired);
    }

    protected static void ConfigurarCampoDecimal(
        EntityTypeBuilder<T> builder,
        System.Linq.Expressions.Expression<Func<T, decimal>> propertyExpression,
        string columnName,
        int precision = 18,
        int scale = 2)
    {
        builder.Property(propertyExpression)
            .HasColumnName(columnName)
            .HasPrecision(precision, scale)
            .IsRequired();
    }

    protected static void ConfigurarCampoDecimalOpcional(
        EntityTypeBuilder<T> builder,
        System.Linq.Expressions.Expression<Func<T, decimal?>> propertyExpression,
        string columnName,
        int precision = 18,
        int scale = 2)
    {
        builder.Property(propertyExpression)
            .HasColumnName(columnName)
            .HasPrecision(precision, scale)
            .IsRequired(false);
    }

    protected static void ConfigurarEnum<TEnum>(
        EntityTypeBuilder<T> builder,
        System.Linq.Expressions.Expression<Func<T, TEnum>> propertyExpression,
        string columnName) where TEnum : struct, Enum
    {
        builder.Property(propertyExpression)
            .HasColumnName(columnName)
            .HasConversion<string>()
            .IsRequired();
    }

    protected static void ConfigurarEnumOpcional<TEnum>(
        EntityTypeBuilder<T> builder,
        System.Linq.Expressions.Expression<Func<T, TEnum?>> propertyExpression,
        string columnName) where TEnum : struct, Enum
    {
        builder.Property(propertyExpression)
            .HasColumnName(columnName)
            .HasConversion<string>()
            .IsRequired(false);
    }
}