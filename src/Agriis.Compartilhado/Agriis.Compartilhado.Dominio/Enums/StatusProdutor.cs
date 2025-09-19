namespace Agriis.Compartilhado.Dominio.Enums;

/// <summary>
/// Status do produtor no sistema
/// </summary>
public enum StatusProdutor
{
    /// <summary>
    /// Aguardando validação automática via SERPRO
    /// </summary>
    PendenteValidacaoAutomatica = 1,
    
    /// <summary>
    /// Aguardando validação manual por administrador
    /// </summary>
    PendenteValidacaoManual = 2,
    
    /// <summary>
    /// Aguardando validação de CNPJ
    /// </summary>
    PendenteCnpj = 3,
    
    /// <summary>
    /// Autorizado automaticamente pelo sistema
    /// </summary>
    AutorizadoAutomaticamente = 4,
    
    /// <summary>
    /// Autorizado manualmente por administrador
    /// </summary>
    AutorizadoManualmente = 5,
    
    /// <summary>
    /// Acesso negado
    /// </summary>
    Negado = 6
}