using Microsoft.EntityFrameworkCore;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Fornecedores.Dominio.Entidades;
using Agriis.Fornecedores.Dominio.Interfaces;

namespace Agriis.Fornecedores.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de usuários fornecedores
/// </summary>
public class UsuarioFornecedorRepository : RepositoryBase<UsuarioFornecedor, DbContext>, IUsuarioFornecedorRepository
{
    public UsuarioFornecedorRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UsuarioFornecedor>> ObterPorUsuarioAsync(int usuarioId, bool apenasAtivos = true, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(uf => uf.Fornecedor)
            .Include(uf => uf.Usuario)
            .Include(uf => uf.Territorios)
            .Where(uf => uf.UsuarioId == usuarioId);

        if (apenasAtivos)
        {
            query = query.Where(uf => uf.Ativo);
        }

        return await query
            .OrderBy(uf => uf.Fornecedor.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UsuarioFornecedor>> ObterPorFornecedorAsync(int fornecedorId, bool apenasAtivos = true, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(uf => uf.Usuario)
            .Include(uf => uf.Fornecedor)
            .Include(uf => uf.Territorios)
            .Where(uf => uf.FornecedorId == fornecedorId);

        if (apenasAtivos)
        {
            query = query.Where(uf => uf.Ativo);
        }

        return await query
            .OrderBy(uf => uf.Usuario.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<UsuarioFornecedor?> ObterPorUsuarioFornecedorAsync(int usuarioId, int fornecedorId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(uf => uf.Usuario)
            .Include(uf => uf.Fornecedor)
            .Include(uf => uf.Territorios)
            .FirstOrDefaultAsync(uf => uf.UsuarioId == usuarioId && uf.FornecedorId == fornecedorId, cancellationToken);
    } 
   public async Task<IEnumerable<UsuarioFornecedor>> ObterPorRoleAsync(int fornecedorId, Roles role, bool apenasAtivos = true, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(uf => uf.Usuario)
            .Include(uf => uf.Fornecedor)
            .Include(uf => uf.Territorios)
            .Where(uf => uf.FornecedorId == fornecedorId && uf.Role == role);

        if (apenasAtivos)
        {
            query = query.Where(uf => uf.Ativo);
        }

        return await query
            .OrderBy(uf => uf.Usuario.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UsuarioFornecedor>> ObterComTerritoriosAsync(int fornecedorId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(uf => uf.Usuario)
            .Include(uf => uf.Fornecedor)
            .Include(uf => uf.Territorios.Where(t => t.Ativo))
            .Where(uf => uf.FornecedorId == fornecedorId && uf.Ativo)
            .OrderBy(uf => uf.Usuario.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteAssociacaoAtivaAsync(int usuarioId, int fornecedorId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(uf => uf.UsuarioId == usuarioId && uf.FornecedorId == fornecedorId && uf.Ativo, cancellationToken);
    }

    public override async Task<UsuarioFornecedor?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(uf => uf.Usuario)
            .Include(uf => uf.Fornecedor)
            .Include(uf => uf.Territorios)
            .FirstOrDefaultAsync(uf => uf.Id == id, cancellationToken);
    }
}