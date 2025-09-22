using Agriis.Tests.Shared.Generators;
using FluentAssertions;
using Xunit;

namespace Agriis.Tests.Unit.Generators;

/// <summary>
/// Testes unitários para o gerador de dados de teste
/// </summary>
public class TestDataGeneratorTests
{
    private readonly TestDataGenerator _generator;

    public TestDataGeneratorTests()
    {
        _generator = new TestDataGenerator();
    }

    [Fact]
    public void GerarCpf_DeveGerarCpfValido()
    {
        // Act
        var cpf = _generator.GerarCpf();

        // Assert
        cpf.Should().NotBeNullOrEmpty();
        cpf.Should().HaveLength(11);
        cpf.Should().MatchRegex(@"^\d{11}$");
    }

    [Fact]
    public void GerarCnpj_DeveGerarCnpjValido()
    {
        // Act
        var cnpj = _generator.GerarCnpj();

        // Assert
        cnpj.Should().NotBeNullOrEmpty();
        cnpj.Should().HaveLength(14);
        cnpj.Should().MatchRegex(@"^\d{14}$");
    }

    [Fact]
    public void GerarEmail_DeveGerarEmailValido()
    {
        // Act
        var email = _generator.GerarEmail();

        // Assert
        email.Should().NotBeNullOrEmpty();
        email.Should().Contain("@");
        email.Should().Contain(".");
    }

    [Fact]
    public void GerarNome_DeveGerarNomeCompleto()
    {
        // Act
        var nome = _generator.GerarNome();

        // Assert
        nome.Should().NotBeNullOrEmpty();
        nome.Should().Contain(" "); // Nome completo deve ter espaço
    }

    [Fact]
    public void GerarEndereco_DeveGerarEnderecoCompleto()
    {
        // Act
        var endereco = _generator.GerarEndereco();

        // Assert
        endereco.Should().NotBeNull();
        endereco.Logradouro.Should().NotBeNullOrEmpty();
        endereco.Numero.Should().NotBeNullOrEmpty();
        endereco.Bairro.Should().NotBeNullOrEmpty();
        endereco.Cep.Should().NotBeNullOrEmpty();
        endereco.Cidade.Should().NotBeNullOrEmpty();
        endereco.Estado.Should().NotBeNullOrEmpty();
        endereco.Latitude.Should().BeInRange(-90, 90);
        endereco.Longitude.Should().BeInRange(-180, 180);
    }

    [Fact]
    public void GerarAreaPlantio_DeveRespeitarLimites()
    {
        // Arrange
        var min = 10m;
        var max = 100m;

        // Act
        var area = _generator.GerarAreaPlantio(min, max);

        // Assert
        area.Should().BeInRange(min, max);
    }

    [Fact]
    public void GerarNomeCultura_DeveRetornarCulturaValida()
    {
        // Act
        var cultura = _generator.GerarNomeCultura();

        // Assert
        cultura.Should().NotBeNullOrEmpty();
        
        var culturasValidas = new[]
        {
            "Soja", "Milho", "Algodão", "Feijão", "Arroz", "Trigo", "Cana-de-açúcar",
            "Café", "Sorgo", "Girassol", "Amendoim", "Mandioca", "Batata"
        };
        
        culturasValidas.Should().Contain(cultura);
    }

    [Fact]
    public void GerarPeriodoSafra_DeveGerarPeriodoValido()
    {
        // Act
        var periodo = _generator.GerarPeriodoSafra();

        // Assert
        periodo.Should().NotBeNull();
        periodo.DataFimPlantio.Should().BeAfter(periodo.DataInicioPlantio);
        periodo.AnoColheita.Should().Be(periodo.DataFimPlantio.Year);
    }

    [Fact]
    public void GerarDimensoes_DeveGerarDimensoesPositivas()
    {
        // Act
        var dimensoes = _generator.GerarDimensoes();

        // Assert
        dimensoes.Should().NotBeNull();
        dimensoes.Altura.Should().BePositive();
        dimensoes.Largura.Should().BePositive();
        dimensoes.Profundidade.Should().BePositive();
    }

    [Fact]
    public void GerarLista_DeveGerarListaComTamanhoCorreto()
    {
        // Arrange
        var min = 2;
        var max = 5;

        // Act
        var lista = _generator.GerarLista(() => _generator.GerarNome(), min, max);

        // Assert
        lista.Should().NotBeNull();
        lista.Count.Should().BeInRange(min, max);
        lista.Should().OnlyContain(nome => !string.IsNullOrEmpty(nome));
    }

    [Fact]
    public void EscolherAleatorio_DeveEscolherItemDaLista()
    {
        // Arrange
        var opcoes = new[] { "A", "B", "C", "D" };

        // Act
        var escolhido = _generator.EscolherAleatorio(opcoes);

        // Assert
        opcoes.Should().Contain(escolhido);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(100, 999)]
    [InlineData(1000, 9999)]
    public void GerarId_DeveRespeitarIntervalo(int min, int max)
    {
        // Act
        var id = _generator.GerarId(min, max);

        // Assert
        id.Should().BeInRange(min, max);
    }

    [Fact]
    public void GerarDataPassado_DeveGerarDataNoPassado()
    {
        // Act
        var data = _generator.GerarDataPassado();

        // Assert
        data.Should().BeBefore(DateTime.Now);
    }

    [Fact]
    public void GerarDataFuturo_DeveGerarDataNoFuturo()
    {
        // Act
        var data = _generator.GerarDataFuturo();

        // Assert
        data.Should().BeAfter(DateTime.Now);
    }

    [Fact]
    public void GerarDataEntre_DeveGerarDataNoIntervalo()
    {
        // Arrange
        var inicio = DateTime.Now.AddDays(-10);
        var fim = DateTime.Now.AddDays(10);

        // Act
        var data = _generator.GerarDataEntre(inicio, fim);

        // Assert
        data.Should().BeOnOrAfter(inicio);
        data.Should().BeOnOrBefore(fim);
    }
}