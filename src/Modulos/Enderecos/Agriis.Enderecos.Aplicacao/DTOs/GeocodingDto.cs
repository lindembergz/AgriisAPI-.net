using System.ComponentModel.DataAnnotations;

namespace Agriis.Enderecos.Aplicacao.DTOs;

/// <summary>
/// DTO para requisição de geocodificação reversa
/// </summary>
public class GeocodingReversoRequestDto
{
    /// <summary>
    /// Latitude
    /// </summary>
    [Required(ErrorMessage = "Latitude é obrigatória")]
    [Range(-90, 90, ErrorMessage = "Latitude deve estar entre -90 e 90")]
    public double Latitude { get; set; }
    
    /// <summary>
    /// Longitude
    /// </summary>
    [Required(ErrorMessage = "Longitude é obrigatória")]
    [Range(-180, 180, ErrorMessage = "Longitude deve estar entre -180 e 180")]
    public double Longitude { get; set; }
    
    /// <summary>
    /// Idioma preferido para o resultado (pt-BR, en, etc.)
    /// </summary>
    public string? Idioma { get; set; } = "pt-BR";
}

/// <summary>
/// DTO para requisição de geocodificação direta
/// </summary>
public class GeocodingDiretoRequestDto
{
    /// <summary>
    /// Endereço completo para busca
    /// </summary>
    [Required(ErrorMessage = "Endereço é obrigatório")]
    [StringLength(500, ErrorMessage = "Endereço deve ter no máximo 500 caracteres")]
    public string Endereco { get; set; } = string.Empty;
    
    /// <summary>
    /// Restringir busca ao Brasil
    /// </summary>
    public bool RestringirAoBrasil { get; set; } = true;
    
    /// <summary>
    /// Idioma preferido para o resultado (pt-BR, en, etc.)
    /// </summary>
    public string? Idioma { get; set; } = "pt-BR";
}

/// <summary>
/// DTO para validação de coordenadas
/// </summary>
public class ValidacaoCoordenadasDto
{
    /// <summary>
    /// Indica se as coordenadas são válidas
    /// </summary>
    public bool Validas { get; set; }
    
    /// <summary>
    /// Indica se as coordenadas estão no Brasil
    /// </summary>
    public bool EstaNoBrasil { get; set; }
    
    /// <summary>
    /// Mensagem explicativa
    /// </summary>
    public string? Mensagem { get; set; }
}