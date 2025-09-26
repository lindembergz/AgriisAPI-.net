using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Enums;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Referencias.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de unidades de medida
/// </summary>
public class UnidadeMedidaRepository : ReferenciaRepositoryBase<UnidadeMedida, DbContext>, IUnidadeMedidaRepository
{
    public UnidadeMedidaRepository(DbContext context) : base(context)
    {
    }

    /// <summary>
    /// Verifica se existe uma unidade com o símbolo especificado
    /// </summary>
    public async Task<bool> ExisteSimboloAsync(string simbolo, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<UnidadeMedida>().Where(u => u.Simbolo == simbolo);
        
        if (idExcluir.HasValue)
            query = query.Where(u => u.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se existe uma unidade com o nome especificado
    /// </summary>
    public new async Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<UnidadeMedida>().Where(u => u.Nome == nome);
        
        if (idExcluir.HasValue)
            query = query.Where(u => u.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém unidades por tipo
    /// </summary>
    public async Task<IEnumerable<UnidadeMedida>> ObterPorTipoAsync(TipoUnidadeMedida tipo, CancellationToken cancellationToken = default)
    {
        return await Context.Set<UnidadeMedida>()
            .Where(u => u.Tipo == tipo)
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém unidades ativas por tipo
    /// </summary>
    public async Task<IEnumerable<UnidadeMedida>> ObterAtivasPorTipoAsync(TipoUnidadeMedida tipo, CancellationToken cancellationToken = default)
    {
        return await Context.Set<UnidadeMedida>()
            .Where(u => u.Tipo == tipo && u.Ativo)
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se a unidade possui embalagens cadastradas
    /// </summary>
    public async Task<bool> PossuiEmbalagensAsync(int unidadeId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Embalagem>()
            .AnyAsync(e => e.UnidadeMedidaId == unidadeId, cancellationToken);
    }

    /// <summary>
    /// Expressão para filtrar unidades ativas
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<UnidadeMedida, bool>> GetAtivoExpression()
    {
        return u => u.Ativo;
    }

    /// <summary>
    /// Expressão para filtrar por código (símbolo)
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<UnidadeMedida, bool>> GetCodigoExpression(string codigo)
    {
        return u => u.Simbolo == codigo;
    }

    /// <summary>
    /// Expressão para filtrar por nome
    /// </summary>
    protected override System.Linq.Expressions.Expression<Func<UnidadeMedida, bool>> GetNomeExpression(string nome)
    {
        return u => u.Nome == nome;
    }

    /// <summary>
    /// Define o status ativo da unidade
    /// </summary>
    protected override void SetAtivo(UnidadeMedida entidade, bool ativo)
    {
        if (ativo)
            entidade.Ativar();
        else
            entidade.Desativar();
    }

    /// <summary>
    /// Verifica se uma unidade pode ser removida
    /// </summary>
    public override async Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        // Verificar se a unidade possui embalagens ou está sendo usada em produtos
        var possuiEmbalagens = await PossuiEmbalagensAsync(id, cancellationToken);
        
        if (possuiEmbalagens)
            return false;

        // Verificar se está sendo usada em produtos (quando a refatoração for implementada)
        // TODO: Implementar verificação de uso em produtos
        return true;
    }

    /// <summary>
    /// Obtém unidades de medida compatíveis para conversão (mesmo tipo)
    /// </summary>
    public async Task<IEnumerable<UnidadeMedida>> ObterCompativeisParaConversaoAsync(int unidadeId, CancellationToken cancellationToken = default)
    {
        var unidade = await Context.Set<UnidadeMedida>()
            .FirstOrDefaultAsync(u => u.Id == unidadeId, cancellationToken);

        if (unidade == null)
            return Enumerable.Empty<UnidadeMedida>();

        return await Context.Set<UnidadeMedida>()
            .Where(u => u.Tipo == unidade.Tipo && u.Id != unidadeId && u.Ativo)
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Calcula conversão entre duas unidades de medida
    /// </summary>
    public async Task<decimal?> CalcularConversaoAsync(int unidadeOrigemId, int unidadeDestinoId, decimal valor, CancellationToken cancellationToken = default)
    {
        var unidades = await Context.Set<UnidadeMedida>()
            .Where(u => u.Id == unidadeOrigemId || u.Id == unidadeDestinoId)
            .ToListAsync(cancellationToken);

        var origem = unidades.FirstOrDefault(u => u.Id == unidadeOrigemId);
        var destino = unidades.FirstOrDefault(u => u.Id == unidadeDestinoId);

        if (origem == null || destino == null || origem.Tipo != destino.Tipo)
            return null;

        if (!origem.FatorConversao.HasValue || !destino.FatorConversao.HasValue)
            return null;

        // Converte para unidade base e depois para unidade destino
        var valorBase = valor * origem.FatorConversao.Value;
        return valorBase / destino.FatorConversao.Value;
    }

    /// <summary>
    /// Obtém estatísticas de unidades por tipo
    /// </summary>
    public async Task<Dictionary<TipoUnidadeMedida, int>> ObterEstatisticasPorTipoAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<UnidadeMedida>()
            .Where(u => u.Ativo)
            .GroupBy(u => u.Tipo)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    /// <summary>
    /// Obtém unidade por símbolo
    /// </summary>
    public async Task<UnidadeMedida?> ObterPorSimboloAsync(string simbolo, CancellationToken cancellationToken = default)
    {
        return await Context.Set<UnidadeMedida>()
            .FirstOrDefaultAsync(u => u.Simbolo == simbolo, cancellationToken);
    }

    /// <summary>
    /// Busca unidades por símbolo (busca parcial)
    /// </summary>
    public async Task<IEnumerable<UnidadeMedida>> BuscarPorSimboloAsync(string simbolo, TipoUnidadeMedida? tipo = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<UnidadeMedida>()
            .Where(u => u.Simbolo.Contains(simbolo) && u.Ativo);

        if (tipo.HasValue)
            query = query.Where(u => u.Tipo == tipo.Value);

        return await query
            .OrderBy(u => u.Simbolo)
            .ToListAsync(cancellationToken);
    }
}