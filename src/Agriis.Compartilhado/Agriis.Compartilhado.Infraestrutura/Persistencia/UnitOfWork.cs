using Agriis.Compartilhado.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Agriis.Compartilhado.Infraestrutura.Persistencia;

/// <summary>
/// Implementação do padrão Unit of Work usando Entity Framework Core
/// </summary>
/// <typeparam name="TContext">Tipo do contexto do Entity Framework</typeparam>
public class UnitOfWork<TContext> : IUnitOfWork
    where TContext : DbContext
{
    private readonly TContext _context;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed = false;
    
    /// <summary>
    /// Construtor
    /// </summary>
    /// <param name="context">Contexto do Entity Framework</param>
    public UnitOfWork(TContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    /// <summary>
    /// Salva todas as alterações pendentes no contexto
    /// </summary>
    public async Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Erro de concorrência ao salvar alterações", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Erro ao salvar alterações no banco de dados", ex);
        }
    }
    
    /// <summary>
    /// Inicia uma nova transação
    /// </summary>
    public async Task IniciarTransacaoAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("Uma transação já está em andamento");
        }
        
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }
    
    /// <summary>
    /// Confirma a transação atual
    /// </summary>
    public async Task ConfirmarTransacaoAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("Nenhuma transação ativa para confirmar");
        }
        
        try
        {
            await SalvarAlteracoesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await ReverterTransacaoAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
    
    /// <summary>
    /// Reverte a transação atual
    /// </summary>
    public async Task ReverterTransacaoAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("Nenhuma transação ativa para reverter");
        }
        
        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
    
    /// <summary>
    /// Executa uma operação dentro de uma transação
    /// </summary>
    public async Task<T> ExecutarEmTransacaoAsync<T>(Func<Task<T>> operacao, CancellationToken cancellationToken = default)
    {
        if (operacao == null)
            throw new ArgumentNullException(nameof(operacao));
        
        var transacaoIniciada = _currentTransaction == null;
        
        if (transacaoIniciada)
        {
            await IniciarTransacaoAsync(cancellationToken);
        }
        
        try
        {
            var resultado = await operacao();
            
            if (transacaoIniciada)
            {
                await ConfirmarTransacaoAsync(cancellationToken);
            }
            
            return resultado;
        }
        catch
        {
            if (transacaoIniciada && _currentTransaction != null)
            {
                await ReverterTransacaoAsync(cancellationToken);
            }
            throw;
        }
    }
    
    /// <summary>
    /// Executa uma operação dentro de uma transação sem retorno
    /// </summary>
    public async Task ExecutarEmTransacaoAsync(Func<Task> operacao, CancellationToken cancellationToken = default)
    {
        await ExecutarEmTransacaoAsync(async () =>
        {
            await operacao();
            return true;
        }, cancellationToken);
    }
    
    /// <summary>
    /// Descarta todas as alterações pendentes
    /// </summary>
    public void DescartarAlteracoes()
    {
        foreach (var entry in _context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.State = EntityState.Unchanged;
                    break;
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Deleted:
                    entry.Reload();
                    break;
            }
        }
    }
    
    /// <summary>
    /// Verifica se há alterações pendentes
    /// </summary>
    public bool TemAlteracoesPendentes()
    {
        return _context.ChangeTracker.HasChanges();
    }
    
    /// <summary>
    /// Libera os recursos
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Libera os recursos de forma protegida
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _currentTransaction?.Dispose();
            _context?.Dispose();
            _disposed = true;
        }
    }
}