using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Usuarios.Dominio.Entidades;
using Agriis.Compartilhado.Infraestrutura.Persistencia.Conversores;

namespace Agriis.Usuarios.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade UsuarioRole
/// </summary>
public class UsuarioRoleConfiguration : IEntityTypeConfiguration<UsuarioRole>
{
    public void Configure(EntityTypeBuilder<UsuarioRole> builder)
    {
        // Tabela
        builder.ToTable("usuario_roles");
        
        // Chave primária
        builder.HasKey(ur => ur.Id);
        builder.Property(ur => ur.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        // Propriedades
        builder.Property(ur => ur.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();
        
        builder.Property(ur => ur.Role)
            .HasColumnName("role")
            .HasConversion(EnumConverters.RolesConverter)
            .IsRequired();
        
        builder.Property(ur => ur.DataAtribuicao)
            .HasColumnName("data_atribuicao")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        
        // Auditoria
        builder.Property(ur => ur.DataCriacao)
            .HasColumnName("data_criacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        
        builder.Property(ur => ur.DataAtualizacao)
            .HasColumnName("data_atualizacao")
            .HasColumnType("timestamp with time zone");
        
        // Índices
        builder.HasIndex(ur => ur.UsuarioId)
            .HasDatabaseName("ix_usuario_roles_usuario_id");
        
        builder.HasIndex(ur => ur.Role)
            .HasDatabaseName("ix_usuario_roles_role");
        
        // Índice único para evitar roles duplicadas por usuário
        builder.HasIndex(ur => new { ur.UsuarioId, ur.Role })
            .IsUnique()
            .HasDatabaseName("ix_usuario_roles_usuario_role_unique");
        
        // Relacionamentos
        builder.HasOne(ur => ur.Usuario)
            .WithMany(u => u.UsuarioRoles)
            .HasForeignKey(ur => ur.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}