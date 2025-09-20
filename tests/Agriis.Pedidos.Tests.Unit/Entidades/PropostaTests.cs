using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Enums;
using Xunit;

namespace Agriis.Pedidos.Tests.Unit.Entidades;

/// <summary>
/// Testes unitários para a entidade Proposta
/// </summary>
public class PropostaTests
{
    [Fact]
    public void Proposta_DeveCriarPropostaProdutor_ComDadosValidos()
    {
        // Arrange
        var pedidoId = 1;
        var acaoComprador = AcaoCompradorPedido.Iniciou;
        var usuarioProdutorId = 10;
        var observacao = "Iniciou a negociação";
        
        // Act
        var proposta = new Proposta(pedidoId, acaoComprador, usuarioProdutorId, observacao);
        
        // Assert
        Assert.Equal(pedidoId, proposta.PedidoId);
        Assert.Equal(acaoComprador, proposta.AcaoComprador);
        Assert.Equal(usuarioProdutorId, proposta.UsuarioProdutorId);
        Assert.Equal(observacao, proposta.Observacao);
        Assert.Null(proposta.UsuarioFornecedorId);
        Assert.True(proposta.EhPropostaProdutor());
        Assert.False(proposta.EhPropostaFornecedor());
    }
    
    [Fact]
    public void Proposta_DeveCriarPropostaFornecedor_ComDadosValidos()
    {
        // Arrange
        var pedidoId = 1;
        var observacao = "Proposta de desconto especial";
        var usuarioFornecedorId = 20;
        
        // Act
        var proposta = new Proposta(pedidoId, observacao, usuarioFornecedorId);
        
        // Assert
        Assert.Equal(pedidoId, proposta.PedidoId);
        Assert.Null(proposta.AcaoComprador);
        Assert.Equal(observacao, proposta.Observacao);
        Assert.Equal(usuarioFornecedorId, proposta.UsuarioFornecedorId);
        Assert.Null(proposta.UsuarioProdutorId);
        Assert.False(proposta.EhPropostaProdutor());
        Assert.True(proposta.EhPropostaFornecedor());
    }
    
    [Fact]
    public void Proposta_DeveRejeitarPedidoIdInvalido()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Proposta(0, AcaoCompradorPedido.Iniciou, 10));
            
        Assert.Throws<ArgumentException>(() => 
            new Proposta(-1, AcaoCompradorPedido.Iniciou, 10));
    }
    
    [Fact]
    public void Proposta_DeveRejeitarUsuarioProdutorIdInvalido()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Proposta(1, AcaoCompradorPedido.Iniciou, 0));
            
        Assert.Throws<ArgumentException>(() => 
            new Proposta(1, AcaoCompradorPedido.Iniciou, -1));
    }
    
    [Fact]
    public void Proposta_DeveRejeitarObservacaoVaziaParaFornecedor()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Proposta(1, "", 20));
            
        Assert.Throws<ArgumentException>(() => 
            new Proposta(1, "   ", 20));
            
        Assert.Throws<ArgumentException>(() => 
            new Proposta(1, null!, 20));
    }
    
    [Fact]
    public void Proposta_DeveRejeitarUsuarioFornecedorIdInvalido()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Proposta(1, "Observação válida", 0));
            
        Assert.Throws<ArgumentException>(() => 
            new Proposta(1, "Observação válida", -1));
    }
    
    [Theory]
    [InlineData(AcaoCompradorPedido.Iniciou)]
    [InlineData(AcaoCompradorPedido.Aceitou)]
    [InlineData(AcaoCompradorPedido.AlterouCarrinho)]
    [InlineData(AcaoCompradorPedido.Cancelou)]
    public void Proposta_DeveAceitarTodasAcoesComprador(AcaoCompradorPedido acao)
    {
        // Arrange & Act
        var proposta = new Proposta(1, acao, 10);
        
        // Assert
        Assert.Equal(acao, proposta.AcaoComprador);
    }
}