using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Segmentacoes.Dominio.Entidades;
using Agriis.Segmentacoes.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Segmentacoes.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de grupos de segmentação
/// </summary>
public class GrupoRepository : RepositoryBase<Grupo>, IGrupoRepository
{
    public GrupoRepository(DbContext context) : base(context)
    {
    }
    
    /// <summary>
    /// Obtém todos os grupos de uma segmentação
    /// </summary>
    /// <param name="segmentacaoId">ID da segmentação</param>
    /// <returns>Lista de grupos</returns>
    public async Task<IEnumerable<Grupo>> ObterPorSegmentacaoAsync(int segmentacaoId)
    {
        return await _dbSet
            .Where(g => g.SegmentacaoId == segmentacaoId)
            .OrderBy(g => g.AreaMinima)
            .ToListAsync();
    }
    
    /// <summary>
    /// Obtém grupos ativos de uma segmentação
    /// </summary>
    /// <param name="segmentacaoId">ID da segmentação</param>
    /// <returns>Lista de grupos ativos</returns>
    public async Task<IEnumerable<Grupo>> ObterAtivosPorSegmentacaoAsync(int segmentacaoId)
    {
        return await _dbSet
            .Where(g => g.SegmentacaoId == segmentacaoId && g.Ativo)
            .OrderBy(g => g.AreaMinima)
            .ToListAsync();
    }
    
    /// <summary>
    /// Obtém grupo que se enquadra para uma área específica
    /// </summary>
    /// <param name="segmentacaoId">ID da segmentação</param>
    /// <param name="area">Área em hectares</param>
    /// <returns>Grupo que se enquadra ou null</returns>
    public async Task<Grupo?> ObterPorAreaAsync(int segmentacaoId, decimal area)
    {
        return await _dbSet
            .Where(g => g.SegmentacaoId == segmentacaoId && 
                       g.Ativo && 
                       g.AreaMinima <= area && 
                       (g.AreaMaxima == null || g.AreaMaxima >= area))
            .OrderBy(g => g.AreaMinima)
            .FirstOrDefaultAsync();
    }
    
    /// <summary>
    /// Obtém grupo com descontos carregados
    /// </summary>
    /// <param name="id">ID do grupo</param>
    /// <returns>Grupo completo</returns>
    public async Task<Grupo?> ObterComDescontosAsync(int id)
    {
        return await _dbSet
            .Include(g => g.GruposSegmentacao)
            .Where(g => g.Id == id)
            .FirstOrDefaultAsync();
    }
    
    /// <summary>
    /// Verifica se existe sobreposição de faixas de área
    /// </summary>
    /// <param name="segmentacaoId">ID da segmentação</param>
    /// <param name="areaMinima">Área mínima</param>
    /// <param name="areaMaxima">Área máxima</param>
    /// <param name="excluirId">ID do grupo a excluir da verificação</param>
    /// <returns>True se existe sobreposição</returns>
    public async Task<bool> ExisteSobreposicaoAsync(int segmentacaoId, decimal areaMinima, decimal? areaMaxima, int? excluirId = null)
    {
        var query = _dbSet.Where(g => g.SegmentacaoId == segmentacaoId && g.Ativo);
        
        if (excluirId.HasValue)
            query = query.Where(g => g.Id != excluirId.Value);
        
        // Verifica sobreposição:
        // 1. Área mínima do novo grupo está dentro de um grupo existente
        // 2. Área máxima do novo grupo está dentro de um grupo existente
        // 3. Novo grupo engloba completamente um grupo existente
        var sobreposicao = await query.AnyAsync(g =>
            // Caso 1: Nova área mínima está dentro de grupo existente
            (g.AreaMinima <= areaMinima && (g.AreaMaxima == null || g.AreaMaxima >= areaMinima)) ||
            // Caso 2: Nova área máxima está dentro de grupo existente (se definida)
            (areaMaxima.HasValue && g.AreaMinima <= areaMaxima && (g.AreaMaxima == null || g.AreaMaxima >= areaMaxima)) ||
            // Caso 3: Novo grupo engloba grupo existente
            (areaMinima <= g.AreaMinima && (areaMaxima == null || (g.AreaMaxima.HasValue && areaMaxima >= g.AreaMaxima)))
        );
        
        return sobreposicao;
    }
    
    /// <summary>
    /// Sobrescreve o método base para incluir validações específicas
    /// </summary>
    public override async Task<Grupo> AdicionarAsync(Grupo entidade)
    {
        // Validar sobreposição de faixas
        var existeSobreposicao = await ExisteSobreposicaoAsync(
            entidade.SegmentacaoId, 
            entidade.AreaMinima, 
            entidade.AreaMaxima);
            
        if (existeSobreposicao)
        {
            throw new InvalidOperationException(
                $"Existe sobreposição de faixas de área para o grupo '{entidade.Nome}'. " +
                $"Área: {entidade.AreaMinima} - {entidade.AreaMaxima?.ToString() ?? "∞"} hectares.");
        }
        
        return await base.AdicionarAsync(entidade);
    }
    
    /// <summary>
    /// Sobrescreve o método base para incluir validações específicas
    /// </summary>
    public override async Task AtualizarAsync(Grupo entidade)
    {
        // Validar sobreposição de faixas
        var existeSobreposicao = await ExisteSobreposicaoAsync(
            entidade.SegmentacaoId, 
            entidade.AreaMinima, 
            entidade.AreaMaxima, 
            entidade.Id);
            
        if (existeSobreposicao)
        {
            throw new InvalidOperationException(
                $"Existe sobreposição de faixas de área para o grupo '{entidade.Nome}'. " +
                $"Área: {entidade.AreaMinima} - {entidade.AreaMaxima?.ToString() ?? "∞"} hectares.");
        }
        
        await base.AtualizarAsync(entidade);
    }
}