using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.Enums;
using Agriis.Produtos.Dominio.ObjetosValor;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace Agriis.Tests.Unit.Entidades;

/// <summary>
/// Testes unitários para a entidade Produto
/// </summary>
public class ProdutoTests
{
    [Fact]
    public void Produto_DeveCriarProdutoFabricante()
    {
        // Arrange
        var nome = "Semente de Soja";
        var codigo = "SOJ001";
        var tipo = TipoProduto.Fabricante;
        var unidade = TipoUnidade.Quilo;
        var dimensoes = new DimensoesProduto(10, 20, 30, 25, 800);
        var categoriaId = 1;
        var fornecedorId = 1;

        // Act
        var produto = new Produto(nome, codigo, tipo, unidade, dimensoes, categoriaId, fornecedorId);

        // Assert
        produto.Nome.Should().Be(nome);
        produto.Codigo.Should().Be(codigo);
        produto.Tipo.Should().Be(tipo);
        produto.Unidade.Should().Be(unidade);
        produto.Dimensoes.Should().Be(dimensoes);
        produto.CategoriaId.Should().Be(categoriaId);
        produto.FornecedorId.Should().Be(fornecedorId);
        produto.Status.Should().Be(StatusProduto.Ativo);
        produto.TipoCalculoPeso.Should().Be(TipoCalculoPeso.PesoNominal);
        produto.ProdutoRestrito.Should().BeFalse();
        produto.ProdutoPaiId.Should().BeNull();
        produto.EhFabricante().Should().BeTrue();
        produto.EhRevendedor().Should().BeFalse();
        produto.EstaAtivo().Should().BeTrue();
    }

    [Fact]
    public void Produto_DeveCriarProdutoRevendedor()
    {
        // Arrange
        var nome = "Fertilizante NPK";
        var codigo = "NPK001";
        var tipo = TipoProduto.Revendedor;
        var unidade = TipoUnidade.Quilo;
        var dimensoes = new DimensoesProduto(50, 30, 10, 50, 1200);
        var categoriaId = 2;
        var fornecedorId = 1;
        var produtoPaiId = 123;

        // Act
        var produto = new Produto(nome, codigo, tipo, unidade, dimensoes, categoriaId, fornecedorId, 
            produtoPaiId: produtoPaiId);

        // Assert
        produto.Tipo.Should().Be(tipo);
        produto.ProdutoPaiId.Should().Be(produtoPaiId);
        produto.EhFabricante().Should().BeFalse();
        produto.EhRevendedor().Should().BeTrue();
        produto.TemProdutoPai().Should().BeTrue();
    }

    [Fact]
    public void Produto_DeveCriarComParametrosOpcionais()
    {
        // Arrange
        var nome = "Defensivo ABC";
        var codigo = "DEF001";
        var tipo = TipoProduto.Fabricante;
        var unidade = TipoUnidade.Litro;
        var dimensoes = new DimensoesProduto(15, 10, 25, 1, 900);
        var categoriaId = 3;
        var fornecedorId = 1;
        var descricao = "Defensivo para controle de pragas";
        var marca = "AgroTech";
        var tipoCalculoPeso = TipoCalculoPeso.PesoCubado;
        var produtoRestrito = true;
        var observacoesRestricao = "Requer receituário agronômico";

        // Act
        var produto = new Produto(nome, codigo, tipo, unidade, dimensoes, categoriaId, fornecedorId,
            descricao, marca, tipoCalculoPeso, produtoRestrito, observacoesRestricao);

        // Assert
        produto.Descricao.Should().Be(descricao);
        produto.Marca.Should().Be(marca);
        produto.TipoCalculoPeso.Should().Be(tipoCalculoPeso);
        produto.ProdutoRestrito.Should().Be(produtoRestrito);
        produto.ObservacoesRestricao.Should().Be(observacoesRestricao);
    }

    [Theory]
    [InlineData("")]
    public void Produto_DeveLancarExcecaoParaNomeInvalido(string nomeInvalido)
    {
        // Arrange
        var dimensoes = CriarDimensoesValidas();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Produto(nomeInvalido, "COD001", TipoProduto.Fabricante, TipoUnidade.Quilo, dimensoes, 1, 1));
    }

    [Fact]
    public void Produto_DeveLancarExcecaoParaNomeNulo()
    {
        // Arrange
        var dimensoes = CriarDimensoesValidas();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Produto(null!, "COD001", TipoProduto.Fabricante, TipoUnidade.Quilo, dimensoes, 1, 1));
    }

    [Theory]
    [InlineData("")]
    public void Produto_DeveLancarExcecaoParaCodigoInvalido(string codigoInvalido)
    {
        // Arrange
        var dimensoes = CriarDimensoesValidas();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Produto("Produto", codigoInvalido, TipoProduto.Fabricante, TipoUnidade.Quilo, dimensoes, 1, 1));
    }

    [Fact]
    public void Produto_DeveLancarExcecaoParaCodigoNulo()
    {
        // Arrange
        var dimensoes = CriarDimensoesValidas();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Produto("Produto", null!, TipoProduto.Fabricante, TipoUnidade.Quilo, dimensoes, 1, 1));
    }

    [Fact]
    public void Produto_DeveLancarExcecaoParaDimensoesNulas()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Produto("Produto", "COD001", TipoProduto.Fabricante, TipoUnidade.Quilo, null!, 1, 1));
    }

    [Fact]
    public void Produto_DeveLancarExcecaoParaProdutoRevendedorSemPai()
    {
        // Arrange
        var dimensoes = CriarDimensoesValidas();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            new Produto("Produto", "COD001", TipoProduto.Revendedor, TipoUnidade.Quilo, dimensoes, 1, 1));
        exception.Message.Should().Contain("Produtos revendedores devem ter um produto pai");
    }

    [Fact]
    public void Produto_DeveLancarExcecaoParaProdutoFabricanteComPai()
    {
        // Arrange
        var dimensoes = CriarDimensoesValidas();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            new Produto("Produto", "COD001", TipoProduto.Fabricante, TipoUnidade.Quilo, dimensoes, 1, 1, 
                produtoPaiId: 123));
        exception.Message.Should().Contain("Produtos fabricantes não podem ter produto pai");
    }

    [Fact]
    public void Produto_DeveAtualizarInformacoes()
    {
        // Arrange
        var produto = CriarProdutoValido();
        var novoNome = "Novo Nome";
        var novaDescricao = "Nova Descrição";
        var novaMarca = "Nova Marca";

        // Act
        produto.AtualizarInformacoes(novoNome, novaDescricao, novaMarca);

        // Assert
        produto.Nome.Should().Be(novoNome);
        produto.Descricao.Should().Be(novaDescricao);
        produto.Marca.Should().Be(novaMarca);
        produto.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produto_DeveAtualizarCodigo()
    {
        // Arrange
        var produto = CriarProdutoValido();
        var novoCodigo = "NOVO001";

        // Act
        produto.AtualizarCodigo(novoCodigo);

        // Assert
        produto.Codigo.Should().Be(novoCodigo);
        produto.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produto_DeveAtualizarDimensoes()
    {
        // Arrange
        var produto = CriarProdutoValido();
        var novasDimensoes = new DimensoesProduto(20, 30, 40, 50, 1000);

        // Act
        produto.AtualizarDimensoes(novasDimensoes);

        // Assert
        produto.Dimensoes.Should().Be(novasDimensoes);
        produto.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produto_DeveAtualizarTipoCalculoPeso()
    {
        // Arrange
        var produto = CriarProdutoValido();
        var novoTipo = TipoCalculoPeso.PesoCubado;

        // Act
        produto.AtualizarTipoCalculoPeso(novoTipo);

        // Assert
        produto.TipoCalculoPeso.Should().Be(novoTipo);
        produto.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produto_DeveDefinirRestricao()
    {
        // Arrange
        var produto = CriarProdutoValido();
        var observacoes = "Produto controlado";

        // Act
        produto.DefinirRestricao(true, observacoes);

        // Assert
        produto.ProdutoRestrito.Should().BeTrue();
        produto.ObservacoesRestricao.Should().Be(observacoes);
        produto.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produto_DeveRemoverRestricao()
    {
        // Arrange
        var produto = CriarProdutoValido();
        produto.DefinirRestricao(true, "Observações");

        // Act
        produto.DefinirRestricao(false);

        // Assert
        produto.ProdutoRestrito.Should().BeFalse();
        produto.ObservacoesRestricao.Should().BeNull();
    }

    [Fact]
    public void Produto_DeveAtualizarCategoria()
    {
        // Arrange
        var produto = CriarProdutoValido();
        var novaCategoriaId = 999;

        // Act
        produto.AtualizarCategoria(novaCategoriaId);

        // Assert
        produto.CategoriaId.Should().Be(novaCategoriaId);
        produto.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produto_DeveDefinirProdutoPai()
    {
        // Arrange
        var produto = CriarProdutoRevendedor();
        var novoProdutoPaiId = 456;

        // Act
        produto.DefinirProdutoPai(novoProdutoPaiId);

        // Assert
        produto.ProdutoPaiId.Should().Be(novoProdutoPaiId);
        produto.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produto_DeveLancarExcecaoAoDefinirProdutoPaiComoProprioId()
    {
        // Arrange
        var produto = CriarProdutoRevendedor();
        
        // Simular que o produto tem ID
        var propriedadeId = typeof(Agriis.Compartilhado.Dominio.Entidades.EntidadeBase).GetProperty("Id");
        propriedadeId!.SetValue(produto, 123);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => produto.DefinirProdutoPai(123));
        exception.Message.Should().Contain("Um produto não pode ser pai de si mesmo");
    }

    [Fact]
    public void Produto_DeveAtivar()
    {
        // Arrange
        var produto = CriarProdutoValido();
        produto.Inativar();

        // Act
        produto.Ativar();

        // Assert
        produto.Status.Should().Be(StatusProduto.Ativo);
        produto.EstaAtivo().Should().BeTrue();
        produto.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produto_DeveInativar()
    {
        // Arrange
        var produto = CriarProdutoValido();

        // Act
        produto.Inativar();

        // Assert
        produto.Status.Should().Be(StatusProduto.Inativo);
        produto.EstaAtivo().Should().BeFalse();
        produto.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produto_DeveDescontinuar()
    {
        // Arrange
        var produto = CriarProdutoValido();

        // Act
        produto.Descontinuar();

        // Assert
        produto.Status.Should().Be(StatusProduto.Descontinuado);
        produto.EstaAtivo().Should().BeFalse();
        produto.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produto_DeveAdicionarCultura()
    {
        // Arrange
        var produto = CriarProdutoValido();
        var culturaId = 1;

        // Act
        produto.AdicionarCultura(culturaId);

        // Assert
        produto.ObterCulturasCompativeis().Should().Contain(culturaId);
        produto.EhCompativelComCultura(culturaId).Should().BeTrue();
        produto.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produto_NaoDeveAdicionarCulturaDuplicada()
    {
        // Arrange
        var produto = CriarProdutoValido();
        var culturaId = 1;
        produto.AdicionarCultura(culturaId);
        var quantidadeAntes = produto.ProdutosCulturas.Count;

        // Act
        produto.AdicionarCultura(culturaId);

        // Assert
        produto.ProdutosCulturas.Count.Should().Be(quantidadeAntes);
    }

    [Fact]
    public void Produto_DeveRemoverCultura()
    {
        // Arrange
        var produto = CriarProdutoValido();
        var culturaId = 1;
        produto.AdicionarCultura(culturaId);

        // Act
        produto.RemoverCultura(culturaId);

        // Assert
        produto.ObterCulturasCompativeis().Should().NotContain(culturaId);
        produto.EhCompativelComCultura(culturaId).Should().BeFalse();
        produto.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produto_DeveAtualizarDadosAdicionais()
    {
        // Arrange
        var produto = CriarProdutoValido();
        var dados = JsonDocument.Parse("""{"propriedade": "valor", "numero": 123}""");

        // Act
        produto.AtualizarDadosAdicionais(dados);

        // Assert
        produto.DadosAdicionais.Should().NotBeNull();
        produto.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produto_DeveCalcularPesoParaFretePesoNominal()
    {
        // Arrange
        var dimensoes = new DimensoesProduto(10, 10, 10, 5, 1000);
        var produto = new Produto("Produto", "COD001", TipoProduto.Fabricante, TipoUnidade.Quilo, 
            dimensoes, 1, 1, tipoCalculoPeso: TipoCalculoPeso.PesoNominal);

        // Act
        var peso = produto.CalcularPesoParaFrete();

        // Assert
        peso.Should().Be(5); // Peso nominal
    }

    [Fact]
    public void Produto_DeveCalcularPesoParaFretePesoCubado()
    {
        // Arrange
        var dimensoes = new DimensoesProduto(10, 10, 10, 0.5m, 1000); // Volume pequeno, densidade alta
        var produto = new Produto("Produto", "COD001", TipoProduto.Fabricante, TipoUnidade.Quilo, 
            dimensoes, 1, 1, tipoCalculoPeso: TipoCalculoPeso.PesoCubado);

        // Act
        var peso = produto.CalcularPesoParaFrete();

        // Assert
        peso.Should().Be(1); // Peso cúbico (0.001 m³ * 1000 kg/m³ = 1 kg) > peso nominal (0.5 kg)
    }

    [Fact]
    public void Produto_DeveVerificarSeTemProdutosFilhos()
    {
        // Arrange
        var produto = CriarProdutoValido();

        // Act & Assert
        produto.TemProdutosFilhos().Should().BeFalse();
        
        // Simular adição de produto filho
        var produtoFilho = CriarProdutoRevendedor();
        produto.ProdutosFilhos.Add(produtoFilho);
        
        produto.TemProdutosFilhos().Should().BeTrue();
    }

    private static Produto CriarProdutoValido()
    {
        var dimensoes = CriarDimensoesValidas();
        return new Produto("Produto Teste", "TEST001", TipoProduto.Fabricante, TipoUnidade.Quilo, 
            dimensoes, 1, 1);
    }

    private static Produto CriarProdutoRevendedor()
    {
        var dimensoes = CriarDimensoesValidas();
        return new Produto("Produto Revendedor", "REV001", TipoProduto.Revendedor, TipoUnidade.Quilo, 
            dimensoes, 1, 1, produtoPaiId: 123);
    }

    private static DimensoesProduto CriarDimensoesValidas()
    {
        return new DimensoesProduto(10, 20, 30, 25, 800);
    }
}