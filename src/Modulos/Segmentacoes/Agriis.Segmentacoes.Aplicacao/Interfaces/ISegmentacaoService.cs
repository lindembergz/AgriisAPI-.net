using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Segmentacoes.Aplicacao.DTOs;

namespace Agriis.Segmentacoes.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviços de segmentação
/// </summary>
public interface ISegmentacaoService
{
    /// <summary>
    /// Obtém todas as segmentações de um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Lista de segmentações</returns>
    Task<Result<IEnumerable<SegmentacaoDto>>> ObterPorFornecedorAsync(int fornecedorId);
    
    /// <summary>
    /// Obtém segmentação por ID
    /// </summary>
    /// <param name="id">ID da segmentação</param>
    /// <returns>Segmentação ou erro</returns>
    Task<Result<SegmentacaoDto>> ObterPorIdAsync(int id);
    
    /// <summary>
    /// Obtém segmentação completa com grupos e descontos
    /// </summary>
    /// <param name="id">ID da segmentação</param>
    /// <returns>Segmentação completa ou erro</returns>
    Task<Result<SegmentacaoDto>> ObterCompletaAsync(int id);
    
    /// <summary>
    /// Obtém a segmentação padrão de um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Segmentação padrão ou erro</returns>
    Task<Result<SegmentacaoDto>> ObterPadraoAsync(int fornecedorId);
    
    /// <summary>
    /// Cria uma nova segmentação
    /// </summary>
    /// <param name="dto">Dados da segmentação</param>
    /// <returns>Segmentação criada ou erro</returns>
    Task<Result<SegmentacaoDto>> CriarAsync(CriarSegmentacaoDto dto);
    
    /// <summary>
    /// Atualiza uma segmentação
    /// </summary>
    /// <param name="id">ID da segmentação</param>
    /// <param name="dto">Dados atualizados</param>
    /// <returns>Segmentação atualizada ou erro</returns>
    Task<Result<SegmentacaoDto>> AtualizarAsync(int id, AtualizarSegmentacaoDto dto);
    
    /// <summary>
    /// Remove uma segmentação
    /// </summary>
    /// <param name="id">ID da segmentação</param>
    /// <returns>Resultado da operação</returns>
    Task<Result> RemoverAsync(int id);
    
    /// <summary>
    /// Ativa uma segmentação
    /// </summary>
    /// <param name="id">ID da segmentação</param>
    /// <returns>Resultado da operação</returns>
    Task<Result> AtivarAsync(int id);
    
    /// <summary>
    /// Desativa uma segmentação
    /// </summary>
    /// <param name="id">ID da segmentação</param>
    /// <returns>Resultado da operação</returns>
    Task<Result> DesativarAsync(int id);
    
    /// <summary>
    /// Define uma segmentação como padrão
    /// </summary>
    /// <param name="id">ID da segmentação</param>
    /// <returns>Resultado da operação</returns>
    Task<Result> DefinirComoPadraoAsync(int id);
}