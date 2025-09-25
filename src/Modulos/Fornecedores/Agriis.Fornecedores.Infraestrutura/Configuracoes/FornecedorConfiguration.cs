using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Fornecedores.Dominio.Entidades;

namespace Agriis.Fornecedores.Infraestrutura.Configuracoes;

/// <summary>
/// Configuração do Entity Framework para a entidade Fornecedor
/// </summary>
public class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        // Tabela
        builder.ToTable("Fornecedor");

        // Chave primária
        builder.HasKey(f => f.Id);

        // Propriedades básicas
        builder.Property(f => f.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(f => f.Nome)
            .HasColumnName("Nome")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.InscricaoEstadual)
            .HasColumnName("InscricaoEstadual")
            .HasMaxLength(50);

        builder.Property(f => f.Endereco)
            .HasColumnName("Endereco")
            .HasMaxLength(500);

        builder.Property(f => f.Municipio)
            .HasColumnName("Municipio")
            .HasMaxLength(100);

        builder.Property(f => f.Uf)
            .HasColumnName("Uf")
            .HasMaxLength(2);

        builder.Property(f => f.Cep)
            .HasColumnName("Cep")
            .HasMaxLength(10);

        builder.Property(f => f.Complemento)
            .HasColumnName("Complemento")
            .HasMaxLength(200);

        builder.Property(f => f.Latitude)
            .HasColumnName("Latitude")
            .HasColumnType("decimal(10,8)");

        builder.Property(f => f.Longitude)
            .HasColumnName("Longitude")
            .HasColumnType("decimal(11,8)");

        builder.Property(f => f.Telefone)
            .HasColumnName("Telefone")
            .HasMaxLength(20);

        builder.Property(f => f.Email)
            .HasColumnName("Email")
            .HasMaxLength(100);

        builder.Property(f => f.LogoUrl)
            .HasColumnName("LogoUrl")
            .HasMaxLength(500);

        builder.Property(f => f.MoedaPadrao)
            .HasColumnName("MoedaPadrao")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(f => f.PedidoMinimo)
            .HasColumnName("PedidoMinimo")
            .HasColumnType("decimal(18,2)");

        builder.Property(f => f.TokenLincros)
            .HasColumnName("TokenLincros")
            .HasMaxLength(100);

        builder.Property(f => f.Ativo)
            .HasColumnName("Ativo")
            .IsRequired();

        builder.Property(f => f.DataCriacao)
            .HasColumnName("DataCriacao")
            .IsRequired();

        builder.Property(f => f.DataAtualizacao)
            .HasColumnName("DataAtualizacao");
            
        // Objeto de valor CNPJ
        builder.Property(f => f.Cnpj)
            .HasColumnName("Cnpj")
            .HasMaxLength(14)
            .IsRequired()
            .HasConversion(
                v => v.Valor,
                v => new Agriis.Compartilhado.Dominio.ObjetosValor.Cnpj(v));

        // Propriedades JSON
        builder.Property(f => f.DadosAdicionais)
            .HasColumnName("DadosAdicionais")
            .HasColumnType("jsonb");

        // Relacionamentos
        builder.HasMany(f => f.UsuariosFornecedores)
            .WithOne(uf => uf.Fornecedor)
            .HasForeignKey(uf => uf.FornecedorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex("Cnpj")
            .HasDatabaseName("IX_Fornecedor_Cnpj")
            .IsUnique();

        builder.HasIndex(f => f.Nome)
            .HasDatabaseName("IX_Fornecedor_Nome");

        builder.HasIndex(f => f.Ativo)
            .HasDatabaseName("IX_Fornecedor_Ativo");

        builder.HasIndex(f => f.MoedaPadrao)
            .HasDatabaseName("IX_Fornecedor_MoedaPadrao");

        builder.HasIndex(f => f.DataCriacao)
            .HasDatabaseName("IX_Fornecedor_DataCriacao");

        builder.HasIndex(f => f.Email)
            .HasDatabaseName("IX_Fornecedor_Email")
            .HasFilter("\"Email\" IS NOT NULL");

        // Configurações adicionais
        builder.Navigation(f => f.UsuariosFornecedores)
            .EnableLazyLoading(false);
    }
}