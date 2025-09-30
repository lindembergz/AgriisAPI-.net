using Agriis.Autenticacao.Dominio.Entidades;
using Agriis.Autenticacao.Dominio.Interfaces;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Autenticacao.Infraestrutura.Repositorios;

/// <summary>
/// Repositório para refresh tokens
/// </summary>
public class RefreshTokenRepository : RepositoryBase<RefreshToken, DbContext>, IRefreshTokenRepository
{
    public RefreshTokenRepository(DbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> ObterPorTokenAsync(string token)
    {
        return await DbSet
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<IEnumerable<RefreshToken>> ObterTokensValidosPorUsuarioAsync(int usuarioId)
    {
        return await DbSet
            .Where(rt => rt.UsuarioId == usuarioId && 
                        !rt.Revogado && 
                        rt.DataExpiracao > DateTimeOffset.UtcNow)
            .OrderByDescending(rt => rt.DataCriacao)
            .ToListAsync();
    }

    public async Task RevogarTodosTokensUsuarioAsync(int usuarioId)
    {
        var tokens = await DbSet
            .Where(rt => rt.UsuarioId == usuarioId && !rt.Revogado)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.Revogar();
        }

        DbSet.UpdateRange(tokens);
    }

    public async Task<int> RemoverTokensExpiradosAsync()
    {
        var tokensExpirados = await DbSet
            .Where(rt => rt.DataExpiracao <= DateTimeOffset.UtcNow || rt.Revogado)
            .ToListAsync();

        if (tokensExpirados.Any())
        {
            DbSet.RemoveRange(tokensExpirados);
        }

        return tokensExpirados.Count;
    }

    public async Task<bool> ExisteTokenValidoParaUsuarioAsync(int usuarioId)
    {
        return await DbSet
            .AnyAsync(rt => rt.UsuarioId == usuarioId && 
                           !rt.Revogado && 
                           rt.DataExpiracao > DateTimeOffset.UtcNow);
    }
}