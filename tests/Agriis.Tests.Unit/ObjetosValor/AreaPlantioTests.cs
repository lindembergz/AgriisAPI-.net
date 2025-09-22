using Agriis.Compartilhado.Dominio.ObjetosValor;
using FluentAssertions;
using Xunit;

namespace Agriis.Tests.Unit.ObjetosValor;

/// <summary>
/// Testes unitários para o objeto de valor AreaPlantio
/// </summary>
public class AreaPlantioTests
{
    [Fact]
    public void AreaPlantio_DeveCriarComValorValido()
    {
        // Arrange
        var valor = 100.5m;

        // Act
        var area = new AreaPlantio(valor);

        // Assert
        area.Valor.Should().Be(valor);
        area.ValorFormatado.Should().Be("100,50 ha");
    }

    [Fact]
    public void AreaPlantio_DeveArredondarParaQuatroCasasDecimais()
    {
        // Arrange
        var valor = 100.123456789m;

        // Act
        var area = new AreaPlantio(valor);

        // Assert
        area.Valor.Should().Be(100.1235m);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.1)]
    public void AreaPlantio_DeveLancarExcecaoParaValorNegativo(decimal valorInvalido)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new AreaPlantio(valorInvalido));
        exception.Message.Should().Contain("Área de plantio não pode ser negativa");
    }

    [Fact]
    public void AreaPlantio_DeveLancarExcecaoParaValorMuitoGrande()
    {
        // Arrange
        var valorMuitoGrande = 1_000_001m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new AreaPlantio(valorMuitoGrande));
        exception.Message.Should().Contain("Área de plantio excede o limite máximo permitido");
    }

    [Fact]
    public void AreaPlantio_DeveCalcularMetrosQuadrados()
    {
        // Arrange
        var area = new AreaPlantio(1m); // 1 hectare

        // Act
        var metrosQuadrados = area.EmMetrosQuadrados;

        // Assert
        metrosQuadrados.Should().Be(10000m);
    }

    [Fact]
    public void AreaPlantio_DeveCalcularAlqueiresPaulistas()
    {
        // Arrange
        var area = new AreaPlantio(2.42m); // 1 alqueire paulista

        // Act
        var alqueires = area.EmAlqueiresPaulistas;

        // Assert
        alqueires.Should().BeApproximately(1m, 0.01m);
    }

    [Fact]
    public void AreaPlantio_DeveCalcularAlqueiresMineiros()
    {
        // Arrange
        var area = new AreaPlantio(4.84m); // 1 alqueire mineiro

        // Act
        var alqueires = area.EmAlqueiresMineiros;

        // Assert
        alqueires.Should().BeApproximately(1m, 0.01m);
    }

    [Fact]
    public void AreaPlantio_DeveSomarAreas()
    {
        // Arrange
        var area1 = new AreaPlantio(100m);
        var area2 = new AreaPlantio(50m);

        // Act
        var resultado = area1 + area2;

        // Assert
        resultado.Valor.Should().Be(150m);
    }

    [Fact]
    public void AreaPlantio_DeveSubtrairAreas()
    {
        // Arrange
        var area1 = new AreaPlantio(100m);
        var area2 = new AreaPlantio(30m);

        // Act
        var resultado = area1 - area2;

        // Assert
        resultado.Valor.Should().Be(70m);
    }

    [Fact]
    public void AreaPlantio_DeveMultiplicarPorFator()
    {
        // Arrange
        var area = new AreaPlantio(100m);
        var fator = 2m;

        // Act
        var resultado = area * fator;

        // Assert
        resultado.Valor.Should().Be(200m);
    }

    [Fact]
    public void AreaPlantio_DeveDividirPorFator()
    {
        // Arrange
        var area = new AreaPlantio(100m);
        var fator = 2m;

        // Act
        var resultado = area / fator;

        // Assert
        resultado.Valor.Should().Be(50m);
    }

    [Fact]
    public void AreaPlantio_DeveLancarExcecaoAoDividirPorZero()
    {
        // Arrange
        var area = new AreaPlantio(100m);

        // Act & Assert
        Assert.Throws<DivideByZeroException>(() => area / 0);
    }

    [Fact]
    public void AreaPlantio_DeveCompararMaior()
    {
        // Arrange
        var area1 = new AreaPlantio(100m);
        var area2 = new AreaPlantio(50m);

        // Act & Assert
        (area1 > area2).Should().BeTrue();
        (area2 > area1).Should().BeFalse();
    }

    [Fact]
    public void AreaPlantio_DeveCompararMenor()
    {
        // Arrange
        var area1 = new AreaPlantio(50m);
        var area2 = new AreaPlantio(100m);

        // Act & Assert
        (area1 < area2).Should().BeTrue();
        (area2 < area1).Should().BeFalse();
    }

    [Fact]
    public void AreaPlantio_DeveCompararMaiorOuIgual()
    {
        // Arrange
        var area1 = new AreaPlantio(100m);
        var area2 = new AreaPlantio(100m);
        var area3 = new AreaPlantio(50m);

        // Act & Assert
        (area1 >= area2).Should().BeTrue();
        (area1 >= area3).Should().BeTrue();
        (area3 >= area1).Should().BeFalse();
    }

    [Fact]
    public void AreaPlantio_DeveCompararMenorOuIgual()
    {
        // Arrange
        var area1 = new AreaPlantio(100m);
        var area2 = new AreaPlantio(100m);
        var area3 = new AreaPlantio(150m);

        // Act & Assert
        (area1 <= area2).Should().BeTrue();
        (area1 <= area3).Should().BeTrue();
        (area3 <= area1).Should().BeFalse();
    }

    [Fact]
    public void AreaPlantio_DeveSerIgualQuandoValoresIguais()
    {
        // Arrange
        var area1 = new AreaPlantio(100m);
        var area2 = new AreaPlantio(100m);

        // Act & Assert
        area1.Should().Be(area2);
        area1.GetHashCode().Should().Be(area2.GetHashCode());
    }

    [Fact]
    public void AreaPlantio_DeveConverterImplicitamenteDeDecimal()
    {
        // Arrange
        decimal valor = 100.5m;

        // Act
        AreaPlantio area = valor;

        // Assert
        area.Valor.Should().Be(valor);
    }

    [Fact]
    public void AreaPlantio_DeveConverterImplicitamenteParaDecimal()
    {
        // Arrange
        var area = new AreaPlantio(100.5m);

        // Act
        decimal valor = area;

        // Assert
        valor.Should().Be(100.5m);
    }

    [Fact]
    public void AreaPlantio_DeveCrearDeMetrosQuadrados()
    {
        // Arrange
        var metrosQuadrados = 10000m; // 1 hectare

        // Act
        var area = AreaPlantio.DeMetrosQuadrados(metrosQuadrados);

        // Assert
        area.Valor.Should().Be(1m);
    }

    [Fact]
    public void AreaPlantio_DeveCrearDeAlqueiresPaulistas()
    {
        // Arrange
        var alqueires = 1m;

        // Act
        var area = AreaPlantio.DeAlqueiresPaulistas(alqueires);

        // Assert
        area.Valor.Should().Be(2.42m);
    }

    [Fact]
    public void AreaPlantio_DeveCrearDeAlqueiresMineiros()
    {
        // Arrange
        var alqueires = 1m;

        // Act
        var area = AreaPlantio.DeAlqueiresMineiros(alqueires);

        // Assert
        area.Valor.Should().Be(4.84m);
    }

    [Fact]
    public void AreaPlantio_ToString_DeveRetornarValorFormatado()
    {
        // Arrange
        var area = new AreaPlantio(100.5m);

        // Act
        var resultado = area.ToString();

        // Assert
        resultado.Should().Be("100,50 ha");
    }
}