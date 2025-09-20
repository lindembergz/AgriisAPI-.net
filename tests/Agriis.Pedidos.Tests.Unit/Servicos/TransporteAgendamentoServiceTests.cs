using Xunit;
using Moq;
using Agriis.Pedidos.Dominio.Servicos;
using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.ObjetosValor;
using Agriis.Produtos.Dominio.Enums;
using Agriis.Compartilhado.Dominio.Enums;

namespace Agriis.Pedidos.Tests.Unit.Servicos;

/// <summary>
/// Testes unitários para TransporteAgendamentoService
/// </summary>
public class TransporteAgendamentoServiceTests
{
    private readonly Mock<FreteCalculoService> _mockFreteCalculoService;
    private readonly TransporteAgendamentoService _service;

    public TransporteAgendamentoServiceTests()
    {
        _mockFreteCalculoService = new Mock<FreteCalculoService>();
        _service = new TransporteAgendamentoService(_mockFreteCalculoService.Object);
    }

    [Fact]
    public void CriarAgendamentoTransporte_ComDadosValidos_DeveCriarTransporte()
    {
        // Arrange
        var dimensoes = new DimensoesProduto(10, 10, 10, 1.0m, 500);
        var produto = new Produto("Produto", "TESTE", TipoProduto.Fabricante, TipoUnidade.Quilo, dimensoes, 1, 1);
        var pedidoItem = new PedidoItem(1, 1, 100m, 10.0m);
        
        // Simular que o produto está associado ao item
        typeof(PedidoItem).GetProperty("Produto")?.SetValue(pedidoItem, produto);

        var quantidade = 50m;
        var dataAgendamento = DateTime.UtcNow.AddDays(1);
        var enderecoOrigem = "Origem Teste";
        var enderecoDestino = "Destino Teste";
        var distanciaKm = 100m;
        var observacoes = "Observações teste";

        _mockFreteCalculoService
            .Setup(x => x.ValidarDisponibilidadeQuantidade(pedidoItem, quantidade))
            .Returns(true);

        _mockFreteCalculoService
            .Setup(x => x.CalcularFrete(produto, quantidade, distanciaKm, It.IsAny<decimal>(), It.IsAny<decimal>()))
            .Returns(new CalculoFreteResult(
                PesoTotal: 50m,
                VolumeTotal: 0.05m,
                PesoCubadoTotal: 25m,
                PesoParaFrete: 50m,
                ValorFrete: 250m,
                DistanciaKm: distanciaKm,
                TipoCalculoUtilizado: TipoCalculoPeso.PesoNominal
            ));

        // Act
        var transporte = _service.CriarAgendamentoTransporte(
            pedidoItem, quantidade, dataAgendamento, enderecoOrigem, enderecoDestino, distanciaKm, observacoes);

        // Assert
        Assert.NotNull(transporte);
        Assert.Equal(pedidoItem.Id, transporte.PedidoItemId);
        Assert.Equal(quantidade, transporte.Quantidade);
        Assert.Equal(dataAgendamento, transporte.DataAgendamento);
        Assert.Equal(enderecoOrigem, transporte.EnderecoOrigem);
        Assert.Equal(enderecoDestino, transporte.EnderecoDestino);
        Assert.Equal(250m, transporte.ValorFrete);
        Assert.Equal(observacoes, transporte.Observacoes);
        Assert.Equal(50m, transporte.PesoTotal);
        Assert.Equal(0.05m, transporte.VolumeTotal);
    }

    [Fact]
    public void CriarAgendamentoTransporte_ComQuantidadeIndisponivel_DeveLancarExcecao()
    {
        // Arrange
        var pedidoItem = new PedidoItem(1, 1, 100m, 10.0m);
        var quantidade = 150m; // Mais que a quantidade do item
        var dataAgendamento = DateTime.UtcNow.AddDays(1);

        _mockFreteCalculoService
            .Setup(x => x.ValidarDisponibilidadeQuantidade(pedidoItem, quantidade))
            .Returns(false);

        _mockFreteCalculoService
            .Setup(x => x.CalcularQuantidadeDisponivel(pedidoItem))
            .Returns(100m);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _service.CriarAgendamentoTransporte(pedidoItem, quantidade, dataAgendamento));

        Assert.Contains("Quantidade solicitada (150) excede a disponível (100)", exception.Message);
    }

    [Fact]
    public void CriarAgendamentoTransporte_ComDataPassada_DeveLancarExcecao()
    {
        // Arrange
        var pedidoItem = new PedidoItem(1, 1, 100m, 10.0m);
        var quantidade = 50m;
        var dataAgendamento = DateTime.UtcNow.AddDays(-1); // Data no passado

        _mockFreteCalculoService
            .Setup(x => x.ValidarDisponibilidadeQuantidade(pedidoItem, quantidade))
            .Returns(true);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _service.CriarAgendamentoTransporte(pedidoItem, quantidade, dataAgendamento));
    }

    [Fact]
    public void ReagendarTransporte_ComNovaDataValida_DeveAtualizarTransporte()
    {
        // Arrange
        var transporte = new PedidoItemTransporte(1, 50m, 100m);
        var dataOriginal = DateTime.UtcNow.AddDays(1);
        transporte.AgendarTransporte(dataOriginal);

        var novaData = DateTime.UtcNow.AddDays(2);
        var observacoes = "Reagendamento necessário";

        // Act
        _service.ReagendarTransporte(transporte, novaData, observacoes);

        // Assert
        Assert.Equal(novaData, transporte.DataAgendamento);
        Assert.Contains("Reagendado para", transporte.Observacoes);
        Assert.Contains(observacoes, transporte.Observacoes);
    }

    [Fact]
    public void AtualizarValorFrete_ComNovoValor_DeveAtualizarERegistrarObservacao()
    {
        // Arrange
        var transporte = new PedidoItemTransporte(1, 50m, 100m);
        var novoValor = 150m;
        var motivo = "Ajuste de preço";

        // Act
        _service.AtualizarValorFrete(transporte, novoValor, motivo);

        // Assert
        Assert.Equal(novoValor, transporte.ValorFrete);
        Assert.Contains("Valor do frete alterado de R$ 100,00 para R$ 150,00", transporte.Observacoes);
        Assert.Contains(motivo, transporte.Observacoes);
    }

    [Fact]
    public void ValidarMultiplosAgendamentos_ComAgendamentosValidos_DeveRetornarSucesso()
    {
        // Arrange
        var pedidoItem1 = new PedidoItem(1, 1, 100m, 10.0m);
        var pedidoItem2 = new PedidoItem(2, 2, 200m, 15.0m);

        var agendamentos = new List<SolicitacaoAgendamento>
        {
            new(pedidoItem1, 50m, DateTime.UtcNow.AddDays(1)),
            new(pedidoItem2, 100m, DateTime.UtcNow.AddDays(2))
        };

        _mockFreteCalculoService
            .Setup(x => x.ValidarDisponibilidadeQuantidade(It.IsAny<PedidoItem>(), It.IsAny<decimal>()))
            .Returns(true);

        // Act
        var resultado = _service.ValidarMultiplosAgendamentos(agendamentos);

        // Assert
        Assert.True(resultado.EhValido);
        Assert.Empty(resultado.Erros);
    }

    [Fact]
    public void ValidarMultiplosAgendamentos_ComAgendamentosInvalidos_DeveRetornarErros()
    {
        // Arrange
        var pedidoItem = new PedidoItem(1, 1, 100m, 10.0m);

        var agendamentos = new List<SolicitacaoAgendamento>
        {
            new(pedidoItem, 150m, DateTime.UtcNow.AddDays(1)) // Quantidade maior que disponível
        };

        _mockFreteCalculoService
            .Setup(x => x.ValidarDisponibilidadeQuantidade(pedidoItem, 150m))
            .Returns(false);

        _mockFreteCalculoService
            .Setup(x => x.CalcularQuantidadeDisponivel(pedidoItem))
            .Returns(100m);

        // Act
        var resultado = _service.ValidarMultiplosAgendamentos(agendamentos);

        // Assert
        Assert.False(resultado.EhValido);
        Assert.NotEmpty(resultado.Erros);
        Assert.Contains("Quantidade solicitada (150) excede a disponível (100)", resultado.Erros.First());
    }

    [Fact]
    public void CalcularResumoTransporte_ComPedidoComTransportes_DeveCalcularResumoCorreto()
    {
        // Arrange
        var pedido = new Pedido(1, 1, true, true);
        
        var item1 = new PedidoItem(pedido.Id, 1, 100m, 10.0m);
        var item2 = new PedidoItem(pedido.Id, 2, 200m, 15.0m);
        
        var transporte1 = new PedidoItemTransporte(item1.Id, 50m, 100m);
        transporte1.AtualizarPesoVolume(50m, 0.05m);
        transporte1.AgendarTransporte(DateTime.UtcNow.AddDays(1));
        
        var transporte2 = new PedidoItemTransporte(item2.Id, 100m, 200m);
        transporte2.AtualizarPesoVolume(100m, 0.1m);
        transporte2.AgendarTransporte(DateTime.UtcNow.AddDays(2));

        item1.ItensTransporte.Add(transporte1);
        item2.ItensTransporte.Add(transporte2);
        
        pedido.Itens.Add(item1);
        pedido.Itens.Add(item2);

        // Act
        var resumo = _service.CalcularResumoTransporte(pedido);

        // Assert
        Assert.Equal(2, resumo.TotalItens);
        Assert.Equal(2, resumo.ItensComTransporte);
        Assert.Equal(2, resumo.TotalTransportes);
        Assert.Equal(2, resumo.TransportesAgendados);
        Assert.Equal(150m, resumo.PesoTotal); // 50 + 100
        Assert.Equal(0.15m, resumo.VolumeTotal); // 0.05 + 0.1
        Assert.Equal(300m, resumo.ValorFreteTotal); // 100 + 200
        Assert.NotNull(resumo.ProximoAgendamento);
    }

    [Fact]
    public void CriarAgendamentoTransporte_ComParametrosNulos_DeveLancarExcecao()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _service.CriarAgendamentoTransporte(null!, 50m, DateTime.UtcNow.AddDays(1)));
    }
}