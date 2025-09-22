using Agriis.Compartilhado.Dominio.ObjetosValor;
using FluentAssertions;
using Xunit;

namespace Agriis.Tests.Unit.ObjetosValor;

/// <summary>
/// Testes unitários para o objeto de valor CNPJ
/// </summary>
public class CnpjTests
{
    [Fact]
    public void Cnpj_DeveCriarComCnpjValido()
    {
        // Arrange
        var cnpjValido = "11222333000181"; // CNPJ válido

        // Act
        var cnpj = new Cnpj(cnpjValido);

        // Assert
        cnpj.Valor.Should().Be(cnpjValido);
        cnpj.ValorFormatado.Should().Be("11.222.333/0001-81");
    }

    [Fact]
    public void Cnpj_DeveCriarComCnpjFormatado()
    {
        // Arrange
        var cnpjFormatado = "11.222.333/0001-81";

        // Act
        var cnpj = new Cnpj(cnpjFormatado);

        // Assert
        cnpj.Valor.Should().Be("11222333000181");
        cnpj.ValorFormatado.Should().Be(cnpjFormatado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cnpj_DeveLancarExcecaoParaValorVazio(string cnpjInvalido)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Cnpj(cnpjInvalido));
        exception.Message.Should().Contain("CNPJ não pode ser vazio ou nulo");
    }

    [Fact]
    public void Cnpj_DeveLancarExcecaoParaValorNulo()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Cnpj(null!));
        exception.Message.Should().Contain("CNPJ não pode ser vazio ou nulo");
    }

    [Theory]
    [InlineData("1234567890123")] // Muito curto
    [InlineData("123456789012345")] // Muito longo
    [InlineData("11111111111111")] // Todos os dígitos iguais
    [InlineData("12345678901234")] // Dígitos verificadores inválidos
    public void Cnpj_DeveLancarExcecaoParaCnpjInvalido(string cnpjInvalido)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Cnpj(cnpjInvalido));
        exception.Message.Should().Contain("CNPJ inválido");
    }

    [Fact]
    public void Cnpj_DeveSerIgualQuandoValoresIguais()
    {
        // Arrange
        var cnpj1 = new Cnpj("11222333000181");
        var cnpj2 = new Cnpj("11.222.333/0001-81");

        // Act & Assert
        cnpj1.Should().Be(cnpj2);
        cnpj1.GetHashCode().Should().Be(cnpj2.GetHashCode());
    }

    [Fact]
    public void Cnpj_DeveSerDiferenteQuandoValoresDiferentes()
    {
        // Arrange
        var cnpj1 = new Cnpj("11222333000181");
        var cnpj2 = new Cnpj("22333444000195");

        // Act & Assert
        cnpj1.Should().NotBe(cnpj2);
    }

    [Fact]
    public void Cnpj_DeveConverterImplicitamenteDeString()
    {
        // Arrange
        string cnpjString = "11222333000181";

        // Act
        Cnpj cnpj = cnpjString;

        // Assert
        cnpj.Valor.Should().Be(cnpjString);
    }

    [Fact]
    public void Cnpj_DeveConverterImplicitamenteParaString()
    {
        // Arrange
        var cnpj = new Cnpj("11222333000181");

        // Act
        string cnpjString = cnpj;

        // Assert
        cnpjString.Should().Be("11222333000181");
    }

    [Fact]
    public void Cnpj_ToString_DeveRetornarValorFormatado()
    {
        // Arrange
        var cnpj = new Cnpj("11222333000181");

        // Act
        var resultado = cnpj.ToString();

        // Assert
        resultado.Should().Be("11.222.333/0001-81");
    }

    [Theory]
    [InlineData("11222333000181", true)]
    [InlineData("22333444000195", true)]
    [InlineData("33444555000109", true)]
    [InlineData("12345678901234", false)]
    [InlineData("11111111111111", false)]
    [InlineData("00000000000000", false)]
    public void Cnpj_DeveValidarCorretamente(string cnpjTeste, bool esperado)
    {
        // Act & Assert
        if (esperado)
        {
            var cnpj = new Cnpj(cnpjTeste);
            cnpj.Valor.Should().Be(cnpjTeste);
        }
        else
        {
            Assert.Throws<ArgumentException>(() => new Cnpj(cnpjTeste));
        }
    }
}