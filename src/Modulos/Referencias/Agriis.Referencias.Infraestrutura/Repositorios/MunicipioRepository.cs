using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Referencias.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de municípios
/// </summary>
public class MunicipioRepository : ReferenciaRepositoryBase<Municipio, DbContext>, IMunicipioRepository
{
    public MunicipioRepository(DbContext context) : base(context)
    {
    }

    /// <summary>
    /// Verifica se existe um município com o nome especificado
    /// </summary>
    public new async Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Municipio>().Where(m => m.Nome == nome);
        
        if (idExcluir.HasValue)
            query = query.Where(m => m.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se existe um município com o código IBGE especificado
    /// </summary>
    public async Task<bool> ExisteCodigoIbgeAsync(string codigoIbge, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Municipio>().Where(m => m.CodigoIbge == codigoIbge);
        
        if (idExcluir.HasValue)
            query = query.Where(m => m.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se existe um município com o nome especificado na UF
    /// </summary>
    public async Task<bool> ExisteNomeNaUfAsync(string nome, int ufId, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Municipio>().Where(m => m.Nome == nome && m.UfId == ufId);
        
        if (idExcluir.HasValue)
            query = query.Where(m => m.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém municípios por UF
    /// </summary>
    public async Task<IEnumerable<Municipio>> ObterPorUfAsync(int ufId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Municipio>()
            .Include(m => m.Uf)
            .ThenInclude(u => u.Pais)
            .Where(m => m.UfId == ufId)
            .OrderBy(m => m.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém municípios ativos por UF
    /// </summary>
    public async Task<IEnumerable<Municipio>> ObterAtivosPorUfAsync(int ufId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Municipio>()
            .Include(m => m.Uf)
            .ThenInclude(u => u.Pais)
            .Where(m => m.UfId == ufId && m.Ativo)
            .OrderBy(m => m.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca municípios por nome (busca parcial)
    /// </summary>
    public async Task<IEnumerable<Municipio>> BuscarPorNomeAsync(string nome, int? ufId = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Municipio>()
            .Include(m => m.Uf)
            .ThenInclude(u => u.Pais)
            .Where(m => m.Nome.Contains(nome));

        if (ufId.HasValue)
            query = query.Where(m => m.UfId == ufId.Value);

        return await query
            .OrderBy(m => m.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Expressão para filtrar municípios ativos
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Municipio, bool>> GetAtivoExpression()
    {
        return m => m.Ativo;
    }

    /// <summary>
    /// Expressão para filtrar por código (código IBGE)
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Municipio, bool>> GetCodigoExpression(string codigo)
    {
        return m => m.CodigoIbge == codigo;
    }

    /// <summary>
    /// Expressão para filtrar por nome
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Municipio, bool>> GetNomeExpression(string nome)
    {
        return m => m.Nome == nome;
    }

    /// <summary>
    /// Define o status ativo do município
    /// </summary>
    protected override void SetAtivo(Municipio entidade, bool ativo)
    {
        if (ativo)
            entidade.Ativar();
        else
            entidade.Desativar();
    }

    /// <summary>
    /// Verifica se um município pode ser removido
    /// </summary>
    public override async Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        // Verificar se o município está sendo usado em outras entidades (fornecedores, endereços, etc.)
        // TODO: Implementar verificações quando as entidades que usam município forem refatoradas
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Busca municípios por código IBGE (busca parcial)
    /// </summary>
    public async Task<IEnumerable<Municipio>> BuscarPorCodigoIbgeAsync(string codigoIbge, int? ufId = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Municipio>()
            .Include(m => m.Uf)
            .ThenInclude(u => u.Pais)
            .Where(m => m.CodigoIbge.Contains(codigoIbge) && m.Ativo);

        if (ufId.HasValue)
            query = query.Where(m => m.UfId == ufId.Value);

        return await query
            .OrderBy(m => m.CodigoIbge)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém municípios por região (baseado no código IBGE)
    /// </summary>
    public async Task<IEnumerable<Municipio>> ObterPorRegiaoAsync(string prefixoCodigoIbge, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Municipio>()
            .Include(m => m.Uf)
            .ThenInclude(u => u.Pais)
            .Where(m => m.CodigoIbge.StartsWith(prefixoCodigoIbge) && m.Ativo)
            .OrderBy(m => m.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém estatísticas de municípios por UF
    /// </summary>
    public async Task<Dictionary<string, int>> ObterEstatisticasPorUfAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Municipio>()
            .Include(m => m.Uf)
            .Where(m => m.Ativo)
            .GroupBy(m => m.Uf.Nome)
            .Select(g => new { UfNome = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UfNome, x => x.Count, cancellationToken);
    }

    /// <summary>
    /// Sobrescreve o método para incluir a UF e país nas consultas
    /// </summary>
    public override async Task<Municipio?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Municipio>()
            .Include(m => m.Uf)
            .ThenInclude(u => u.Pais)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    /// <summary>
    /// Sobrescreve o método para incluir a UF nas consultas
    /// </summary>
    public override async Task<IEnumerable<Municipio>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Municipio>()
            .Include(m => m.Uf)
            .ThenInclude(u => u.Pais)
            .OrderBy(m => m.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Sobrescreve o método para incluir a UF nas consultas
    /// </summary>
    public override async Task<IEnumerable<Municipio>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Municipio>()
            .Include(m => m.Uf)
            .ThenInclude(u => u.Pais)
            .Where(m => m.Ativo)
            .OrderBy(m => m.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém um município por código IBGE
    /// </summary>
    public async Task<Municipio?> ObterPorCodigoIbgeAsync(string codigoIbge, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Municipio>()
            .Include(m => m.Uf)
            .ThenInclude(u => u.Pais)
            .FirstOrDefaultAsync(m => m.CodigoIbge == codigoIbge, cancellationToken);
    }
}