using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Produtores.Aplicacao.DTOs;

namespace Agriis.Produtores.Aplicacao.Interfaces;

/// <summary>
/// Interface do serviço de aplicação de produtores
/// </summary>
public interface IProdutorService
{
    /// <summary>
    /// Obtém um produtor por ID
    /// </summary>
    /// <param name="id">ID do produtor</param>
    /// <returns>Produtor encontrado ou null</returns>
    Task<ProdutorDto?> ObterPorIdAsync(int id);
    
    /// <summary>
    /// Obtém um produtor por CPF
    /// </summary>
    /// <param name="cpf">CPF do produtor</param>
    /// <returns>Produtor encontrado ou null</returns>
    Task<ProdutorDto?> ObterPorCpfAsync(string cpf);
    
    /// <summary>
    /// Obtém um produtor por CNPJ
    /// </summary>
    /// <param name="cnpj">CNPJ do produtor</param>
    /// <returns>Produtor encontrado ou null</returns>
    Task<ProdutorDto?> ObterPorCnpjAsync(string cnpj);
    
    /// <summary>
    /// Obtém produtores paginados com filtros
    /// </summary>
    /// <param name="filtros">Filtros de busca</param>
    /// <returns>Resultado paginado</returns>
    Task<PagedResult<ProdutorDto>> ObterPaginadoAsync(FiltrosProdutorDto filtros);
    
    /// <summary>
    /// Cria um novo produtor
    /// </summary>
    /// <param name="dto">Dados do produtor</param>
    /// <returns>Produtor criado</returns>
    Task<Result<ProdutorDto>> CriarAsync(CriarProdutorDto dto);
    
    /// <summary>
    /// Cria um novo produtor com estrutura completa (usuário master e relacionamentos)
    /// </summary>
    /// <param name="request">Dados completos do produtor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Produtor criado</returns>
    Task<Result<ProdutorDto>> CriarCompletoAsync(CriarProdutorCompletoRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza um produtor existente
    /// </summary>
    /// <param name="id">ID do produtor</param>
    /// <param name="dto">Dados atualizados</param>
    /// <returns>Produtor atualizado</returns>
    Task<Result<ProdutorDto>> AtualizarAsync(int id, AtualizarProdutorDto dto);
    
    /// <summary>
    /// Remove um produtor
    /// </summary>
    /// <param name="id">ID do produtor</param>
    /// <returns>Resultado da operação</returns>
    Task<Result<bool>> RemoverAsync(int id);
    
    /// <summary>
    /// Valida um produtor automaticamente via SERPRO
    /// </summary>
    /// <param name="id">ID do produtor</param>
    /// <returns>Resultado da validação</returns>
    Task<Result<ProdutorDto>> ValidarAutomaticamenteAsync(int id);
    
    /// <summary>
    /// Autoriza um produtor manualmente
    /// </summary>
    /// <param name="id">ID do produtor</param>
    /// <param name="usuarioAutorizacaoId">ID do usuário que está autorizando</param>
    /// <returns>Resultado da operação</returns>
    Task<Result<ProdutorDto>> AutorizarManualmenteAsync(int id, int usuarioAutorizacaoId);
    
    /// <summary>
    /// Nega um produtor
    /// </summary>
    /// <param name="id">ID do produtor</param>
    /// <param name="usuarioAutorizacaoId">ID do usuário que está negando</param>
    /// <returns>Resultado da operação</returns>
    Task<Result<ProdutorDto>> NegarAsync(int id, int usuarioAutorizacaoId);
    
    /// <summary>
    /// Obtém estatísticas dos produtores
    /// </summary>
    /// <returns>Estatísticas</returns>
    Task<ProdutorEstatisticasDto> ObterEstatisticasAsync();
    
    /// <summary>
    /// Obtém produtores por fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Lista de produtores</returns>
    Task<IEnumerable<ProdutorDto>> ObterPorFornecedorAsync(int fornecedorId);
}