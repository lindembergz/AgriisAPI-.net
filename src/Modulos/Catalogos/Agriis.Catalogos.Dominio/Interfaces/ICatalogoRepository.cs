using Agriis.Catalogos.Dominio.Entidades;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Compartilhado.Dominio.Interfaces;

namespace Agriis.Catalogos.Dominio.Interfaces;

public interface ICatalogoRepository : IRepository<Catalogo>
{
    Task<Catalogo?> ObterPorChaveUnicaAsync(int safraId, int pontoDistribuicaoId, int culturaId, int categoriaId);
    Task<IEnumerable<Catalogo>> ObterPorSafraAsync(int safraId);
    Task<IEnumerable<Catalogo>> ObterPorPontoDistribuicaoAsync(int pontoDistribuicaoId);
    Task<IEnumerable<Catalogo>> ObterPorCulturaAsync(int culturaId);
    Task<IEnumerable<Catalogo>> ObterVigentesAsync(DateTime data);
    Task<PagedResult<Catalogo>> ObterPaginadoAsync(int pagina, int tamanhoPagina, 
        int? safraId = null, int? pontoDistribuicaoId = null, int? culturaId = null, 
        int? categoriaId = null, Moeda? moeda = null, bool? ativo = null);
    Task<bool> ExisteChaveUnicaAsync(int safraId, int pontoDistribuicaoId, int culturaId, int categoriaId, int? catalogoIdExcluir = null);
}