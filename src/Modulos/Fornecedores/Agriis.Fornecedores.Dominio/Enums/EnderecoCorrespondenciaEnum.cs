namespace Agriis.Fornecedores.Dominio.Enums;

/// <summary>
/// Enum que define as opções de endereço de correspondência para fornecedores
/// </summary>
public enum EnderecoCorrespondenciaEnum
{
    /// <summary>
    /// Usar o mesmo endereço do faturamento para correspondência
    /// </summary>
    MesmoFaturamento = 0,
    
    /// <summary>
    /// Usar um endereço diferente do faturamento para correspondência
    /// </summary>
    DiferenteFaturamento = 1
}