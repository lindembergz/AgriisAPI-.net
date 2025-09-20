using Agriis.Catalogos.Dominio.Entidades;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.Interfaces;

namespace Agriis.Catalogos.Dominio.Interfaces;

public interface ICatalogoItemRepository : IRepository<CatalogoItem>
{
    Task<IEnumerable<CatalogoItem>> ObterPorCatalogoAsync(int catalogoId);
    Task<CatalogoItem?> ObterPorCatalogoEProdutoAsync(int catalogoId, int produtoId);
    Task<IEnumerable<CatalogoItem>> ObterPorProdutoAsync(int produtoId);
    Task<PagedResult<CatalogoItem>> ObterPaginadoAsync(int pagina, int tamanhoPagina, 
        int? catalogoId = null, int? produtoId = null, bool? ativo = null);
}