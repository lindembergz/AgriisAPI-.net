using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Enums;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Referencias.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de atividades agropecuárias
/// </summary>
public class AtividadeAgropecuariaRepository : ReferenciaRepositoryBase<AtividadeAgropecuaria, DbContext>, IAtividadeAgropecuariaRepository
{
    public AtividadeAgropecuariaRepository(DbContext context) : base(context)
    {
    }

    /// <summary>
    /// Verifica se existe uma atividade com a descrição especificada
    /// </summary>
    public async Task<bool> ExisteDescricaoAsync(string descricao, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<AtividadeAgropecuaria>().Where(a => a.Descricao == descricao);
        
        if (idExcluir.HasValue)
            query = query.Where(a => a.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém atividades por tipo
    /// </summary>
    public async Task<IEnumerable<AtividadeAgropecuaria>> ObterPorTipoAsync(TipoAtividadeAgropecuaria tipo, CancellationToken cancellationToken = default)
    {
        return await Context.Set<AtividadeAgropecuaria>()
            .Where(a => a.Tipo == tipo)
            .OrderBy(a => a.Descricao)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém atividades ativas por tipo
    /// </summary>
    public async Task<IEnumerable<AtividadeAgropecuaria>> ObterAtivasPorTipoAsync(TipoAtividadeAgropecuaria tipo, CancellationToken cancellationToken = default)
    {
        return await Context.Set<AtividadeAgropecuaria>()
            .Where(a => a.Tipo == tipo && a.Ativo)
            .OrderBy(a => a.Descricao)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Expressão para filtrar atividades ativas
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<AtividadeAgropecuaria, bool>> GetAtivoExpression()
    {
        return a => a.Ativo;
    }

    /// <summary>
    /// Expressão para filtrar por código
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<AtividadeAgropecuaria, bool>> GetCodigoExpression(string codigo)
    {
        return a => a.Codigo == codigo;
    }

    /// <summary>
    /// Expressão para filtrar por nome (descrição)
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<AtividadeAgropecuaria, bool>> GetNomeExpression(string nome)
    {
        return a => a.Descricao == nome;
    }

    /// <summary>
    /// Define o status ativo da atividade
    /// </summary>
    protected override void SetAtivo(AtividadeAgropecuaria entidade, bool ativo)
    {
        if (ativo)
            entidade.Ativar();
        else
            entidade.Desativar();
    }

    /// <summary>
    /// Verifica se uma atividade pode ser removida
    /// </summary>
    public override async Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        // Verificar se a atividade está sendo usada em outras entidades (produtores, produtos, etc.)
        // TODO: Implementar verificações quando as entidades que usam atividade forem refatoradas
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Obtém estatísticas de atividades por tipo
    /// </summary>
    public async Task<Dictionary<TipoAtividadeAgropecuaria, int>> ObterEstatisticasPorTipoAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<AtividadeAgropecuaria>()
            .Where(a => a.Ativo)
            .GroupBy(a => a.Tipo)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    /// <summary>
    /// Busca atividades por descrição (busca parcial)
    /// </summary>
    public async Task<IEnumerable<AtividadeAgropecuaria>> BuscarPorDescricaoAsync(string descricao, TipoAtividadeAgropecuaria? tipo = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<AtividadeAgropecuaria>()
            .Where(a => a.Descricao.Contains(descricao) && a.Ativo);

        if (tipo.HasValue)
            query = query.Where(a => a.Tipo == tipo.Value);

        return await query
            .OrderBy(a => a.Descricao)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém todas as atividades agrupadas por tipo
    /// </summary>
    public async Task<Dictionary<TipoAtividadeAgropecuaria, IEnumerable<AtividadeAgropecuaria>>> ObterAgrupadasPorTipoAsync(CancellationToken cancellationToken = default)
    {
        var atividades = await Context.Set<AtividadeAgropecuaria>()
            .Where(a => a.Ativo)
            .OrderBy(a => a.Descricao)
            .ToListAsync(cancellationToken);

        return atividades.GroupBy(a => a.Tipo)
            .ToDictionary(g => g.Key, g => g.AsEnumerable());
    }

    /// <summary>
    /// Obtém uma atividade agropecuária por código
    /// </summary>
    public async Task<AtividadeAgropecuaria?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await Context.Set<AtividadeAgropecuaria>()
            .FirstOrDefaultAsync(a => a.Codigo == codigo, cancellationToken);
    }
}