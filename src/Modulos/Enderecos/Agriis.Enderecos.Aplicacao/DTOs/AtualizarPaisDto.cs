using System.ComponentModel.DataAnnotations;

namespace Agriis.Enderecos.Aplicacao.DTOs;

/// <summary>
/// DTO para atualização de um país
/// </summary>
public class AtualizarPaisDto
{
    /// <summary>
    /// Nome do país
    /// </summary>
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome não pode ter mais de 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Código do país (ISO 2-3 caracteres)
    /// </summary>
    [Required(ErrorMessage = "Código é obrigatório")]
    [StringLength(3, MinimumLength = 2, ErrorMessage = "Código deve ter entre 2 e 3 caracteres")]
    public string Codigo { get; set; } = string.Empty;
}