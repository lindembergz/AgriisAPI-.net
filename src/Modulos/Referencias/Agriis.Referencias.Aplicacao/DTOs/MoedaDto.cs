using System.ComponentModel.DataAnnotations;

namespace Agriis.Referencias.Aplicacao.DTOs;

/// <summary>
/// DTO para leitura de moeda
/// </summary>
public class MoedaDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Simbolo { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// DTO para criação de moeda
/// </summary>
public class CriarMoedaDto
{
    [Required(ErrorMessage = "O código da moeda é obrigatório")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "O código deve ter exatamente 3 caracteres")]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "O código deve conter apenas letras maiúsculas")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome da moeda é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O símbolo da moeda é obrigatório")]
    [StringLength(5, MinimumLength = 1, ErrorMessage = "O símbolo deve ter entre 1 e 5 caracteres")]
    public string Simbolo { get; set; } = string.Empty;
}

/// <summary>
/// DTO para atualização de moeda
/// </summary>
public class AtualizarMoedaDto
{
    [Required(ErrorMessage = "O nome da moeda é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O símbolo da moeda é obrigatório")]
    [StringLength(5, MinimumLength = 1, ErrorMessage = "O símbolo deve ter entre 1 e 5 caracteres")]
    public string Simbolo { get; set; } = string.Empty;

    public bool Ativo { get; set; }

    [Required(ErrorMessage = "A versão de concorrência é obrigatória")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}