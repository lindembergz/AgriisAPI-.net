using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Produtores.Dominio.Entidades;

namespace Agriis.Produtores.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade UsuarioProdutor
/// </summary>
public class UsuarioProdutorConfiguration : IEntityTypeConfiguration<UsuarioProdutor>
{
    public void Configure(EntityTypeBuilder<UsuarioProdutor> builder)
    {
        // Tabela
        builder.ToTable("UsuarioProdutor");

        // Chave primária
        builder.HasKey(up => up.Id);

        // Propriedades
        builder.Property(up => up.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(up => up.UsuarioId)
            .HasColumnName("UsuarioId")
            .IsRequired();

        builder.Property(up => up.ProdutorId)
            .HasColumnName("ProdutorId")
            .IsRequired();

        builder.Property(up => up.EhProprietario)
            .HasColumnName("EhProprietario")
            .IsRequired();

        builder.Property(up => up.Ativo)
            .HasColumnName("Ativo")
            .IsRequired();

        builder.Property(up => up.DataCriacao)
            .HasColumnName("DataCriacao")
            .IsRequired();

        builder.Property(up => up.DataAtualizacao)
            .HasColumnName("DataAtualizacao");

        // Relacionamentos
        builder.HasOne(up => up.Usuario)
            .WithMany()
            .HasForeignKey(up => up.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(up => up.Produtor)
            .WithMany(p => p.UsuariosProdutores)
            .HasForeignKey(up => up.ProdutorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(up => up.UsuarioId)
            .HasDatabaseName("IX_UsuarioProdutor_UsuarioId");

        builder.HasIndex(up => up.ProdutorId)
            .HasDatabaseName("IX_UsuarioProdutor_ProdutorId");

        builder.HasIndex(up => new { up.UsuarioId, up.ProdutorId })
            .HasDatabaseName("IX_UsuarioProdutor_UsuarioId_ProdutorId")
            .IsUnique();

        builder.HasIndex(up => up.EhProprietario)
            .HasDatabaseName("IX_UsuarioProdutor_EhProprietario");

        builder.HasIndex(up => up.Ativo)
            .HasDatabaseName("IX_UsuarioProdutor_Ativo");

        // Configurações adicionais
        builder.Navigation(up => up.Usuario)
            .EnableLazyLoading(false);

        builder.Navigation(up => up.Produtor)
            .EnableLazyLoading(false);
    }
}