namespace Agriis.Produtores.Dominio.Enums;

/// <summary>
/// Status do produtor no sistema
/// </summary>
public enum StatusProdutor
{
    /// <summary>
    /// Aguardando validação automática via SERPRO
    /// </summary>
    PendenteValidacaoAutomatica = 0,
    
    /// <summary>
    /// Aguardando validação manual por administrador
    /// </summary>
    PendenteValidacaoManual = 1,
    
    /// <summary>
    /// Pendente de validação de CNPJ
    /// </summary>
    PendenteCnpj = 2,
    
    /// <summary>
    /// Autorizado automaticamente pelo sistema
    /// </summary>
    AutorizadoAutomaticamente = 3,
    
    /// <summary>
    /// Autorizado manualmente por administrador
    /// </summary>
    AutorizadoManualmente = 4,
    
    /// <summary>
    /// Negado pelo sistema ou administrador
    /// </summary>
    Negado = 5
}