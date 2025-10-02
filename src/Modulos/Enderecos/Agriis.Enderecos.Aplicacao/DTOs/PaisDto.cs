namespace Agriis.Enderecos.Aplicacao.DTOs;

/// <summary>
/// DTO para representação de um país
/// </summary>
public class PaisDto
{
    /// <summary>
    /// ID do país
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Código do país (ISO 2-3 caracteres)
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nome do país
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Indica se o país está ativo
    /// </summary>
    public bool Ativo { get; set; }

    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime DataCriacao { get; set; }

    /// <summary>
    /// Data da última modificação
    /// </summary>
    public DateTime? DataModificacao { get; set; }
}