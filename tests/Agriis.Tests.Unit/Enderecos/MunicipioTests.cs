using Agriis.Enderecos.Dominio.Entidades;
using FluentAssertions;
using Xunit;

namespace Agriis.Tests.Unit.Enderecos;

/// <summary>
/// Testes unitários para a entidade Municipio
/// </summary>
public class MunicipioTests
{
    [Fact]
    public void Municipio_Constructor_ShouldCreateValidInstance()
    {
        // Arrange
        var nome = "São Paulo";
        var codigoIbge = 3550308;
        var estadoId = 1;

        // Act
        var municipio = new Municipio(nome, codigoIbge, estadoId);

        // Assert
        municipio.Nome.Should().Be(nome);
        municipio.CodigoIbge.Should().Be(codigoIbge);
        municipio.EstadoId.Should().Be(estadoId);
        municipio.CepPrincipal.Should().BeNull();
        municipio.Latitude.Should().BeNull();
        municipio.Longitude.Should().BeNull();
        municipio.Localizacao.Should().BeNull();
        municipio.Enderecos.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Municipio_Constructor_WithOptionalParameters_ShouldSetAllProperties()
    {
        // Arrange
        var nome = "São Paulo";
        var codigoIbge = 3550308;
        var estadoId = 1;
        var cepPrincipal = "01000000";
        var latitude = -23.5505;
        var longitude = -46.6333;

        // Act
        var municipio = new Municipio(nome, codigoIbge, estadoId, cepPrincipal, latitude, longitude);

        // Assert
        municipio.Nome.Should().Be(nome);
        municipio.CodigoIbge.Should().Be(codigoIbge);
        municipio.EstadoId.Should().Be(estadoId);
        municipio.CepPrincipal.Should().Be(cepPrincipal);
        municipio.Latitude.Should().Be(latitude);
        municipio.Longitude.Should().Be(longitude);
        municipio.Localizacao.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Municipio_Constructor_ShouldThrowException_WhenNomeIsInvalid(string nome)
    {
        // Act & Assert
        var act = () => new Municipio(nome, 3550308, 1);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Nome do município é obrigatório*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Municipio_Constructor_ShouldThrowException_WhenCodigoIbgeIsInvalid(int codigoIbge)
    {
        // Act & Assert
        var act = () => new Municipio("São Paulo", codigoIbge, 1);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Código IBGE deve ser maior que zero*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Municipio_Constructor_ShouldThrowException_WhenEstadoIdIsInvalid(int estadoId)
    {
        // Act & Assert
        var act = () => new Municipio("São Paulo", 3550308, estadoId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("ID do estado deve ser maior que zero*");
    }

    [Fact]
    public void Municipio_DefinirLocalizacao_ShouldSetCoordinatesAndCreatePoint()
    {
        // Arrange
        var municipio = new Municipio("São Paulo", 3550308, 1);
        var latitude = -23.5505;
        var longitude = -46.6333;

        // Act
        municipio.DefinirLocalizacao(latitude, longitude);

        // Assert
        municipio.Latitude.Should().Be(latitude);
        municipio.Longitude.Should().Be(longitude);
        municipio.Localizacao.Should().NotBeNull();
        municipio.Localizacao!.X.Should().Be(longitude);
        municipio.Localizacao.Y.Should().Be(latitude);
        municipio.Localizacao.SRID.Should().Be(4326);
    }

    [Fact]
    public void Municipio_DefinirLocalizacao_WithNullValues_ShouldClearLocation()
    {
        // Arrange
        var municipio = new Municipio("São Paulo", 3550308, 1, null, -23.5505, -46.6333);

        // Act
        municipio.DefinirLocalizacao(null, null);

        // Assert
        municipio.Latitude.Should().BeNull();
        municipio.Longitude.Should().BeNull();
        municipio.Localizacao.Should().BeNull();
    }

    [Theory]
    [InlineData(-91, -46.6333)]
    [InlineData(91, -46.6333)]
    [InlineData(-23.5505, -181)]
    [InlineData(-23.5505, 181)]
    public void Municipio_DefinirLocalizacao_ShouldThrowException_WhenCoordinatesAreInvalid(double latitude, double longitude)
    {
        // Arrange
        var municipio = new Municipio("São Paulo", 3550308, 1);

        // Act & Assert
        var act = () => municipio.DefinirLocalizacao(latitude, longitude);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Municipio_PossuiLocalizacao_ShouldReturnTrue_WhenLocationIsSet()
    {
        // Arrange
        var municipio = new Municipio("São Paulo", 3550308, 1, null, -23.5505, -46.6333);

        // Act & Assert
        municipio.PossuiLocalizacao().Should().BeTrue();
    }

    [Fact]
    public void Municipio_PossuiLocalizacao_ShouldReturnFalse_WhenLocationIsNotSet()
    {
        // Arrange
        var municipio = new Municipio("São Paulo", 3550308, 1);

        // Act & Assert
        municipio.PossuiLocalizacao().Should().BeFalse();
    }

    [Fact]
    public void Municipio_CalcularDistanciaKm_ShouldReturnDistance_WhenBothHaveLocation()
    {
        // Arrange
        var municipio1 = new Municipio("São Paulo", 3550308, 1, null, -23.5505, -46.6333);
        var municipio2 = new Municipio("Rio de Janeiro", 3304557, 2, null, -22.9068, -43.1729);

        // Act
        var distancia = municipio1.CalcularDistanciaKm(municipio2);

        // Assert
        distancia.Should().NotBeNull();
        distancia.Should().BeGreaterThan(0);
        distancia.Should().BeLessThan(1000); // Distance between SP and RJ should be less than 1000km
    }

    [Fact]
    public void Municipio_CalcularDistanciaKm_ShouldReturnNull_WhenOneDoesNotHaveLocation()
    {
        // Arrange
        var municipio1 = new Municipio("São Paulo", 3550308, 1, null, -23.5505, -46.6333);
        var municipio2 = new Municipio("Rio de Janeiro", 3304557, 2);

        // Act
        var distancia = municipio1.CalcularDistanciaKm(municipio2);

        // Assert
        distancia.Should().BeNull();
    }

    [Fact]
    public void Municipio_Atualizar_ShouldUpdateAllProperties()
    {
        // Arrange
        var municipio = new Municipio("São Paulo", 3550308, 1);
        var novoNome = "Município de São Paulo";
        var novoCodigoIbge = 3550308;
        var novoCep = "01000000";
        var novaLatitude = -23.5505;
        var novaLongitude = -46.6333;

        // Act
        municipio.Atualizar(novoNome, novoCodigoIbge, novoCep, novaLatitude, novaLongitude);

        // Assert
        municipio.Nome.Should().Be(novoNome);
        municipio.CodigoIbge.Should().Be(novoCodigoIbge);
        municipio.CepPrincipal.Should().Be(novoCep);
        municipio.Latitude.Should().Be(novaLatitude);
        municipio.Longitude.Should().Be(novaLongitude);
        municipio.Localizacao.Should().NotBeNull();
    }

    [Fact]
    public void Municipio_Atualizar_ShouldUpdateDataAtualizacao()
    {
        // Arrange
        var municipio = new Municipio("São Paulo", 3550308, 1);
        var dataAtualizacaoAnterior = municipio.DataAtualizacao;

        // Act
        Thread.Sleep(10); // Ensure time difference
        municipio.Atualizar("Município de São Paulo", 3550308);

        // Assert
        municipio.DataAtualizacao.Should().BeAfter(dataAtualizacaoAnterior);
    }

    [Fact]
    public void Municipio_Atualizar_ShouldValidateParameters()
    {
        // Arrange
        var municipio = new Municipio("São Paulo", 3550308, 1);

        // Act & Assert
        var act = () => municipio.Atualizar("", 3550308);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Nome do município é obrigatório*");
    }

    [Fact]
    public void Municipio_ShouldHaveCorrectNavigationProperties()
    {
        // Arrange & Act
        var municipio = new Municipio("São Paulo", 3550308, 1);

        // Assert
        municipio.Enderecos.Should().NotBeNull("Enderecos collection should be initialized");
        municipio.Enderecos.Should().BeEmpty("Enderecos collection should start empty");
    }

    [Theory]
    [InlineData(-23.5505, -46.6333)] // São Paulo
    [InlineData(-22.9068, -43.1729)] // Rio de Janeiro
    [InlineData(-15.7942, -47.8822)] // Brasília
    [InlineData(-30.0346, -51.2177)] // Porto Alegre
    public void Municipio_ShouldAcceptValidBrazilianCoordinates(double latitude, double longitude)
    {
        // Act
        var act = () => new Municipio("Teste", 1234567, 1, null, latitude, longitude);

        // Assert
        act.Should().NotThrow();
    }
}