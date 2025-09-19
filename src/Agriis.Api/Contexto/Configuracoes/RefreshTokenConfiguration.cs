using Agriis.Autenticacao.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Api.Contexto.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para RefreshToken
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        // Chave primária
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Token
        builder.Property(rt => rt.Token)
            .HasColumnName("token")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("ix_refresh_tokens_token");

        // UsuarioId
        builder.Property(rt => rt.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.HasIndex(rt => rt.UsuarioId)
            .HasDatabaseName("ix_refresh_tokens_usuario_id");

        // DataExpiracao
        builder.Property(rt => rt.DataExpiracao)
            .HasColumnName("data_expiracao")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Revogado
        builder.Property(rt => rt.Revogado)
            .HasColumnName("revogado")
            .HasDefaultValue(false)
            .IsRequired();

        // DataRevogacao
        builder.Property(rt => rt.DataRevogacao)
            .HasColumnName("data_revogacao")
            .HasColumnType("timestamp with time zone");

        // EnderecoIp
        builder.Property(rt => rt.EnderecoIp)
            .HasColumnName("endereco_ip")
            .HasMaxLength(45); // IPv6 máximo

        // UserAgent
        builder.Property(rt => rt.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(1000);

        // Campos de auditoria herdados de EntidadeBase
        builder.Property(rt => rt.DataCriacao)
            .HasColumnName("data_criacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(rt => rt.DataAtualizacao)
            .HasColumnName("data_atualizacao")
            .HasColumnType("timestamp with time zone");

        // Índices para performance
        builder.HasIndex(rt => new { rt.UsuarioId, rt.Revogado, rt.DataExpiracao })
            .HasDatabaseName("ix_refresh_tokens_usuario_valido");

        builder.HasIndex(rt => rt.DataExpiracao)
            .HasDatabaseName("ix_refresh_tokens_data_expiracao");
    }
}