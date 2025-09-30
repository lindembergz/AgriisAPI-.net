using Agriis.Produtos.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Produtos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Categoria
/// </summary>
public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        // Tabela
        builder.ToTable("Categoria");

        // Chave primária
        builder.HasKey(c => c.Id);

        // Propriedades básicas
        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Descricao)
            .HasMaxLength(500);

        builder.Property(c => c.Tipo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.Ativo)
            .IsRequired();

        builder.Property(c => c.CategoriaPaiId);

        builder.Property(c => c.Ordem)
            .IsRequired();

        // Propriedades de auditoria
        builder.Property(c => c.DataCriacao)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(c => c.DataAtualizacao)
            .HasColumnType("timestamptz");

        // Relacionamentos
        builder.HasOne(c => c.CategoriaPai)
            .WithMany(c => c.SubCategorias)
            .HasForeignKey(c => c.CategoriaPaiId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.SubCategorias)
            .WithOne(c => c.CategoriaPai)
            .HasForeignKey(c => c.CategoriaPaiId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Produtos)
            .WithOne(p => p.Categoria)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(c => c.Nome)
            .IsUnique()
            .HasDatabaseName("IX_Categorias_Nome");

        builder.HasIndex(c => c.Tipo)
            .HasDatabaseName("IX_Categorias_Tipo");

        builder.HasIndex(c => c.CategoriaPaiId)
            .HasDatabaseName("IX_Categorias_CategoriaPaiId");

        builder.HasIndex(c => c.Ativo)
            .HasDatabaseName("IX_Categorias_Ativo");

        builder.HasIndex(c => c.Ordem)
            .HasDatabaseName("IX_Categorias_Ordem");
    }
}