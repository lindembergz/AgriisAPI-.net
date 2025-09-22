using Agriis.Produtos.Dominio.ObjetosValor;
using FluentAssertions;
using Xunit;

namespace Agriis.Tests.Unit.ObjetosValor;

/// <summary>
/// Testes unitários para o objeto de valor DimensoesProduto
/// </summary>
public class DimensoesProdutoTests
{
    [Fact]
    public void DimensoesProduto_DeveCriarComValoresValidos()
    {
        // Arrange
        var altura = 10m;
        var largura = 20m;
        var comprimento = 30m;
        var pesoNominal = 25m;
        var densidade = 800m;

        // Act
        var dimensoes = new DimensoesProduto(altura, largura, comprimento, pesoNominal, densidade);

        // Assert
        dimensoes.Altura.Should().Be(altura);
        dimensoes.Largura.Should().Be(largura);
        dimensoes.Comprimento.Should().Be(comprimento);
        dimensoes.PesoNominal.Should().Be(pesoNominal);
        dimensoes.Densidade.Should().Be(densidade);
    }

    [Fact]
    public void DimensoesProduto_DeveCriarSemDensidade()
    {
        // Arrange
        var altura = 10m;
        var largura = 20m;
        var comprimento = 30m;
        var pesoNominal = 25m;

        // Act
        var dimensoes = new DimensoesProduto(altura, largura, comprimento, pesoNominal);

        // Assert
        dimensoes.Altura.Should().Be(altura);
        dimensoes.Largura.Should().Be(largura);
        dimensoes.Comprimento.Should().Be(comprimento);
        dimensoes.PesoNominal.Should().Be(pesoNominal);
        dimensoes.Densidade.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DimensoesProduto_DeveLancarExcecaoParaAlturaInvalida(decimal alturaInvalida)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new DimensoesProduto(alturaInvalida, 20, 30, 25));
        exception.Message.Should().Contain("Altura deve ser maior que zero");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DimensoesProduto_DeveLancarExcecaoParaLarguraInvalida(decimal larguraInvalida)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new DimensoesProduto(10, larguraInvalida, 30, 25));
        exception.Message.Should().Contain("Largura deve ser maior que zero");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DimensoesProduto_DeveLancarExcecaoParaComprimentoInvalido(decimal comprimentoInvalido)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new DimensoesProduto(10, 20, comprimentoInvalido, 25));
        exception.Message.Should().Contain("Comprimento deve ser maior que zero");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DimensoesProduto_DeveLancarExcecaoParaPesoNominalInvalido(decimal pesoInvalido)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new DimensoesProduto(10, 20, 30, pesoInvalido));
        exception.Message.Should().Contain("Peso nominal deve ser maior que zero");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DimensoesProduto_DeveLancarExcecaoParaDensidadeInvalida(decimal densidadeInvalida)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new DimensoesProduto(10, 20, 30, 25, densidadeInvalida));
        exception.Message.Should().Contain("Densidade deve ser maior que zero");
    }

    [Fact]
    public void DimensoesProduto_DeveCalcularVolumeCorretamente()
    {
        // Arrange
        var dimensoes = new DimensoesProduto(10, 20, 30, 25); // 10cm x 20cm x 30cm = 6000 cm³

        // Act
        var volume = dimensoes.CalcularVolume();

        // Assert
        volume.Should().Be(0.006m); // 6000 cm³ = 0.006 m³
    }

    [Fact]
    public void DimensoesProduto_DeveCalcularPesoCubadoComDensidade()
    {
        // Arrange
        var dimensoes = new DimensoesProduto(10, 10, 10, 5, 1000); // 1000 cm³ = 0.001 m³, densidade 1000 kg/m³

        // Act
        var pesoCubado = dimensoes.CalcularPesoCubado();

        // Assert
        pesoCubado.Should().Be(1m); // 0.001 m³ * 1000 kg/m³ = 1 kg
    }

    [Fact]
    public void DimensoesProduto_DeveRetornarNullParaPesoCubadoSemDensidade()
    {
        // Arrange
        var dimensoes = new DimensoesProduto(10, 10, 10, 5); // Sem densidade

        // Act
        var pesoCubado = dimensoes.CalcularPesoCubado();

        // Assert
        pesoCubado.Should().BeNull();
    }

    [Fact]
    public void DimensoesProduto_DeveUsarPesoNominalQuandoSemDensidade()
    {
        // Arrange
        var pesoNominal = 5m;
        var dimensoes = new DimensoesProduto(10, 10, 10, pesoNominal); // Sem densidade

        // Act
        var pesoParaFrete = dimensoes.ObterPesoParaFrete();

        // Assert
        pesoParaFrete.Should().Be(pesoNominal);
    }

    [Fact]
    public void DimensoesProduto_DeveUsarMaiorPesoQuandoComDensidade()
    {
        // Arrange
        var pesoNominal = 5m;
        var dimensoes = new DimensoesProduto(20, 20, 20, pesoNominal, 1000); // Peso cúbico = 8 kg

        // Act
        var pesoParaFrete = dimensoes.ObterPesoParaFrete();

        // Assert
        pesoParaFrete.Should().Be(8m); // Peso cúbico (8 kg) > peso nominal (5 kg)
    }

    [Fact]
    public void DimensoesProduto_DeveUsarPesoNominalQuandoMaior()
    {
        // Arrange
        var pesoNominal = 10m;
        var dimensoes = new DimensoesProduto(10, 10, 10, pesoNominal, 500); // Peso cúbico = 0.5 kg

        // Act
        var pesoParaFrete = dimensoes.ObterPesoParaFrete();

        // Assert
        pesoParaFrete.Should().Be(pesoNominal); // Peso nominal (10 kg) > peso cúbico (0.5 kg)
    }

    [Fact]
    public void DimensoesProduto_DeveSerIgualQuandoValoresIguais()
    {
        // Arrange
        var dimensoes1 = new DimensoesProduto(10, 20, 30, 25, 800);
        var dimensoes2 = new DimensoesProduto(10, 20, 30, 25, 800);

        // Act & Assert
        dimensoes1.Should().Be(dimensoes2);
        dimensoes1.GetHashCode().Should().Be(dimensoes2.GetHashCode());
    }

    [Fact]
    public void DimensoesProduto_DeveSerDiferenteQuandoValoresDiferentes()
    {
        // Arrange
        var dimensoes1 = new DimensoesProduto(10, 20, 30, 25, 800);
        var dimensoes2 = new DimensoesProduto(15, 20, 30, 25, 800);

        // Act & Assert
        dimensoes1.Should().NotBe(dimensoes2);
    }

    [Fact]
    public void DimensoesProduto_DeveSerIgualQuandoAmbasSemDensidade()
    {
        // Arrange
        var dimensoes1 = new DimensoesProduto(10, 20, 30, 25);
        var dimensoes2 = new DimensoesProduto(10, 20, 30, 25);

        // Act & Assert
        dimensoes1.Should().Be(dimensoes2);
    }

    [Fact]
    public void DimensoesProduto_DeveSerDiferenteQuandoUmaComDensidadeOutraSem()
    {
        // Arrange
        var dimensoes1 = new DimensoesProduto(10, 20, 30, 25, 800);
        var dimensoes2 = new DimensoesProduto(10, 20, 30, 25);

        // Act & Assert
        dimensoes1.Should().NotBe(dimensoes2);
    }

    [Fact]
    public void DimensoesProduto_DeveCalcularVolumeGrande()
    {
        // Arrange
        var dimensoes = new DimensoesProduto(100, 100, 100, 50); // 1.000.000 cm³

        // Act
        var volume = dimensoes.CalcularVolume();

        // Assert
        volume.Should().Be(1m); // 1.000.000 cm³ = 1 m³
    }

    [Fact]
    public void DimensoesProduto_DeveCalcularPesoCubadoComDensidadeBaixa()
    {
        // Arrange
        var dimensoes = new DimensoesProduto(100, 100, 100, 50, 100); // 1 m³, densidade 100 kg/m³

        // Act
        var pesoCubado = dimensoes.CalcularPesoCubado();

        // Assert
        pesoCubado.Should().Be(100m); // 1 m³ * 100 kg/m³ = 100 kg
    }

    [Fact]
    public void DimensoesProduto_DeveCalcularPesoCubadoComDensidadeAlta()
    {
        // Arrange
        var dimensoes = new DimensoesProduto(10, 10, 10, 0.1m, 2000); // 0.001 m³, densidade 2000 kg/m³

        // Act
        var pesoCubado = dimensoes.CalcularPesoCubado();

        // Assert
        pesoCubado.Should().Be(2m); // 0.001 m³ * 2000 kg/m³ = 2 kg
    }
}