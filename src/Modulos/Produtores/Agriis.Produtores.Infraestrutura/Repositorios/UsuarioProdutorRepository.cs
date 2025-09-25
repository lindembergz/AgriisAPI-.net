using Microsoft.EntityFrameworkCore;
using Agriis.Produtores.Dominio.Entidades;
using Agriis.Produtores.Dominio.Interfaces;

namespace Agriis.Produtores.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de relacionamento usuário-produtor
/// </summary>
public class UsuarioProdutorRepository : IUsuarioProdutorRepository
{
    private readonly DbContext _context;

    public UsuarioProdutorRepository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<UsuarioProdutor> AdicionarAsync(UsuarioProdutor usuarioProdutor, CancellationToken cancellationToken = default)
    {
        if (usuarioProdutor == null)
            throw new ArgumentNullException(nameof(usuarioProdutor));

        _context.Set<UsuarioProdutor>().Add(usuarioProdutor);
        await _context.SaveChangesAsync(cancellationToken);
        return usuarioProdutor;
    }

    public async Task<UsuarioProdutor?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UsuarioProdutor>()
            .Include(up => up.Usuario)
            .Include(up => up.Produtor)
            .FirstOrDefaultAsync(up => up.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<UsuarioProdutor>> ObterPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UsuarioProdutor>()
            .Include(up => up.Produtor)
            .Where(up => up.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UsuarioProdutor>> ObterPorProdutorAsync(int produtorId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UsuarioProdutor>()
            .Include(up => up.Usuario)
            .Where(up => up.ProdutorId == produtorId)
            .ToListAsync(cancellationToken);
    }

    public async Task<UsuarioProdutor?> ObterPorUsuarioEProdutorAsync(int usuarioId, int produtorId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UsuarioProdutor>()
            .Include(up => up.Usuario)
            .Include(up => up.Produtor)
            .FirstOrDefaultAsync(up => up.UsuarioId == usuarioId && up.ProdutorId == produtorId, cancellationToken);
    }

    public async Task AtualizarAsync(UsuarioProdutor usuarioProdutor, CancellationToken cancellationToken = default)
    {
        if (usuarioProdutor == null)
            throw new ArgumentNullException(nameof(usuarioProdutor));

        _context.Set<UsuarioProdutor>().Update(usuarioProdutor);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuarioProdutor = await _context.Set<UsuarioProdutor>().FindAsync(new object[] { id }, cancellationToken);
        if (usuarioProdutor != null)
        {
            _context.Set<UsuarioProdutor>().Remove(usuarioProdutor);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExisteRelacionamentoAsync(int usuarioId, int produtorId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UsuarioProdutor>()
            .AnyAsync(up => up.UsuarioId == usuarioId && up.ProdutorId == produtorId, cancellationToken);
    }
}