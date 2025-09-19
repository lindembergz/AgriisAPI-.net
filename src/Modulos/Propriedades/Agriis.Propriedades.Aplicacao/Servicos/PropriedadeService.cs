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

public class PropriedadeService : IPropriedadeService
{
    private readonly IPropriedadeRepository _propriedadeRepository;
    private readonly PropriedadeDomainService _domainService;
    private readonly IMapper _mapper;

    public PropriedadeService(
        IPropriedadeRepository propriedadeRepository,
        PropriedadeDomainService domainService,
        IMapper mapper)
    {
        _propriedadeRepository = propriedadeRepository;
        _domainService = domainService;
        _mapper = mapper;
    }

    public async Task<Result<PropriedadeDto>> ObterPorIdAsync(int id)
    {
        try
        {
            var propriedade = await _propriedadeRepository.ObterPorIdAsync(id);
            if (propriedade == null)
                return Result<PropriedadeDto>.Failure("Propriedade não encontrada");

            var dto = _mapper.Map<PropriedadeDto>(propriedade);
            return Result<PropriedadeDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<PropriedadeDto>.Failure($"Erro ao obter propriedade: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PropriedadeDto>>> ObterPorProdutorAsync(int produtorId)
    {
        try
        {
            var propriedades = await _propriedadeRepository.ObterPorProdutorAsync(produtorId);
            var dtos = _mapper.Map<IEnumerable<PropriedadeDto>>(propriedades);
            return Result<IEnumerable<PropriedadeDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<PropriedadeDto>>.Failure($"Erro ao obter propriedades: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PropriedadeDto>>> ObterPorCulturaAsync(int culturaId)
    {
        try
        {
            var propriedades = await _propriedadeRepository.ObterPorCulturaAsync(culturaId);
            var dtos = _mapper.Map<IEnumerable<PropriedadeDto>>(propriedades);
            return Result<IEnumerable<PropriedadeDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<PropriedadeDto>>.Failure($"Erro ao obter propriedades: {ex.Message}");
        }
    }

    public async Task<Result<PropriedadeDto>> CriarAsync(PropriedadeCreateDto dto)
    {
        try
        {
            var propriedade = new Propriedade(
                dto.Nome,
                dto.ProdutorId,
                new AreaPlantio(dto.AreaTotal),
                dto.Nirf,
                dto.InscricaoEstadual,
                dto.EnderecoId);

            if (dto.DadosAdicionais != null)
            {
                propriedade.DefinirDadosAdicionais(dto.DadosAdicionais);
            }

            var propriedadeCriada = await _propriedadeRepository.AdicionarAsync(propriedade);
            
            var resultado = _mapper.Map<PropriedadeDto>(propriedadeCriada);
            return Result<PropriedadeDto>.Success(resultado);
        }
        catch (Exception ex)
        {
            return Result<PropriedadeDto>.Failure($"Erro ao criar propriedade: {ex.Message}");
        }
    }

    public async Task<Result<PropriedadeDto>> AtualizarAsync(int id, PropriedadeUpdateDto dto)
    {
        try
        {
            var propriedade = await _propriedadeRepository.ObterPorIdAsync(id);
            if (propriedade == null)
                return Result<PropriedadeDto>.Failure("Propriedade não encontrada");

            propriedade.AtualizarDados(
                dto.Nome,
                new AreaPlantio(dto.AreaTotal),
                dto.Nirf,
                dto.InscricaoEstadual,
                dto.EnderecoId);

            if (dto.DadosAdicionais != null)
            {
                propriedade.DefinirDadosAdicionais(dto.DadosAdicionais);
            }

            await _propriedadeRepository.AtualizarAsync(propriedade);
            
            var resultado = _mapper.Map<PropriedadeDto>(propriedade);
            return Result<PropriedadeDto>.Success(resultado);
        }
        catch (Exception ex)
        {
            return Result<PropriedadeDto>.Failure($"Erro ao atualizar propriedade: {ex.Message}");
        }
    }

    public async Task<Result> RemoverAsync(int id)
    {
        try
        {
            var podeRemover = await _domainService.PodeRemoverPropriedadeAsync(id);
            if (!podeRemover)
                return Result.Failure("Não é possível remover propriedade com talhões ou culturas ativas");

            await _propriedadeRepository.RemoverAsync(id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Erro ao remover propriedade: {ex.Message}");
        }
    }

    public async Task<Result<PropriedadeDto>> ObterCompletaAsync(int id)
    {
        try
        {
            var propriedade = await _propriedadeRepository.ObterCompletaAsync(id);
            if (propriedade == null)
                return Result<PropriedadeDto>.Failure("Propriedade não encontrada");

            var dto = _mapper.Map<PropriedadeDto>(propriedade);
            return Result<PropriedadeDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<PropriedadeDto>.Failure($"Erro ao obter propriedade completa: {ex.Message}");
        }
    }

    public async Task<Result<decimal>> CalcularAreaTotalPorProdutorAsync(int produtorId)
    {
        try
        {
            var areaTotal = await _domainService.CalcularAreaTotalProdutorAsync(produtorId);
            return Result<decimal>.Success(areaTotal.Valor);
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure($"Erro ao calcular área total: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PropriedadeDto>>> BuscarPropriedadesProximasAsync(
        double latitude, double longitude, double raioKm)
    {
        try
        {
            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            var ponto = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
            
            var propriedades = await _domainService.BuscarPropriedadesProximasAsync(ponto, raioKm);
            var dtos = _mapper.Map<IEnumerable<PropriedadeDto>>(propriedades);
            
            return Result<IEnumerable<PropriedadeDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<PropriedadeDto>>.Failure($"Erro ao buscar propriedades próximas: {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<int, decimal>>> ObterEstatisticasCulturasPorProdutorAsync(int produtorId)
    {
        try
        {
            var estatisticas = await _domainService.ObterEstatisticasCulturasPorProdutorAsync(produtorId);
            return Result<Dictionary<int, decimal>>.Success(estatisticas);
        }
        catch (Exception ex)
        {
            return Result<Dictionary<int, decimal>>.Failure($"Erro ao obter estatísticas: {ex.Message}");
        }
    }
}