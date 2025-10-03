using Agriis.Enderecos.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agriis.Enderecos.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Pais
/// </summary>
public class PaisConfiguration : IEntityTypeConfiguration<Pais>
{
    public void Configure(EntityTypeBuilder<Pais> builder)
    {
        // Tabela
        builder.ToTable("paises", "public");

        // Chave primária
        builder.HasKey(p => p.Id)
            .HasName("PK_paises");

        // Propriedades
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(p => p.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.Nome)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Ativo)
            .HasColumnName("ativo")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired();

        builder.Property(p => p.DataAtualizacao)
            .HasColumnName("data_atualizacao");



        // Índices
        builder.HasIndex(p => p.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_paises_codigo");

        builder.HasIndex(p => p.Nome)
            .HasDatabaseName("IX_paises_nome");

        builder.HasIndex(p => p.Ativo)
            .HasDatabaseName("IX_paises_ativo");

        builder.HasIndex(p => p.DataCriacao)
            .HasDatabaseName("IX_paises_data_criacao");

        builder.HasIndex(p => p.DataAtualizacao)
            .HasDatabaseName("IX_paises_data_atualizacao");

        // Relacionamentos
        builder.HasMany(p => p.Estados)
            .WithOne(e => e.Pais)
            .HasForeignKey(e => e.PaisId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_estados_paises");
    }
}