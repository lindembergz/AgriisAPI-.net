using System.ComponentModel.DataAnnotations;
using Agriis.Referencias.Dominio.Enums;

namespace Agriis.Referencias.Aplicacao.DTOs;

/// <summary>
/// DTO para leitura de atividade agropecuária
/// </summary>
public class AtividadeAgropecuariaDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public TipoAtividadeAgropecuaria Tipo { get; set; }
    public string TipoDescricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// DTO para criação de atividade agropecuária
/// </summary>
public class CriarAtividadeAgropecuariaDto
{
    [Required(ErrorMessage = "O código da atividade é obrigatório")]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "O código deve ter entre 2 e 10 caracteres")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição da atividade é obrigatória")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "A descrição deve ter entre 5 e 200 caracteres")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tipo da atividade é obrigatório")]
    [EnumDataType(typeof(TipoAtividadeAgropecuaria), ErrorMessage = "Tipo de atividade inválido")]
    public TipoAtividadeAgropecuaria Tipo { get; set; }
}

/// <summary>
/// DTO para atualização de atividade agropecuária
/// </summary>
public class AtualizarAtividadeAgropecuariaDto
{
    [Required(ErrorMessage = "A descrição da atividade é obrigatória")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "A descrição deve ter entre 5 e 200 caracteres")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tipo da atividade é obrigatório")]
    [EnumDataType(typeof(TipoAtividadeAgropecuaria), ErrorMessage = "Tipo de atividade inválido")]
    public TipoAtividadeAgropecuaria Tipo { get; set; }

    public bool Ativo { get; set; }

    [Required(ErrorMessage = "A versão de concorrência é obrigatória")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}