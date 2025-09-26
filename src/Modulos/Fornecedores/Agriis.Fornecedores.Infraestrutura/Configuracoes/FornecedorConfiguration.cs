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

        builder.Property(f => f.Logradouro)
            .HasColumnName("Logradouro")
            .HasMaxLength(500);

        builder.Property(f => f.UfId)
            .HasColumnName("UfId");

        builder.Property(f => f.MunicipioId)
            .HasColumnName("MunicipioId");

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

        builder.Property(f => f.Moeda)
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

        // Relacionamento com UF (tabela estados)
        builder.HasOne(f => f.Uf)
            .WithMany()
            .HasForeignKey(f => f.UfId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Fornecedor_Estados_UfId");

        // Relacionamento com Municipio (tabela municipios_referencia)
        builder.HasOne(f => f.Municipio)
            .WithMany()
            .HasForeignKey(f => f.MunicipioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Fornecedor_MunicipiosReferencia_MunicipioId");

        // Índices
        builder.HasIndex("Cnpj")
            .HasDatabaseName("IX_Fornecedor_Cnpj")
            .IsUnique();

        builder.HasIndex(f => f.Nome)
            .HasDatabaseName("IX_Fornecedor_Nome");

        builder.HasIndex(f => f.Ativo)
            .HasDatabaseName("IX_Fornecedor_Ativo");

        builder.HasIndex(f => f.Moeda)
            .HasDatabaseName("IX_Fornecedor_MoedaPadrao");

        builder.HasIndex(f => f.DataCriacao)
            .HasDatabaseName("IX_Fornecedor_DataCriacao");

        builder.HasIndex(f => f.Email)
            .HasDatabaseName("IX_Fornecedor_Email")
            .HasFilter("\"Email\" IS NOT NULL");

        builder.HasIndex(f => f.UfId)
            .HasDatabaseName("IX_Fornecedor_UfId");

        builder.HasIndex(f => f.MunicipioId)
            .HasDatabaseName("IX_Fornecedor_MunicipioId");

        // Configurações adicionais
        builder.Navigation(f => f.UsuariosFornecedores)
            .EnableLazyLoading(false);
    }
}