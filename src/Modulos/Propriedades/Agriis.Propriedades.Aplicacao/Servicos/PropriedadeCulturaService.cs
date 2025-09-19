using AutoMapper;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Propriedades.Aplicacao.DTOs;
using Agriis.Propriedades.Aplicacao.Interfaces;
using Agriis.Propriedades.Dominio.Entidades;
using Agriis.Propriedades.Dominio.Interfaces;
using Agriis.Propriedades.Dominio.Servicos;

namespace Agriis.Propriedades.Aplicacao.Servicos;

public class PropriedadeCulturaService : IPropriedadeCulturaService
{
    private readonly IPropriedadeCulturaRepository _propriedadeCulturaRepository;
    private readonly PropriedadeDomainService _domainService;
    private readonly IMapper _mapper;

    public PropriedadeCulturaService(
        IPropriedadeCulturaRepository propriedadeCulturaRepository,
        PropriedadeDomainService domainService,
        IMapper mapper)
    {
        _propriedadeCulturaRepository = propriedadeCulturaRepository;
        _domainService = domainService;
        _mapper = mapper;
    }

    public async Task<Result<PropriedadeCulturaDto>> ObterPorIdAsync(int id)
    {
        try
        {
            var propriedadeCultura = await _propriedadeCulturaRepository.ObterPorIdAsync(id);
            if (propriedadeCultura == null)
                return Result<PropriedadeCulturaDto>.Failure("Propriedade cultura não encontrada");

            var dto = _mapper.Map<PropriedadeCulturaDto>(propriedadeCultura);
            return Result<PropriedadeCulturaDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<PropriedadeCulturaDto>.Failure($"Erro ao obter propriedade cultura: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PropriedadeCulturaDto>>> ObterPorPropriedadeAsync(int propriedadeId)
    {
        try
        {
            var propriedadeCulturas = await _propriedadeCulturaRepository.ObterPorPropriedadeAsync(propriedadeId);
            var dtos = _mapper.Map<IEnumerable<PropriedadeCulturaDto>>(propriedadeCulturas);
            return Result<IEnumerable<PropriedadeCulturaDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<PropriedadeCulturaDto>>.Failure($"Erro ao obter culturas da propriedade: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PropriedadeCulturaDto>>> ObterPorCulturaAsync(int culturaId)
    {
        try
        {
            var propriedadeCulturas = await _propriedadeCulturaRepository.ObterPorCulturaAsync(culturaId);
            var dtos = _mapper.Map<IEnumerable<PropriedadeCulturaDto>>(propriedadeCulturas);
            return Result<IEnumerable<PropriedadeCulturaDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<PropriedadeCulturaDto>>.Failure($"Erro ao obter propriedades da cultura: {ex.Message}");
        }
    }

    public async Task<Result<PropriedadeCulturaDto>> CriarAsync(PropriedadeCulturaCreateDto dto)
    {
        try
        {
            // Verificar se já existe essa combinação propriedade-cultura
            var existente = await _propriedadeCulturaRepository.ObterPorPropriedadeECulturaAsync(dto.PropriedadeId, dto.CulturaId);
            if (existente != null)
                return Result<PropriedadeCulturaDto>.Failure("Cultura já está associada a esta propriedade");

            // Validar se a área da cultura não excede a área da propriedade
            var areaValida = await _domainService.ValidarAreaCulturaAsync(dto.PropriedadeId, new AreaPlantio(dto.Area));
            if (!areaValida)
                return Result<PropriedadeCulturaDto>.Failure("Área da cultura excede a área disponível da propriedade");

            var propriedadeCultura = new PropriedadeCultura(dto.PropriedadeId, dto.CulturaId, new AreaPlantio(dto.Area), dto.SafraId);
            
            if (dto.DataPlantio.HasValue || dto.DataColheitaPrevista.HasValue)
            {
                propriedadeCultura.DefinirDatasPlantioColheita(dto.DataPlantio, dto.DataColheitaPrevista);
            }

            if (!string.IsNullOrEmpty(dto.Observacoes))
            {
                propriedadeCultura.AdicionarObservacoes(dto.Observacoes);
            }
            var propriedadeCulturaCriada = await _propriedadeCulturaRepository.AdicionarAsync(propriedadeCultura);
            
            var resultado = _mapper.Map<PropriedadeCulturaDto>(propriedadeCulturaCriada);
            return Result<PropriedadeCulturaDto>.Success(resultado);
        }
        catch (Exception ex)
        {
            return Result<PropriedadeCulturaDto>.Failure($"Erro ao criar propriedade cultura: {ex.Message}");
        }
    }

    public async Task<Result<PropriedadeCulturaDto>> AtualizarAsync(int id, PropriedadeCulturaUpdateDto dto)
    {
        try
        {
            var propriedadeCultura = await _propriedadeCulturaRepository.ObterPorIdAsync(id);
            if (propriedadeCultura == null)
                return Result<PropriedadeCulturaDto>.Failure("Propriedade cultura não encontrada");

            // Validar se a nova área não excede a área da propriedade
            var areaValida = await _domainService.ValidarAreaCulturaAsync(propriedadeCultura.PropriedadeId, new AreaPlantio(dto.Area));
            if (!areaValida)
                return Result<PropriedadeCulturaDto>.Failure("Nova área da cultura excede a área disponível da propriedade");

            propriedadeCultura.AtualizarArea(new AreaPlantio(dto.Area));

            if (dto.SafraId.HasValue)
            {
                propriedadeCultura.DefinirSafra(dto.SafraId.Value);
            }

            if (dto.DataPlantio.HasValue || dto.DataColheitaPrevista.HasValue)
            {
                propriedadeCultura.DefinirDatasPlantioColheita(dto.DataPlantio, dto.DataColheitaPrevista);
            }

            if (!string.IsNullOrEmpty(dto.Observacoes))
            {
                propriedadeCultura.AdicionarObservacoes(dto.Observacoes);
            }

            await _propriedadeCulturaRepository.AtualizarAsync(propriedadeCultura);
            
            var resultado = _mapper.Map<PropriedadeCulturaDto>(propriedadeCultura);
            return Result<PropriedadeCulturaDto>.Success(resultado);
        }
        catch (Exception ex)
        {
            return Result<PropriedadeCulturaDto>.Failure($"Erro ao atualizar propriedade cultura: {ex.Message}");
        }
    }

    public async Task<Result> RemoverAsync(int id)
    {
        try
        {
            await _propriedadeCulturaRepository.RemoverAsync(id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Erro ao remover propriedade cultura: {ex.Message}");
        }
    }

    public async Task<Result<decimal>> CalcularAreaTotalPorCulturaAsync(int culturaId)
    {
        try
        {
            var areaTotal = await _domainService.CalcularAreaTotalCulturaAsync(culturaId);
            return Result<decimal>.Success(areaTotal.Valor);
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure($"Erro ao calcular área total da cultura: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PropriedadeCulturaDto>>> ObterEmPeriodoPlantioAsync()
    {
        try
        {
            var propriedadeCulturas = await _propriedadeCulturaRepository.ObterEmPeriodoPlantioAsync();
            var dtos = _mapper.Map<IEnumerable<PropriedadeCulturaDto>>(propriedadeCulturas);
            return Result<IEnumerable<PropriedadeCulturaDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<PropriedadeCulturaDto>>.Failure($"Erro ao obter culturas em período de plantio: {ex.Message}");
        }
    }
}