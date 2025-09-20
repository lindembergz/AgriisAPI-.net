using Microsoft.EntityFrameworkCore;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Pagamentos.Dominio.Entidades;
using Agriis.Pagamentos.Dominio.Interfaces;

namespace Agriis.Pagamentos.Infraestrutura.Repositorios;

/// <summary>
/// Repositório para associações cultura-fornecedor-forma de pagamento
/// </summary>
public class CulturaFormaPagamentoRepository : RepositoryBase<CulturaFormaPagamento, DbContext>, ICulturaFormaPagamentoRepository
{
    public CulturaFormaPagamentoRepository(DbContext context) : base(context)
    {
    }

    public async Task<CulturaFormaPagamento?> ObterPorFornecedorCulturaFormaPagamentoAsync(
        int fornecedorId, 
        int culturaId, 
        int formaPagamentoId)
    {
        return await DbSet
            .Include(x => x.FormaPagamento)
            .FirstOrDefaultAsync(x => 
                x.FornecedorId == fornecedorId && 
                x.CulturaId == culturaId && 
                x.FormaPagamentoId == formaPagamentoId);
    }

    public async Task<IEnumerable<CulturaFormaPagamento>> ObterPorFornecedorAsync(int fornecedorId)
    {
        return await DbSet
            .Include(x => x.FormaPagamento)
            .Where(x => x.FornecedorId == fornecedorId && x.Ativo)
            .OrderBy(x => x.FormaPagamento.Descricao)
            .ToListAsync();
    }

    public async Task<IEnumerable<CulturaFormaPagamento>> ObterPorCulturaAsync(int culturaId)
    {
        return await DbSet
            .Include(x => x.FormaPagamento)
            .Where(x => x.CulturaId == culturaId && x.Ativo)
            .OrderBy(x => x.FormaPagamento.Descricao)
            .ToListAsync();
    }

    public async Task<IEnumerable<FormaPagamento>> ObterFormasPagamentoPorFornecedorCulturaAsync(
        int fornecedorId, 
        int culturaId)
    {
        return await DbSet
            .Include(x => x.FormaPagamento)
            .Where(x => x.FornecedorId == fornecedorId && 
                       x.CulturaId == culturaId && 
                       x.Ativo && 
                       x.FormaPagamento.Ativo)
            .Select(x => x.FormaPagamento)
            .OrderBy(x => x.Descricao)
            .ToListAsync();
    }

    public async Task<bool> ExisteAssociacaoAtivaAsync(int fornecedorId, int culturaId, int formaPagamentoId)
    {
        return await DbSet
            .AnyAsync(x => x.FornecedorId == fornecedorId && 
                          x.CulturaId == culturaId && 
                          x.FormaPagamentoId == formaPagamentoId && 
                          x.Ativo);
    }
}