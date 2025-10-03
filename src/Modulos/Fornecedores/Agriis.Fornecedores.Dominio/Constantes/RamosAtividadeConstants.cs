namespace Agriis.Fornecedores.Dominio.Constantes;

/// <summary>
/// Constantes para os ramos de atividade disponíveis para fornecedores
/// </summary>
public static class RamosAtividadeConstants
{
    /// <summary>
    /// Lista de ramos de atividade pré-definidos para fornecedores
    /// </summary>
    public static readonly List<string> RamosDisponiveis = new()
    {
        "Sementes",
        "Fertilizantes",
        "Defensivos Agrícolas",
        "Máquinas e Equipamentos",
        "Irrigação",
        "Nutrição Animal",
        "Tecnologia Agrícola",
        "Consultoria Agronômica"
    };
    
    /// <summary>
    /// Verifica se um ramo de atividade é válido
    /// </summary>
    /// <param name="ramo">Ramo de atividade a ser validado</param>
    /// <returns>True se o ramo é válido, false caso contrário</returns>
    public static bool IsRamoValido(string ramo)
    {
        return !string.IsNullOrWhiteSpace(ramo) && RamosDisponiveis.Contains(ramo);
    }
    
    /// <summary>
    /// Valida uma lista de ramos de atividade
    /// </summary>
    /// <param name="ramos">Lista de ramos a serem validados</param>
    /// <returns>True se todos os ramos são válidos, false caso contrário</returns>
    public static bool ValidarRamos(IEnumerable<string> ramos)
    {
        return ramos?.All(IsRamoValido) ?? true;
    }
}