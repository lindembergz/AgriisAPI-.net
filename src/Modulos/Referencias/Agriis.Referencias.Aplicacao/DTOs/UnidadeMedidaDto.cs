using System.ComponentModel.DataAnnotations;
using Agriis.Referencias.Dominio.Enums;

namespace Agriis.Referencias.Aplicacao.DTOs;

/// <summary>
/// DTO para leitura de unidade de medida
/// </summary>
public class UnidadeMedidaDto
{
    public int Id { get; set; }
    public string Simbolo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public TipoUnidadeMedida Tipo { get; set; }
    public string TipoDescricao { get; set; } = string.Empty;
    public decimal? FatorConversao { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// DTO para criação de unidade de medida
/// </summary>
public class CriarUnidadeMedidaDto
{
    [Required(ErrorMessage = "O símbolo da unidade de medida é obrigatório")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "O símbolo deve ter entre 1 e 10 caracteres")]
    public string Simbolo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome da unidade de medida é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tipo da unidade de medida é obrigatório")]
    [EnumDataType(typeof(TipoUnidadeMedida), ErrorMessage = "Tipo de unidade de medida inválido")]
    public TipoUnidadeMedida Tipo { get; set; }

    [Range(0.000001, 999999.999999, ErrorMessage = "O fator de conversão deve ser um valor positivo")]
    public decimal? FatorConversao { get; set; }
}

/// <summary>
/// DTO para atualização de unidade de medida
/// </summary>
public class AtualizarUnidadeMedidaDto
{
    [Required(ErrorMessage = "O nome da unidade de medida é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Range(0.000001, 999999.999999, ErrorMessage = "O fator de conversão deve ser um valor positivo")]
    public decimal? FatorConversao { get; set; }

    public bool Ativo { get; set; }

    [Required(ErrorMessage = "A versão de concorrência é obrigatória")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}