using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Agriis.Enderecos.Dominio.Interfaces;
using Agriis.Enderecos.Aplicacao.DTOs;
using Agriis.Enderecos.Aplicacao.Interfaces;
using Agriis.Enderecos.Dominio.Entidades;
using Agriis.Compartilhado.Aplicacao.Resultados;
using System.ComponentModel.DataAnnotations;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de endereços, estados e municípios
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EnderecosController : ControllerBase
{
    private readonly IPaisService _paisService;
    private readonly IEstadoRepository _estadoRepository;
    private readonly IMunicipioRepository _municipioRepository;
    private readonly IEnderecoRepository _enderecoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<EnderecosController> _logger;

    public EnderecosController(
        IPaisService paisService,
        IEstadoRepository estadoRepository,
        IMunicipioRepository municipioRepository,
        IEnderecoRepository enderecoRepository,
        IMapper mapper,
        ILogger<EnderecosController> logger)
    {
        _paisService = paisService;
        _estadoRepository = estadoRepository;
        _municipioRepository = municipioRepository;
        _enderecoRepository = enderecoRepository;
        _mapper = mapper;
        _logger = logger;
    }

    #region Países

    /// <summary>
    /// Obtém todos os países com seus estados
    /// </summary>
    [HttpGet("paises")]
    public async Task<ActionResult<IEnumerable<PaisDto>>> ObterPaises()
    {
        try
        {
            var paises = await _paisService.ObterTodosComEstadosAsync();
            return Ok(paises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter países");
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém país por ID
    /// </summary>
    [HttpGet("paises/{id:int}")]
    public async Task<ActionResult<PaisDto>> ObterPaisPorId(int id)
    {
        try
        {
            var pais = await _paisService.ObterPorIdAsync(id);
            if (pais == null)
                return NotFound(new { error_code = "PAIS_NAO_ENCONTRADO", error_description = "País não encontrado" });

            return Ok(pais);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter país por ID: {Id}", id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém países ativos com seus estados
    /// </summary>
    [HttpGet("paises/ativos")]
    public async Task<ActionResult<IEnumerable<PaisDto>>> ObterPaisesAtivos()
    {
        try
        {
            var paises = await _paisService.ObterAtivosComEstadosAsync();
            return Ok(paises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter países ativos");
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém países ativos com contadores
    /// </summary>
    [HttpGet("paises/ativos/com-contadores")]
    public async Task<ActionResult<IEnumerable<PaisComContadorDto>>> ObterPaisesAtivosComContadores()
    {
        try
        {
            var paises = await _paisService.ObterTodosAsync();
            return Ok(paises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter países com contadores");
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém todos os países com paginação
    /// </summary>
    [HttpGet("paises/todos")]
    public async Task<ActionResult<IEnumerable<PaisComContadorDto>>> ObterTodosPaises(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 50)
    {
        try
        {
            if (pagina < 1)
                return BadRequest(new { error_code = "PAGINA_INVALIDA", error_description = "Página deve ser maior que zero" });

            if (tamanhoPagina < 1 || tamanhoPagina > 100)
                tamanhoPagina = Math.Max(1, Math.Min(100, tamanhoPagina));

            var paises = await _paisService.ObterTodosAsync(pagina, tamanhoPagina);
            return Ok(paises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter todos os países");
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria um novo país
    /// </summary>
    [HttpPost("paises")]
    public async Task<ActionResult<PaisDto>> CriarPais([FromBody] CriarPaisDto criarPaisDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _paisService.CriarAsync(criarPaisDto);
            
            if (!resultado.IsSuccess)
                return BadRequest(new { error_code = "ERRO_VALIDACAO", error_description = resultado.Error });

            return CreatedAtAction(nameof(ObterPaisPorId), new { id = resultado.Value!.Id }, resultado.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar país: {Nome}", criarPaisDto.Nome);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza um país existente
    /// </summary>
    [HttpPut("paises/{id:int}")]
    public async Task<ActionResult<PaisDto>> AtualizarPais(int id, [FromBody] AtualizarPaisDto atualizarPaisDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _paisService.AtualizarAsync(id, atualizarPaisDto);
            
            if (!resultado.IsSuccess)
            {
                if (resultado.Error!.Contains("não encontrado"))
                    return NotFound(new { error_code = "PAIS_NAO_ENCONTRADO", error_description = resultado.Error });
                
                return BadRequest(new { error_code = "ERRO_VALIDACAO", error_description = resultado.Error });
            }

            return Ok(resultado.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar país: {Id}", id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Exclui um país (soft delete)
    /// </summary>
    [HttpDelete("paises/{id:int}")]
    public async Task<ActionResult> ExcluirPais(int id)
    {
        try
        {
            var resultado = await _paisService.ExcluirAsync(id);
            
            if (!resultado.IsSuccess)
            {
                if (resultado.Error!.Contains("não encontrado"))
                    return NotFound(new { error_code = "PAIS_NAO_ENCONTRADO", error_description = resultado.Error });
                
                return BadRequest(new { error_code = "ERRO_VALIDACAO", error_description = resultado.Error });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir país: {Id}", id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Ativa um país
    /// </summary>
    [HttpPatch("paises/{id:int}/ativar")]
    public async Task<ActionResult> AtivarPais(int id)
    {
        try
        {
            var resultado = await _paisService.AtivarAsync(id);
            
            if (!resultado.IsSuccess)
            {
                if (resultado.Error!.Contains("não encontrado"))
                    return NotFound(new { error_code = "PAIS_NAO_ENCONTRADO", error_description = resultado.Error });
                
                return BadRequest(new { error_code = "ERRO_VALIDACAO", error_description = resultado.Error });
            }

            return Ok(new { message = "País ativado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar país: {Id}", id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Desativa um país
    /// </summary>
    [HttpPatch("paises/{id:int}/desativar")]
    public async Task<ActionResult> DesativarPais(int id)
    {
        try
        {
            var resultado = await _paisService.DesativarAsync(id);
            
            if (!resultado.IsSuccess)
            {
                if (resultado.Error!.Contains("não encontrado"))
                    return NotFound(new { error_code = "PAIS_NAO_ENCONTRADO", error_description = resultado.Error });
                
                return BadRequest(new { error_code = "ERRO_VALIDACAO", error_description = resultado.Error });
            }

            return Ok(new { message = "País desativado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar país: {Id}", id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Verifica se um país existe por código
    /// </summary>
    [HttpGet("paises/validar-codigo")]
    public async Task<ActionResult<ValidationResponseDto>> ValidarCodigoPais(
        [FromQuery] string codigo,
        [FromQuery] int? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return BadRequest(new { error_code = "CODIGO_OBRIGATORIO", error_description = "Código é obrigatório" });

            var existe = await _paisService.ExistePorCodigoAsync(codigo);
            
            return Ok(new ValidationResponseDto 
            { 
                IsValid = !existe,
                Message = existe ? "Código já existe" : "Código disponível"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar código país: {Codigo}", codigo);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Verifica se um país existe por nome
    /// </summary>
    [HttpGet("paises/validar-nome")]
    public async Task<ActionResult<ValidationResponseDto>> ValidarNomePais(
        [FromQuery] string nome,
        [FromQuery] int? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nome))
                return BadRequest(new { error_code = "NOME_OBRIGATORIO", error_description = "Nome é obrigatório" });

            var existe = await _paisService.ExistePorNomeAsync(nome, excludeId);
            
            return Ok(new ValidationResponseDto 
            { 
                IsValid = !existe,
                Message = existe ? "Nome já existe" : "Nome disponível"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar nome país: {Nome}", nome);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Verifica se país tem UFs/Estados
    /// </summary>
    [HttpGet("paises/{paisId:int}/tem-ufs")]
    public async Task<ActionResult<ExistenceResponseDto>> VerificarPaisTemUfs(int paisId)
    {
        try
        {
            var estados = await _estadoRepository.ObterPorPaisAsync(paisId);
            var hasUfs = estados.Any();
            
            return Ok(new ExistenceResponseDto 
            { 
                Exists = hasUfs,
                HasDependencies = hasUfs
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar UFs do país: {PaisId}", paisId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Verifica se país pode ser removido
    /// </summary>
    [HttpGet("paises/{paisId:int}/pode-remover")]
    public async Task<ActionResult<ExistenceResponseDto>> VerificarPaisPodeRemover(int paisId)
    {
        try
        {
            var pais = await _paisService.ObterPorIdAsync(paisId);
            if (pais == null)
                return NotFound(new { error_code = "PAIS_NAO_ENCONTRADO", error_description = "País não encontrado" });

            var estados = await _estadoRepository.ObterPorPaisAsync(paisId);
            var hasEstados = estados.Any();
            
            return Ok(new ExistenceResponseDto 
            { 
                Exists = !hasEstados, // Pode remover se NÃO tem estados
                HasDependencies = hasEstados
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar se país pode ser removido: {PaisId}", paisId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém UFs de um país
    /// </summary>
    [HttpGet("paises/{paisId:int}/ufs")]
    public async Task<ActionResult<IEnumerable<EstadoDto>>> ObterUfsPorPais(int paisId)
    {
        try
        {
            if (paisId != 1)
            {
                return Ok(new EstadoDto[0]); // Retorna array vazio para outros países
            }
            
            var estados = await _estadoRepository.ObterPorPaisAsync(paisId);
            var estadosDto = _mapper.Map<IEnumerable<EstadoDto>>(estados);
            
            return Ok(estadosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter UFs do país: {PaisId}", paisId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    #endregion

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

    /// <summary>
    /// Cria um novo estado
    /// </summary>
    [HttpPost("estados")]
    public async Task<ActionResult<EstadoDto>> CriarEstado([FromBody] CriarEstadoDto criarEstadoDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var estado = new Estado(criarEstadoDto.Nome, criarEstadoDto.Codigo, criarEstadoDto.CodigoIbge, criarEstadoDto.Regiao, criarEstadoDto.PaisId);
            
            await _estadoRepository.AdicionarAsync(estado);

            var estadoDto = _mapper.Map<EstadoDto>(estado);
            return CreatedAtAction(nameof(ObterEstadoPorId), new { id = estado.Id }, estadoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar estado: {Nome}", criarEstadoDto.Nome);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza um estado
    /// </summary>
    [HttpPut("estados/{id:int}")]
    public async Task<ActionResult<EstadoDto>> AtualizarEstado(int id, [FromBody] AtualizarEstadoDto atualizarEstadoDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var estado = await _estadoRepository.ObterPorIdAsync(id);
            if (estado == null)
                return NotFound(new { error_code = "ESTADO_NAO_ENCONTRADO", error_description = "Estado não encontrado" });

            estado.Atualizar(atualizarEstadoDto.Nome, atualizarEstadoDto.Codigo, atualizarEstadoDto.CodigoIbge, atualizarEstadoDto.Regiao, atualizarEstadoDto.PaisId);
            
            await _estadoRepository.AtualizarAsync(estado);

            var estadoDto = _mapper.Map<EstadoDto>(estado);
            return Ok(estadoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar estado: {Id}", id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Remove um estado
    /// </summary>
    [HttpDelete("estados/{id:int}")]
    public async Task<ActionResult> RemoverEstado(int id)
    {
        try
        {
            var estado = await _estadoRepository.ObterPorIdAsync(id);
            if (estado == null)
                return NotFound(new { error_code = "ESTADO_NAO_ENCONTRADO", error_description = "Estado não encontrado" });

            // Verificar se há municípios associados
            var municipios = await _municipioRepository.ObterPorEstadoAsync(id);
            if (municipios.Any())
                return BadRequest(new { error_code = "ESTADO_COM_MUNICIPIOS", error_description = "Não é possível remover um estado que possui municípios" });

            await _estadoRepository.RemoverAsync(estado);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover estado: {Id}", id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém estados por país
    /// </summary>
    [HttpGet("estados/pais/{paisId:int}")]
    public async Task<ActionResult<IEnumerable<EstadoDto>>> ObterEstadosPorPais(int paisId)
    {
        try
        {
            if (paisId <= 0)
            {
                _logger.LogWarning("ID de país inválido: {PaisId}", paisId);
                return BadRequest(new { error_code = "PAIS_ID_INVALIDO", error_description = "ID do país deve ser maior que zero" });
            }

            _logger.LogDebug("Obtendo estados por país: {PaisId}", paisId);
            var estados = await _estadoRepository.ObterPorPaisAsync(paisId);
            var estadosDto = _mapper.Map<IEnumerable<EstadoDto>>(estados);
            
            _logger.LogDebug("Encontrados {Count} estados para o país {PaisId}", estadosDto.Count(), paisId);
            return Ok(estadosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estados por país: {PaisId}", paisId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém estados ativos
    /// </summary>
    [HttpGet("estados/ativos")]
    public async Task<ActionResult<IEnumerable<EstadoDto>>> ObterEstadosAtivos()
    {
        try
        {
            _logger.LogDebug("Obtendo estados ativos");
            var estados = await _estadoRepository.ObterTodosAsync();
            var estadosDto = _mapper.Map<IEnumerable<EstadoDto>>(estados);
            
            _logger.LogDebug("Encontrados {Count} estados ativos", estadosDto.Count());
            return Ok(estadosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estados ativos");
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Valida se código UF é único
    /// </summary>
    [HttpGet("estados/validar-codigo")]
    public async Task<ActionResult<ValidationResponseDto>> ValidarCodigoEstado(
        [FromQuery] string codigo, 
        [FromQuery] int paisId = 1, 
        [FromQuery] int? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return BadRequest(new { error_code = "CODIGO_OBRIGATORIO", error_description = "Código é obrigatório" });

            var existe = await _estadoRepository.ExisteCodigoAsync(codigo, excludeId);
            
            return Ok(new ValidationResponseDto 
            { 
                IsValid = !existe,
                Message = existe ? "Código já existe" : "Código disponível"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar código estado: {Codigo}", codigo);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Verifica se estado tem municípios
    /// </summary>
    [HttpGet("estados/{ufId:int}/tem-municipios")]
    public async Task<ActionResult<ExistenceResponseDto>> VerificarEstadoTemMunicipios(int ufId)
    {
        try
        {
            var municipios = await _municipioRepository.ObterPorEstadoAsync(ufId);
            var hasMunicipios = municipios.Any();
            
            return Ok(new ExistenceResponseDto 
            { 
                Exists = hasMunicipios,
                HasDependencies = hasMunicipios
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar municípios do estado: {UfId}", ufId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém contagem de municípios por estado
    /// </summary>
    [HttpGet("estados/{ufId:int}/municipios/count")]
    public async Task<ActionResult<CountResponseDto>> ObterContagemMunicipiosPorEstado(int ufId)
    {
        try
        {
            var municipios = await _municipioRepository.ObterPorEstadoAsync(ufId);
            var count = municipios.Count();
            
            return Ok(new CountResponseDto { Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter contagem de municípios do estado: {UfId}", ufId);
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
    public async Task<ActionResult<IEnumerable<MunicipioFrontendDto>>> BuscarMunicipios(
        [FromQuery] string nome, 
        [FromQuery] int? estadoId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nome))
                return BadRequest(new { error_code = "NOME_OBRIGATORIO", error_description = "Nome é obrigatório para busca" });

            var municipios = await _municipioRepository.BuscarPorNomeAsync(nome, estadoId);
            var municipiosDto = _mapper.Map<IEnumerable<MunicipioFrontendDto>>(municipios);
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

    /// <summary>
    /// Obtém todos os municípios ou filtra por estado
    /// </summary>
    [HttpGet("municipios")]
    public async Task<ActionResult<IEnumerable<MunicipioFrontendDto>>> ObterMunicipios(
        [FromQuery] string? include = null,
        [FromQuery] int? estado_id = null)
    {
        try
        {
            _logger.LogInformation("🔍 ObterMunicipios chamado - include: {Include}, estado_id: {EstadoId}", include, estado_id);
            
            IEnumerable<Municipio> municipios;
            
            if (estado_id.HasValue)
            {
                _logger.LogInformation("🏛️ Filtrando municípios por estado ID: {EstadoId}", estado_id.Value);
                
                // Validar se o estado existe
                var estado = await _estadoRepository.ObterPorIdAsync(estado_id.Value);
                if (estado == null)
                {
                    _logger.LogWarning("❌ Estado não encontrado: {EstadoId}", estado_id.Value);
                    return NotFound(new { error_code = "ESTADO_NAO_ENCONTRADO", error_description = "Estado não encontrado" });
                }
                
                _logger.LogInformation("✅ Estado encontrado: {EstadoNome} ({EstadoUf})", estado.Nome, estado.Uf);
                municipios = await _municipioRepository.ObterPorEstadoAsync(estado_id.Value);
                _logger.LogInformation("📊 Municípios encontrados para o estado: {Count}", municipios.Count());
            }
            else
            {
                _logger.LogInformation("🌍 Obtendo todos os municípios (sem filtro)");
                municipios = await _municipioRepository.ObterTodosAsync();
                _logger.LogInformation("📊 Total de municípios: {Count}", municipios.Count());
            }
            
            var municipiosDto = _mapper.Map<IEnumerable<MunicipioFrontendDto>>(municipios);
            _logger.LogInformation("✅ Retornando {Count} municípios mapeados", municipiosDto.Count());
            
            return Ok(municipiosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao obter municípios. EstadoId: {EstadoId}", estado_id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém municípios ativos
    /// </summary>
    [HttpGet("municipios/ativos")]
    public async Task<ActionResult<IEnumerable<MunicipioFrontendDto>>> ObterMunicipiosAtivos()
    {
        try
        {
            _logger.LogDebug("Obtendo municípios ativos");
            
            var municipios = await _municipioRepository.ObterTodosAsync();
            var municipiosDto = _mapper.Map<IEnumerable<MunicipioFrontendDto>>(municipios);
            
            _logger.LogDebug("Obtidos {Count} municípios ativos", municipiosDto.Count());
            return Ok(municipiosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter municípios ativos");
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria um novo município
    /// </summary>
    [HttpPost("municipios")]
    public async Task<ActionResult<MunicipioDto>> CriarMunicipio([FromBody] CriarMunicipioDto criarMunicipioDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar se o estado existe
            var estado = await _estadoRepository.ObterPorIdAsync(criarMunicipioDto.EstadoId);
            if (estado == null)
                return BadRequest(new { error_code = "ESTADO_NAO_ENCONTRADO", error_description = "Estado não encontrado" });

            var municipio = new Municipio(criarMunicipioDto.Nome, criarMunicipioDto.CodigoIbge, criarMunicipioDto.EstadoId, 
                criarMunicipioDto.CepPrincipal, criarMunicipioDto.Latitude, criarMunicipioDto.Longitude);
            
            await _municipioRepository.AdicionarAsync(municipio);

            var municipioDto = _mapper.Map<MunicipioDto>(municipio);
            return CreatedAtAction(nameof(ObterMunicipioPorId), new { id = municipio.Id }, municipioDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar município: {Nome}", criarMunicipioDto.Nome);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza um município
    /// </summary>
    [HttpPut("municipios/{id:int}")]
    public async Task<ActionResult<MunicipioDto>> AtualizarMunicipio(int id, [FromBody] AtualizarMunicipioDto atualizarMunicipioDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var municipio = await _municipioRepository.ObterPorIdAsync(id);
            if (municipio == null)
                return NotFound(new { error_code = "MUNICIPIO_NAO_ENCONTRADO", error_description = "Município não encontrado" });

            municipio.Atualizar(atualizarMunicipioDto.Nome, atualizarMunicipioDto.CodigoIbge, 
                atualizarMunicipioDto.CepPrincipal, atualizarMunicipioDto.Latitude, atualizarMunicipioDto.Longitude);
            
            await _municipioRepository.AtualizarAsync(municipio);

            var municipioDto = _mapper.Map<MunicipioDto>(municipio);
            return Ok(municipioDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar município: {Id}", id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Remove um município
    /// </summary>
    [HttpDelete("municipios/{id:int}")]
    public async Task<ActionResult> RemoverMunicipio(int id)
    {
        try
        {
            var municipio = await _municipioRepository.ObterPorIdAsync(id);
            if (municipio == null)
                return NotFound(new { error_code = "MUNICIPIO_NAO_ENCONTRADO", error_description = "Município não encontrado" });

            // Verificar se há endereços associados
            var enderecos = await _enderecoRepository.ObterPorMunicipioAsync(id);
            if (enderecos.Any())
                return BadRequest(new { error_code = "MUNICIPIO_COM_ENDERECOS", error_description = "Não é possível remover um município que possui endereços" });

            await _municipioRepository.RemoverAsync(municipio);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover município: {Id}", id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém municípios por UF ID (compatibilidade com frontend)
    /// </summary>
    [HttpGet("municipios/uf/{ufId:int}")]
    public async Task<ActionResult<IEnumerable<MunicipioFrontendDto>>> ObterMunicipiosPorUfId(int ufId)
    {
        try
        {
            _logger.LogInformation("🔍 ObterMunicipiosPorUfId chamado - ufId: {UfId}", ufId);
            
            var municipios = await _municipioRepository.ObterPorEstadoAsync(ufId);
            _logger.LogInformation("📊 Municípios encontrados para UF ID {UfId}: {Count}", ufId, municipios.Count());
            
            var municipiosDto = _mapper.Map<IEnumerable<MunicipioFrontendDto>>(municipios);
            _logger.LogInformation("✅ Retornando {Count} municípios mapeados", municipiosDto.Count());
            
            return Ok(municipiosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao obter municípios por UF ID: {UfId}", ufId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém municípios por UF em formato dropdown
    /// </summary>
    [HttpGet("municipios/uf/{ufId:int}/dropdown")]
    public async Task<ActionResult<IEnumerable<DropdownMunicipioDto>>> ObterMunicipiosDropdownPorUf(int ufId)
    {
        try
        {
            var municipios = await _municipioRepository.ObterPorEstadoAsync(ufId);
            var dropdownDto = municipios.Select(m => new DropdownMunicipioDto
            {
                Id = m.Id,
                Nome = m.Nome,
                CodigoIbge = m.CodigoIbge
            });
            
            return Ok(dropdownDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter dropdown de municípios por UF: {UfId}", ufId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém municípios com paginação
    /// </summary>
    [HttpGet("municipios/paginado")]
    public async Task<ActionResult<PaginatedResponse<MunicipioDto>>> ObterMunicipiosPaginado(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] int? ufId = null,
        [FromQuery] string? search = null)
    {
        try
        {
            // Validação de parâmetros
            if (page < 1)
            {
                _logger.LogWarning("Página inválida: {Page}", page);
                return BadRequest(new { error_code = "PAGINA_INVALIDA", error_description = "Página deve ser maior que zero" });
            }
            
            if (size < 1 || size > 100)
            {
                _logger.LogWarning("Tamanho de página inválido: {Size}", size);
                size = Math.Max(1, Math.Min(100, size)); // Corrige automaticamente
            }

            if (ufId.HasValue && ufId.Value <= 0)
            {
                _logger.LogWarning("ID de UF inválido: {UfId}", ufId);
                return BadRequest(new { error_code = "UF_ID_INVALIDO", error_description = "ID da UF deve ser maior que zero" });
            }

            _logger.LogDebug("Obtendo municípios paginados: Page={Page}, Size={Size}, UfId={UfId}, Search={Search}", 
                page, size, ufId, search);

            var (items, totalCount) = await _municipioRepository.ObterPaginadoAsync(page, size, ufId, search);
            var municipiosDto = _mapper.Map<IEnumerable<MunicipioDto>>(items);
            
            var totalPages = (int)Math.Ceiling((double)totalCount / size);
            
            var response = new PaginatedResponse<MunicipioDto>
            {
                Items = municipiosDto,
                TotalItems = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = size
            };
            
            _logger.LogDebug("Retornando {Count} municípios de {Total} total", municipiosDto.Count(), totalCount);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter municípios paginados: Page={Page}, Size={Size}, UfId={UfId}", 
                page, size, ufId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Verifica se código IBGE existe
    /// </summary>
    [HttpGet("municipios/existe-codigo-ibge/{codigoIbge:int}")]
    public async Task<ActionResult<ExistenceResponseDto>> VerificarCodigoIbgeExiste(int codigoIbge, [FromQuery] int? idExcluir = null)
    {
        try
        {
            var existe = await _municipioRepository.ExisteCodigoIbgeAsync(codigoIbge, idExcluir);
            
            return Ok(new ExistenceResponseDto 
            { 
                Exists = existe
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar código IBGE: {CodigoIbge}", codigoIbge);
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

    /// <summary>
    /// Obtém todos os endereços
    /// </summary>
    [HttpGet("enderecos")]
    public async Task<ActionResult<IEnumerable<EnderecoDto>>> ObterEnderecos()
    {
        try
        {
            var enderecos = await _enderecoRepository.ObterTodosAsync();
            var enderecosDto = _mapper.Map<IEnumerable<EnderecoDto>>(enderecos);
            return Ok(enderecosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter endereços");
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém endereço por ID
    /// </summary>
    [HttpGet("enderecos/{id:int}")]
    public async Task<ActionResult<EnderecoDto>> ObterEnderecoPorId(int id)
    {
        try
        {
            var endereco = await _enderecoRepository.ObterPorIdAsync(id);
            if (endereco == null)
                return NotFound(new { error_code = "ENDERECO_NAO_ENCONTRADO", error_description = "Endereço não encontrado" });

            var enderecoDto = _mapper.Map<EnderecoDto>(endereco);
            return Ok(enderecoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter endereço por ID: {Id}", id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria um novo endereço
    /// </summary>
    [HttpPost("enderecos")]
    public async Task<ActionResult<EnderecoDto>> CriarEndereco([FromBody] CriarEnderecoDto criarEnderecoDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar se o município existe
            var municipio = await _municipioRepository.ObterPorIdAsync(criarEnderecoDto.MunicipioId);
            if (municipio == null)
                return BadRequest(new { error_code = "MUNICIPIO_NAO_ENCONTRADO", error_description = "Município não encontrado" });

            var endereco = new Endereco(criarEnderecoDto.Cep, criarEnderecoDto.Logradouro, criarEnderecoDto.Bairro,
                criarEnderecoDto.MunicipioId, criarEnderecoDto.EstadoId, criarEnderecoDto.Numero, criarEnderecoDto.Complemento,
                criarEnderecoDto.Latitude, criarEnderecoDto.Longitude);
            
            await _enderecoRepository.AdicionarAsync(endereco);

            var enderecoDto = _mapper.Map<EnderecoDto>(endereco);
            return CreatedAtAction(nameof(ObterEnderecoPorId), new { id = endereco.Id }, enderecoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar endereço: {Logradouro}", criarEnderecoDto.Logradouro);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza um endereço
    /// </summary>
    [HttpPut("enderecos/{id:int}")]
    public async Task<ActionResult<EnderecoDto>> AtualizarEndereco(int id, [FromBody] AtualizarEnderecoDto atualizarEnderecoDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var endereco = await _enderecoRepository.ObterPorIdAsync(id);
            if (endereco == null)
                return NotFound(new { error_code = "ENDERECO_NAO_ENCONTRADO", error_description = "Endereço não encontrado" });

            endereco.Atualizar(atualizarEnderecoDto.Cep, atualizarEnderecoDto.Logradouro, atualizarEnderecoDto.Bairro,
                atualizarEnderecoDto.Numero, atualizarEnderecoDto.Complemento, atualizarEnderecoDto.Latitude, atualizarEnderecoDto.Longitude);
            
            await _enderecoRepository.AtualizarAsync(endereco);

            var enderecoDto = _mapper.Map<EnderecoDto>(endereco);
            return Ok(enderecoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar endereço: {Id}", id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Remove um endereço
    /// </summary>
    [HttpDelete("enderecos/{id:int}")]
    public async Task<ActionResult> RemoverEndereco(int id)
    {
        try
        {
            var endereco = await _enderecoRepository.ObterPorIdAsync(id);
            if (endereco == null)
                return NotFound(new { error_code = "ENDERECO_NAO_ENCONTRADO", error_description = "Endereço não encontrado" });

            await _enderecoRepository.RemoverAsync(endereco);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover endereço: {Id}", id);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    #endregion
}