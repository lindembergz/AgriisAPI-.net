using Agriis.Catalogos.Aplicacao.DTOs;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.Enums;

namespace Agriis.Catalogos.Aplicacao.Interfaces;

public interface ICatalogoService
{
    Task<Result<CatalogoDto>> ObterPorIdAsync(int id);
    Task<Result<CatalogoDto>> CriarAsync(CriarCatalogoDto dto);
    Task<Result<CatalogoDto>> AtualizarAsync(int id, AtualizarCatalogoDto dto);
    Task<Result> RemoverAsync(int id);
    Task<Result<PagedResult<CatalogoDto>>> ObterPaginadoAsync(int pagina, int tamanhoPagina, 
        int? safraId = null, int? pontoDistribuicaoId = null, int? culturaId = null, 
        int? categoriaId = null, Moeda? moeda = null, bool? ativo = null);
    Task<Result<IEnumerable<CatalogoDto>>> ObterVigentesAsync(DateTime data);
    Task<Result<CatalogoDto>> ObterPorChaveUnicaAsync(int safraId, int pontoDistribuicaoId, int culturaId, int categoriaId);
    
    // Métodos para itens do catálogo
    Task<Result<CatalogoItemDto>> AdicionarItemAsync(int catalogoId, CriarCatalogoItemDto dto);
    Task<Result<CatalogoItemDto>> AtualizarItemAsync(int catalogoId, int itemId, AtualizarCatalogoItemDto dto);
    Task<Result> RemoverItemAsync(int catalogoId, int itemId);
    Task<Result<decimal?>> ConsultarPrecoAsync(int catalogoId, int produtoId, ConsultarPrecoDto dto);
}