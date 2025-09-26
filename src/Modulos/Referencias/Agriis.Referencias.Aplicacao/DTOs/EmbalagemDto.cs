using System.ComponentModel.DataAnnotations;

namespace Agriis.Referencias.Aplicacao.DTOs;

/// <summary>
/// DTO para leitura de embalagem
/// </summary>
public class EmbalagemDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int UnidadeMedidaId { get; set; }
    public string UnidadeMedidaNome { get; set; } = string.Empty;
    public string UnidadeMedidaSimbolo { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// DTO para criação de embalagem
/// </summary>
public class CriarEmbalagemDto
{
    [Required(ErrorMessage = "O nome da embalagem é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "A unidade de medida é obrigatória")]
    [Range(1, int.MaxValue, ErrorMessage = "Deve ser selecionada uma unidade de medida válida")]
    public int UnidadeMedidaId { get; set; }
}

/// <summary>
/// DTO para atualização de embalagem
/// </summary>
public class AtualizarEmbalagemDto
{
    [Required(ErrorMessage = "O nome da embalagem é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "A unidade de medida é obrigatória")]
    [Range(1, int.MaxValue, ErrorMessage = "Deve ser selecionada uma unidade de medida válida")]
    public int UnidadeMedidaId { get; set; }

    public bool Ativo { get; set; }

    [Required(ErrorMessage = "A versão de concorrência é obrigatória")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}