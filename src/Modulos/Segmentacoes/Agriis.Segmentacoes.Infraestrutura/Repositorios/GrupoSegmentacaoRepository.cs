using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Segmentacoes.Dominio.Entidades;
using Agriis.Segmentacoes.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Segmentacoes.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de grupos de segmentação (descontos por categoria)
/// </summary>
public class GrupoSegmentacaoRepository : RepositoryBase<GrupoSegmentacao, DbContext>, IGrupoSegmentacaoRepository
{
    public GrupoSegmentacaoRepository(DbContext context) : base(context)
    {
    }
    
    /// <summary>
    /// Obtém todos os descontos de um grupo
    /// </summary>
    /// <param name="grupoId">ID do grupo</param>
    /// <returns>Lista de descontos</returns>
    public async Task<IEnumerable<GrupoSegmentacao>> ObterPorGrupoAsync(int grupoId)
    {
        return await DbSet
            .Where(gs => gs.GrupoId == grupoId)
            .OrderBy(gs => gs.CategoriaId)
            .ToListAsync();
    }
    
    /// <summary>
    /// Obtém descontos ativos de um grupo
    /// </summary>
    /// <param name="grupoId">ID do grupo</param>
    /// <returns>Lista de descontos ativos</returns>
    public async Task<IEnumerable<GrupoSegmentacao>> ObterAtivosPorGrupoAsync(int grupoId)
    {
        return await DbSet
            .Where(gs => gs.GrupoId == grupoId && gs.Ativo)
            .OrderBy(gs => gs.CategoriaId)
            .ToListAsync();
    }
    
    /// <summary>
    /// Obtém desconto específico por grupo e categoria
    /// </summary>
    /// <param name="grupoId">ID do grupo</param>
    /// <param name="categoriaId">ID da categoria</param>
    /// <returns>Desconto ou null</returns>
    public async Task<GrupoSegmentacao?> ObterPorGrupoECategoriaAsync(int grupoId, int categoriaId)
    {
        return await DbSet
            .Where(gs => gs.GrupoId == grupoId && gs.CategoriaId == categoriaId)
            .FirstOrDefaultAsync();
    }
    
    /// <summary>
    /// Obtém todos os descontos de uma categoria
    /// </summary>
    /// <param name="categoriaId">ID da categoria</param>
    /// <returns>Lista de descontos</returns>
    public async Task<IEnumerable<GrupoSegmentacao>> ObterPorCategoriaAsync(int categoriaId)
    {
        return await DbSet
            .Include(gs => gs.Grupo)
                .ThenInclude(g => g.Segmentacao)
            .Where(gs => gs.CategoriaId == categoriaId)
            .OrderBy(gs => gs.Grupo.Segmentacao.Nome)
            .ThenBy(gs => gs.Grupo.Nome)
            .ToListAsync();
    }
    
    /// <summary>
    /// Verifica se já existe desconto para a combinação grupo/categoria
    /// </summary>
    /// <param name="grupoId">ID do grupo</param>
    /// <param name="categoriaId">ID da categoria</param>
    /// <param name="excluirId">ID do desconto a excluir da verificação</param>
    /// <returns>True se já existe</returns>
    public async Task<bool> ExisteAsync(int grupoId, int categoriaId, int? excluirId = null)
    {
        var query = DbSet.Where(gs => gs.GrupoId == grupoId && gs.CategoriaId == categoriaId);
        
        if (excluirId.HasValue)
            query = query.Where(gs => gs.Id != excluirId.Value);
            
        return await query.AnyAsync();
    }
    
    /// <summary>
    /// Sobrescreve o método base para incluir validações específicas
    /// </summary>
    public override async Task<GrupoSegmentacao> AdicionarAsync(GrupoSegmentacao entidade, CancellationToken cancellationToken = default)
    {
        // Validar se já existe desconto para esta combinação
        var jaExiste = await ExisteAsync(entidade.GrupoId, entidade.CategoriaId);
        
        if (jaExiste)
        {
            throw new InvalidOperationException(
                $"Já existe um desconto configurado para o grupo {entidade.GrupoId} e categoria {entidade.CategoriaId}.");
        }
        
        return await base.AdicionarAsync(entidade, cancellationToken);
    }
    
    /// <summary>
    /// Sobrescreve o método base para incluir validações específicas
    /// </summary>
    public override async Task AtualizarAsync(GrupoSegmentacao entidade, CancellationToken cancellationToken = default)
    {
        // Validar se já existe desconto para esta combinação (excluindo o atual)
        var jaExiste = await ExisteAsync(entidade.GrupoId, entidade.CategoriaId, entidade.Id);
        
        if (jaExiste)
        {
            throw new InvalidOperationException(
                $"Já existe um desconto configurado para o grupo {entidade.GrupoId} e categoria {entidade.CategoriaId}.");
        }
        
        await base.AtualizarAsync(entidade, cancellationToken);
    }
}