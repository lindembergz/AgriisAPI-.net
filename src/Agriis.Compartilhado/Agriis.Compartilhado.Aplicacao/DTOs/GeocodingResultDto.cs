
namespace Agriis.Compartilhado.Aplicacao.DTOs;

/// <summary>
/// DTO para resultado de geocodificação
/// </summary>
public class GeocodingResultDto
{
    /// <summary>
    /// Latitude
    /// </summary>
    public double Latitude { get; set; }
    
    /// <summary>
    /// Longitude
    /// </summary>
    public double Longitude { get; set; }
    
    /// <summary>
    /// Endereço formatado completo
    /// </summary>
    public string EnderecoFormatado { get; set; } = string.Empty;
    
    /// <summary>
    /// Logradouro (rua, avenida, etc.)
    /// </summary>
    public string? Logradouro { get; set; }
    
    /// <summary>
    /// Número do endereço
    /// </summary>
    public string? Numero { get; set; }
    
    /// <summary>
    /// Bairro
    /// </summary>
    public string? Bairro { get; set; }
    
    /// <summary>
    /// CEP
    /// </summary>
    public string? Cep { get; set; }
    
    /// <summary>
    /// Nome do município
    /// </summary>
    public string? Municipio { get; set; }
    
    /// <summary>
    /// UF do estado
    /// </summary>
    public string? Uf { get; set; }
    
    /// <summary>
    /// Nome do estado
    /// </summary>
    public string? Estado { get; set; }
    
    /// <summary>
    /// País
    /// </summary>
    public string? Pais { get; set; }
    
    /// <summary>
    /// Código do país
    /// </summary>
    public string? CodigoPais { get; set; }
    
    /// <summary>
    /// Nível de precisão do resultado
    /// </summary>
    public string? TipoPrecisao { get; set; }
    
    /// <summary>
    /// Indica se o resultado está no Brasil
    /// </summary>
    public bool EstaNoBrasil { get; set; }
}
