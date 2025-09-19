using Microsoft.EntityFrameworkCore;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Produtores.Dominio.Entidades;
using Agriis.Produtores.Dominio.Interfaces;

namespace Agriis.Produtores.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de relacionamentos usuário-produtor
/// </summary>
public class UsuarioProdutorRepository : RepositoryBase<UsuarioProdutor, DbContext>, IUsuarioProdutorRepository
{
    public UsuarioProdutorRepository(DbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UsuarioProdutor>> ObterPorUsuarioAsync(int usuarioId, bool apenasAtivos = true)
    {
        var query = Context.Set<UsuarioProdutor>()
            .Include(up => up.Usuario)
            .Include(up => up.Produtor)
            .Where(up => up.UsuarioId == usuarioId);

        if (apenasAtivos)
        {
            query = query.Where(up => up.Ativo);
        }

        return await query
            .OrderByDescending(up => up.EhProprietario)
            .ThenBy(up => up.Produtor.Nome)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UsuarioProdutor>> ObterPorProdutorAsync(int produtorId, bool apenasAtivos = true)
    {
        var query = Context.Set<UsuarioProdutor>()
            .Include(up => up.Usuario)
            .Include(up => up.Produtor)
            .Where(up => up.ProdutorId == produtorId);

        if (apenasAtivos)
        {
            query = query.Where(up => up.Ativo);
        }

        return await query
            .OrderByDescending(up => up.EhProprietario)
            .ThenBy(up => up.Usuario.Nome)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<UsuarioProdutor?> ObterPorUsuarioEProdutorAsync(int usuarioId, int produtorId)
    {
        return await Context.Set<UsuarioProdutor>()
            .Include(up => up.Usuario)
            .Include(up => up.Produtor)
            .FirstOrDefaultAsync(up => up.UsuarioId == usuarioId && up.ProdutorId == produtorId);
    }

    /// <inheritdoc />
    public async Task<UsuarioProdutor?> ObterProprietarioPrincipalAsync(int produtorId)
    {
        return await Context.Set<UsuarioProdutor>()
            .Include(up => up.Usuario)
            .Include(up => up.Produtor)
            .FirstOrDefaultAsync(up => up.ProdutorId == produtorId && up.EhProprietario && up.Ativo);
    }

    /// <inheritdoc />
    public async Task<bool> UsuarioTemAcessoAoProdutorAsync(int usuarioId, int produtorId)
    {
        return await Context.Set<UsuarioProdutor>()
            .AnyAsync(up => up.UsuarioId == usuarioId && up.ProdutorId == produtorId && up.Ativo);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Produtor>> ObterProdutoresDoUsuarioAsync(int usuarioId)
    {
        return await Context.Set<UsuarioProdutor>()
            .Include(up => up.Produtor)
            .ThenInclude(p => p.UsuariosProdutores)
            .ThenInclude(up => up.Usuario)
            .Where(up => up.UsuarioId == usuarioId && up.Ativo)
            .Select(up => up.Produtor)
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    /// <inheritdoc />
    public override async Task<UsuarioProdutor?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<UsuarioProdutor>()
            .Include(up => up.Usuario)
            .Include(up => up.Produtor)
            .FirstOrDefaultAsync(up => up.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<UsuarioProdutor>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<UsuarioProdutor>()
            .Include(up => up.Usuario)
            .Include(up => up.Produtor)
            .OrderBy(up => up.Usuario.Nome)
            .ThenBy(up => up.Produtor.Nome)
            .ToListAsync(cancellationToken);
    }
}