using Microsoft.EntityFrameworkCore;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Pagamentos.Dominio.Entidades;
using Agriis.Pagamentos.Dominio.Interfaces;

namespace Agriis.Pagamentos.Infraestrutura.Repositorios;

/// <summary>
/// Repositório para formas de pagamento
/// </summary>
public class FormaPagamentoRepository : RepositoryBase<FormaPagamento, DbContext>, IFormaPagamentoRepository
{
    public FormaPagamentoRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<FormaPagamento>> ObterAtivasAsync()
    {
        return await DbSet
            .Where(x => x.Ativo)
            .OrderBy(x => x.Descricao)
            .ToListAsync();
    }

    public async Task<IEnumerable<FormaPagamento>> ObterPorPedidoIdAsync(int pedidoId)
    {
        // Esta consulta replica a lógica do Python que busca formas de pagamento
        // baseadas nos itens do pedido e suas culturas/fornecedores
        var query = @"
            SELECT DISTINCT fp.id, fp.descricao, fp.ativo, fp.data_criacao, fp.data_atualizacao
            FROM forma_pagamento fp
            INNER JOIN cultura_forma_pagamento cfp ON cfp.forma_pagamento_id = fp.id
            INNER JOIN catalogo c ON c.cultura_id = cfp.cultura_id
            INNER JOIN ponto_distribuicao pd ON pd.id = c.ponto_distribuicao_id
            INNER JOIN catalogo_item ci ON ci.catalogo_id = c.id
            INNER JOIN pedido_item pi ON pi.catalogo_item_id = ci.id
            WHERE pi.pedido_id = {0}
              AND cfp.fornecedor_id = pd.fornecedor_id
              AND cfp.ativo = true
              AND fp.ativo = true
            ORDER BY fp.descricao";

        return await DbSet
            .FromSqlRaw(query, pedidoId)
            .ToListAsync();
    }

    public async Task<bool> ExisteAtivaAsync(int id)
    {
        return await DbSet
            .AnyAsync(x => x.Id == id && x.Ativo);
    }
}