using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Segmentacoes.Dominio.Entidades;
using Agriis.Segmentacoes.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Segmentacoes.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de segmentações
/// </summary>
public class SegmentacaoRepository : RepositoryBase<Segmentacao>, ISegmentacaoRepository
{
    public SegmentacaoRepository(DbContext context) : base(context)
    {
    }
    
    /// <summary>
    /// Obtém todas as segmentações de um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Lista de segmentações</returns>
    public async Task<IEnumerable<Segmentacao>> ObterPorFornecedorAsync(int fornecedorId)
    {
        return await _dbSet
            .Where(s => s.FornecedorId == fornecedorId)
            .OrderBy(s => s.Nome)
            .ToListAsync();
    }
    
    /// <summary>
    /// Obtém a segmentação padrão de um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Segmentação padrão ou null</returns>
    public async Task<Segmentacao?> ObterPadraoAsync(int fornecedorId)
    {
        return await _dbSet
            .Where(s => s.FornecedorId == fornecedorId && s.EhPadrao && s.Ativo)
            .FirstOrDefaultAsync();
    }
    
    /// <summary>
    /// Obtém segmentações ativas de um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Lista de segmentações ativas</returns>
    public async Task<IEnumerable<Segmentacao>> ObterAtivasPorFornecedorAsync(int fornecedorId)
    {
        return await _dbSet
            .Where(s => s.FornecedorId == fornecedorId && s.Ativo)
            .OrderByDescending(s => s.EhPadrao)
            .ThenBy(s => s.Nome)
            .ToListAsync();
    }
    
    /// <summary>
    /// Verifica se existe uma segmentação padrão para o fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="excluirId">ID da segmentação a excluir da verificação</param>
    /// <returns>True se existe segmentação padrão</returns>
    public async Task<bool> ExistePadraoAsync(int fornecedorId, int? excluirId = null)
    {
        var query = _dbSet.Where(s => s.FornecedorId == fornecedorId && s.EhPadrao && s.Ativo);
        
        if (excluirId.HasValue)
            query = query.Where(s => s.Id != excluirId.Value);
            
        return await query.AnyAsync();
    }
    
    /// <summary>
    /// Obtém segmentação com grupos e descontos carregados
    /// </summary>
    /// <param name="id">ID da segmentação</param>
    /// <returns>Segmentação completa</returns>
    public async Task<Segmentacao?> ObterCompletaAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Grupos)
                .ThenInclude(g => g.GruposSegmentacao)
            .Where(s => s.Id == id)
            .FirstOrDefaultAsync();
    }
    
    /// <summary>
    /// Sobrescreve o método base para incluir validações específicas
    /// </summary>
    public override async Task<Segmentacao> AdicionarAsync(Segmentacao entidade)
    {
        // Se está marcando como padrão, desmarcar outras
        if (entidade.EhPadrao)
        {
            var segmentacoesPadrao = await _dbSet
                .Where(s => s.FornecedorId == entidade.FornecedorId && s.EhPadrao && s.Ativo)
                .ToListAsync();
                
            foreach (var segmentacao in segmentacoesPadrao)
            {
                segmentacao.RemoverComoPadrao();
            }
        }
        
        return await base.AdicionarAsync(entidade);
    }
    
    /// <summary>
    /// Sobrescreve o método base para incluir validações específicas
    /// </summary>
    public override async Task AtualizarAsync(Segmentacao entidade)
    {
        // Se está marcando como padrão, desmarcar outras
        if (entidade.EhPadrao)
        {
            var segmentacoesPadrao = await _dbSet
                .Where(s => s.FornecedorId == entidade.FornecedorId && s.EhPadrao && s.Ativo && s.Id != entidade.Id)
                .ToListAsync();
                
            foreach (var segmentacao in segmentacoesPadrao)
            {
                segmentacao.RemoverComoPadrao();
            }
        }
        
        await base.AtualizarAsync(entidade);
    }
}