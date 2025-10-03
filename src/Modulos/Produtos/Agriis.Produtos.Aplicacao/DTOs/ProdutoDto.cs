using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Produtos.Dominio.Enums;

namespace Agriis.Produtos.Aplicacao.DTOs;

/// <summary>
/// DTO para exibição de produto
/// </summary>
public class ProdutoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public TipoProduto Tipo { get; set; }
    public StatusProduto Status { get; set; }
    public int UnidadeMedidaId { get; set; }
    public string? UnidadeMedidaNome { get; set; }
    public string? UnidadeMedidaSimbolo { get; set; }
    public int? EmbalagemId { get; set; }
    public string? EmbalagemNome { get; set; }
    public decimal QuantidadeEmbalagem { get; set; }
    public int? AtividadeAgropecuariaId { get; set; }
    public string? AtividadeAgropecuariaNome { get; set; }
    public TipoCalculoPeso TipoCalculoPeso { get; set; }
    public bool ProdutoRestrito { get; set; }
    public string? ObservacoesRestricao { get; set; }
    public int CategoriaId { get; set; }
    public string? CategoriaNome { get; set; }
    public int FornecedorId { get; set; }
    public string? FornecedorNome { get; set; }
    public int? ProdutoPaiId { get; set; }
    public string? ProdutoPaiNome { get; set; }
    public DimensoesProdutoDto Dimensoes { get; set; } = null!;
    public List<int> CulturasIds { get; set; } = new();
    public List<string> CulturasNomes { get; set; } = new();
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

/// <summary>
/// DTO para criação de produto
/// </summary>
public class CriarProdutoDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public TipoProduto Tipo { get; set; }
    public int UnidadeMedidaId { get; set; }
    public int? EmbalagemId { get; set; }
    public decimal QuantidadeEmbalagem { get; set; } = 1.0m;
    public int? AtividadeAgropecuariaId { get; set; }
    public TipoCalculoPeso TipoCalculoPeso { get; set; } = TipoCalculoPeso.PesoNominal;
    public bool ProdutoRestrito { get; set; }
    public string? ObservacoesRestricao { get; set; }
    public int CategoriaId { get; set; }
    public int FornecedorId { get; set; }
    public int? ProdutoPaiId { get; set; }
    public CriarDimensoesProdutoDto Dimensoes { get; set; } = null!;
    public List<int> CulturasIds { get; set; } = new();
}

/// <summary>
/// DTO para atualização de produto
/// </summary>
public class AtualizarProdutoDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public int UnidadeMedidaId { get; set; }
    public int? EmbalagemId { get; set; }
    public decimal QuantidadeEmbalagem { get; set; } = 1.0m;
    public int? AtividadeAgropecuariaId { get; set; }
    public TipoCalculoPeso TipoCalculoPeso { get; set; }
    public bool ProdutoRestrito { get; set; }
    public string? ObservacoesRestricao { get; set; }
    public int CategoriaId { get; set; }
    public AtualizarDimensoesProdutoDto Dimensoes { get; set; } = null!;
    public List<int> CulturasIds { get; set; } = new();
}

/// <summary>
/// DTO para dimensões do produto
/// </summary>
public class DimensoesProdutoDto
{
    public decimal Altura { get; set; }
    public decimal Largura { get; set; }
    public decimal Comprimento { get; set; }
    public decimal PesoNominal { get; set; }
    public decimal PesoEmbalagem { get; set; }
    public decimal? Pms { get; set; }
    public decimal QuantidadeMinima { get; set; }
    public string Embalagem { get; set; } = string.Empty;
    public decimal? FaixaDensidadeInicial { get; set; }
    public decimal? FaixaDensidadeFinal { get; set; }
    public decimal Volume { get; set; }
    public decimal? PesoCubado { get; set; }
    public decimal PesoParaFrete { get; set; }
}

/// <summary>
/// DTO para criação de dimensões do produto
/// </summary>
public class CriarDimensoesProdutoDto
{
    public decimal Altura { get; set; }
    public decimal Largura { get; set; }
    public decimal Comprimento { get; set; }
    public decimal PesoNominal { get; set; }
    public decimal PesoEmbalagem { get; set; }
    public decimal? Pms { get; set; }
    public decimal QuantidadeMinima { get; set; }
    public string Embalagem { get; set; } = string.Empty;
    public decimal? FaixaDensidadeInicial { get; set; }
    public decimal? FaixaDensidadeFinal { get; set; }
}

/// <summary>
/// DTO para atualização de dimensões do produto
/// </summary>
public class AtualizarDimensoesProdutoDto
{
    public decimal Altura { get; set; }
    public decimal Largura { get; set; }
    public decimal Comprimento { get; set; }
    public decimal PesoNominal { get; set; }
    public decimal PesoEmbalagem { get; set; }
    public decimal? Pms { get; set; }
    public decimal QuantidadeMinima { get; set; }
    public string Embalagem { get; set; } = string.Empty;
    public decimal? FaixaDensidadeInicial { get; set; }
    public decimal? FaixaDensidadeFinal { get; set; }
}

/// <summary>
/// DTO resumido para listagens
/// </summary>
public class ProdutoResumoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public TipoProduto Tipo { get; set; }
    public StatusProduto Status { get; set; }
    public string? CategoriaNome { get; set; }
    public string? FornecedorNome { get; set; }
    public bool ProdutoRestrito { get; set; }
}