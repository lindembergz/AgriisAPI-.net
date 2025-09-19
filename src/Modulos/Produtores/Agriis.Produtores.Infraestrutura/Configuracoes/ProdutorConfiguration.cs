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

        builder.Property(p => p.DataAutorizacao)
            .HasColumnName("DataAutorizacao")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("Status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.UsuarioAutorizacaoId)
            .HasColumnName("UsuarioAutorizacaoId");

        builder.Property(p => p.DataCriacao)
            .HasColumnName("DataCriacao")
            .IsRequired();

        builder.Property(p => p.DataAtualizacao)
            .HasColumnName("DataAtualizacao");

        // Objetos de valor
        builder.OwnsOne(p => p.Cpf, cpf =>
        {
            cpf.Property(c => c.Valor)
                .HasColumnName("Cpf")
                .HasMaxLength(11);
        });

        builder.OwnsOne(p => p.Cnpj, cnpj =>
        {
            cnpj.Property(c => c.Valor)
                .HasColumnName("Cnpj")
                .HasMaxLength(14);
        });

        builder.OwnsOne(p => p.AreaPlantio, area =>
        {
            area.Property(a => a.Valor)
                .HasColumnName("AreaPlantio")
                .HasColumnType("decimal(18,4)")
                .IsRequired();
        });

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
        builder.HasIndex(p => p.Cpf!.Valor)
            .HasDatabaseName("IX_Produtor_Cpf")
            .IsUnique()
            .HasFilter("\"Cpf\" IS NOT NULL");

        builder.HasIndex(p => p.Cnpj!.Valor)
            .HasDatabaseName("IX_Produtor_Cnpj")
            .IsUnique()
            .HasFilter("\"Cnpj\" IS NOT NULL");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Produtor_Status");

        builder.HasIndex(p => p.DataCriacao)
            .HasDatabaseName("IX_Produtor_DataCriacao");

        builder.HasIndex(p => p.AreaPlantio!.Valor)
            .HasDatabaseName("IX_Produtor_AreaPlantio");

        // Configurações adicionais
        builder.Navigation(p => p.UsuariosProdutores)
            .EnableLazyLoading(false);
    }
}