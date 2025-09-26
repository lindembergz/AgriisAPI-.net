using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.Enums;
using Agriis.Produtos.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Produtos.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de produtos
/// </summary>
public class ProdutoRepository : RepositoryBase<Produto, DbContext>, IProdutoRepository
{
    public ProdutoRepository(DbContext context) : base(context)
    {
    }

    /// <summary>
    /// Aplica os includes padrão para navigation properties
    /// </summary>
    private IQueryable<Produto> ApplyIncludes(IQueryable<Produto> query)
    {
        return query
            .Include(p => p.Categoria)
            .Include(p => p.UnidadeMedida)
            .Include(p => p.Embalagem)
            .Include(p => p.AtividadeAgropecuaria)
            .Include(p => p.ProdutoPai)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo));
    }

    public async Task<Produto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(DbSet)
            .FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterPorFornecedorAsync(int fornecedorId, CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(DbSet)
            .Where(p => p.FornecedorId == fornecedorId)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterPorCategoriaAsync(int categoriaId, CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(DbSet)
            .Where(p => p.CategoriaId == categoriaId)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterPorCulturaAsync(int culturaId, CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(DbSet)
            .Where(p => p.ProdutosCulturas.Any(pc => pc.CulturaId == culturaId && pc.Ativo))
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterPorTipoAsync(TipoProduto tipo, CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(DbSet)
            .Where(p => p.Tipo == tipo)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(DbSet)
            .Where(p => p.Status == StatusProduto.Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterFabricantesAsync(CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(DbSet)
            .Where(p => p.Tipo == TipoProduto.Fabricante)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterProdutosFilhosAsync(int produtoPaiId, CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(DbSet)
            .Where(p => p.ProdutoPaiId == produtoPaiId)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterRestritosAsync(CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(DbSet)
            .Where(p => p.ProdutoRestrito)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> BuscarPorNomeOuCodigoAsync(string termo, CancellationToken cancellationToken = default)
    {
        var termoLower = termo.ToLower();
        
        return await ApplyIncludes(DbSet)
            .Where(p => p.Nome.ToLower().Contains(termoLower) || 
                       p.Codigo.ToLower().Contains(termoLower) ||
                       (p.Marca != null && p.Marca.ToLower().Contains(termoLower)))
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteComCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(p => p.Codigo == codigo);
        
        if (idExcluir.HasValue)
            query = query.Where(p => p.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterComCulturasAsync(CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(DbSet)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterPorCulturasAsync(IEnumerable<int> culturasIds, CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(DbSet)
            .Where(p => p.ProdutosCulturas.Any(pc => culturasIds.Contains(pc.CulturaId) && pc.Ativo))
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Produto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(DbSet)
            .Include(p => p.ProdutosFilhos) // Adicionar filhos também para o método por ID
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Produto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await ApplyIncludes(DbSet)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }
}