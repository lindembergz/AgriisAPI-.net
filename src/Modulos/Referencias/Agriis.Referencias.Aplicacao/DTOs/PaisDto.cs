using System.ComponentModel.DataAnnotations;

namespace Agriis.Referencias.Aplicacao.DTOs;

/// <summary>
/// DTO para leitura de país
/// </summary>
public class PaisDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Quantidade de UFs associadas a este país
    /// </summary>
    public int QuantidadeUfs { get; set; }
}

/// <summary>
/// DTO para criação de país
/// </summary>
public class CriarPaisDto
{
    [Required(ErrorMessage = "O código do país é obrigatório")]
    [StringLength(3, MinimumLength = 2, ErrorMessage = "O código deve ter entre 2 e 3 caracteres")]
    [RegularExpression(@"^[A-Z]{2,3}$", ErrorMessage = "O código deve conter apenas letras maiúsculas")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome do país é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;
}

/// <summary>
/// DTO para atualização de país
/// </summary>
public class AtualizarPaisDto
{
    [Required(ErrorMessage = "O nome do país é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    public bool Ativo { get; set; }

    [Required(ErrorMessage = "A versão de concorrência é obrigatória")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}