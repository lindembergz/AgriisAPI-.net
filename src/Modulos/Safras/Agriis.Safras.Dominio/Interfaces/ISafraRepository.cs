using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Safras.Dominio.Entidades;

namespace Agriis.Safras.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de Safras
/// </summary>
public interface ISafraRepository : IRepository<Safra>
{
    /// <summary>
    /// Obtém a safra atual (ativa no momento)
    /// </summary>
    /// <returns>Safra atual ou null se não houver</returns>
    Task<Safra?> ObterSafraAtualAsync();
    
    /// <summary>
    /// Obtém safras por ano de colheita
    /// </summary>
    /// <param name="anoColheita">Ano de colheita</param>
    /// <returns>Lista de safras do ano especificado</returns>
    Task<IEnumerable<Safra>> ObterPorAnoColheitaAsync(int anoColheita);
    
    /// <summary>
    /// Obtém safras ordenadas por data de plantio inicial
    /// </summary>
    /// <returns>Lista de safras ordenadas</returns>
    Task<IEnumerable<Safra>> ObterTodasOrdenadasAsync();
    
    /// <summary>
    /// Verifica se existe uma safra com o mesmo período de plantio
    /// </summary>
    /// <param name="plantioInicial">Data inicial do plantio</param>
    /// <param name="plantioFinal">Data final do plantio</param>
    /// <param name="plantioNome">Nome do plantio</param>
    /// <param name="idExcluir">ID da safra a excluir da verificação (para atualizações)</param>
    /// <returns>True se existe conflito</returns>
    Task<bool> ExisteConflitoPeriodoAsync(DateTime plantioInicial, DateTime plantioFinal, string plantioNome, int? idExcluir = null);
    
    /// <summary>
    /// Obtém safras ativas (dentro do período de plantio)
    /// </summary>
    /// <returns>Lista de safras ativas</returns>
    Task<IEnumerable<Safra>> ObterSafrasAtivasAsync();
}