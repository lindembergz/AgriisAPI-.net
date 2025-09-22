using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Enums;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace Agriis.Tests.Unit.Entidades;

/// <summary>
/// Testes unitários para a entidade Pedido
/// </summary>
public class PedidoTests
{
    [Fact]
    public void Pedido_DeveCriarComParametrosValidos()
    {
        // Arrange
        var fornecedorId = 1;
        var produtorId = 2;
        var permiteContato = true;
        var negociarPedido = true;
        var diasLimite = 7;

        // Act
        var pedido = new Pedido(fornecedorId, produtorId, permiteContato, negociarPedido, diasLimite);

        // Assert
        pedido.FornecedorId.Should().Be(fornecedorId);
        pedido.ProdutorId.Should().Be(produtorId);
        pedido.PermiteContato.Should().Be(permiteContato);
        pedido.NegociarPedido.Should().Be(negociarPedido);
        pedido.Status.Should().Be(StatusPedido.EmNegociacao);
        pedido.StatusCarrinho.Should().Be(StatusCarrinho.EmAberto);
        pedido.QuantidadeItens.Should().Be(0);
        pedido.DataLimiteInteracao.Should().BeCloseTo(DateTime.UtcNow.AddDays(diasLimite), TimeSpan.FromSeconds(1));
        pedido.Itens.Should().BeEmpty();
        pedido.Propostas.Should().BeEmpty();
    }

    [Fact]
    public void Pedido_DeveCriarComDiasLimitePadrao()
    {
        // Arrange
        var fornecedorId = 1;
        var produtorId = 2;

        // Act
        var pedido = new Pedido(fornecedorId, produtorId, true, true);

        // Assert
        pedido.DataLimiteInteracao.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Pedido_DeveLancarExcecaoParaFornecedorIdInvalido(int fornecedorIdInvalido)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Pedido(fornecedorIdInvalido, 1, true, true));
        exception.Message.Should().Contain("ID do fornecedor deve ser maior que zero");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Pedido_DeveLancarExcecaoParaProdutorIdInvalido(int produtorIdInvalido)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Pedido(1, produtorIdInvalido, true, true));
        exception.Message.Should().Contain("ID do produtor deve ser maior que zero");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Pedido_DeveLancarExcecaoParaDiasLimiteInvalido(int diasInvalidos)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Pedido(1, 2, true, true, diasInvalidos));
        exception.Message.Should().Contain("Dias limite deve ser maior que zero");
    }

    [Fact]
    public void Pedido_DeveAdicionarItem()
    {
        // Arrange
        var pedido = CriarPedidoValido();
        var item = CriarPedidoItemValido(pedido.Id);

        // Act
        pedido.AdicionarItem(item);

        // Assert
        pedido.Itens.Should().Contain(item);
        pedido.QuantidadeItens.Should().Be(1);
        pedido.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Pedido_DeveLancarExcecaoAoAdicionarItemNulo()
    {
        // Arrange
        var pedido = CriarPedidoValido();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => pedido.AdicionarItem(null!));
    }

    [Fact]
    public void Pedido_DeveLancarExcecaoAoAdicionarItemQuandoNaoEmNegociacao()
    {
        // Arrange
        var pedido = CriarPedidoValido();
        pedido.FecharPedido();
        var item = CriarPedidoItemValido(pedido.Id);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => pedido.AdicionarItem(item));
        exception.Message.Should().Contain("Não é possível adicionar itens a um pedido que não está em negociação");
    }

    [Fact]
    public void Pedido_DeveRemoverItem()
    {
        // Arrange
        var pedido = CriarPedidoValido();
        var item = CriarPedidoItemValido(pedido.Id);
        pedido.AdicionarItem(item);
        
        // Simular ID do item
        var propriedadeId = typeof(Agriis.Compartilhado.Dominio.Entidades.EntidadeBase).GetProperty("Id");
        propriedadeId!.SetValue(item, 1);

        // Act
        pedido.RemoverItem(1);

        // Assert
        pedido.Itens.Should().NotContain(item);
        pedido.QuantidadeItens.Should().Be(0);
        pedido.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Pedido_DeveLancarExcecaoAoRemoverItemQuandoNaoEmNegociacao()
    {
        // Arrange
        var pedido = CriarPedidoValido();
        pedido.FecharPedido();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => pedido.RemoverItem(1));
        exception.Message.Should().Contain("Não é possível remover itens de um pedido que não está em negociação");
    }

    [Fact]
    public void Pedido_DeveFecharPedido()
    {
        // Arrange
        var pedido = CriarPedidoValido();
        var item = CriarPedidoItemValido(pedido.Id);
        pedido.AdicionarItem(item);

        // Act
        pedido.FecharPedido();

        // Assert
        pedido.Status.Should().Be(StatusPedido.Fechado);
        pedido.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Pedido_DeveLancarExcecaoAoFecharPedidoNaoEmNegociacao()
    {
        // Arrange
        var pedido = CriarPedidoValido();
        pedido.CancelarPorComprador();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => pedido.FecharPedido());
        exception.Message.Should().Contain("Apenas pedidos em negociação podem ser fechados");
    }

    [Fact]
    public void Pedido_DeveLancarExcecaoAoFecharPedidoSemItens()
    {
        // Arrange
        var pedido = CriarPedidoValido();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => pedido.FecharPedido());
        exception.Message.Should().Contain("Não é possível fechar um pedido sem itens");
    }

    [Fact]
    public void Pedido_DeveCancelarPorComprador()
    {
        // Arrange
        var pedido = CriarPedidoValido();

        // Act
        pedido.CancelarPorComprador();

        // Assert
        pedido.Status.Should().Be(StatusPedido.CanceladoPeloComprador);
        pedido.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Pedido_DeveLancarExcecaoAoCancelarPedidoFechado()
    {
        // Arrange
        var pedido = CriarPedidoValido();
        var item = CriarPedidoItemValido(pedido.Id);
        pedido.AdicionarItem(item);
        pedido.FecharPedido();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => pedido.CancelarPorComprador());
        exception.Message.Should().Contain("Não é possível cancelar um pedido já fechado");
    }

    [Fact]
    public void Pedido_DeveCancelarPorTempoLimite()
    {
        // Arrange
        var pedido = CriarPedidoValido();

        // Act
        pedido.CancelarPorTempoLimite();

        // Assert
        pedido.Status.Should().Be(StatusPedido.CanceladoPorTempoLimite);
        pedido.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Pedido_DeveAtualizarStatus()
    {
        // Arrange
        var pedido = CriarPedidoValido();
        var novoStatus = StatusPedido.Fechado;

        // Act
        pedido.AtualizarStatus(novoStatus);

        // Assert
        pedido.Status.Should().Be(novoStatus);
        pedido.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Pedido_DeveAtualizarTotais()
    {
        // Arrange
        var pedido = CriarPedidoValido();
        var totais = JsonDocument.Parse("""{"valor_total": 1000.50, "desconto": 50.25}""");

        // Act
        pedido.AtualizarTotais(totais);

        // Assert
        pedido.Totais.Should().NotBeNull();
        pedido.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Pedido_DeveVerificarSeDentroPrazoLimite()
    {
        // Arrange
        var pedido = CriarPedidoValido();

        // Act & Assert
        pedido.EstaDentroPrazoLimite().Should().BeTrue();
    }

    [Fact]
    public void Pedido_DeveVerificarSeForaPrazoLimite()
    {
        // Arrange
        var pedido = new Pedido(1, 2, true, true, 1);
        
        // Simular que o prazo já passou
        var propriedadeDataLimite = typeof(Pedido).GetProperty("DataLimiteInteracao");
        propriedadeDataLimite!.SetValue(pedido, DateTime.UtcNow.AddDays(-1));

        // Act & Assert
        pedido.EstaDentroPrazoLimite().Should().BeFalse();
    }

    [Fact]
    public void Pedido_DeveAtualizarPrazoLimite()
    {
        // Arrange
        var pedido = CriarPedidoValido();
        var novosDias = 10;

        // Act
        pedido.AtualizarPrazoLimite(novosDias);

        // Assert
        pedido.DataLimiteInteracao.Should().BeCloseTo(DateTime.UtcNow.AddDays(novosDias), TimeSpan.FromSeconds(1));
        pedido.DataAtualizacao.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Pedido_DeveLancarExcecaoParaPrazoLimiteInvalido(int diasInvalidos)
    {
        // Arrange
        var pedido = CriarPedidoValido();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => pedido.AtualizarPrazoLimite(diasInvalidos));
        exception.Message.Should().Contain("Dias deve ser maior que zero");
    }

    [Fact]
    public void Pedido_DeveRecalcularQuantidadeItensAoAdicionarItem()
    {
        // Arrange
        var pedido = CriarPedidoValido();
        var item1 = CriarPedidoItemValido(pedido.Id);
        var item2 = CriarPedidoItemValido(pedido.Id);

        // Act
        pedido.AdicionarItem(item1);
        pedido.AdicionarItem(item2);

        // Assert
        pedido.QuantidadeItens.Should().Be(2);
    }

    [Fact]
    public void Pedido_DeveRecalcularQuantidadeItensAoRemoverItem()
    {
        // Arrange
        var pedido = CriarPedidoValido();
        var item1 = CriarPedidoItemValido(pedido.Id);
        var item2 = CriarPedidoItemValido(pedido.Id);
        pedido.AdicionarItem(item1);
        pedido.AdicionarItem(item2);
        
        // Simular IDs dos itens
        var propriedadeId = typeof(Agriis.Compartilhado.Dominio.Entidades.EntidadeBase).GetProperty("Id");
        propriedadeId!.SetValue(item1, 1);
        propriedadeId!.SetValue(item2, 2);

        // Act
        pedido.RemoverItem(1);

        // Assert
        pedido.QuantidadeItens.Should().Be(1);
    }

    private static Pedido CriarPedidoValido()
    {
        return new Pedido(1, 2, true, true, 7);
    }

    private static PedidoItem CriarPedidoItemValido(int pedidoId)
    {
        return new PedidoItem(pedidoId, 1, 10m, 100m, 5m, "Observações teste");
    }
}