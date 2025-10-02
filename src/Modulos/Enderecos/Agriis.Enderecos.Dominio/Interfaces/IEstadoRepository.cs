using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Enderecos.Dominio.Entidades;

namespace Agriis.Enderecos.Dominio.Interfaces;

/// <summary>
/// Interface do repositório de Estados
/// </summary>
public interface IEstadoRepository : IRepository<Estado>
{
    /// <summary>
    /// Obtém um estado pela sigla (UF)
    /// </summary>
    /// <param name="uf">Sigla do estado</param>
    /// <returns>Estado encontrado ou null</returns>
    Task<Estado?> ObterPorUfAsync(string uf);
    
    /// <summary>
    /// Obtém um estado pelo código IBGE
    /// </summary>
    /// <param name="codigoIbge">Código IBGE do estado</param>
    /// <returns>Estado encontrado ou null</returns>
    Task<Estado?> ObterPorCodigoIbgeAsync(int codigoIbge);
    
    /// <summary>
    /// Obtém estados por região
    /// </summary>
    /// <param name="regiao">Nome da região</param>
    /// <returns>Lista de estados da região</returns>
    Task<IEnumerable<Estado>> ObterPorRegiaoAsync(string regiao);
    
    /// <summary>
    /// Obtém todos os estados com seus municípios
    /// </summary>
    /// <returns>Lista de estados com municípios</returns>
    Task<IEnumerable<Estado>> ObterTodosComMunicipiosAsync();
    
    /// <summary>
    /// Verifica se existe um estado com a UF especificada
    /// </summary>
    /// <param name="uf">Sigla do estado</param>
    /// <returns>True se existe</returns>
    Task<bool> ExistePorUfAsync(string uf);
    
    /// <summary>
    /// Verifica se existe um estado com o código IBGE especificado
    /// </summary>
    /// <param name="codigoIbge">Código IBGE</param>
    /// <returns>True se existe</returns>
    Task<bool> ExistePorCodigoIbgeAsync(int codigoIbge);
    
    /// <summary>
    /// Obtém estados por país
    /// </summary>
    /// <param name="paisId">ID do país</param>
    /// <returns>Lista de estados do país</returns>
    Task<IEnumerable<Estado>> ObterPorPaisAsync(int paisId);
    
    /// <summary>
    /// Verifica se existe um estado com o código especificado, excluindo um ID específico
    /// </summary>
    /// <param name="codigo">Código/UF do estado</param>
    /// <param name="excludeId">ID a ser excluído da verificação</param>
    /// <returns>True se existe outro estado com o mesmo código</returns>
    Task<bool> ExisteCodigoAsync(string codigo, int? excludeId = null);
}