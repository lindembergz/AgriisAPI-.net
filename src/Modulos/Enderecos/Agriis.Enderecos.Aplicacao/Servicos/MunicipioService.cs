using AutoMapper;
using Agriis.Enderecos.Aplicacao.DTOs;
using Agriis.Enderecos.Aplicacao.Interfaces;
using Agriis.Enderecos.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.Enderecos.Aplicacao.Servicos;

/// <summary>
/// Serviço de aplicação para municípios
/// </summary>
public class MunicipioService : IMunicipioService
{
    private readonly IMunicipioRepository _municipioRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<MunicipioService> _logger;

    public MunicipioService(
        IMunicipioRepository municipioRepository,
        IMapper mapper,
        ILogger<MunicipioService> logger)
    {
        _municipioRepository = municipioRepository ?? throw new ArgumentNullException(nameof(municipioRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MunicipioDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var municipio = await _municipioRepository.ObterPorIdAsync(id, cancellationToken);
            return municipio != null ? _mapper.Map<MunicipioDto>(municipio) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter município por ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<MunicipioDto>> ObterPorEstadoAsync(int estadoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var municipios = await _municipioRepository.ObterPorEstadoAsync(estadoId);
            return _mapper.Map<IEnumerable<MunicipioDto>>(municipios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter municípios do estado {EstadoId}", estadoId);
            throw;
        }
    }

    public async Task<IEnumerable<MunicipioDto>> BuscarPorNomeAsync(string nome, int? estadoId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var municipios = await _municipioRepository.BuscarPorNomeAsync(nome, estadoId);
            return _mapper.Map<IEnumerable<MunicipioDto>>(municipios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar municípios por nome {Nome}", nome);
            throw;
        }
    }
}