using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Pedidos.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de propostas
/// </summary>
public class PropostaRepository : RepositoryBase<Proposta, DbContext>, IPropostaRepository
{
    /// <summary>
    /// Construtor do repositório de propostas
    /// </summary>
    /// <param name="context">Contexto do banco de dados</param>
    public PropostaRepository(DbContext context) : base(context)
    {
    }
    
    /// <summary>
    /// Obtém a última proposta de um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Última proposta ou null se não existir</returns>
    public async Task<Proposta?> ObterUltimaPorPedidoAsync(int pedidoId)
    {
        return await DbSet
            .Where(p => p.PedidoId == pedidoId)
            .OrderByDescending(p => p.DataCriacao)
            .FirstOrDefaultAsync();
    }
    
    /// <summary>
    /// Lista todas as propostas de um pedido com paginação
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="pagina">Número da página</param>
    /// <param name="tamanhoPagina">Tamanho da página</param>
    /// <param name="ordenacao">Campo de ordenação</param>
    /// <returns>Lista paginada de propostas</returns>
    public async Task<PagedResult<Proposta>> ListarPorPedidoAsync(int pedidoId, int pagina, int tamanhoPagina, string? ordenacao = null)
    {
        var query = DbSet.Where(p => p.PedidoId == pedidoId);
        
        // Aplicar ordenação
        query = ordenacao?.ToLower() switch
        {
            "datacriacao" => query.OrderBy(p => p.DataCriacao),
            "datacriacao desc" => query.OrderByDescending(p => p.DataCriacao),
            _ => query.OrderByDescending(p => p.DataCriacao) // Padrão: mais recente primeiro
        };
        
        var totalItens = await query.CountAsync();
        var itens = await query
            .Skip(pagina * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();
        
        return new PagedResult<Proposta>(itens, pagina + 1, tamanhoPagina, totalItens);
    }
    
    /// <summary>
    /// Verifica se existe alguma proposta para o pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>True se existe proposta</returns>
    public async Task<bool> ExistePropostaPorPedidoAsync(int pedidoId)
    {
        return await DbSet.AnyAsync(p => p.PedidoId == pedidoId);
    }
}