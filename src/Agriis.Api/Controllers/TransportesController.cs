using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Agriis.Pedidos.Aplicacao.Interfaces;
using Agriis.Pedidos.Aplicacao.DTOs;
using FluentValidation;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controller para operações de transporte
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransportesController : ControllerBase
{
    private readonly ITransporteService _transporteService;
    private readonly IValidator<CalcularFreteDto> _calcularFreteValidator;
    private readonly IValidator<CalcularFreteConsolidadoDto> _calcularFreteConsolidadoValidator;
    private readonly IValidator<AgendarTransporteDto> _agendarTransporteValidator;
    private readonly IValidator<ReagendarTransporteDto> _reagendarTransporteValidator;
    private readonly IValidator<AtualizarValorFreteDto> _atualizarValorFreteValidator;
    private readonly IValidator<ValidarAgendamentosDto> _validarAgendamentosValidator;
    private readonly ILogger<TransportesController> _logger;

    public TransportesController(
        ITransporteService transporteService,
        IValidator<CalcularFreteDto> calcularFreteValidator,
        IValidator<CalcularFreteConsolidadoDto> calcularFreteConsolidadoValidator,
        IValidator<AgendarTransporteDto> agendarTransporteValidator,
        IValidator<ReagendarTransporteDto> reagendarTransporteValidator,
        IValidator<AtualizarValorFreteDto> atualizarValorFreteValidator,
        IValidator<ValidarAgendamentosDto> validarAgendamentosValidator,
        ILogger<TransportesController> logger)
    {
        _transporteService = transporteService ?? throw new ArgumentNullException(nameof(transporteService));
        _calcularFreteValidator = calcularFreteValidator ?? throw new ArgumentNullException(nameof(calcularFreteValidator));
        _calcularFreteConsolidadoValidator = calcularFreteConsolidadoValidator ?? throw new ArgumentNullException(nameof(calcularFreteConsolidadoValidator));
        _agendarTransporteValidator = agendarTransporteValidator ?? throw new ArgumentNullException(nameof(agendarTransporteValidator));
        _reagendarTransporteValidator = reagendarTransporteValidator ?? throw new ArgumentNullException(nameof(reagendarTransporteValidator));
        _atualizarValorFreteValidator = atualizarValorFreteValidator ?? throw new ArgumentNullException(nameof(atualizarValorFreteValidator));
        _validarAgendamentosValidator = validarAgendamentosValidator ?? throw new ArgumentNullException(nameof(validarAgendamentosValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calcula o frete para um produto
    /// </summary>
    /// <param name="dto">Dados para cálculo do frete</param>
    /// <returns>Resultado do cálculo de frete</returns>
    [HttpPost("calcular-frete")]
    public async Task<IActionResult> CalcularFrete([FromBody] CalcularFreteDto dto)
    {
        var validationResult = await _calcularFreteValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { 
                error_code = "VALIDATION_ERROR", 
                error_description = "Dados inválidos",
                errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var resultado = await _transporteService.CalcularFreteAsync(dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { 
                error_code = "CALCULATION_ERROR", 
                error_description = resultado.Error 
            });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Calcula o frete consolidado para múltiplos produtos
    /// </summary>
    /// <param name="dto">Dados para cálculo consolidado</param>
    /// <returns>Resultado do cálculo consolidado</returns>
    [HttpPost("calcular-frete-consolidado")]
    public async Task<IActionResult> CalcularFreteConsolidado([FromBody] CalcularFreteConsolidadoDto dto)
    {
        var validationResult = await _calcularFreteConsolidadoValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { 
                error_code = "VALIDATION_ERROR", 
                error_description = "Dados inválidos",
                errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var resultado = await _transporteService.CalcularFreteConsolidadoAsync(dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { 
                error_code = "CALCULATION_ERROR", 
                error_description = resultado.Error 
            });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Agenda um transporte para um item de pedido
    /// </summary>
    /// <param name="dto">Dados do agendamento</param>
    /// <returns>Transporte agendado</returns>
    [HttpPost("agendar")]
    public async Task<IActionResult> AgendarTransporte([FromBody] AgendarTransporteDto dto)
    {
        var validationResult = await _agendarTransporteValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { 
                error_code = "VALIDATION_ERROR", 
                error_description = "Dados inválidos",
                errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var resultado = await _transporteService.AgendarTransporteAsync(dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { 
                error_code = "SCHEDULING_ERROR", 
                error_description = resultado.Error 
            });
        }

        return CreatedAtAction(nameof(ObterTransporte), new { id = resultado.Value.Id }, resultado.Value);
    }

    /// <summary>
    /// Reagenda um transporte existente
    /// </summary>
    /// <param name="id">ID do transporte</param>
    /// <param name="dto">Dados do reagendamento</param>
    /// <returns>Transporte reagendado</returns>
    [HttpPut("{id}/reagendar")]
    public async Task<IActionResult> ReagendarTransporte(int id, [FromBody] ReagendarTransporteDto dto)
    {
        var validationResult = await _reagendarTransporteValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { 
                error_code = "VALIDATION_ERROR", 
                error_description = "Dados inválidos",
                errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var resultado = await _transporteService.ReagendarTransporteAsync(id, dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { 
                error_code = "RESCHEDULING_ERROR", 
                error_description = resultado.Error 
            });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Atualiza o valor do frete de um transporte
    /// </summary>
    /// <param name="id">ID do transporte</param>
    /// <param name="dto">Dados da atualização</param>
    /// <returns>Transporte atualizado</returns>
    [HttpPut("{id}/valor-frete")]
    public async Task<IActionResult> AtualizarValorFrete(int id, [FromBody] AtualizarValorFreteDto dto)
    {
        var validationResult = await _atualizarValorFreteValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { 
                error_code = "VALIDATION_ERROR", 
                error_description = "Dados inválidos",
                errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var resultado = await _transporteService.AtualizarValorFreteAsync(id, dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { 
                error_code = "UPDATE_ERROR", 
                error_description = resultado.Error 
            });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém um transporte por ID
    /// </summary>
    /// <param name="id">ID do transporte</param>
    /// <returns>Dados do transporte</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterTransporte(int id)
    {
        var resultado = await _transporteService.ObterTransportePorIdAsync(id);
        
        if (!resultado.IsSuccess)
        {
            return NotFound(new { 
                error_code = "TRANSPORT_NOT_FOUND", 
                error_description = resultado.Error 
            });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Lista os transportes de um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Lista de transportes</returns>
    [HttpGet("pedido/{pedidoId}")]
    public async Task<IActionResult> ListarTransportesPedido(int pedidoId)
    {
        var resultado = await _transporteService.ListarTransportesPedidoAsync(pedidoId);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { 
                error_code = "LIST_ERROR", 
                error_description = resultado.Error 
            });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Valida múltiplos agendamentos
    /// </summary>
    /// <param name="dto">Dados dos agendamentos</param>
    /// <returns>Resultado da validação</returns>
    [HttpPost("validar-agendamentos")]
    public async Task<IActionResult> ValidarMultiplosAgendamentos([FromBody] ValidarAgendamentosDto dto)
    {
        var validationResult = await _validarAgendamentosValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { 
                error_code = "VALIDATION_ERROR", 
                error_description = "Dados inválidos",
                errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var resultado = await _transporteService.ValidarMultiplosAgendamentosAsync(dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { 
                error_code = "VALIDATION_ERROR", 
                error_description = resultado.Error 
            });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém o resumo de transporte de um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Resumo do transporte</returns>
    [HttpGet("pedido/{pedidoId}/resumo")]
    public async Task<IActionResult> ObterResumoTransporte(int pedidoId)
    {
        var resultado = await _transporteService.ObterResumoTransporteAsync(pedidoId);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { 
                error_code = "SUMMARY_ERROR", 
                error_description = resultado.Error 
            });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Cancela um transporte agendado
    /// </summary>
    /// <param name="id">ID do transporte</param>
    /// <param name="motivo">Motivo do cancelamento</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelarTransporte(int id, [FromQuery] string? motivo = null)
    {
        var resultado = await _transporteService.CancelarTransporteAsync(id, motivo);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { 
                error_code = "CANCELLATION_ERROR", 
                error_description = resultado.Error 
            });
        }

        return NoContent();
    }
}