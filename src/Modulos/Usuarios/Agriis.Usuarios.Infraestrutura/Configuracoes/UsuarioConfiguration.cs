using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Usuarios.Dominio.Entidades;
using Agriis.Compartilhado.Infraestrutura.Persistencia.Conversores;

namespace Agriis.Usuarios.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Usuario
/// </summary>
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        // Tabela
        builder.ToTable("usuarios");
        
        // Chave primária
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        // Propriedades básicas
        builder.Property(u => u.Nome)
            .HasColumnName("nome")
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(u => u.Celular)
            .HasColumnName("celular")
            .HasMaxLength(20);
        
        // CPF como objeto de valor
        builder.Property(u => u.Cpf)
            .HasColumnName("cpf")
            .HasMaxLength(11)
            .HasConversion(ValueObjectConverters.CpfNullableConverter);
        
        builder.Property(u => u.SenhaHash)
            .HasColumnName("senha_hash")
            .HasMaxLength(255);
        
        builder.Property(u => u.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true);
        
        builder.Property(u => u.UltimoLogin)
            .HasColumnName("ultimo_login")
            .HasColumnType("timestamp with time zone");
        
        builder.Property(u => u.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500);
        
        // Auditoria
        builder.Property(u => u.DataCriacao)
            .HasColumnName("data_criacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        
        builder.Property(u => u.DataAtualizacao)
            .HasColumnName("data_atualizacao")
            .HasColumnType("timestamp with time zone");
        
        // Índices
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_usuarios_email");
        
        builder.HasIndex(u => u.Cpf)
            .IsUnique()
            .HasDatabaseName("ix_usuarios_cpf")
            .HasFilter("cpf IS NOT NULL");
        
        builder.HasIndex(u => u.Ativo)
            .HasDatabaseName("ix_usuarios_ativo");
        
        // Relacionamentos
        builder.HasMany(u => u.UsuarioRoles)
            .WithOne(ur => ur.Usuario)
            .HasForeignKey(ur => ur.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}