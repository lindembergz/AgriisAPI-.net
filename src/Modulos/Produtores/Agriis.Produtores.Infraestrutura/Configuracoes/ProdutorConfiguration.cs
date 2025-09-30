using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Produtores.Dominio.Entidades;
using Agriis.Produtores.Dominio.Enums;

namespace Agriis.Produtores.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Produtor
/// </summary>
public class ProdutorConfiguration : IEntityTypeConfiguration<Produtor>
{
    public void Configure(EntityTypeBuilder<Produtor> builder)
    {
        // Tabela
        builder.ToTable("Produtor");

        // Chave primária
        builder.HasKey(p => p.Id);

        // Propriedades básicas
        builder.Property(p => p.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Nome)
            .HasColumnName("Nome")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.InscricaoEstadual)
            .HasColumnName("InscricaoEstadual")
            .HasMaxLength(50);

        builder.Property(p => p.TipoAtividade)
            .HasColumnName("TipoAtividade")
            .HasMaxLength(100);

        builder.Property(p => p.Telefone1)
            .HasColumnName("Telefone1")
            .HasMaxLength(20);

        builder.Property(p => p.Telefone2)
            .HasColumnName("Telefone2")
            .HasMaxLength(20);

        builder.Property(p => p.Telefone3)
            .HasColumnName("Telefone3")
            .HasMaxLength(20);

        builder.Property(p => p.Email)
            .HasColumnName("Email")
            .HasMaxLength(100);

        builder.Property(p => p.DataAutorizacao)
            .HasColumnName("DataAutorizacao")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("Status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.UsuarioAutorizacaoId)
            .HasColumnName("UsuarioAutorizacaoId");

        builder.Property(p => p.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(p => p.DataAtualizacao)
            .HasColumnName("DataAtualizacao")
            .HasColumnType("timestamptz");

        // Objetos de valor - configuração simplificada
        builder.Property(p => p.Cpf)
            .HasColumnName("Cpf")
            .HasMaxLength(11)
            .HasConversion(
                v => v != null ? v.Valor : null,
                v => v != null ? new Cpf(v) : null);

        builder.Property(p => p.Cnpj)
            .HasColumnName("Cnpj")
            .HasMaxLength(14)
            .HasConversion(
                v => v != null ? v.Valor : null,
                v => v != null ? new Cnpj(v) : null);

        builder.Property(p => p.AreaPlantio)
            .HasColumnName("AreaPlantio")
            .HasColumnType("decimal(18,4)")
            .IsRequired()
            .HasConversion(
                v => v.Valor,
                v => new AreaPlantio(v));

        // Propriedades JSON
        builder.Property(p => p.RetornosApiCheckProdutor)
            .HasColumnName("RetornosApiCheckProdutor")
            .HasColumnType("jsonb");

        builder.Property(p => p.Culturas)
            .HasColumnName("Culturas")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new List<int>());

        // Relacionamentos
        builder.HasOne(p => p.UsuarioAutorizacao)
            .WithMany()
            .HasForeignKey(p => p.UsuarioAutorizacaoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.UsuariosProdutores)
            .WithOne(up => up.Produtor)
            .HasForeignKey(up => up.ProdutorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex("Cpf")
            .HasDatabaseName("IX_Produtor_Cpf")
            .IsUnique()
            .HasFilter("\"Cpf\" IS NOT NULL");

        builder.HasIndex("Cnpj")
            .HasDatabaseName("IX_Produtor_Cnpj")
            .IsUnique()
            .HasFilter("\"Cnpj\" IS NOT NULL");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Produtor_Status");

        builder.HasIndex(p => p.DataCriacao)
            .HasDatabaseName("IX_Produtor_DataCriacao");

        builder.HasIndex("AreaPlantio")
            .HasDatabaseName("IX_Produtor_AreaPlantio");

        // Configurações adicionais
        builder.Navigation(p => p.UsuariosProdutores)
            .EnableLazyLoading(false);
    }
}