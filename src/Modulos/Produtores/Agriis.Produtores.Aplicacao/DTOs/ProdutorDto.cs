using Agriis.Produtores.Dominio.Enums;

namespace Agriis.Produtores.Aplicacao.DTOs;

/// <summary>
/// DTO para transferência de dados do produtor
/// </summary>
public class ProdutorDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? CpfFormatado { get; set; }
    public string? Cnpj { get; set; }
    public string? CnpjFormatado { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? TipoAtividade { get; set; }
    public string? Telefone1 { get; set; }
    public string? Telefone2 { get; set; }
    public string? Telefone3 { get; set; }
    public string? Email { get; set; }
    public decimal AreaPlantio { get; set; }
    public string AreaPlantioFormatada { get; set; } = string.Empty;
    public DateTime DataAutorizacao { get; set; }
    public StatusProdutor Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public int? UsuarioAutorizacaoId { get; set; }
    public List<int> Culturas { get; set; } = new();
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    
    // Propriedades calculadas
    public bool EstaAutorizado { get; set; }
    public bool EhPessoaFisica { get; set; }
    public bool EhPessoaJuridica { get; set; }
    public string DocumentoPrincipal { get; set; } = string.Empty;
}

/// <summary>
/// DTO para criação de produtor
/// </summary>
public class CriarProdutorDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Cnpj { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? TipoAtividade { get; set; }
    public string? Telefone1 { get; set; }
    public string? Telefone2 { get; set; }
    public string? Telefone3 { get; set; }
    public string? Email { get; set; }
    public decimal AreaPlantio { get; set; }
    public List<int> Culturas { get; set; } = new();
}

/// <summary>
/// DTO para atualização de produtor
/// </summary>
public class AtualizarProdutorDto
{
    public string Nome { get; set; } = string.Empty;
    public string? InscricaoEstadual { get; set; }
    public string? TipoAtividade { get; set; }
    public string? Telefone1 { get; set; }
    public string? Telefone2 { get; set; }
    public string? Telefone3 { get; set; }
    public string? Email { get; set; }
    public decimal AreaPlantio { get; set; }
    public List<int> Culturas { get; set; } = new();
}

/// <summary>
/// DTO para filtros de busca de produtores
/// </summary>
public class FiltrosProdutorDto
{
    public string? Filtro { get; set; }
    public StatusProdutor? Status { get; set; }
    public int? CulturaId { get; set; }
    public decimal? AreaMinima { get; set; }
    public decimal? AreaMaxima { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
}

/// <summary>
/// DTO para estatísticas de produtores
/// </summary>
public class ProdutorEstatisticasDto
{
    public int TotalProdutores { get; set; }
    public int ProdutoresAutorizados { get; set; }
    public int ProdutoresPendentes { get; set; }
    public int ProdutoresNegados { get; set; }
    public decimal AreaTotalPlantio { get; set; }
    public decimal AreaMediaPlantio { get; set; }
    public string AreaTotalFormatada { get; set; } = string.Empty;
    public string AreaMediaFormatada { get; set; } = string.Empty;
}