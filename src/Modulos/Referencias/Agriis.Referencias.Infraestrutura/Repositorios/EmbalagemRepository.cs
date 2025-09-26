using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Enums;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Referencias.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de embalagens
/// </summary>
public class EmbalagemRepository : ReferenciaRepositoryBase<Embalagem, DbContext>, IEmbalagemRepository
{
    public EmbalagemRepository(DbContext context) : base(context)
    {
    }

    /// <summary>
    /// Verifica se existe uma embalagem com o nome especificado
    /// </summary>
    public new async Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Embalagem>().Where(e => e.Nome == nome);
        
        if (idExcluir.HasValue)
            query = query.Where(e => e.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém embalagens por unidade de medida
    /// </summary>
    public async Task<IEnumerable<Embalagem>> ObterPorUnidadeMedidaAsync(int unidadeMedidaId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Embalagem>()
            .Include(e => e.UnidadeMedida)
            .Where(e => e.UnidadeMedidaId == unidadeMedidaId)
            .OrderBy(e => e.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém embalagens ativas por unidade de medida
    /// </summary>
    public async Task<IEnumerable<Embalagem>> ObterAtivasPorUnidadeMedidaAsync(int unidadeMedidaId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Embalagem>()
            .Include(e => e.UnidadeMedida)
            .Where(e => e.UnidadeMedidaId == unidadeMedidaId && e.Ativo)
            .OrderBy(e => e.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Expressão para filtrar embalagens ativas
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Embalagem, bool>> GetAtivoExpression()
    {
        return e => e.Ativo;
    }

    /// <summary>
    /// Expressão para filtrar por código (não suportado para embalagens)
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Embalagem, bool>> GetCodigoExpression(string codigo)
    {
        throw new NotImplementedException("Embalagem não possui código");
    }

    /// <summary>
    /// Expressão para filtrar por nome
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<Embalagem, bool>> GetNomeExpression(string nome)
    {
        return e => e.Nome == nome;
    }

    /// <summary>
    /// Define o status ativo da embalagem
    /// </summary>
    protected override void SetAtivo(Embalagem entidade, bool ativo)
    {
        if (ativo)
            entidade.Ativar();
        else
            entidade.Desativar();
    }

    /// <summary>
    /// Verifica se uma embalagem pode ser removida
    /// </summary>
    public override async Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        // Verificar se a embalagem está sendo usada em produtos (quando a refatoração for implementada)
        // TODO: Implementar verificações quando as entidades que usam embalagem forem refatoradas
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Obtém embalagens por tipo de unidade de medida
    /// </summary>
    public async Task<IEnumerable<Embalagem>> ObterPorTipoUnidadeMedidaAsync(TipoUnidadeMedida tipo, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Embalagem>()
            .Include(e => e.UnidadeMedida)
            .Where(e => e.UnidadeMedida.Tipo == tipo && e.Ativo)
            .OrderBy(e => e.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca embalagens por nome (busca parcial)
    /// </summary>
    public async Task<IEnumerable<Embalagem>> BuscarPorNomeAsync(string nome, int? unidadeMedidaId = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Embalagem>()
            .Include(e => e.UnidadeMedida)
            .Where(e => e.Nome.Contains(nome) && e.Ativo);

        if (unidadeMedidaId.HasValue)
            query = query.Where(e => e.UnidadeMedidaId == unidadeMedidaId.Value);

        return await query
            .OrderBy(e => e.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém estatísticas de embalagens por unidade de medida
    /// </summary>
    public async Task<Dictionary<string, int>> ObterEstatisticasPorUnidadeMedidaAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Embalagem>()
            .Include(e => e.UnidadeMedida)
            .Where(e => e.Ativo)
            .GroupBy(e => e.UnidadeMedida.Nome)
            .Select(g => new { UnidadeNome = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UnidadeNome, x => x.Count, cancellationToken);
    }

    /// <summary>
    /// Obtém embalagens com suas unidades de medida agrupadas por tipo
    /// </summary>
    public async Task<Dictionary<TipoUnidadeMedida, IEnumerable<Embalagem>>> ObterAgrupadasPorTipoUnidadeAsync(CancellationToken cancellationToken = default)
    {
        var embalagens = await Context.Set<Embalagem>()
            .Include(e => e.UnidadeMedida)
            .Where(e => e.Ativo)
            .OrderBy(e => e.Nome)
            .ToListAsync(cancellationToken);

        return embalagens.GroupBy(e => e.UnidadeMedida.Tipo)
            .ToDictionary(g => g.Key, g => g.AsEnumerable());
    }

    /// <summary>
    /// Sobrescreve o método para incluir a unidade de medida nas consultas
    /// </summary>
    public override async Task<Embalagem?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Embalagem>()
            .Include(e => e.UnidadeMedida)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <summary>
    /// Sobrescreve o método para incluir a unidade de medida nas consultas
    /// </summary>
    public override async Task<IEnumerable<Embalagem>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Embalagem>()
            .Include(e => e.UnidadeMedida)
            .OrderBy(e => e.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Sobrescreve o método para incluir a unidade de medida nas consultas
    /// </summary>
    public override async Task<IEnumerable<Embalagem>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Embalagem>()
            .Include(e => e.UnidadeMedida)
            .Where(e => e.Ativo)
            .OrderBy(e => e.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém uma embalagem por nome
    /// </summary>
    public async Task<Embalagem?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Embalagem>()
            .Include(e => e.UnidadeMedida)
            .FirstOrDefaultAsync(e => e.Nome == nome, cancellationToken);
    }

    /// <summary>
    /// Busca embalagens por nome (busca parcial)
    /// </summary>
    public async Task<IEnumerable<Embalagem>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Embalagem>()
            .Include(e => e.UnidadeMedida)
            .Where(e => e.Nome.Contains(nome) && e.Ativo)
            .OrderBy(e => e.Nome)
            .ToListAsync(cancellationToken);
    }
}