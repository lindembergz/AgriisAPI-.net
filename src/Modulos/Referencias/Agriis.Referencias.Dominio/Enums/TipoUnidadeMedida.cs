namespace Agriis.Referencias.Dominio.Enums;

/// <summary>
/// Tipos de unidades de medida disponíveis no sistema
/// </summary>
public enum TipoUnidadeMedida
{
    /// <summary>
    /// Unidades de peso (kg, g, t, etc.)
    /// </summary>
    Peso = 1,
    
    /// <summary>
    /// Unidades de volume (L, mL, m³, etc.)
    /// </summary>
    Volume = 2,
    
    /// <summary>
    /// Unidades de área (m², ha, etc.)
    /// </summary>
    Area = 3,
    
    /// <summary>
    /// Unidades simples (unidade, peça, etc.)
    /// </summary>
    Unidade = 4
}