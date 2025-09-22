using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Produtores.Dominio.Entidades;
using Agriis.Produtores.Dominio.Enums;
using Agriis.Produtores.Dominio.Interfaces;
using Agriis.Produtores.Dominio.Servicos;
using FluentAssertions;
using Moq;
using System.Text.Json;
using Xunit;

namespace Agriis.Tests.Unit.Servicos;

/// <summary>
/// Testes unitários para o serviço de domínio ProdutorDomainService
/// </summary>
public class ProdutorDomainServiceTests
{
    private readonly Mock<ISerproService> _serproServiceMock;
    private readonly ProdutorDomainService _produtorDomainService;

    public ProdutorDomainServiceTests()
    {
        _serproServiceMock = new Mock<ISerproService>();
        _produtorDomainService = new ProdutorDomainService(_serproServiceMock.Object);
    }

    [Fact]
    public void ProdutorDomainService_DeveLancarExcecaoParaSerproServiceNulo()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ProdutorDomainService(null!));
    }

    [Fact]
    public async Task ValidarAutomaticamenteAsync_DeveLancarExcecaoParaProdutorNulo()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _produtorDomainService.ValidarAutomaticamenteAsync(null!));
    }

    [Fact]
    public async Task ValidarAutomaticamenteAsync_DeveValidarCpfComSucesso()
    {
        // Arrange
        var produtor = CriarProdutorComCpf();
        var retornoSerpro = JsonDocument.Parse("""{"status": "success", "valid": true}""");
        
        _serproServiceMock
            .Setup(x => x.ValidarCpfAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(new SerproValidationResult 
            { 
                Sucesso = true, 
                DocumentoValido = true, 
                DadosRetorno = retornoSerpro 
            }));

        // Act
        var resultado = await _produtorDomainService.ValidarAutomaticamenteAsync(produtor);

        // Assert
        resultado.Sucesso.Should().BeTrue();
        resultado.StatusResultante.Should().Be(StatusProdutor.AutorizadoAutomaticamente);
        resultado.Mensagem.Should().Be("Produtor autorizado automaticamente");
        produtor.Status.Should().Be(StatusProdutor.AutorizadoAutomaticamente);
        produtor.RetornosApiCheckProdutor.Should().NotBeNull();
        
        _serproServiceMock.Verify(x => x.ValidarCpfAsync(produtor.Cpf!.Valor), Times.Once);
    }

    [Fact]
    public async Task ValidarAutomaticamenteAsync_DeveValidarCnpjComSucesso()
    {
        // Arrange
        var produtor = CriarProdutorComCnpj();
        var retornoSerpro = JsonDocument.Parse("""{"status": "success", "valid": true}""");
        
        _serproServiceMock
            .Setup(x => x.ValidarCnpjAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(new SerproValidationResult 
            { 
                Sucesso = true, 
                DocumentoValido = true, 
                DadosRetorno = retornoSerpro 
            }));

        // Act
        var resultado = await _produtorDomainService.ValidarAutomaticamenteAsync(produtor);

        // Assert
        resultado.Sucesso.Should().BeTrue();
        resultado.StatusResultante.Should().Be(StatusProdutor.AutorizadoAutomaticamente);
        produtor.Status.Should().Be(StatusProdutor.AutorizadoAutomaticamente);
        
        _serproServiceMock.Verify(x => x.ValidarCnpjAsync(produtor.Cnpj!.Valor), Times.Once);
    }

    [Fact]
    public async Task ValidarAutomaticamenteAsync_DeveNegarQuandoDocumentoInvalido()
    {
        // Arrange
        var produtor = CriarProdutorComCpf();
        var retornoSerpro = JsonDocument.Parse("""{"status": "success", "valid": false}""");
        
        _serproServiceMock
            .Setup(x => x.ValidarCpfAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(new SerproValidationResult 
            { 
                Sucesso = true, 
                DocumentoValido = false, 
                DadosRetorno = retornoSerpro 
            }));

        // Act
        var resultado = await _produtorDomainService.ValidarAutomaticamenteAsync(produtor);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.StatusResultante.Should().Be(StatusProdutor.Negado);
        resultado.Mensagem.Should().Be("Documento inválido no SERPRO");
        produtor.Status.Should().Be(StatusProdutor.Negado);
    }

    [Fact]
    public async Task ValidarAutomaticamenteAsync_DeveEncaminharParaValidacaoManualQuandoErroNaConsulta()
    {
        // Arrange
        var produtor = CriarProdutorComCpf();
        
        _serproServiceMock
            .Setup(x => x.ValidarCpfAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(new SerproValidationResult 
            { 
                Sucesso = false, 
                DocumentoValido = false 
            }));

        // Act
        var resultado = await _produtorDomainService.ValidarAutomaticamenteAsync(produtor);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.StatusResultante.Should().Be(StatusProdutor.PendenteValidacaoManual);
        resultado.Mensagem.Should().Contain("Erro na validação automática");
        produtor.Status.Should().Be(StatusProdutor.PendenteValidacaoManual);
    }

    [Fact]
    public async Task ValidarAutomaticamenteAsync_DeveEncaminharParaValidacaoManualQuandoExcecao()
    {
        // Arrange
        var produtor = CriarProdutorComCpf();
        
        _serproServiceMock
            .Setup(x => x.ValidarCpfAsync(It.IsAny<string>()))
            .Throws(new Exception("Erro de conexão"));

        // Act
        var resultado = await _produtorDomainService.ValidarAutomaticamenteAsync(produtor);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.StatusResultante.Should().Be(StatusProdutor.PendenteValidacaoManual);
        resultado.Mensagem.Should().Contain("Erro na validação automática: Erro de conexão");
        produtor.Status.Should().Be(StatusProdutor.PendenteValidacaoManual);
    }

    [Theory]
    [InlineData(StatusProdutor.PendenteValidacaoAutomatica, true)]
    [InlineData(StatusProdutor.PendenteValidacaoManual, true)]
    [InlineData(StatusProdutor.AutorizadoAutomaticamente, true)]
    [InlineData(StatusProdutor.AutorizadoManualmente, true)]
    [InlineData(StatusProdutor.Negado, false)]
    public void PodeSerEditado_DeveRetornarCorreto(StatusProdutor status, bool esperado)
    {
        // Arrange
        var produtor = CriarProdutorComCpf();
        produtor.AtualizarStatus(status);

        // Act
        var resultado = _produtorDomainService.PodeSerEditado(produtor);

        // Assert
        resultado.Should().Be(esperado);
    }

    [Fact]
    public void PodeSerEditado_DeveRetornarFalseParaProdutorNulo()
    {
        // Act
        var resultado = _produtorDomainService.PodeSerEditado(null);

        // Assert
        resultado.Should().BeFalse();
    }

    [Theory]
    [InlineData(StatusProdutor.AutorizadoAutomaticamente, true)]
    [InlineData(StatusProdutor.AutorizadoManualmente, true)]
    [InlineData(StatusProdutor.PendenteValidacaoAutomatica, false)]
    [InlineData(StatusProdutor.PendenteValidacaoManual, false)]
    [InlineData(StatusProdutor.Negado, false)]
    public void PodeFazerPedidos_DeveRetornarCorreto(StatusProdutor status, bool esperado)
    {
        // Arrange
        var produtor = CriarProdutorComCpf();
        produtor.AtualizarStatus(status);

        // Act
        var resultado = _produtorDomainService.PodeFazerPedidos(produtor);

        // Assert
        resultado.Should().Be(esperado);
    }

    [Fact]
    public void PodeFazerPedidos_DeveRetornarFalseParaProdutorNulo()
    {
        // Act
        var resultado = _produtorDomainService.PodeFazerPedidos(null);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void CalcularAreaTotalPlantio_DeveCalcularCorretamente()
    {
        // Arrange
        var produtor = CriarProdutorComCpf();
        var areasPropriedades = new[] { 100.5m, 250.75m, 50.25m };

        // Act
        var areaTotal = _produtorDomainService.CalcularAreaTotalPlantio(produtor, areasPropriedades);

        // Assert
        areaTotal.Should().Be(401.5m);
    }

    [Fact]
    public void CalcularAreaTotalPlantio_DeveArredondarParaQuatroCasas()
    {
        // Arrange
        var produtor = CriarProdutorComCpf();
        var areasPropriedades = new[] { 100.123456m, 200.987654m };

        // Act
        var areaTotal = _produtorDomainService.CalcularAreaTotalPlantio(produtor, areasPropriedades);

        // Assert
        areaTotal.Should().Be(301.1111m);
    }

    [Fact]
    public void CalcularAreaTotalPlantio_DeveRetornarZeroParaProdutorNulo()
    {
        // Arrange
        var areasPropriedades = new[] { 100m, 200m };

        // Act
        var areaTotal = _produtorDomainService.CalcularAreaTotalPlantio(null, areasPropriedades);

        // Assert
        areaTotal.Should().Be(0);
    }

    [Fact]
    public void CalcularAreaTotalPlantio_DeveRetornarZeroParaAreasNulas()
    {
        // Arrange
        var produtor = CriarProdutorComCpf();

        // Act
        var areaTotal = _produtorDomainService.CalcularAreaTotalPlantio(produtor, null);

        // Assert
        areaTotal.Should().Be(0);
    }

    [Fact]
    public void CalcularAreaTotalPlantio_DeveRetornarZeroParaListaVazia()
    {
        // Arrange
        var produtor = CriarProdutorComCpf();
        var areasPropriedades = Array.Empty<decimal>();

        // Act
        var areaTotal = _produtorDomainService.CalcularAreaTotalPlantio(produtor, areasPropriedades);

        // Assert
        areaTotal.Should().Be(0);
    }

    private static Produtor CriarProdutorComCpf()
    {
        return new Produtor(
            "João Silva",
            new Cpf("11144477735"),
            null,
            "123456789",
            "Agricultura",
            new AreaPlantio(100m));
    }

    private static Produtor CriarProdutorComCnpj()
    {
        return new Produtor(
            "Fazenda ABC Ltda",
            null,
            new Cnpj("11222333000181"),
            "123456789",
            "Agricultura",
            new AreaPlantio(500m));
    }
}

