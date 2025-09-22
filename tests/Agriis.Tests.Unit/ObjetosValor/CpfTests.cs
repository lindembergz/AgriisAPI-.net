using Agriis.Compartilhado.Dominio.ObjetosValor;
using FluentAssertions;
using Xunit;

namespace Agriis.Tests.Unit.ObjetosValor;

/// <summary>
/// Testes unitários para o objeto de valor CPF
/// </summary>
public class CpfTests
{
    [Fact]
    public void Cpf_DeveCriarComCpfValido()
    {
        // Arrange
        var cpfValido = "11144477735"; // CPF válido

        // Act
        var cpf = new Cpf(cpfValido);

        // Assert
        cpf.Valor.Should().Be(cpfValido);
        cpf.ValorFormatado.Should().Be("111.444.777-35");
    }

    [Fact]
    public void Cpf_DeveCriarComCpfFormatado()
    {
        // Arrange
        var cpfFormatado = "111.444.777-35";

        // Act
        var cpf = new Cpf(cpfFormatado);

        // Assert
        cpf.Valor.Should().Be("11144477735");
        cpf.ValorFormatado.Should().Be(cpfFormatado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cpf_DeveLancarExcecaoParaValorVazio(string cpfInvalido)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Cpf(cpfInvalido));
        exception.Message.Should().Contain("CPF não pode ser vazio ou nulo");
    }

    [Fact]
    public void Cpf_DeveLancarExcecaoParaValorNulo()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Cpf(null!));
        exception.Message.Should().Contain("CPF não pode ser vazio ou nulo");
    }

    [Theory]
    [InlineData("123456789")] // Muito curto
    [InlineData("123456789012")] // Muito longo
    [InlineData("11111111111")] // Todos os dígitos iguais
    [InlineData("12345678901")] // Dígitos verificadores inválidos
    public void Cpf_DeveLancarExcecaoParaCpfInvalido(string cpfInvalido)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Cpf(cpfInvalido));
        exception.Message.Should().Contain("CPF inválido");
    }

    [Fact]
    public void Cpf_DeveSerIgualQuandoValoresIguais()
    {
        // Arrange
        var cpf1 = new Cpf("11144477735");
        var cpf2 = new Cpf("111.444.777-35");

        // Act & Assert
        cpf1.Should().Be(cpf2);
        cpf1.GetHashCode().Should().Be(cpf2.GetHashCode());
    }

    [Fact]
    public void Cpf_DeveSerDiferenteQuandoValoresDiferentes()
    {
        // Arrange
        var cpf1 = new Cpf("11144477735");
        var cpf2 = new Cpf("22255588846");

        // Act & Assert
        cpf1.Should().NotBe(cpf2);
    }

    [Fact]
    public void Cpf_DeveConverterImplicitamenteDeString()
    {
        // Arrange
        string cpfString = "11144477735";

        // Act
        Cpf cpf = cpfString;

        // Assert
        cpf.Valor.Should().Be(cpfString);
    }

    [Fact]
    public void Cpf_DeveConverterImplicitamenteParaString()
    {
        // Arrange
        var cpf = new Cpf("11144477735");

        // Act
        string cpfString = cpf;

        // Assert
        cpfString.Should().Be("11144477735");
    }

    [Fact]
    public void Cpf_ToString_DeveRetornarValorFormatado()
    {
        // Arrange
        var cpf = new Cpf("11144477735");

        // Act
        var resultado = cpf.ToString();

        // Assert
        resultado.Should().Be("111.444.777-35");
    }

    [Theory]
    [InlineData("11144477735", true)]
    [InlineData("22255588846", true)]
    [InlineData("33366699957", true)]
    [InlineData("12345678901", false)]
    [InlineData("11111111111", false)]
    [InlineData("00000000000", false)]
    public void Cpf_DeveValidarCorretamente(string cpfTeste, bool esperado)
    {
        // Act & Assert
        if (esperado)
        {
            var cpf = new Cpf(cpfTeste);
            cpf.Valor.Should().Be(cpfTeste);
        }
        else
        {
            Assert.Throws<ArgumentException>(() => new Cpf(cpfTeste));
        }
    }
}