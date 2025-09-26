using Microsoft.AspNetCore.Mvc;
using Agriis.Referencias.Aplicacao.Interfaces;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controlador para endpoints cascateados de referências (otimizado para dropdowns)
/// </summary>
[ApiController]
[Route("api/referencias")]
public class ReferenciasCascataController : ControllerBase
{
    private readonly IUfService _ufService;
    private readonly IMunicipioService _municipioService;
    private readonly IEmbalagemService _embalagemService;
    private readonly ILogger<ReferenciasCascataController> _logger;

    public ReferenciasCascataController(
        IUfService ufService,
        IMunicipioService municipioService,
        IEmbalagemService embalagemService,
        ILogger<ReferenciasCascataController> logger)
    {
        _ufService = ufService;
        _municipioService = municipioService;
        _embalagemService = embalagemService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém UFs de um país específico (otimizado para dropdowns)
    /// </summary>
    /// <param name="paisId">ID do país</param>
    [HttpGet("ufs/pais/{paisId:int}")]
    public async Task<IActionResult> ObterUfsPorPais(int paisId)
    {
        try
        {
            _logger.LogDebug("Obtendo UFs do país {PaisId} para dropdown cascateado", paisId);
            
            var ufs = await _ufService.ObterPorPaisAsync(paisId);
            
            // Retornar apenas dados essenciais para dropdown
            var ufsDropdown = ufs
                .Where(u => u.Ativo)
                .Select(u => new { 
                    Id = u.Id, 
                    Nome = u.Nome, 
                    Codigo = u.Codigo 
                })
                .OrderBy(u => u.Nome);
            
            _logger.LogDebug("Encontradas {Count} UFs ativas para o país {PaisId}", ufsDropdown.Count(), paisId);
            
            return Ok(ufsDropdown);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter UFs do país {PaisId}", paisId);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém municípios de uma UF específica (otimizado para dropdowns)
    /// </summary>
    /// <param name="ufId">ID da UF</param>
    [HttpGet("municipios/uf/{ufId:int}")]
    public async Task<IActionResult> ObterMunicipiosPorUf(int ufId)
    {
        try
        {
            _logger.LogDebug("Obtendo municípios da UF {UfId} para dropdown cascateado", ufId);
            
            var municipios = await _municipioService.ObterPorUfAsync(ufId);
            
            // Retornar apenas dados essenciais para dropdown
            var municipiosDropdown = municipios
                .Where(m => m.Ativo)
                .Select(m => new { 
                    Id = m.Id, 
                    Nome = m.Nome, 
                    CodigoIbge = m.CodigoIbge 
                })
                .OrderBy(m => m.Nome);
            
            _logger.LogDebug("Encontrados {Count} municípios ativos para a UF {UfId}", municipiosDropdown.Count(), ufId);
            
            return Ok(municipiosDropdown);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter municípios da UF {UfId}", ufId);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém embalagens de uma unidade de medida específica (otimizado para dropdowns)
    /// </summary>
    /// <param name="unidadeId">ID da unidade de medida</param>
    [HttpGet("embalagens/unidade/{unidadeId:int}")]
    public async Task<IActionResult> ObterEmbalagensPorUnidade(int unidadeId)
    {
        try
        {
            _logger.LogDebug("Obtendo embalagens da unidade {UnidadeId} para dropdown cascateado", unidadeId);
            
            var embalagens = await _embalagemService.ObterPorUnidadeMedidaAsync(unidadeId);
            
            // Retornar apenas dados essenciais para dropdown
            var embalagensDropdown = embalagens
                .Where(e => e.Ativo)
                .Select(e => new { 
                    Id = e.Id, 
                    Nome = e.Nome, 
                    Descricao = e.Descricao 
                })
                .OrderBy(e => e.Nome);
            
            _logger.LogDebug("Encontradas {Count} embalagens ativas para a unidade {UnidadeId}", embalagensDropdown.Count(), unidadeId);
            
            return Ok(embalagensDropdown);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter embalagens da unidade {UnidadeId}", unidadeId);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém dados geográficos completos (País > UF > Município) para um município específico
    /// </summary>
    /// <param name="municipioId">ID do município</param>
    [HttpGet("geografico/municipio/{municipioId:int}")]
    public async Task<IActionResult> ObterDadosGeograficosPorMunicipio(int municipioId)
    {
        try
        {
            _logger.LogDebug("Obtendo dados geográficos completos para o município {MunicipioId}", municipioId);
            
            var municipio = await _municipioService.ObterPorIdAsync(municipioId);
            
            if (municipio == null)
            {
                _logger.LogWarning("Município {MunicipioId} não encontrado", municipioId);
                return NotFound(new { 
                    ErrorCode = "ENTITY_NOT_FOUND", 
                    ErrorDescription = "Município não encontrado",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            var dadosGeograficos = new
            {
                Municipio = new { municipio.Id, municipio.Nome, municipio.CodigoIbge },
                Uf = new { Id = municipio.UfId, Nome = municipio.UfNome, Codigo = municipio.UfCodigo },
                Pais = new { Id = 1, Nome = "Brasil", Codigo = "BR" } // Assumindo Brasil como padrão
            };
            
            _logger.LogDebug("Dados geográficos obtidos para o município {MunicipioId}", municipioId);
            
            return Ok(dadosGeograficos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter dados geográficos do município {MunicipioId}", municipioId);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém todas as referências básicas para inicialização de formulários
    /// </summary>
    [HttpGet("inicializacao")]
    public async Task<IActionResult> ObterDadosInicializacao()
    {
        try
        {
            _logger.LogDebug("Obtendo dados de inicialização para formulários");
            
            // Buscar dados básicos mais comuns
            var paisesTask = _ufService.ObterPorPaisAsync(1); // Brasil (assumindo ID 1)
            
            await Task.WhenAll(paisesTask);
            
            var ufs = await paisesTask;
            
            var dadosInicializacao = new
            {
                Ufs = ufs.Where(u => u.Ativo).Select(u => new { u.Id, u.Nome, u.Codigo }).OrderBy(u => u.Nome),
                TiposAtividade = Enum.GetValues<Agriis.Referencias.Dominio.Enums.TipoAtividadeAgropecuaria>()
                    .Select(t => new { Valor = (int)t, Nome = t.ToString() }),
                TiposUnidadeMedida = Enum.GetValues<Agriis.Referencias.Dominio.Enums.TipoUnidadeMedida>()
                    .Select(t => new { Valor = (int)t, Nome = t.ToString() })
            };
            
            _logger.LogDebug("Dados de inicialização obtidos com sucesso");
            
            return Ok(dadosInicializacao);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter dados de inicialização");
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}