using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Agriis.Enderecos.Dominio.Interfaces;
using Agriis.Enderecos.Aplicacao.DTOs;
using Agriis.Enderecos.Dominio.Entidades;
using Agriis.Compartilhado.Aplicacao.Resultados;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de endereços, estados e municípios
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EnderecosController : ControllerBase
{
    private readonly IEstadoRepository _estadoRepository;
    private readonly IMunicipioRepository _municipioRepository;
    private readonly IEnderecoRepository _enderecoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<EnderecosController> _logger;

    public EnderecosController(
        IEstadoRepository estadoRepository,
        IMunicipioRepository municipioRepository,
        IEnderecoRepository enderecoRepository,
        IMapper mapper,
        ILogger<EnderecosController> logger)
    {
        _estadoRepository = estadoRepository;
        _municipioRepository = municipioRepository;
        _enderecoRepository = enderecoRepository;
        _mapper = mapper;
        _logger = logger;
    }

    #region Estados

    /// <summary>
    /// Obtém todos os estados
    /// </summary>
    [HttpGet("estados")]
    public async Task<ActionResult<IEnumerable<EstadoDto>>> ObterEstados()
    {
        try
        {
            var estados = await _estadoRepository.ObterTodosAsync();
            var estadosDto = _mapper.Map<IEnumerable<EstadoDto>>(estados);
            return Ok(estadosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estados");
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém um estado por ID
    /// </summary>
    [HttpGet("estados/{id:int}")]
    public async Task<ActionResult<EstadoDto>> ObterEstadoPorId(int id)
    {
        try
        {
            var estado = await _estadoRepository.ObterPorIdAsync(id);
            if (estado == null)
                return NotFound(new { error_code = "ESTADO_NAO_ENCONTRADO", error_description = "Estado não encontrado" });

            var estadoDto = _mapper.Map<EstadoDto>(estado);
            return Ok(estadoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estado por ID: {Id}", id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém um estado por UF
    /// </summary>
    [HttpGet("estados/uf/{uf}")]
    public async Task<ActionResult<EstadoDto>> ObterEstadoPorUf(string uf)
    {
        try
        {
            var estado = await _estadoRepository.ObterPorUfAsync(uf);
            if (estado == null)
                return NotFound(new { error_code = "ESTADO_NAO_ENCONTRADO", error_description = "Estado não encontrado" });

            var estadoDto = _mapper.Map<EstadoDto>(estado);
            return Ok(estadoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estado por UF: {Uf}", uf);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    #endregion

    #region Municípios

    /// <summary>
    /// Obtém municípios por estado
    /// </summary>
    [HttpGet("municipios/estado/{estadoId:int}")]
    public async Task<ActionResult<IEnumerable<MunicipioDto>>> ObterMunicipiosPorEstado(int estadoId)
    {
        try
        {
            var municipios = await _municipioRepository.ObterPorEstadoAsync(estadoId);
            var municipiosDto = _mapper.Map<IEnumerable<MunicipioDto>>(municipios);
            return Ok(municipiosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter municípios por estado: {EstadoId}", estadoId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém municípios por UF
    /// </summary>
    [HttpGet("municipios/uf/{uf}")]
    public async Task<ActionResult<IEnumerable<MunicipioDto>>> ObterMunicipiosPorUf(string uf)
    {
        try
        {
            var municipios = await _municipioRepository.ObterPorUfAsync(uf);
            var municipiosDto = _mapper.Map<IEnumerable<MunicipioDto>>(municipios);
            return Ok(municipiosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter municípios por UF: {Uf}", uf);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Busca municípios por nome
    /// </summary>
    [HttpGet("municipios/buscar")]
    public async Task<ActionResult<IEnumerable<MunicipioDto>>> BuscarMunicipios(
        [FromQuery] string nome, 
        [FromQuery] int? estadoId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nome))
                return BadRequest(new { error_code = "NOME_OBRIGATORIO", error_description = "Nome é obrigatório para busca" });

            var municipios = await _municipioRepository.BuscarPorNomeAsync(nome, estadoId);
            var municipiosDto = _mapper.Map<IEnumerable<MunicipioDto>>(municipios);
            return Ok(municipiosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar municípios por nome: {Nome}", nome);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém municípios próximos a uma localização
    /// </summary>
    [HttpGet("municipios/proximos")]
    public async Task<ActionResult<IEnumerable<MunicipioProximoDto>>> ObterMunicipiosProximos(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double raioKm = 50,
        [FromQuery] int limite = 10)
    {
        try
        {
            if (latitude < -90 || latitude > 90)
                return BadRequest(new { error_code = "LATITUDE_INVALIDA", error_description = "Latitude deve estar entre -90 e 90" });

            if (longitude < -180 || longitude > 180)
                return BadRequest(new { error_code = "LONGITUDE_INVALIDA", error_description = "Longitude deve estar entre -180 e 180" });

            var municipios = await _municipioRepository.ObterProximosAsync(latitude, longitude, raioKm, limite);
            var municipiosDto = _mapper.Map<IEnumerable<MunicipioProximoDto>>(municipios);
            
            return Ok(municipiosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter municípios próximos: Lat={Latitude}, Lng={Longitude}", latitude, longitude);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém um município por ID
    /// </summary>
    [HttpGet("municipios/{id:int}")]
    public async Task<ActionResult<MunicipioDto>> ObterMunicipioPorId(int id)
    {
        try
        {
            var municipio = await _municipioRepository.ObterPorIdAsync(id);
            if (municipio == null)
                return NotFound(new { error_code = "MUNICIPIO_NAO_ENCONTRADO", error_description = "Município não encontrado" });

            var municipioDto = _mapper.Map<MunicipioDto>(municipio);
            return Ok(municipioDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter município por ID: {Id}", id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    #endregion

    #region Endereços

    /// <summary>
    /// Obtém endereços por CEP
    /// </summary>
    [HttpGet("enderecos/cep/{cep}")]
    public async Task<ActionResult<IEnumerable<EnderecoDto>>> ObterEnderecosPorCep(string cep)
    {
        try
        {
            var enderecos = await _enderecoRepository.ObterPorCepAsync(cep);
            var enderecosDto = _mapper.Map<IEnumerable<EnderecoDto>>(enderecos);
            return Ok(enderecosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter endereços por CEP: {Cep}", cep);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém endereços por município
    /// </summary>
    [HttpGet("enderecos/municipio/{municipioId:int}")]
    public async Task<ActionResult<IEnumerable<EnderecoDto>>> ObterEnderecosPorMunicipio(int municipioId)
    {
        try
        {
            var enderecos = await _enderecoRepository.ObterPorMunicipioAsync(municipioId);
            var enderecosDto = _mapper.Map<IEnumerable<EnderecoDto>>(enderecos);
            return Ok(enderecosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter endereços por município: {MunicipioId}", municipioId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Busca endereços por logradouro
    /// </summary>
    [HttpGet("enderecos/buscar")]
    public async Task<ActionResult<IEnumerable<EnderecoDto>>> BuscarEnderecos(
        [FromQuery] string? logradouro = null,
        [FromQuery] string? bairro = null,
        [FromQuery] int? municipioId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(logradouro) && string.IsNullOrWhiteSpace(bairro))
                return BadRequest(new { error_code = "PARAMETRO_OBRIGATORIO", error_description = "Logradouro ou bairro é obrigatório para busca" });

            IEnumerable<Endereco> enderecos;

            if (!string.IsNullOrWhiteSpace(logradouro))
                enderecos = await _enderecoRepository.BuscarPorLogradouroAsync(logradouro, municipioId);
            else
                enderecos = await _enderecoRepository.BuscarPorBairroAsync(bairro!, municipioId);

            var enderecosDto = _mapper.Map<IEnumerable<EnderecoDto>>(enderecos);
            return Ok(enderecosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar endereços: Logradouro={Logradouro}, Bairro={Bairro}", logradouro, bairro);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém endereços próximos a uma localização
    /// </summary>
    [HttpGet("enderecos/proximos")]
    public async Task<ActionResult<IEnumerable<EnderecoProximoDto>>> ObterEnderecosProximos(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double raioKm = 10,
        [FromQuery] int limite = 10)
    {
        try
        {
            if (latitude < -90 || latitude > 90)
                return BadRequest(new { error_code = "LATITUDE_INVALIDA", error_description = "Latitude deve estar entre -90 e 90" });

            if (longitude < -180 || longitude > 180)
                return BadRequest(new { error_code = "LONGITUDE_INVALIDA", error_description = "Longitude deve estar entre -180 e 180" });

            var enderecos = await _enderecoRepository.ObterProximosAsync(latitude, longitude, raioKm, limite);
            var enderecosDto = _mapper.Map<IEnumerable<EnderecoProximoDto>>(enderecos);
            
            return Ok(enderecosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter endereços próximos: Lat={Latitude}, Lng={Longitude}", latitude, longitude);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Calcula distância entre dois endereços
    /// </summary>
    [HttpGet("enderecos/distancia")]
    public async Task<ActionResult<object>> CalcularDistancia(
        [FromQuery] int enderecoOrigemId,
        [FromQuery] int enderecoDestinoId)
    {
        try
        {
            var distancia = await _enderecoRepository.CalcularDistanciaAsync(enderecoOrigemId, enderecoDestinoId);
            
            if (!distancia.HasValue)
                return BadRequest(new { error_code = "CALCULO_IMPOSSIVEL", error_description = "Não foi possível calcular a distância. Verifique se os endereços possuem localização definida." });

            return Ok(new { distancia_km = Math.Round(distancia.Value, 2) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular distância entre endereços: {OrigemId} -> {DestinoId}", enderecoOrigemId, enderecoDestinoId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Calcula distância entre dois municípios
    /// </summary>
    [HttpGet("municipios/distancia")]
    public async Task<ActionResult<object>> CalcularDistanciaMunicipios(
        [FromQuery] int municipioOrigemId,
        [FromQuery] int municipioDestinoId)
    {
        try
        {
            var distancia = await _municipioRepository.CalcularDistanciaAsync(municipioOrigemId, municipioDestinoId);
            
            if (!distancia.HasValue)
                return BadRequest(new { error_code = "CALCULO_IMPOSSIVEL", error_description = "Não foi possível calcular a distância. Verifique se os municípios possuem localização definida." });

            return Ok(new { distancia_km = Math.Round(distancia.Value, 2) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular distância entre municípios: {OrigemId} -> {DestinoId}", municipioOrigemId, municipioDestinoId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    #endregion
}