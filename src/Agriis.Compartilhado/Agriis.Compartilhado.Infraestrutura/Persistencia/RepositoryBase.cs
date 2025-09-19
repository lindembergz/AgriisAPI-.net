using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Agriis.Compartilhado.Infraestrutura.Persistencia;

/// <summary>
/// Implementação base para repositórios usando Entity Framework Core
/// </summary>
/// <typeparam name="T">Tipo da entidade</typeparam>
/// <typeparam name="TContext">Tipo do contexto do Entity Framework</typeparam>
public abstract class RepositoryBase<T, TContext> : IRepository<T>
    where T : EntidadeBase
    where TContext : DbContext
{
    protected readonly TContext Context;
    protected readonly DbSet<T> DbSet;
    
    /// <summary>
    /// Construtor
    /// </summary>
    /// <param name="context">Contexto do Entity Framework</param>
    protected RepositoryBase(TContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = Context.Set<T>();
    }
    
    /// <summary>
    /// Obtém uma entidade por seu ID
    /// </summary>
    public virtual async Task<T?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }
    
    /// <summary>
    /// Obtém todas as entidades
    /// </summary>
    public virtual async Task<IEnumerable<T>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }
    
    /// <summary>
    /// Obtém entidades que atendem a uma condição
    /// </summary>
    public virtual async Task<IEnumerable<T>> ObterPorCondicaoAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }
    
    /// <summary>
    /// Obtém uma única entidade que atende a uma condição
    /// </summary>
    public virtual async Task<T?> ObterPrimeiroAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }
    
    /// <summary>
    /// Adiciona uma nova entidade
    /// </summary>
    public virtual async Task<T> AdicionarAsync(T entidade, CancellationToken cancellationToken = default)
    {
        if (entidade == null)
            throw new ArgumentNullException(nameof(entidade));
            
        var entityEntry = await DbSet.AddAsync(entidade, cancellationToken);
        return entityEntry.Entity;
    }
    
    /// <summary>
    /// Adiciona múltiplas entidades
    /// </summary>
    public virtual async Task AdicionarVariasAsync(IEnumerable<T> entidades, CancellationToken cancellationToken = default)
    {
        if (entidades == null)
            throw new ArgumentNullException(nameof(entidades));
            
        await DbSet.AddRangeAsync(entidades, cancellationToken);
    }
    
    /// <summary>
    /// Atualiza uma entidade existente
    /// </summary>
    public virtual Task AtualizarAsync(T entidade, CancellationToken cancellationToken = default)
    {
        if (entidade == null)
            throw new ArgumentNullException(nameof(entidade));
            
        entidade.AtualizarDataModificacao();
        DbSet.Update(entidade);
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Remove uma entidade por ID
    /// </summary>
    public virtual async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var entidade = await ObterPorIdAsync(id, cancellationToken);
        if (entidade != null)
        {
            DbSet.Remove(entidade);
        }
    }
    
    /// <summary>
    /// Remove uma entidade
    /// </summary>
    public virtual Task RemoverAsync(T entidade, CancellationToken cancellationToken = default)
    {
        if (entidade == null)
            throw new ArgumentNullException(nameof(entidade));
            
        DbSet.Remove(entidade);
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Remove múltiplas entidades
    /// </summary>
    public virtual Task RemoverVariasAsync(IEnumerable<T> entidades, CancellationToken cancellationToken = default)
    {
        if (entidades == null)
            throw new ArgumentNullException(nameof(entidades));
            
        DbSet.RemoveRange(entidades);
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Verifica se existe uma entidade com o ID especificado
    /// </summary>
    public virtual async Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }
    
    /// <summary>
    /// Verifica se existe uma entidade que atende a uma condição
    /// </summary>
    public virtual async Task<bool> ExisteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }
    
    /// <summary>
    /// Conta o número de entidades
    /// </summary>
    public virtual async Task<int> ContarAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }
    
    /// <summary>
    /// Conta o número de entidades que atendem a uma condição
    /// </summary>
    public virtual async Task<int> ContarAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(predicate, cancellationToken);
    }
}