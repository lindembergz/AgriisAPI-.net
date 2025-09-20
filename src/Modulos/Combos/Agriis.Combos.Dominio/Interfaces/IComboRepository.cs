using Agriis.Combos.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.Interfaces;

namespace Agriis.Combos.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de combos
/// </summary>
public interface IComboRepository : IRepository<Combo>
{
    /// <summary>
    /// Obtém combos ativos por fornecedor
    /// </summary>
    Task<IEnumerable<Combo>> ObterPorFornecedorAsync(int fornecedorId);

    /// <summary>
    /// Obtém combos vigentes (ativos e dentro do período)
    /// </summary>
    Task<IEnumerable<Combo>> ObterCombosVigentesAsync();

    /// <summary>
    /// Obtém combos por safra
    /// </summary>
    Task<IEnumerable<Combo>> ObterPorSafraAsync(int safraId);

    /// <summary>
    /// Obtém combos válidos para um produtor (considerando hectare e localização)
    /// </summary>
    Task<IEnumerable<Combo>> ObterCombosValidosParaProdutorAsync(
        int produtorId, 
        decimal hectareProdutor, 
        int municipioId);

    /// <summary>
    /// Obtém combo com todos os relacionamentos
    /// </summary>
    Task<Combo?> ObterCompletoAsync(int id);

    /// <summary>
    /// Verifica se existe combo ativo para o fornecedor na safra
    /// </summary>
    Task<bool> ExisteComboAtivoAsync(int fornecedorId, int safraId, string nome);

    /// <summary>
    /// Obtém combos que expiram em determinado período
    /// </summary>
    Task<IEnumerable<Combo>> ObterCombosExpirandoAsync(DateTime dataLimite);
}