using Xunit;
using Moq;
using Agriis.Pedidos.Dominio.Servicos;
using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Segmentacoes.Dominio.Servicos;
using Agriis.Produtos.Aplicacao.Interfaces;
using Agriis.Produtores.Aplicacao.Interfaces;
using Agriis.Catalogos.Aplicacao.Interfaces;
using Agriis.Produtos.Aplicacao.DTOs;
using Agriis.Produtores.Aplicacao.DTOs;
using Agriis.Catalogos.Aplicacao.DTOs;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Pedidos.Dominio.Interfaces;

namespace Agriis.Pedidos.Tests.Unit.Servicos;

public class CarrinhoComprasServiceTests
{
    private readonly Mock<CalculoDescontoSegmentadoService> _calculoDescontoServiceMock;
    private readonly Mock<IProdutoService> _produtoServiceMock;
    private readonly Mock<IProdutorService> _produtorServiceMock;
    private readonly Mock<ICatalogoService> _catalogoServiceMock;
    private readonly CarrinhoComprasService _carrinhoService;

    public CarrinhoComprasServiceTests()
    {
        _calculoDescontoServiceMock = new Mock<CalculoDescontoSegmentadoService>(
            Mock.Of<Agriis.Segmentacoes.Dominio.Interfaces.ISegmentacaoRepository>(),
            Mock.Of<Agriis.Segmentacoes.Dominio.Interfaces.IGrupoRepository>(),
            Mock.Of<Agriis.Segmentacoes.Dominio.Interfaces.IGrupoSegmentacaoRepository>());
        
        _produtoServiceMock = new Mock<IProdutoService>();
        _produtorServiceMock = new Mock<IProdutorService>();
        _catalogoServiceMock = new Mock<ICatalogoService>();

        var propostaRepositoryMock = new Mock<IPropostaRepository>();
        
        _carrinhoService = new CarrinhoComprasService(
            _calculoDescontoServiceMock.Object,
            _produtoServiceMock.Object,
            _produtorServiceMock.Object,
            _catalogoServiceMock.Object,
            propostaRepositoryMock.Object);
    }

    [Fact]
    public void CalcularTotais_DeveCalcularCorretamente()
    {
        // Arrange
        var pedido = new Pedido(1, 1, true, true);
        var item1 = new PedidoItem(1, 1, 10, 100, 5); // 10 * 100 = 1000, desconto 5% = 50, final = 950
        var item2 = new PedidoItem(1, 2, 5, 200, 10); // 5 * 200 = 1000, desconto 10% = 100, final = 900

        pedido.AdicionarItem(item1);
        pedido.AdicionarItem(item2);

        // Act
        var totais = _carrinhoService.CalcularTotais(pedido);

        // Assert
        Assert.Equal(2000, totais.ValorBruto);
        Assert.Equal(150, totais.ValorDesconto);
        Assert.Equal(1850, totais.ValorLiquido);
        Assert.Equal(2, totais.QuantidadeItens);
        Assert.Equal(7.5m, totais.PercentualDescontoMedio); // 150/2000 * 100 = 7.5%
    }

    [Fact]
    public void VerificarPrazoLimite_DeveRetornarTrueQuandoDentroPrazo()
    {
        // Arrange
        var pedido = new Pedido(1, 1, true, true, 7); // 7 dias de prazo

        // Act
        var resultado = _carrinhoService.VerificarPrazoLimite(pedido);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public async Task AdicionarItemAsync_DeveLancarExcecao_QuandoProdutoNaoExiste()
    {
        // Arrange
        var pedido = new Pedido(1, 1, true, true);
        _produtoServiceMock.Setup(x => x.ObterPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProdutoDto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _carrinhoService.AdicionarItemAsync(pedido, 1, 10, 1));
    }

    [Fact]
    public async Task AdicionarItemAsync_DeveLancarExcecao_QuandoQuantidadeInvalida()
    {
        // Arrange
        var pedido = new Pedido(1, 1, true, true);
        var produto = new ProdutoDto { Id = 1, Nome = "Produto Teste", CategoriaId = 1 };
        
        _produtoServiceMock.Setup(x => x.ObterPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(produto);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _carrinhoService.AdicionarItemAsync(pedido, 1, 0, 1)); // Quantidade zero
    }
}