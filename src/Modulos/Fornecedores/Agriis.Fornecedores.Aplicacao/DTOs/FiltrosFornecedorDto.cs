using System.ComponentModel.DataAnnotations;

namespace Agriis.Fornecedores.Aplicacao.DTOs;

/// <summary>
/// DTO para filtros de consulta paginada de fornecedores
/// </summary>
public class FiltrosFornecedorDto
{
    /// <summary>
    /// Número da página (começando em 1)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior que 0")]
    public int Pagina { get; set; } = 1;

    /// <summary>
    /// Tamanho da página (quantidade de itens por página)
    /// </summary>
    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100")]
    public int TamanhoPagina { get; set; } = 10;

    /// <summary>
    /// Filtro de texto para busca por nome ou CNPJ
    /// </summary>
    public string? Filtro { get; set; }

    /// <summary>
    /// Filtro por nome do fornecedor
    /// </summary>
    public string? Nome { get; set; }

    /// <summary>
    /// Filtro por CNPJ do fornecedor
    /// </summary>
    public string? Cnpj { get; set; }

    /// <summary>
    /// Filtro por status ativo/inativo
    /// </summary>
    public bool? Ativo { get; set; }

    /// <summary>
    /// Filtro por UF (ID do estado)
    /// </summary>
    public int? UfId { get; set; }

    /// <summary>
    /// Filtro por município (ID do município)
    /// </summary>
    public int? MunicipioId { get; set; }

    /// <summary>
    /// Filtro por moeda padrão
    /// </summary>
    public int? MoedaPadrao { get; set; }

    /// <summary>
    /// Filtro por ramos de atividade
    /// </summary>
    public List<string>? RamosAtividade { get; set; }

    /// <summary>
    /// Filtro por endereço de correspondência
    /// </summary>
    public string? EnderecoCorrespondencia { get; set; }
}