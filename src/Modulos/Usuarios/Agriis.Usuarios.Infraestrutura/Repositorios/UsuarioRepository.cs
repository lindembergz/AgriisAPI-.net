using Microsoft.EntityFrameworkCore;
using Agriis.Usuarios.Dominio.Entidades;
using Agriis.Usuarios.Dominio.Interfaces;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Compartilhado.Infraestrutura.Persistencia;

namespace Agriis.Usuarios.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de usuários
/// </summary>
public class UsuarioRepository : RepositoryBase<Usuario, DbContext>, IUsuarioRepository
{
    public UsuarioRepository(DbContext context) : base(context)
    {
    }
    
    public async Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Usuario>()
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
    }
    
    public async Task<Usuario?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Usuario>()
            .FirstOrDefaultAsync(u => u.Cpf != null && u.Cpf.Valor == cpf, cancellationToken);
    }
    
    public async Task<IEnumerable<Usuario>> ObterPorRoleAsync(Roles role, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Usuario>()
            .Include(u => u.UsuarioRoles)
            .Where(u => u.UsuarioRoles.Any(ur => ur.Role == role))
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Usuario>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Usuario>()
            .Include(u => u.UsuarioRoles)
            .Where(u => u.Ativo)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<bool> ExisteEmailAsync(string email, int? usuarioIdExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Usuario>()
            .Where(u => u.Email == email.ToLowerInvariant());
        
        if (usuarioIdExcluir.HasValue)
        {
            query = query.Where(u => u.Id != usuarioIdExcluir.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
    
    public async Task<bool> ExisteCpfAsync(string cpf, int? usuarioIdExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Usuario>()
            .Where(u => u.Cpf != null && u.Cpf.Valor == cpf);
        
        if (usuarioIdExcluir.HasValue)
        {
            query = query.Where(u => u.Id != usuarioIdExcluir.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
    
    public async Task<(IEnumerable<Usuario> Usuarios, int Total)> ObterPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? filtro = null, 
        bool apenasAtivos = true, 
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Usuario>()
            .Include(u => u.UsuarioRoles)
            .AsQueryable();
        
        // Filtrar apenas ativos se solicitado
        if (apenasAtivos)
        {
            query = query.Where(u => u.Ativo);
        }
        
        // Aplicar filtro de busca
        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var filtroLower = filtro.ToLowerInvariant();
            query = query.Where(u => 
                u.Nome.ToLower().Contains(filtroLower) ||
                u.Email.ToLower().Contains(filtroLower));
        }
        
        // Contar total
        var total = await query.CountAsync(cancellationToken);
        
        // Aplicar paginação
        var usuarios = await query
            .OrderBy(u => u.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);
        
        return (usuarios, total);
    }
    
    public async Task<Usuario?> ObterComRolesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Usuario>()
            .Include(u => u.UsuarioRoles)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
    
    public async Task<Usuario?> ObterPorEmailComRolesAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Usuario>()
            .Include(u => u.UsuarioRoles)
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
    }
}