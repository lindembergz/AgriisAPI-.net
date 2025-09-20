using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Safras.Aplicacao.DTOs;

namespace Agriis.Safras.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviços de Safra
/// </summary>
public interface ISafraService
{
    /// <summary>
    /// Obtém uma safra por ID
    /// </summary>
    /// <param name="id">ID da safra</param>
    /// <returns>Dados da safra</returns>
    Task<Result<SafraDto>> ObterPorIdAsync(int id);
    
    /// <summary>
    /// Obtém todas as safras ordenadas por data de plantio
    /// </summary>
    /// <returns>Lista de safras</returns>
    Task<Result<IEnumerable<SafraDto>>> ObterTodasAsync();
    
    /// <summary>
    /// Obtém a safra atual (ativa)
    /// </summary>
    /// <returns>Safra atual ou null se não houver</returns>
    Task<Result<SafraAtualDto?>> ObterSafraAtualAsync();
    
    /// <summary>
    /// Obtém safras por ano de colheita
    /// </summary>
    /// <param name="anoColheita">Ano de colheita</param>
    /// <returns>Lista de safras do ano especificado</returns>
    Task<Result<IEnumerable<SafraDto>>> ObterPorAnoColheitaAsync(int anoColheita);
    
    /// <summary>
    /// Cria uma nova safra
    /// </summary>
    /// <param name="dto">Dados para criação</param>
    /// <returns>Safra criada</returns>
    Task<Result<SafraDto>> CriarAsync(CriarSafraDto dto);
    
    /// <summary>
    /// Atualiza uma safra existente
    /// </summary>
    /// <param name="id">ID da safra</param>
    /// <param name="dto">Dados para atualização</param>
    /// <returns>Safra atualizada</returns>
    Task<Result<SafraDto>> AtualizarAsync(int id, AtualizarSafraDto dto);
    
    /// <summary>
    /// Remove uma safra
    /// </summary>
    /// <param name="id">ID da safra</param>
    /// <returns>Resultado da operação</returns>
    Task<Result> RemoverAsync(int id);
}