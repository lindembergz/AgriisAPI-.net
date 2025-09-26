using System.ComponentModel.DataAnnotations;

namespace Agriis.Referencias.Aplicacao.DTOs;

/// <summary>
/// DTO para leitura de UF
/// </summary>
public class UfDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public int PaisId { get; set; }
    public string PaisNome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Quantidade de municípios associados a esta UF
    /// </summary>
    public int QuantidadeMunicipios { get; set; }
}

/// <summary>
/// DTO para criação de UF
/// </summary>
public class CriarUfDto
{
    [Required(ErrorMessage = "O código da UF é obrigatório")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "O código deve ter exatamente 2 caracteres")]
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "O código deve conter apenas letras maiúsculas")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome da UF é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O país é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "Deve ser selecionado um país válido")]
    public int PaisId { get; set; }
}

/// <summary>
/// DTO para atualização de UF
/// </summary>
public class AtualizarUfDto
{
    [Required(ErrorMessage = "O nome da UF é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    public bool Ativo { get; set; }

    [Required(ErrorMessage = "A versão de concorrência é obrigatória")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}