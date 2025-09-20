using AutoMapper;
using Agriis.Catalogos.Aplicacao.DTOs;
using Agriis.Catalogos.Aplicacao.Interfaces;
using Agriis.Catalogos.Dominio.Entidades;
using Agriis.Catalogos.Dominio.Interfaces;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Compartilhado.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.Catalogos.Aplicacao.Servicos;

public class CatalogoService : ICatalogoService
{
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly ICatalogoItemRepository _catalogoItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CatalogoService> _logger;

    public CatalogoService(
        ICatalogoRepository catalogoRepository,
        ICatalogoItemRepository catalogoItemRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CatalogoService> logger)
    {
        _catalogoRepository = catalogoRepository;
        _catalogoItemRepository = catalogoItemRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CatalogoDto>> ObterPorIdAsync(int id)
    {
        try
        {
            var catalogo = await _catalogoRepository.ObterPorIdAsync(id);
            if (catalogo == null)
                return Result<CatalogoDto>.Failure("Catálogo não encontrado");

            var dto = _mapper.Map<CatalogoDto>(catalogo);
            return Result<CatalogoDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter catálogo {Id}", id);
            return Result<CatalogoDto>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<CatalogoDto>> CriarAsync(CriarCatalogoDto dto)
    {
        try
        {
            // Validar unicidade
            var existe = await _catalogoRepository.ExisteChaveUnicaAsync(
                dto.SafraId, dto.PontoDistribuicaoId, dto.CulturaId, dto.CategoriaId);
            
            if (existe)
                return Result<CatalogoDto>.Failure("Já existe um catálogo para esta combinação de safra, ponto de distribuição, cultura e categoria");

            var catalogo = new Catalogo(
                dto.SafraId,
                dto.PontoDistribuicaoId,
                dto.CulturaId,
                dto.CategoriaId,
                dto.Moeda,
                dto.DataInicio,
                dto.DataFim);

            catalogo = await _catalogoRepository.AdicionarAsync(catalogo);
            await _unitOfWork.SalvarAlteracoesAsync();

            var catalogoDto = _mapper.Map<CatalogoDto>(catalogo);
            return Result<CatalogoDto>.Success(catalogoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar catálogo");
            return Result<CatalogoDto>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<CatalogoDto>> AtualizarAsync(int id, AtualizarCatalogoDto dto)
    {
        try
        {
            var catalogo = await _catalogoRepository.ObterPorIdAsync(id);
            if (catalogo == null)
                return Result<CatalogoDto>.Failure("Catálogo não encontrado");

            // Usar reflection para atualizar propriedades
            var propriedades = typeof(AtualizarCatalogoDto).GetProperties();
            foreach (var prop in propriedades)
            {
                var valor = prop.GetValue(dto);
                var propCatalogo = typeof(Catalogo).GetProperty(prop.Name);
                if (propCatalogo != null && propCatalogo.CanWrite)
                {
                    propCatalogo.SetValue(catalogo, valor);
                }
            }

            catalogo.AtualizarDataModificacao();
            await _catalogoRepository.AtualizarAsync(catalogo);
            await _unitOfWork.SalvarAlteracoesAsync();

            var catalogoDto = _mapper.Map<CatalogoDto>(catalogo);
            return Result<CatalogoDto>.Success(catalogoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar catálogo {Id}", id);
            return Result<CatalogoDto>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result> RemoverAsync(int id)
    {
        try
        {
            var catalogo = await _catalogoRepository.ObterPorIdAsync(id);
            if (catalogo == null)
                return Result.Failure("Catálogo não encontrado");

            await _catalogoRepository.RemoverAsync(id);
            await _unitOfWork.SalvarAlteracoesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover catálogo {Id}", id);
            return Result.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<PagedResult<CatalogoDto>>> ObterPaginadoAsync(int pagina, int tamanhoPagina, 
        int? safraId = null, int? pontoDistribuicaoId = null, int? culturaId = null, 
        int? categoriaId = null, Moeda? moeda = null, bool? ativo = null)
    {
        try
        {
            var resultado = await _catalogoRepository.ObterPaginadoAsync(
                pagina, tamanhoPagina, safraId, pontoDistribuicaoId, culturaId, categoriaId, moeda, ativo);

            var dtos = _mapper.Map<IEnumerable<CatalogoDto>>(resultado.Items);
            var pagedResult = new PagedResult<CatalogoDto>(dtos, resultado.TotalCount, pagina, tamanhoPagina);

            return Result<PagedResult<CatalogoDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter catálogos paginados");
            return Result<PagedResult<CatalogoDto>>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<IEnumerable<CatalogoDto>>> ObterVigentesAsync(DateTime data)
    {
        try
        {
            var catalogos = await _catalogoRepository.ObterVigentesAsync(data);
            var dtos = _mapper.Map<IEnumerable<CatalogoDto>>(catalogos);
            return Result<IEnumerable<CatalogoDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter catálogos vigentes para data {Data}", data);
            return Result<IEnumerable<CatalogoDto>>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<CatalogoDto>> ObterPorChaveUnicaAsync(int safraId, int pontoDistribuicaoId, int culturaId, int categoriaId)
    {
        try
        {
            var catalogo = await _catalogoRepository.ObterPorChaveUnicaAsync(safraId, pontoDistribuicaoId, culturaId, categoriaId);
            if (catalogo == null)
                return Result<CatalogoDto>.Failure("Catálogo não encontrado");

            var dto = _mapper.Map<CatalogoDto>(catalogo);
            return Result<CatalogoDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter catálogo por chave única");
            return Result<CatalogoDto>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<CatalogoItemDto>> AdicionarItemAsync(int catalogoId, CriarCatalogoItemDto dto)
    {
        try
        {
            var catalogo = await _catalogoRepository.ObterPorIdAsync(catalogoId);
            if (catalogo == null)
                return Result<CatalogoItemDto>.Failure("Catálogo não encontrado");

            var item = new CatalogoItem(catalogoId, dto.ProdutoId, dto.EstruturaPrecosJson, dto.PrecoBase);
            catalogo.AdicionarItem(item);

            await _catalogoRepository.AtualizarAsync(catalogo);
            await _unitOfWork.SalvarAlteracoesAsync();

            var itemDto = _mapper.Map<CatalogoItemDto>(item);
            return Result<CatalogoItemDto>.Success(itemDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar item ao catálogo {CatalogoId}", catalogoId);
            return Result<CatalogoItemDto>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<CatalogoItemDto>> AtualizarItemAsync(int catalogoId, int itemId, AtualizarCatalogoItemDto dto)
    {
        try
        {
            var item = await _catalogoItemRepository.ObterPorIdAsync(itemId);
            if (item == null || item.CatalogoId != catalogoId)
                return Result<CatalogoItemDto>.Failure("Item não encontrado");

            item.AtualizarPrecos(dto.EstruturaPrecosJson, dto.PrecoBase);
            if (!dto.Ativo)
                item.Desativar();
            else
                item.Ativar();

            await _catalogoItemRepository.AtualizarAsync(item);
            await _unitOfWork.SalvarAlteracoesAsync();

            var itemDto = _mapper.Map<CatalogoItemDto>(item);
            return Result<CatalogoItemDto>.Success(itemDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar item {ItemId} do catálogo {CatalogoId}", itemId, catalogoId);
            return Result<CatalogoItemDto>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result> RemoverItemAsync(int catalogoId, int itemId)
    {
        try
        {
            var catalogo = await _catalogoRepository.ObterPorIdAsync(catalogoId);
            if (catalogo == null)
                return Result.Failure("Catálogo não encontrado");

            var item = await _catalogoItemRepository.ObterPorIdAsync(itemId);
            if (item == null || item.CatalogoId != catalogoId)
                return Result.Failure("Item não encontrado");

            catalogo.RemoverItem(item.ProdutoId);
            await _catalogoRepository.AtualizarAsync(catalogo);
            await _unitOfWork.SalvarAlteracoesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover item {ItemId} do catálogo {CatalogoId}", itemId, catalogoId);
            return Result.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<decimal?>> ConsultarPrecoAsync(int catalogoId, int produtoId, ConsultarPrecoDto dto)
    {
        try
        {
            var item = await _catalogoItemRepository.ObterPorCatalogoEProdutoAsync(catalogoId, produtoId);
            if (item == null)
                return Result<decimal?>.Failure("Item não encontrado no catálogo");

            var preco = item.ObterPrecoPorEstadoEData(dto.Uf, dto.Data);
            return Result<decimal?>.Success(preco);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar preço do produto {ProdutoId} no catálogo {CatalogoId}", produtoId, catalogoId);
            return Result<decimal?>.Failure("Erro interno do servidor");
        }
    }
}