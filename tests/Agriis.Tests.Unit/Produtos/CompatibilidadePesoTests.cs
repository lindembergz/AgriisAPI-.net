using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.Enums;
using Agriis.Produtos.Dominio.ObjetosValor;
using Xunit;

namespace Agriis.Tests.Unit.Produtos;

/// <summary>
/// Testes de compatibilidade para garantir que os cálculos de peso 
/// no C# produzem os mesmos resultados que o sistema Python
/// </summary>
public class CompatibilidadePesoTests
{
    [Fact]
    public void CalcularPesoProduto_ProdutoSementes_DeveReplicarLogicaPython()
    {
        // Arrange - Cenário baseado no sistema Python
        var dimensoes = new DimensoesProduto(
            altura: 10.0m,
            largura: 15.0m,
            comprimento: 20.0m,
            pesoNominal: 50.0m,
            pesoEmbalagem: 25.0m,
            quantidadeMinima: 60000m, // 60.000 sementes
            embalagem: "Saco",
            pms: 300.0m, // 300 gramas por mil sementes
            faixaDensidadeInicial: 500.0m,
            faixaDensidadeFinal: 600.0m
        );

        var categoria = new Categoria("Sementes", CategoriaProduto.Sementes, "Categoria de sementes");
        
        var produto = new Produto(
            nome: "Semente de Soja",
            codigo: "SEM001",
            tipo: TipoProduto.Fabricante,
            unidade: TipoUnidade.Sementes,
            dimensoes: dimensoes,
            categoriaId: 1,
            fornecedorId: 1
        );

        // Simular categoria carregada
        typeof(Produto).GetProperty("Categoria")!.SetValue(produto, categoria);

        // Act
        var pesoCalculado = produto.CalcularPesoProduto();

        // Assert - Replicar cálculo Python exato:
        // peso_produto = ((produto.pms / 1000) / 1000) * produto.quantidade_minima
        // peso_produto = ((300 / 1000) / 1000) * 60000 = (0.3 / 1000) * 60000 = 0.0003 * 60000 = 18.0
        var pesoEsperado = ((300.0m / 1000) / 1000) * 60000m; // 18.0 kg
        
        Assert.Equal(pesoEsperado, pesoCalculado);
        Assert.Equal(18.0m, pesoCalculado);
    }

    [Fact]
    public void CalcularPesoProduto_ProdutoNaoSementes_DeveUsarPesoEmbalagem()
    {
        // Arrange - Produto que não é semente
        var dimensoes = new DimensoesProduto(
            altura: 30.0m,
            largura: 40.0m,
            comprimento: 50.0m,
            pesoNominal: 100.0m,
            pesoEmbalagem: 75.0m,
            quantidadeMinima: 1.0m,
            embalagem: "Tambor",
            pms: null, // Não aplicável para não-sementes
            faixaDensidadeInicial: 800.0m,
            faixaDensidadeFinal: 900.0m
        );

        var categoria = new Categoria("Fertilizantes", CategoriaProduto.Fertilizantes, "Categoria de fertilizantes");
        
        var produto = new Produto(
            nome: "Fertilizante NPK",
            codigo: "FERT001",
            tipo: TipoProduto.Fabricante,
            unidade: TipoUnidade.Quilo,
            dimensoes: dimensoes,
            categoriaId: 2,
            fornecedorId: 1
        );

        // Simular categoria carregada
        typeof(Produto).GetProperty("Categoria")!.SetValue(produto, categoria);

        // Act
        var pesoCalculado = produto.CalcularPesoProduto();

        // Assert - Para não-sementes, deve usar peso_embalagem
        Assert.Equal(75.0m, pesoCalculado);
    }

    [Theory]
    [InlineData(100.0, 60000.0, 6.0)] // PMS 100g, 60k sementes = 6kg
    [InlineData(200.0, 50000.0, 10.0)] // PMS 200g, 50k sementes = 10kg  
    [InlineData(350.0, 40000.0, 14.0)] // PMS 350g, 40k sementes = 14kg
    [InlineData(500.0, 20000.0, 10.0)] // PMS 500g, 20k sementes = 10kg
    public void CalcularPesoProduto_VariosValoresPMS_DeveCalcularCorretamente(
        decimal pms, decimal quantidadeMinima, decimal pesoEsperado)
    {
        // Arrange
        var dimensoes = new DimensoesProduto(
            altura: 10.0m,
            largura: 15.0m,
            comprimento: 20.0m,
            pesoNominal: 50.0m,
            pesoEmbalagem: 25.0m,
            quantidadeMinima: quantidadeMinima,
            embalagem: "Saco",
            pms: pms,
            faixaDensidadeInicial: 500.0m,
            faixaDensidadeFinal: 600.0m
        );

        var categoria = new Categoria("Sementes", CategoriaProduto.Sementes, "Categoria de sementes");
        
        var produto = new Produto(
            nome: "Semente Teste",
            codigo: "TEST001",
            tipo: TipoProduto.Fabricante,
            unidade: TipoUnidade.Sementes,
            dimensoes: dimensoes,
            categoriaId: 1,
            fornecedorId: 1
        );

        // Simular categoria carregada
        typeof(Produto).GetProperty("Categoria")!.SetValue(produto, categoria);

        // Act
        var pesoCalculado = produto.CalcularPesoProduto();

        // Assert - Verificar cálculo: ((pms / 1000) / 1000) * quantidade_minima
        Assert.Equal(pesoEsperado, pesoCalculado);
    }
}