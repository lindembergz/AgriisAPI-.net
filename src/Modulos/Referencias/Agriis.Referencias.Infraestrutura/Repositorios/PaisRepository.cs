using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Referencias.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de países
/// </summary>
public class PaisRepository : ReferenciaRepositoryBase<Pais, DbContext>, IPaisRepository
{
    public PaisRepository(DbContext context) : base(context)
    {
    }

    /// <summary>
    /// Verifica se existe um país com o nome especificado
    /// </summary>
    public new async Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Pais>().Where(p => p.Nome == nome);
        
        if (idExcluir.HasValue)
            query = query.Where(p => p.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se o país possui UFs cadastradas
    /// </summary>
    public async Task<bool> PossuiUfsAsync(int paisId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Uf>()
            .AnyAsync(u => u.PaisId == paisId, cancellationToken);
    }

    /// <summary>
    /// Expressão para filtrar países ativos
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Pais, bool>> GetAtivoExpression()
    {
        return p => p.Ativo;
    }

    /// <summary>
    /// Expressão para filtrar por código
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Pais, bool>> GetCodigoExpression(string codigo)
    {
        return p => p.Codigo == codigo;
    }

    /// <summary>
    /// Expressão para filtrar por nome
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Pais, bool>> GetNomeExpression(string nome)
    {
        return p => p.Nome == nome;
    }

    /// <summary>
    /// Define o status ativo do país
    /// </summary>
    protected override void SetAtivo(Pais entidade, bool ativo)
    {
        if (ativo)
            entidade.Ativar();
        else
            entidade.Desativar();
    }

    /// <summary>
    /// Verifica se um país pode ser removido
    /// </summary>
    public override async Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        // Verificar se o país possui UFs cadastradas
        var possuiUfs = await PossuiUfsAsync(id, cancellationToken);
        
        if (possuiUfs)
            return false;

        // Verificar se está sendo usado em outras entidades (endereços, fornecedores, etc.)
        // Por enquanto, só verifica UFs
        return true;
    }

    /// <summary>
    /// Obtém a contagem de UFs por país
    /// </summary>
    public async Task<int> ContarUfsPorPaisAsync(int paisId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Uf>()
            .CountAsync(u => u.PaisId == paisId, cancellationToken);
    }

    /// <summary>
    /// Obtém países com suas UFs (para relatórios e visualizações hierárquicas)
    /// </summary>
    public async Task<IEnumerable<Pais>> ObterComUfsAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Pais>()
            .Include(p => p.Ufs.Where(u => u.Ativo))
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Sobrescreve o método para incluir as UFs nas consultas quando necessário
    /// </summary>
    public override async Task<Pais?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Pais>()
            .Include(p => p.Ufs)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <summary>
    /// Obtém um país por código
    /// </summary>
    public async Task<Pais?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Pais>()
            .FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);
    }
}