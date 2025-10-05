using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Referencias.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de moedas
/// </summary>
public class MoedaRepository : ReferenciaRepositoryBase<Moeda, DbContext>, IMoedaRepository
{
    public MoedaRepository(DbContext context) : base(context)
    {
    }

    /// <summary>
    /// Verifica se existe uma moeda com o nome especificado
    /// </summary>
    public new async Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Moeda>().Where(m => m.Nome == nome);
        
        if (idExcluir.HasValue)
            query = query.Where(m => m.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se existe uma moeda com o símbolo especificado
    /// </summary>
    public async Task<bool> ExisteSimboloAsync(string simbolo, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Moeda>().Where(m => m.Simbolo == simbolo);
        
        if (idExcluir.HasValue)
            query = query.Where(m => m.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Expressão para filtrar moedas ativas
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Moeda, bool>> GetAtivoExpression()
    {
        return m => m.Ativo;
    }

    /// <summary>
    /// Expressão para filtrar por código
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Moeda, bool>> GetCodigoExpression(string codigo)
    {
        return m => m.Codigo == codigo;
    }

    /// <summary>
    /// Expressão para filtrar por nome
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Moeda, bool>> GetNomeExpression(string nome)
    {
        return m => m.Nome == nome;
    }

    /// <summary>
    /// Define o status ativo da moeda
    /// </summary>
    protected override void SetAtivo(Moeda entidade, bool ativo)
    {
        if (ativo)
            entidade.Ativar();
        else
            entidade.Desativar();
    }

    /// <summary>
    /// Verifica se uma moeda pode ser removida
    /// </summary>
    public override async Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        // Verificar se a moeda está sendo usada em outras entidades (produtos, preços, etc.)
        // Por enquanto, sempre permite remoção - implementar verificações específicas conforme necessário
        // TODO: Implementar verificações quando as entidades que usam moeda forem refatoradas
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Valida se o código da moeda está no formato correto (3 caracteres)
    /// </summary>
    public bool ValidarCodigoMoeda(string codigo)
    {
        return !string.IsNullOrWhiteSpace(codigo) && 
               codigo.Length == 3 && 
               codigo.All(char.IsLetter) && 
               codigo.All(char.IsUpper);
    }

    /// <summary>
    /// Obtém moedas por código (busca parcial para autocomplete)
    /// </summary>
    public async Task<IEnumerable<Moeda>> BuscarPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Moeda>()
            .Where(m => m.Codigo.Contains(codigo.ToUpper()) && m.Ativo)
            .OrderBy(m => m.Codigo)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém uma moeda por código
    /// </summary>
    public async Task<Moeda?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Moeda>()
            .Include(m => m.Pais)
            .FirstOrDefaultAsync(m => m.Codigo == codigo, cancellationToken);
    }

    /// <summary>
    /// Obtém todas as moedas com informações do país
    /// </summary>
    public override async Task<IEnumerable<Moeda>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Moeda>()
            .Include(m => m.Pais)
            .OrderBy(m => m.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém moeda por ID com informações do país
    /// </summary>
    public override async Task<Moeda?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Moeda>()
            .Include(m => m.Pais)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }
}