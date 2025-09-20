using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Pagamentos.Aplicacao.DTOs;

namespace Agriis.Pagamentos.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviços de associação cultura-fornecedor-forma de pagamento
/// </summary>
public interface ICulturaFormaPagamentoService
{
    /// <summary>
    /// Obtém associações por fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Lista de associações do fornecedor</returns>
    Task<Result<IEnumerable<CulturaFormaPagamentoDto>>> ObterPorFornecedorAsync(int fornecedorId);
    
    /// <summary>
    /// Obtém formas de pagamento disponíveis para fornecedor e cultura
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="culturaId">ID da cultura</param>
    /// <returns>Lista de formas de pagamento disponíveis</returns>
    Task<Result<IEnumerable<FormaPagamentoDto>>> ObterFormasPagamentoPorFornecedorCulturaAsync(
        int fornecedorId, 
        int culturaId);
    
    /// <summary>
    /// Cria uma nova associação cultura-fornecedor-forma de pagamento
    /// </summary>
    /// <param name="dto">Dados da associação</param>
    /// <returns>Associação criada</returns>
    Task<Result<CulturaFormaPagamentoDto>> CriarAsync(CriarCulturaFormaPagamentoDto dto);
    
    /// <summary>
    /// Remove uma associação cultura-fornecedor-forma de pagamento
    /// </summary>
    /// <param name="id">ID da associação</param>
    /// <returns>Resultado da operação</returns>
    Task<Result> RemoverAsync(int id);
    
    /// <summary>
    /// Verifica se existe associação ativa
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="culturaId">ID da cultura</param>
    /// <param name="formaPagamentoId">ID da forma de pagamento</param>
    /// <returns>True se existe associação ativa</returns>
    Task<Result<bool>> ExisteAssociacaoAtivaAsync(int fornecedorId, int culturaId, int formaPagamentoId);
}