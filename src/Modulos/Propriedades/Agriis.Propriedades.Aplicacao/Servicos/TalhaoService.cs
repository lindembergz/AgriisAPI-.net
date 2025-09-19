using AutoMapper;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Propriedades.Aplicacao.DTOs;
using Agriis.Propriedades.Aplicacao.Interfaces;
using Agriis.Propriedades.Dominio.Entidades;
using Agriis.Propriedades.Dominio.Interfaces;
using Agriis.Propriedades.Dominio.Servicos;
using NetTopologySuite.Geometries;

namespace Agriis.Propriedades.Aplicacao.Servicos;

public class TalhaoService : ITalhaoService
{
    private readonly ITalhaoRepository _talhaoRepository;
    private readonly PropriedadeDomainService _domainService;
    private readonly IMapper _mapper;

    public TalhaoService(
        ITalhaoRepository talhaoRepository,
        PropriedadeDomainService domainService,
        IMapper mapper)
    {
        _talhaoRepository = talhaoRepository;
        _domainService = domainService;
        _mapper = mapper;
    }

    public async Task<Result<TalhaoDto>> ObterPorIdAsync(int id)
    {
        try
        {
            var talhao = await _talhaoRepository.ObterPorIdAsync(id);
            if (talhao == null)
                return Result<TalhaoDto>.Failure("Talhão não encontrado");

            var dto = _mapper.Map<TalhaoDto>(talhao);
            return Result<TalhaoDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<TalhaoDto>.Failure($"Erro ao obter talhão: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TalhaoDto>>> ObterPorPropriedadeAsync(int propriedadeId)
    {
        try
        {
            var talhoes = await _talhaoRepository.ObterPorPropriedadeAsync(propriedadeId);
            var dtos = _mapper.Map<IEnumerable<TalhaoDto>>(talhoes);
            return Result<IEnumerable<TalhaoDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TalhaoDto>>.Failure($"Erro ao obter talhões: {ex.Message}");
        }
    }

    public async Task<Result<TalhaoDto>> CriarAsync(TalhaoCreateDto dto)
    {
        try
        {
            // Validar se a área do talhão não excede a área da propriedade
            var areaValida = await _domainService.ValidarAreaTalhaoAsync(dto.PropriedadeId, new AreaPlantio(dto.Area));
            if (!areaValida)
                return Result<TalhaoDto>.Failure("Área do talhão excede a área disponível da propriedade");

            var talhao = new Talhao(dto.Nome, new AreaPlantio(dto.Area), dto.PropriedadeId, dto.Descricao);
            
            if (dto.Latitude.HasValue && dto.Longitude.HasValue)
            {
                talhao.DefinirLocalizacao(dto.Latitude.Value, dto.Longitude.Value);
            }
            var talhaoCriado = await _talhaoRepository.AdicionarAsync(talhao);
            
            var resultado = _mapper.Map<TalhaoDto>(talhaoCriado);
            return Result<TalhaoDto>.Success(resultado);
        }
        catch (Exception ex)
        {
            return Result<TalhaoDto>.Failure($"Erro ao criar talhão: {ex.Message}");
        }
    }

    public async Task<Result<TalhaoDto>> AtualizarAsync(int id, TalhaoUpdateDto dto)
    {
        try
        {
            var talhao = await _talhaoRepository.ObterPorIdAsync(id);
            if (talhao == null)
                return Result<TalhaoDto>.Failure("Talhão não encontrado");

            // Validar se a nova área não excede a área da propriedade
            var areaValida = await _domainService.ValidarAreaTalhaoAsync(talhao.PropriedadeId, new AreaPlantio(dto.Area));
            if (!areaValida)
                return Result<TalhaoDto>.Failure("Nova área do talhão excede a área disponível da propriedade");

            talhao.AtualizarDados(dto.Nome, new AreaPlantio(dto.Area), dto.Descricao);

            if (dto.Latitude.HasValue && dto.Longitude.HasValue)
            {
                talhao.DefinirLocalizacao(dto.Latitude.Value, dto.Longitude.Value);
            }

            await _talhaoRepository.AtualizarAsync(talhao);
            
            var resultado = _mapper.Map<TalhaoDto>(talhao);
            return Result<TalhaoDto>.Success(resultado);
        }
        catch (Exception ex)
        {
            return Result<TalhaoDto>.Failure($"Erro ao atualizar talhão: {ex.Message}");
        }
    }

    public async Task<Result> RemoverAsync(int id)
    {
        try
        {
            await _talhaoRepository.RemoverAsync(id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Erro ao remover talhão: {ex.Message}");
        }
    }

    public async Task<Result<decimal>> CalcularAreaTotalPorPropriedadeAsync(int propriedadeId)
    {
        try
        {
            var areaTotal = await _talhaoRepository.CalcularAreaTotalPorPropriedadeAsync(propriedadeId);
            return Result<decimal>.Success(areaTotal);
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure($"Erro ao calcular área total: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TalhaoDto>>> BuscarTalhoesProximosAsync(
        double latitude, double longitude, double raioKm)
    {
        try
        {
            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            var ponto = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
            
            var talhoes = await _domainService.BuscarTalhoesProximosAsync(ponto, raioKm);
            var dtos = _mapper.Map<IEnumerable<TalhaoDto>>(talhoes);
            
            return Result<IEnumerable<TalhaoDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TalhaoDto>>.Failure($"Erro ao buscar talhões próximos: {ex.Message}");
        }
    }
}