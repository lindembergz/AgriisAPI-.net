using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Enderecos.Dominio.Entidades;

namespace Agriis.Enderecos.Dominio.Interfaces;

/// <summary>
/// Interface do repositório de Países
/// </summary>
public interface IPaisRepository : IRepository<Pais>
{
    /// <summary>
    /// Obtém um país por código
    /// </summary>
    /// <param name="codigo">Código do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>País encontrado</returns>
    Task<Pais?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe um país com o código especificado
    /// </summary>
    /// <param name="codigo">Código do país</param>
    /// <param name="idExcluir">ID do país a ser excluído da verificação (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe um país com o nome especificado
    /// </summary>
    /// <param name="nome">Nome do país</param>
    /// <param name="idExcluir">ID do país a ser excluído da verificação (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém países ativos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de países ativos</returns>
    Task<IEnumerable<Pais>> ObterAtivosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca países por nome (busca parcial)
    /// </summary>
    /// <param name="nome">Nome ou parte do nome</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de países encontrados</returns>
    Task<IEnumerable<Pais>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os países com seus estados incluídos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de países com estados</returns>
    Task<IEnumerable<Pais>> ObterTodosComEstadosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém países ativos com seus estados incluídos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de países ativos com estados</returns>
    Task<IEnumerable<Pais>> ObterAtivosComEstadosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os países com paginação
    /// </summary>
    /// <param name="pagina">Número da página</param>
    /// <param name="tamanhoPagina">Tamanho da página</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de países</returns>
    Task<IEnumerable<Pais>> ObterTodosAsync(int pagina = 1, int tamanhoPagina = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe um país com o código especificado
    /// </summary>
    /// <param name="codigo">Código do país</param>
    /// <param name="idExcluir">ID do país a ser excluído da verificação (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExistePorCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe um país com o nome especificado
    /// </summary>
    /// <param name="nome">Nome do país</param>
    /// <param name="idExcluir">ID do país a ser excluído da verificação (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExistePorNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se o país possui estados associados
    /// </summary>
    /// <param name="paisId">ID do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se possui estados</returns>
    Task<bool> PossuiEstadosAsync(int paisId, CancellationToken cancellationToken = default);
}