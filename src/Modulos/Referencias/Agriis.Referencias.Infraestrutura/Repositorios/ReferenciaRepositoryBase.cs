using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Referencias.Infraestrutura.Repositorios;

/// <summary>
/// Implementação base para repositórios de entidades de referência
/// </summary>
/// <typeparam name="T">Tipo da entidade de referência</typeparam>
/// <typeparam name="TContext">Tipo do contexto do Entity Framework</typeparam>
public abstract class ReferenciaRepositoryBase<T, TContext> : RepositoryBase<T, TContext>, IReferenciaRepository<T> 
    where T : EntidadeBase
    where TContext : DbContext
{
    protected ReferenciaRepositoryBase(TContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém apenas as entidades ativas
    /// </summary>
    public virtual async Task<IEnumerable<T>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<T>()
            .Where(GetAtivoExpression())
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se existe uma entidade com o código especificado
    /// </summary>
    public virtual async Task<bool> ExisteCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<T>().Where(GetCodigoExpression(codigo));
        
        if (idExcluir.HasValue)
            query = query.Where(e => e.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém uma entidade por código
    /// </summary>
    public virtual async Task<T?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await Context.Set<T>()
            .Where(GetCodigoExpression(codigo))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se existe uma entidade com o nome especificado
    /// </summary>
    public virtual async Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<T>().Where(GetNomeExpression(nome));
        
        if (idExcluir.HasValue)
            query = query.Where(e => e.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se uma entidade pode ser removida (implementação padrão - sempre pode)
    /// </summary>
    public virtual async Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        // Implementação padrão - sempre pode remover
        // Classes filhas devem sobrescrever se houver dependências
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Ativa uma entidade
    /// </summary>
    public virtual async Task AtivarAsync(int id, CancellationToken cancellationToken = default)
    {
        var entidade = await ObterPorIdAsync(id, cancellationToken);
        if (entidade == null)
            throw new ArgumentException($"Entidade com ID {id} não encontrada");

        SetAtivo(entidade, true);
        await AtualizarAsync(entidade, cancellationToken);
    }

    /// <summary>
    /// Desativa uma entidade
    /// </summary>
    public virtual async Task DesativarAsync(int id, CancellationToken cancellationToken = default)
    {
        var entidade = await ObterPorIdAsync(id, cancellationToken);
        if (entidade == null)
            throw new ArgumentException($"Entidade com ID {id} não encontrada");

        SetAtivo(entidade, false);
        await AtualizarAsync(entidade, cancellationToken);
    }

    /// <summary>
    /// Expressão para filtrar entidades ativas
    /// Classes filhas devem implementar
    /// </summary>
    protected abstract System.Linq.Expressions.Expression<Func<T, bool>> GetAtivoExpression();

    /// <summary>
    /// Expressão para filtrar por código
    /// Classes filhas devem implementar se suportarem código
    /// </summary>
    protected virtual System.Linq.Expressions.Expression<Func<T, bool>> GetCodigoExpression(string codigo)
    {
        throw new NotImplementedException("Entidade não suporta código");
    }

    /// <summary>
    /// Expressão para filtrar por nome
    /// Classes filhas devem implementar se suportarem nome
    /// </summary>
    protected virtual System.Linq.Expressions.Expression<Func<T, bool>> GetNomeExpression(string nome)
    {
        throw new NotImplementedException("Entidade não suporta nome");
    }

    /// <summary>
    /// Define o status ativo da entidade
    /// Classes filhas devem implementar
    /// </summary>
    protected abstract void SetAtivo(T entidade, bool ativo);

    /// <summary>
    /// Atualiza uma entidade com controle de concorrência otimista
    /// </summary>
    public override async Task AtualizarAsync(T entidade, CancellationToken cancellationToken = default)
    {
        if (entidade == null)
            throw new ArgumentNullException(nameof(entidade));

        try
        {
            entidade.AtualizarDataModificacao();
            DbSet.Update(entidade);
            await Context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Recarregar a entidade atual do banco para obter os valores atuais
            var entry = ex.Entries.Single();
            var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);
            
            if (databaseValues == null)
            {
                throw new InvalidOperationException("A entidade foi excluída por outro usuário.");
            }
            
            throw new InvalidOperationException("A entidade foi modificada por outro usuário. Por favor, recarregue os dados e tente novamente.");
        }
    }
}