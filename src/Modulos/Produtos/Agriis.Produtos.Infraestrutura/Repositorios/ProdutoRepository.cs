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

    public async Task<Produto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.ProdutoPai)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo))
            .FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterPorFornecedorAsync(int fornecedorId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo))
            .Where(p => p.FornecedorId == fornecedorId)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterPorCategoriaAsync(int categoriaId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo))
            .Where(p => p.CategoriaId == categoriaId)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterPorCulturaAsync(int culturaId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo))
            .Where(p => p.ProdutosCulturas.Any(pc => pc.CulturaId == culturaId && pc.Ativo))
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterPorTipoAsync(TipoProduto tipo, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo))
            .Where(p => p.Tipo == tipo)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo))
            .Where(p => p.Status == StatusProduto.Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterFabricantesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo))
            .Where(p => p.Tipo == TipoProduto.Fabricante)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterProdutosFilhosAsync(int produtoPaiId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo))
            .Where(p => p.ProdutoPaiId == produtoPaiId)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterRestritosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo))
            .Where(p => p.ProdutoRestrito)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> BuscarPorNomeOuCodigoAsync(string termo, CancellationToken cancellationToken = default)
    {
        var termoLower = termo.ToLower();
        
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo))
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
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo))
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Produto>> ObterPorCulturasAsync(IEnumerable<int> culturasIds, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo))
            .Where(p => p.ProdutosCulturas.Any(pc => culturasIds.Contains(pc.CulturaId) && pc.Ativo))
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Produto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.ProdutoPai)
            .Include(p => p.ProdutosFilhos)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Produto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.ProdutosCulturas.Where(pc => pc.Ativo))
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }
}