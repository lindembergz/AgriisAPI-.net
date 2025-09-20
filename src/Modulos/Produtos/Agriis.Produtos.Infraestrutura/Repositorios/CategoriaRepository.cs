using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.Enums;
using Agriis.Produtos.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Produtos.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de categorias
/// </summary>
public class CategoriaRepository : RepositoryBase<Categoria, DbContext>, ICategoriaRepository
{
    public CategoriaRepository(DbContext context) : base(context)
    {
    }

    public async Task<Categoria?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.CategoriaPai)
            .Include(c => c.SubCategorias.Where(sc => sc.Ativo))
            .FirstOrDefaultAsync(c => c.Nome == nome, cancellationToken);
    }

    public async Task<IEnumerable<Categoria>> ObterAtivasAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.CategoriaPai)
            .Include(c => c.SubCategorias.Where(sc => sc.Ativo))
            .Where(c => c.Ativo)
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Categoria>> ObterPorTipoAsync(CategoriaProduto tipo, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.CategoriaPai)
            .Include(c => c.SubCategorias.Where(sc => sc.Ativo))
            .Where(c => c.Tipo == tipo)
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Categoria>> ObterCategoriasRaizAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.SubCategorias.Where(sc => sc.Ativo))
            .Where(c => c.CategoriaPaiId == null)
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Categoria>> ObterSubCategoriasAsync(int categoriaPaiId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.CategoriaPai)
            .Include(c => c.SubCategorias.Where(sc => sc.Ativo))
            .Where(c => c.CategoriaPaiId == categoriaPaiId)
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Categoria>> ObterComSubCategoriasAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.CategoriaPai)
            .Include(c => c.SubCategorias.Where(sc => sc.Ativo))
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Categoria>> ObterOrdenadasAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.CategoriaPai)
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteComNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(c => c.Nome == nome);
        
        if (idExcluir.HasValue)
            query = query.Where(c => c.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> TemProdutosAsync(int categoriaId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Produto>()
            .AnyAsync(p => p.CategoriaId == categoriaId, cancellationToken);
    }

    public async Task<bool> TemSubCategoriasAsync(int categoriaId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(c => c.CategoriaPaiId == categoriaId && c.Ativo, cancellationToken);
    }

    public override async Task<Categoria?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.CategoriaPai)
            .Include(c => c.SubCategorias.Where(sc => sc.Ativo))
            .Include(c => c.Produtos.Where(p => p.EstaAtivo()))
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Categoria>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.CategoriaPai)
            .Include(c => c.SubCategorias.Where(sc => sc.Ativo))
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }
}