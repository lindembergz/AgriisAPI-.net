using Agriis.Produtos.Dominio.Enums;

namespace Agriis.Produtos.Aplicacao.DTOs;

/// <summary>
/// DTO para exibição de categoria
/// </summary>
public class CategoriaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public CategoriaProduto Tipo { get; set; }
    public bool Ativo { get; set; }
    public int? CategoriaPaiId { get; set; }
    public string? CategoriaPaiNome { get; set; }
    public int Ordem { get; set; }
    public List<CategoriaDto> SubCategorias { get; set; } = new();
    public int QuantidadeProdutos { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

/// <summary>
/// DTO para criação de categoria
/// </summary>
public class CriarCategoriaDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public CategoriaProduto Tipo { get; set; }
    public int? CategoriaPaiId { get; set; }
    public int Ordem { get; set; } = 0;
}

/// <summary>
/// DTO para atualização de categoria
/// </summary>
public class AtualizarCategoriaDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public CategoriaProduto Tipo { get; set; }
    public int? CategoriaPaiId { get; set; }
    public int Ordem { get; set; }
}

/// <summary>
/// DTO resumido para listagens
/// </summary>
public class CategoriaResumoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public CategoriaProduto Tipo { get; set; }
    public bool Ativo { get; set; }
    public int? CategoriaPaiId { get; set; }
    public int Ordem { get; set; }
    public bool TemSubCategorias { get; set; }
    public int QuantidadeProdutos { get; set; }
}