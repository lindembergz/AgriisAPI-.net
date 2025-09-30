using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Fornecedores.Dominio.Entidades;

namespace Agriis.Fornecedores.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade UsuarioFornecedor
/// </summary>
public class UsuarioFornecedorConfiguration : IEntityTypeConfiguration<UsuarioFornecedor>
{
    public void Configure(EntityTypeBuilder<UsuarioFornecedor> builder)
    {
        // Tabela
        builder.ToTable("UsuarioFornecedor");

        // Chave primária
        builder.HasKey(uf => uf.Id);

        // Propriedades básicas
        builder.Property(uf => uf.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(uf => uf.UsuarioId)
            .HasColumnName("UsuarioId")
            .IsRequired();

        builder.Property(uf => uf.FornecedorId)
            .HasColumnName("FornecedorId")
            .IsRequired();

        builder.Property(uf => uf.Role)
            .HasColumnName("Role")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(uf => uf.Ativo)
            .HasColumnName("Ativo")
            .IsRequired();

        builder.Property(uf => uf.DataInicio)
            .HasColumnName("DataInicio")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(uf => uf.DataFim)
            .HasColumnName("DataFim")
            .HasColumnType("timestamptz");

        builder.Property(uf => uf.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(uf => uf.DataAtualizacao)
            .HasColumnName("DataAtualizacao")
            .HasColumnType("timestamptz");

        // Relacionamentos
        builder.HasOne(uf => uf.Usuario)
            .WithMany()
            .HasForeignKey(uf => uf.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uf => uf.Fornecedor)
            .WithMany(f => f.UsuariosFornecedores)
            .HasForeignKey(uf => uf.FornecedorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(uf => uf.Territorios)
            .WithOne(t => t.UsuarioFornecedor)
            .HasForeignKey(t => t.UsuarioFornecedorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(uf => uf.UsuarioId)
            .HasDatabaseName("IX_UsuarioFornecedor_UsuarioId");

        builder.HasIndex(uf => uf.FornecedorId)
            .HasDatabaseName("IX_UsuarioFornecedor_FornecedorId");

        builder.HasIndex(uf => new { uf.UsuarioId, uf.FornecedorId })
            .HasDatabaseName("IX_UsuarioFornecedor_Usuario_Fornecedor")
            .IsUnique();

        builder.HasIndex(uf => uf.Role)
            .HasDatabaseName("IX_UsuarioFornecedor_Role");

        builder.HasIndex(uf => uf.Ativo)
            .HasDatabaseName("IX_UsuarioFornecedor_Ativo");

        builder.HasIndex(uf => uf.DataInicio)
            .HasDatabaseName("IX_UsuarioFornecedor_DataInicio");

        builder.HasIndex(uf => uf.DataCriacao)
            .HasDatabaseName("IX_UsuarioFornecedor_DataCriacao");

        // Configurações adicionais
        builder.Navigation(uf => uf.Usuario)
            .EnableLazyLoading(false);

        builder.Navigation(uf => uf.Fornecedor)
            .EnableLazyLoading(false);

        builder.Navigation(uf => uf.Territorios)
            .EnableLazyLoading(false);
    }
}