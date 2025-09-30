using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Referencias.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de UFs
/// </summary>
public class UfRepository : ReferenciaRepositoryBase<Uf, DbContext>, IUfRepository
{
    public UfRepository(DbContext context) : base(context)
    {
    }

    /// <summary>
    /// Verifica se existe uma UF com o nome especificado
    /// </summary>
    public new async Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Uf>().Where(u => u.Nome == nome);
        
        if (idExcluir.HasValue)
            query = query.Where(u => u.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém UFs por país
    /// </summary>
    public async Task<IEnumerable<Uf>> ObterPorPaisAsync(int paisId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Uf>()
            .Where(u => u.PaisId == paisId)
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém UFs ativas por país
    /// </summary>
    public async Task<IEnumerable<Uf>> ObterAtivasPorPaisAsync(int paisId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Uf>()
            .Where(u => u.PaisId == paisId && u.Ativo)
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se a UF possui municípios cadastrados
    /// </summary>
    public async Task<bool> PossuiMunicipiosAsync(int ufId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Municipio>()
            .AnyAsync(m => m.UfId == ufId, cancellationToken);
    }

    /// <summary>
    /// Expressão para filtrar UFs ativas
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Uf, bool>> GetAtivoExpression()
    {
        return u => u.Ativo;
    }

    /// <summary>
    /// Expressão para filtrar por código
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Uf, bool>> GetCodigoExpression(string codigo)
    {
        return u => u.Codigo == codigo;
    }

    /// <summary>
    /// Expressão para filtrar por nome
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Uf, bool>> GetNomeExpression(string nome)
    {
        return u => u.Nome == nome;
    }

    /// <summary>
    /// Define o status ativo da UF
    /// </summary>
    protected override void SetAtivo(Uf entidade, bool ativo)
    {
        if (ativo)
            entidade.Ativar();
        else
            entidade.Desativar();
    }

    /// <summary>
    /// Verifica se uma UF pode ser removida
    /// </summary>
    public override async Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        // Verificar se a UF possui municípios cadastrados
        var possuiMunicipios = await PossuiMunicipiosAsync(id, cancellationToken);
        
        if (possuiMunicipios)
            return false;

        // Verificar se está sendo usada em outras entidades (fornecedores, endereços, etc.)
        // Por enquanto, só verifica municípios
        return true;
    }

    /// <summary>
    /// Obtém a contagem de municípios por UF
    /// </summary>
    public async Task<int> ContarMunicipiosPorUfAsync(int ufId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Municipio>()
            .CountAsync(m => m.UfId == ufId, cancellationToken);
    }

    /// <summary>
    /// Obtém UFs com seus municípios (para relatórios e visualizações hierárquicas)
    /// </summary>
    public async Task<IEnumerable<Uf>> ObterComMunicipiosAsync(int? paisId = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Uf>()
            .Include(u => u.Municipios.Where(m => m.Ativo))
            .Where(u => u.Ativo);

        if (paisId.HasValue)
            query = query.Where(u => u.PaisId == paisId.Value);

        return await query
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca UFs por nome (busca parcial)
    /// </summary>
    public async Task<IEnumerable<Uf>> BuscarPorNomeAsync(string nome, int? paisId = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Uf>()
            .Where(u => u.Nome.Contains(nome) && u.Ativo);

        if (paisId.HasValue)
            query = query.Where(u => u.PaisId == paisId.Value);

        return await query
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Sobrescreve o método para incluir municípios nas consultas quando necessário
    /// </summary>
    public override async Task<Uf?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Uf>()
            .Include(u => u.Municipios)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>
    /// Sobrescreve o método para obter todas as UFs ordenadas por nome
    /// </summary>
    public override async Task<IEnumerable<Uf>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Uf>()
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Sobrescreve o método para obter UFs ativas ordenadas por nome
    /// </summary>
    public override async Task<IEnumerable<Uf>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Uf>()
            .Where(u => u.Ativo)
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);
    }
}