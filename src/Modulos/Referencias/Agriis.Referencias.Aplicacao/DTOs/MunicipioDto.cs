using System.ComponentModel.DataAnnotations;

namespace Agriis.Referencias.Aplicacao.DTOs;

/// <summary>
/// DTO para leitura de município
/// </summary>
public class MunicipioDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CodigoIbge { get; set; } = string.Empty;
    public int UfId { get; set; }
    public string UfNome { get; set; } = string.Empty;
    public string UfCodigo { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// DTO para criação de município
/// </summary>
public class CriarMunicipioDto
{
    [Required(ErrorMessage = "O nome do município é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O código IBGE é obrigatório")]
    [StringLength(7, MinimumLength = 7, ErrorMessage = "O código IBGE deve ter exatamente 7 dígitos")]
    [RegularExpression(@"^\d{7}$", ErrorMessage = "O código IBGE deve conter apenas números")]
    public string CodigoIbge { get; set; } = string.Empty;

    [Required(ErrorMessage = "A UF é obrigatória")]
    [Range(1, int.MaxValue, ErrorMessage = "Deve ser selecionada uma UF válida")]
    public int UfId { get; set; }
}

/// <summary>
/// DTO para atualização de município
/// </summary>
public class AtualizarMunicipioDto
{
    [Required(ErrorMessage = "O nome do município é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    public bool Ativo { get; set; }

    [Required(ErrorMessage = "A versão de concorrência é obrigatória")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}