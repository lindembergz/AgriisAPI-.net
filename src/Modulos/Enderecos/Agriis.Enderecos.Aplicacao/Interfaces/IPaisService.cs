using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Enderecos.Aplicacao.DTOs;

namespace Agriis.Enderecos.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviços de países
/// </summary>
public interface IPaisService
{
    /// <summary>
    /// Obtém um país por ID
    /// </summary>
    /// <param name="id">ID do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>País encontrado</returns>
    Task<PaisDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um país por código
    /// </summary>
    /// <param name="codigo">Código do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>País encontrado</returns>
    Task<PaisDto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os países ativos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de países ativos</returns>
    Task<IEnumerable<PaisDto>> ObterAtivosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca países por nome
    /// </summary>
    /// <param name="nome">Nome ou parte do nome</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de países encontrados</returns>
    Task<IEnumerable<PaisDto>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os países com seus estados incluídos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de países com estados</returns>
    Task<IEnumerable<PaisDto>> ObterTodosComEstadosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém países ativos com seus estados incluídos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de países ativos com estados</returns>
    Task<IEnumerable<PaisDto>> ObterAtivosComEstadosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os países com paginação
    /// </summary>
    /// <param name="pagina">Número da página</param>
    /// <param name="tamanhoPagina">Tamanho da página</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de países</returns>
    Task<IEnumerable<PaisComContadorDto>> ObterTodosAsync(int pagina = 1, int tamanhoPagina = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria um novo país
    /// </summary>
    /// <param name="criarPaisDto">Dados do país a ser criado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação com o país criado</returns>
    Task<Result<PaisDto>> CriarAsync(CriarPaisDto criarPaisDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um país existente
    /// </summary>
    /// <param name="id">ID do país</param>
    /// <param name="atualizarPaisDto">Dados atualizados do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação com o país atualizado</returns>
    Task<Result<PaisDto>> AtualizarAsync(int id, AtualizarPaisDto atualizarPaisDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui um país (soft delete)
    /// </summary>
    /// <param name="id">ID do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    Task<Result> ExcluirAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ativa um país
    /// </summary>
    /// <param name="id">ID do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    Task<Result> AtivarAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Desativa um país
    /// </summary>
    /// <param name="id">ID do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    Task<Result> DesativarAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um país existe por código
    /// </summary>
    /// <param name="codigo">Código do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe, false caso contrário</returns>
    Task<bool> ExistePorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um país existe por nome
    /// </summary>
    /// <param name="nome">Nome do país</param>
    /// <param name="idExcluir">ID do país a excluir da verificação (para atualizações)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe, false caso contrário</returns>
    Task<bool> ExistePorNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);
}