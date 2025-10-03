using AutoMapper;
using Agriis.Enderecos.Aplicacao.DTOs;
using Agriis.Enderecos.Aplicacao.Interfaces;
using Agriis.Enderecos.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.Enderecos.Aplicacao.Servicos;

/// <summary>
/// Serviço de aplicação para países
/// </summary>
public class PaisService : IPaisService
{
    private readonly IPaisRepository _paisRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PaisService> _logger;

    public PaisService(
        IPaisRepository paisRepository,
        IMapper mapper,
        ILogger<PaisService> logger)
    {
        _paisRepository = paisRepository ?? throw new ArgumentNullException(nameof(paisRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaisDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var pais = await _paisRepository.ObterPorIdAsync(id, cancellationToken);
            return pais != null ? _mapper.Map<PaisDto>(pais) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter país por ID {Id}", id);
            throw;
        }
    }

    public async Task<PaisDto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        try
        {
            var pais = await _paisRepository.ObterPorCodigoAsync(codigo, cancellationToken);
            return pais != null ? _mapper.Map<PaisDto>(pais) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter país por código {Codigo}", codigo);
            throw;
        }
    }

    public async Task<IEnumerable<PaisDto>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var paises = await _paisRepository.ObterAtivosAsync(cancellationToken);
            return _mapper.Map<IEnumerable<PaisDto>>(paises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter países ativos");
            throw;
        }
    }

    public async Task<IEnumerable<PaisDto>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        try
        {
            var paises = await _paisRepository.BuscarPorNomeAsync(nome, cancellationToken);
            return _mapper.Map<IEnumerable<PaisDto>>(paises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar países por nome {Nome}", nome);
            throw;
        }
    }

    public async Task<IEnumerable<PaisDto>> ObterTodosComEstadosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var paises = await _paisRepository.ObterTodosComEstadosAsync(cancellationToken);
            return _mapper.Map<IEnumerable<PaisDto>>(paises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter todos os países com estados");
            throw;
        }
    }

    public async Task<IEnumerable<PaisDto>> ObterAtivosComEstadosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var paises = await _paisRepository.ObterAtivosComEstadosAsync(cancellationToken);
            return _mapper.Map<IEnumerable<PaisDto>>(paises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter países ativos com estados");
            throw;
        }
    }
}