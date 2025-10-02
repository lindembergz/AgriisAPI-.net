using Agriis.Enderecos.Dominio.Entidades;
using FluentAssertions;
using Xunit;

namespace Agriis.Tests.Unit.Enderecos;

/// <summary>
/// Testes unitários para a entidade Estado
/// </summary>
public class EstadoTests
{
    [Fact]
    public void Estado_Constructor_ShouldCreateValidInstance()
    {
        // Arrange
        var nome = "São Paulo";
        var uf = "SP";
        var codigoIbge = 35;
        var regiao = "Sudeste";

        // Act
        var estado = new Estado(nome, uf, codigoIbge, regiao);

        // Assert
        estado.Nome.Should().Be(nome);
        estado.Uf.Should().Be(uf);
        estado.CodigoIbge.Should().Be(codigoIbge);
        estado.Regiao.Should().Be(regiao);
        estado.Municipios.Should().NotBeNull().And.BeEmpty();
        estado.Enderecos.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Estado_Constructor_ShouldNormalizeUfToUpperCase()
    {
        // Arrange
        var uf = "sp";

        // Act
        var estado = new Estado("São Paulo", uf, 35, "Sudeste");

        // Assert
        estado.Uf.Should().Be("SP");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Estado_Constructor_ShouldThrowException_WhenNomeIsInvalid(string nome)
    {
        // Act & Assert
        var act = () => new Estado(nome, "SP", 35, "Sudeste");
        act.Should().Throw<ArgumentException>()
            .WithMessage("Nome do estado é obrigatório*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("S")]
    [InlineData("SPP")]
    public void Estado_Constructor_ShouldThrowException_WhenUfIsInvalid(string uf)
    {
        // Act & Assert
        var act = () => new Estado("São Paulo", uf, 35, "Sudeste");
        act.Should().Throw<ArgumentException>()
            .WithMessage("UF deve ter exatamente 2 caracteres*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Estado_Constructor_ShouldThrowException_WhenCodigoIbgeIsInvalid(int codigoIbge)
    {
        // Act & Assert
        var act = () => new Estado("São Paulo", "SP", codigoIbge, "Sudeste");
        act.Should().Throw<ArgumentException>()
            .WithMessage("Código IBGE deve ser maior que zero*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Estado_Constructor_ShouldThrowException_WhenRegiaoIsInvalid(string regiao)
    {
        // Act & Assert
        var act = () => new Estado("São Paulo", "SP", 35, regiao);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Região é obrigatória*");
    }

    [Fact]
    public void Estado_Atualizar_ShouldUpdateAllProperties()
    {
        // Arrange
        var estado = new Estado("São Paulo", "SP", 35, "Sudeste");
        var novoNome = "Estado de São Paulo";
        var novaUf = "SP";
        var novoCodigoIbge = 35;
        var novaRegiao = "Sudeste";

        // Act
        estado.Atualizar(novoNome, novaUf, novoCodigoIbge, novaRegiao);

        // Assert
        estado.Nome.Should().Be(novoNome);
        estado.Uf.Should().Be(novaUf);
        estado.CodigoIbge.Should().Be(novoCodigoIbge);
        estado.Regiao.Should().Be(novaRegiao);
    }

    [Fact]
    public void Estado_Atualizar_ShouldUpdateDataAtualizacao()
    {
        // Arrange
        var estado = new Estado("São Paulo", "SP", 35, "Sudeste");
        var dataAtualizacaoAnterior = estado.DataAtualizacao;

        // Act
        Thread.Sleep(10); // Ensure time difference
        estado.Atualizar("Estado de São Paulo", "SP", 35, "Sudeste");

        // Assert
        estado.DataAtualizacao.Should().BeAfter(dataAtualizacaoAnterior);
    }

    [Fact]
    public void Estado_Atualizar_ShouldValidateParameters()
    {
        // Arrange
        var estado = new Estado("São Paulo", "SP", 35, "Sudeste");

        // Act & Assert
        var act = () => estado.Atualizar("", "SP", 35, "Sudeste");
        act.Should().Throw<ArgumentException>()
            .WithMessage("Nome do estado é obrigatório*");
    }

    [Theory]
    [InlineData("Norte")]
    [InlineData("Nordeste")]
    [InlineData("Centro-Oeste")]
    [InlineData("Sudeste")]
    [InlineData("Sul")]
    public void Estado_ShouldAcceptValidRegioes(string regiao)
    {
        // Act
        var act = () => new Estado("Estado Teste", "TE", 99, regiao);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Estado_ShouldHaveCorrectNavigationProperties()
    {
        // Arrange & Act
        var estado = new Estado("São Paulo", "SP", 35, "Sudeste");

        // Assert
        estado.Municipios.Should().NotBeNull("Municipios collection should be initialized");
        estado.Enderecos.Should().NotBeNull("Enderecos collection should be initialized");
        estado.Municipios.Should().BeEmpty("Municipios collection should start empty");
        estado.Enderecos.Should().BeEmpty("Enderecos collection should start empty");
    }
}