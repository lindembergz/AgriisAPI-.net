namespace Agriis.Compartilhado.Dominio.Interfaces;

/// <summary>
/// Interface para Unit of Work pattern
/// Gerencia transações e coordena o trabalho de múltiplos repositórios
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Salva todas as alterações pendentes no contexto
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Número de entidades afetadas</returns>
    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Inicia uma nova transação
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task IniciarTransacaoAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Confirma a transação atual
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task ConfirmarTransacaoAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reverte a transação atual
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task ReverterTransacaoAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executa uma operação dentro de uma transação
    /// </summary>
    /// <param name="operacao">Operação a ser executada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    Task<T> ExecutarEmTransacaoAsync<T>(Func<Task<T>> operacao, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executa uma operação dentro de uma transação sem retorno
    /// </summary>
    /// <param name="operacao">Operação a ser executada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task ExecutarEmTransacaoAsync(Func<Task> operacao, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Descarta todas as alterações pendentes
    /// </summary>
    void DescartarAlteracoes();
    
    /// <summary>
    /// Verifica se há alterações pendentes
    /// </summary>
    /// <returns>True se há alterações pendentes</returns>
    bool TemAlteracoesPendentes();
}