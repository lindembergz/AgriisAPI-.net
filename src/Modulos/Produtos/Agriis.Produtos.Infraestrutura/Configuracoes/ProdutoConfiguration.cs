using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.ObjetosValor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Produtos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Produto
/// </summary>
public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        // Tabela
        builder.ToTable("Produto", "public");

        // Chave primária
        builder.HasKey(p => p.Id);

        // Propriedades básicas
        builder.Property(p => p.Nome)
            .HasColumnName("Nome")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Descricao)
            .HasColumnName("Descricao")
            .HasMaxLength(1000);

        builder.Property(p => p.Codigo)
            .HasColumnName("Codigo")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Marca)
            .HasColumnName("Marca")
            .HasMaxLength(100);

        builder.Property(p => p.Tipo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.UnidadeMedidaId)
            .HasColumnName("UnidadeMedidaId")
            .IsRequired();

        builder.Property(p => p.TipoCalculoPeso)
            .HasColumnName("TipoCalculoPeso")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.ProdutoRestrito)
            .HasColumnName("ProdutoRestrito")
            .IsRequired();

        builder.Property(p => p.ObservacoesRestricao)
            .HasColumnName("ObservacoesRestricao")
            .HasMaxLength(500);

        builder.Property(p => p.CategoriaId)
            .HasColumnName("CategoriaId")
            .IsRequired();

        builder.Property(p => p.FornecedorId)
            .HasColumnName("FornecedorId")
            .IsRequired();

        builder.Property(p => p.ProdutoPaiId)
            .HasColumnName("ProdutoPaiId");

        builder.Property(p => p.EmbalagemId)
            .HasColumnName("EmbalagemId");

        builder.Property(p => p.AtividadeAgropecuariaId)
            .HasColumnName("AtividadeAgropecuariaId");

        // Configuração do objeto de valor DimensoesProduto
        builder.OwnsOne(p => p.Dimensoes, dimensoes =>
        {
            dimensoes.Property(d => d.Altura)
                .IsRequired()
                .HasColumnName("Altura")
                .HasPrecision(10, 2);

            dimensoes.Property(d => d.Largura)
                .IsRequired()
                .HasColumnName("Largura")
                .HasPrecision(10, 2);

            dimensoes.Property(d => d.Comprimento)
                .IsRequired()
                .HasColumnName("Comprimento")
                .HasPrecision(10, 2);

            dimensoes.Property(d => d.PesoNominal)
                .IsRequired()
                .HasColumnName("PesoNominal")
                .HasPrecision(10, 3);

            // Novos campos para compatibilidade com sistema Python
            dimensoes.Property(d => d.PesoEmbalagem)
                .IsRequired()
                .HasColumnName("PesoEmbalagem")
                .HasPrecision(10, 3);

            dimensoes.Property(d => d.Pms)
                .HasColumnName("Pms")
                .HasPrecision(10, 3);

            dimensoes.Property(d => d.QuantidadeMinima)
                .IsRequired()
                .HasColumnName("QuantidadeMinima")
                .HasPrecision(10, 3);

            dimensoes.Property(d => d.Embalagem)
                .IsRequired()
                .HasColumnName("Embalagem")
                .HasMaxLength(100);

            dimensoes.Property(d => d.FaixaDensidadeInicial)
                .HasColumnName("FaixaDensidadeInicial")
                .HasPrecision(10, 3);

            dimensoes.Property(d => d.FaixaDensidadeFinal)
                .HasColumnName("FaixaDensidadeFinal")
                .HasPrecision(10, 3);
        });

        // Configuração de JSON para dados adicionais
        builder.Property(p => p.DadosAdicionais)
            .HasColumnType("jsonb");

        // Propriedades de auditoria
        builder.Property(p => p.DataCriacao)
            .IsRequired();

        builder.Property(p => p.DataAtualizacao);

        // Relacionamentos
        builder.HasOne(p => p.Categoria)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Produto_Categoria_CategoriaId");

        builder.HasOne(p => p.ProdutoPai)
            .WithMany(p => p.ProdutosFilhos)
            .HasForeignKey(p => p.ProdutoPaiId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.UnidadeMedida)
            .WithMany()
            .HasForeignKey(p => p.UnidadeMedidaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Produto_UnidadeMedida_UnidadeMedidaId");

        builder.HasOne(p => p.Embalagem)
            .WithMany()
            .HasForeignKey(p => p.EmbalagemId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Produto_Embalagem_EmbalagemId");

        builder.HasOne(p => p.AtividadeAgropecuaria)
            .WithMany()
            .HasForeignKey(p => p.AtividadeAgropecuariaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Produto_AtividadeAgropecuaria_AtividadeAgropecuariaId");

        builder.HasMany(p => p.ProdutosCulturas)
            .WithOne(pc => pc.Produto)
            .HasForeignKey(pc => pc.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(p => p.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_Produtos_Codigo");

        builder.HasIndex(p => p.Nome)
            .HasDatabaseName("IX_Produtos_Nome");

        builder.HasIndex(p => p.CategoriaId)
            .HasDatabaseName("IX_Produtos_CategoriaId");

        builder.HasIndex(p => p.FornecedorId)
            .HasDatabaseName("IX_Produtos_FornecedorId");

        builder.HasIndex(p => p.ProdutoPaiId)
            .HasDatabaseName("IX_Produtos_ProdutoPaiId");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Produtos_Status");

        builder.HasIndex(p => p.Tipo)
            .HasDatabaseName("IX_Produtos_Tipo");

        builder.HasIndex(p => p.ProdutoRestrito)
            .HasDatabaseName("IX_Produtos_ProdutoRestrito");

        builder.HasIndex(p => p.UnidadeMedidaId)
            .HasDatabaseName("IX_Produtos_UnidadeMedidaId");

        builder.HasIndex(p => p.EmbalagemId)
            .HasDatabaseName("IX_Produtos_EmbalagemId");

        builder.HasIndex(p => p.AtividadeAgropecuariaId)
            .HasDatabaseName("IX_Produtos_AtividadeAgropecuariaId");
    }
}