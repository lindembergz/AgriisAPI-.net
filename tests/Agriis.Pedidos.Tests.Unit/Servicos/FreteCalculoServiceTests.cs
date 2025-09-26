using Xunit;
using Agriis.Pedidos.Dominio.Servicos;
using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.ObjetosValor;
using Agriis.Produtos.Dominio.Enums;
using Agriis.Compartilhado.Dominio.Enums;

namespace Agriis.Pedidos.Tests.Unit.Servicos;

/// <summary>
/// Testes unitários para FreteCalculoService
/// </summary>
public class FreteCalculoServiceTests
{
    private readonly FreteCalculoService _service;

    public FreteCalculoServiceTests()
    {
        _service = new FreteCalculoService();
    }

    [Fact]
    public void CalcularFrete_ComProdutoValido_DeveCalcularCorretamente()
    {
        // Arrange
        var dimensoes = new DimensoesProduto(
            altura: 10, // cm
            largura: 10, // cm
            comprimento: 10, // cm
            pesoNominal: 1.0m, // kg
            pesoEmbalagem: 1.0m, // kg
            quantidadeMinima: 1, // quantidade
            embalagem: "Saco",
            faixaDensidadeInicial: 500 // kg/m³
        );

        var produto = new Produto(
            nome: "Produto Teste",
            codigo: "TESTE001",
            tipo: TipoProduto.Fabricante,
            unidadeMedidaId: 1,
            dimensoes: dimensoes,
            categoriaId: 1,
            fornecedorId: 1,
            tipoCalculoPeso: TipoCalculoPeso.PesoNominal
        );

        var quantidade = 10m;
        var distanciaKm = 100m;
        var valorPorKgKm = 0.05m;
        var valorMinimoFrete = 50.00m;

        // Act
        var resultado = _service.CalcularFrete(produto, quantidade, distanciaKm, valorPorKgKm, valorMinimoFrete);

        // Assert
        Assert.Equal(10.0m, resultado.PesoTotal); // 1kg * 10 unidades
        Assert.Equal(0.001m, resultado.VolumeTotal); // 10cm³ * 10 unidades = 1000cm³ = 0.001m³
        Assert.Equal(0.5m, resultado.PesoCubadoTotal); // 0.001m³ * 500kg/m³
        Assert.Equal(10.0m, resultado.PesoParaFrete); // Usando peso nominal
        Assert.Equal(50.00m, resultado.ValorFrete); // Valor mínimo aplicado (10kg * 100km * 0.05 = 50)
        Assert.Equal(TipoCalculoPeso.PesoNominal, resultado.TipoCalculoUtilizado);
    }

    [Fact]
    public void CalcularFrete_ComPesoCubado_DeveUsarPesoCubado()
    {
        // Arrange
        var dimensoes = new DimensoesProduto(
            altura: 50, // cm
            largura: 50, // cm
            comprimento: 50, // cm
            pesoNominal: 1.0m, // kg
            pesoEmbalagem: 1.0m, // kg
            quantidadeMinima: 1, // quantidade
            embalagem: "Saco",
            faixaDensidadeInicial: 200 // kg/m³
        );

        var produto = new Produto(
            nome: "Produto Teste",
            codigo: "TESTE001",
            tipo: TipoProduto.Fabricante,
            unidadeMedidaId: 1,
            dimensoes: dimensoes,
            categoriaId: 1,
            fornecedorId: 1,
            tipoCalculoPeso: TipoCalculoPeso.PesoCubado
        );

        var quantidade = 1m;
        var distanciaKm = 100m;

        // Act
        var resultado = _service.CalcularFrete(produto, quantidade, distanciaKm);

        // Assert
        Assert.Equal(1.0m, resultado.PesoTotal);
        Assert.Equal(0.125m, resultado.VolumeTotal); // 50³ = 125000cm³ = 0.125m³
        Assert.Equal(25.0m, resultado.PesoCubadoTotal); // 0.125m³ * 200kg/m³
        Assert.Equal(25.0m, resultado.PesoParaFrete); // Maior entre peso nominal (1kg) e peso cúbico (25kg)
        Assert.Equal(TipoCalculoPeso.PesoCubado, resultado.TipoCalculoUtilizado);
    }

    [Fact]
    public void CalcularFreteConsolidado_ComMultiplosProdutos_DeveConsolidarCorretamente()
    {
        // Arrange
        var dimensoes1 = new DimensoesProduto(10, 10, 10, 1.0m, 1.0m, 1, "Saco", faixaDensidadeInicial: 500);
        var produto1 = new Produto("Produto 1", "TESTE001", TipoProduto.Fabricante, 1, dimensoes1, 1, 1);

        var dimensoes2 = new DimensoesProduto(20, 20, 20, 2.0m, 2.0m, 1, "Saco", faixaDensidadeInicial: 300);
        var produto2 = new Produto("Produto 2", "TESTE002", TipoProduto.Fabricante, 1, dimensoes2, 1, 1);

        var itens = new List<(Produto produto, decimal quantidade)>
        {
            (produto1, 5m),
            (produto2, 3m)
        };

        var distanciaKm = 100m;

        // Act
        var resultado = _service.CalcularFreteConsolidado(itens, distanciaKm);

        // Assert
        Assert.Equal(2, resultado.CalculosIndividuais.Count());
        Assert.Equal(11.0m, resultado.PesoTotalConsolidado); // (1kg * 5) + (2kg * 3)
        Assert.Equal(0.0245m, resultado.VolumeTotalConsolidado); // (0.001m³ * 5) + (0.008m³ * 3)
        Assert.Equal(4.9m, resultado.PesoCubadoTotalConsolidado); // (0.5kg * 5) + (2.4kg * 3)
    }

    [Fact]
    public void ValidarDisponibilidadeQuantidade_ComQuantidadeDisponivel_DeveRetornarTrue()
    {
        // Arrange
        var pedidoItem = new PedidoItem(1, 1, 100m, 10.0m);
        
        // Simular transportes já agendados
        var transporte1 = new PedidoItemTransporte(pedidoItem.Id, 30m, 50.0m);
        var transporte2 = new PedidoItemTransporte(pedidoItem.Id, 20m, 30.0m);
        
        // Adicionar transportes ao item (simulação)
        pedidoItem.ItensTransporte.Add(transporte1);
        pedidoItem.ItensTransporte.Add(transporte2);

        var quantidadeSolicitada = 40m; // Restam 50m disponíveis (100 - 30 - 20)

        // Act
        var resultado = _service.ValidarDisponibilidadeQuantidade(pedidoItem, quantidadeSolicitada);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void ValidarDisponibilidadeQuantidade_ComQuantidadeIndisponivel_DeveRetornarFalse()
    {
        // Arrange
        var pedidoItem = new PedidoItem(1, 1, 100m, 10.0m);
        
        var transporte1 = new PedidoItemTransporte(pedidoItem.Id, 60m, 100.0m);
        pedidoItem.ItensTransporte.Add(transporte1);

        var quantidadeSolicitada = 50m; // Restam apenas 40m disponíveis

        // Act
        var resultado = _service.ValidarDisponibilidadeQuantidade(pedidoItem, quantidadeSolicitada);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void CalcularQuantidadeDisponivel_ComTransportesAgendados_DeveCalcularCorretamente()
    {
        // Arrange
        var pedidoItem = new PedidoItem(1, 1, 100m, 10.0m);
        
        var transporte1 = new PedidoItemTransporte(pedidoItem.Id, 25m, 50.0m);
        var transporte2 = new PedidoItemTransporte(pedidoItem.Id, 35m, 70.0m);
        
        pedidoItem.ItensTransporte.Add(transporte1);
        pedidoItem.ItensTransporte.Add(transporte2);

        // Act
        var quantidadeDisponivel = _service.CalcularQuantidadeDisponivel(pedidoItem);

        // Assert
        Assert.Equal(40m, quantidadeDisponivel); // 100 - 25 - 35
    }

    [Fact]
    public void CalcularFrete_ComParametrosInvalidos_DeveLancarExcecao()
    {
        // Arrange
        var dimensoes = new DimensoesProduto(10, 10, 10, 1.0m, 1.0m, 1, "Saco");
        var produto = new Produto("Produto", "TESTE", TipoProduto.Fabricante, 1, dimensoes, 1, 1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.CalcularFrete(null!, 10m, 100m));
        Assert.Throws<ArgumentException>(() => _service.CalcularFrete(produto, 0m, 100m));
        Assert.Throws<ArgumentException>(() => _service.CalcularFrete(produto, 10m, 0m));
    }
}