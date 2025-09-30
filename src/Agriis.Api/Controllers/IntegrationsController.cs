using Agriis.Compartilhado.Infraestrutura.Integracoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agriis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class IntegrationsController : ControllerBase
{
    private readonly IAwsService _awsService;
    private readonly INotificationService _notificationService;
    private readonly ICurrencyConverterService _currencyService;
    private readonly ILogger<IntegrationsController> _logger;

    public IntegrationsController(
        IAwsService awsService,
        INotificationService notificationService,
        ICurrencyConverterService currencyService,
        ILogger<IntegrationsController> logger)
    {
        _awsService = awsService ?? throw new ArgumentNullException(nameof(awsService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Upload de arquivo para S3
    /// </summary>
    [HttpPost("aws/upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string? key = null)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error_code = "INVALID_FILE", error_description = "Arquivo não fornecido ou vazio" });
        }

        try
        {
            var fileKey = key ?? $"uploads/{Guid.NewGuid()}/{file.FileName}";
            
            using var stream = file.OpenReadStream();
            var url = await _awsService.UploadFileAsync(fileKey, stream, file.ContentType);

            return Ok(new
            {
                success = true,
                file_key = fileKey,
                file_url = url,
                file_size = file.Length,
                content_type = file.ContentType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload do arquivo {FileName}", file.FileName);
            return StatusCode(500, new { error_code = "UPLOAD_ERROR", error_description = "Erro interno ao fazer upload" });
        }
    }

    /// <summary>
    /// Verificar se arquivo existe no S3
    /// </summary>
    [HttpGet("aws/exists/{*key}")]
    public async Task<IActionResult> FileExists(string key)
    {
        try
        {
            var exists = await _awsService.FileExistsAsync(key);
            return Ok(new { exists, key });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do arquivo {Key}", key);
            return StatusCode(500, new { error_code = "CHECK_ERROR", error_description = "Erro interno ao verificar arquivo" });
        }
    }

    /// <summary>
    /// Gerar URL pré-assinada para download
    /// </summary>
    [HttpGet("aws/presigned-url/{*key}")]
    public async Task<IActionResult> GetPreSignedUrl(string key, [FromQuery] int expirationMinutes = 60)
    {
        try
        {
            var expiration = TimeSpan.FromMinutes(expirationMinutes);
            var url = await _awsService.GetPreSignedUrlAsync(key, expiration);
            
            return Ok(new
            {
                presigned_url = url,
                key,
                expires_in_minutes = expirationMinutes,
                expires_at = DateTime.UtcNow.Add(expiration)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar URL pré-assinada para {Key}", key);
            return StatusCode(500, new { error_code = "PRESIGNED_URL_ERROR", error_description = "Erro interno ao gerar URL" });
        }
    }

    /// <summary>
    /// Listar arquivos no S3
    /// </summary>
    [HttpGet("aws/files")]
    public async Task<IActionResult> ListFiles([FromQuery] string prefix = "")
    {
        try
        {
            var files = await _awsService.ListFilesAsync(prefix);
            return Ok(new { files = files.ToList(), prefix, count = files.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar arquivos com prefixo {Prefix}", prefix);
            return StatusCode(500, new { error_code = "LIST_ERROR", error_description = "Erro interno ao listar arquivos" });
        }
    }

    /// <summary>
    /// Enviar notificação para usuário específico
    /// </summary>
    [HttpPost("notifications/user/{userId}")]
    public async Task<IActionResult> SendUserNotification(string userId, [FromBody] NotificationRequest request)
    {
        try
        {
            await _notificationService.SendNotificationToUserAsync(userId, request.Message, request.Data);
            return Ok(new { success = true, message = "Notificação enviada com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação para usuário {UserId}", userId);
            return StatusCode(500, new { error_code = "NOTIFICATION_ERROR", error_description = "Erro interno ao enviar notificação" });
        }
    }

    /// <summary>
    /// Enviar notificação para grupo
    /// </summary>
    [HttpPost("notifications/group/{groupName}")]
    public async Task<IActionResult> SendGroupNotification(string groupName, [FromBody] NotificationRequest request)
    {
        try
        {
            await _notificationService.SendNotificationToGroupAsync(groupName, request.Message, request.Data);
            return Ok(new { success = true, message = "Notificação enviada para o grupo com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação para grupo {GroupName}", groupName);
            return StatusCode(500, new { error_code = "NOTIFICATION_ERROR", error_description = "Erro interno ao enviar notificação" });
        }
    }

    /// <summary>
    /// Enviar notificação broadcast
    /// </summary>
    [HttpPost("notifications/broadcast")]
    public async Task<IActionResult> SendBroadcastNotification([FromBody] NotificationRequest request)
    {
        try
        {
            await _notificationService.SendNotificationToAllAsync(request.Message, request.Data);
            return Ok(new { success = true, message = "Notificação broadcast enviada com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação broadcast");
            return StatusCode(500, new { error_code = "NOTIFICATION_ERROR", error_description = "Erro interno ao enviar notificação" });
        }
    }

    /// <summary>
    /// Converter moeda
    /// </summary>
    [HttpGet("currency/convert")]
    public async Task<IActionResult> ConvertCurrency(
        [FromQuery] decimal amount,
        [FromQuery] string fromCurrency,
        [FromQuery] string toCurrency)
    {
        try
        {
            var convertedAmount = await _currencyService.ConvertAsync(amount, fromCurrency, toCurrency);
            var exchangeRate = await _currencyService.GetExchangeRateAsync(fromCurrency, toCurrency);

            return Ok(new
            {
                original_amount = amount,
                from_currency = fromCurrency.ToUpper(),
                to_currency = toCurrency.ToUpper(),
                converted_amount = convertedAmount,
                exchange_rate = exchangeRate,
                conversion_date = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao converter {Amount} de {FromCurrency} para {ToCurrency}",
                amount, fromCurrency, toCurrency);
            return StatusCode(500, new { error_code = "CONVERSION_ERROR", error_description = "Erro interno na conversão de moeda" });
        }
    }

    /// <summary>
    /// Obter taxa de câmbio
    /// </summary>
    [HttpGet("currency/rate")]
    public async Task<IActionResult> GetExchangeRate(
        [FromQuery] string fromCurrency,
        [FromQuery] string toCurrency)
    {
        try
        {
            var rate = await _currencyService.GetExchangeRateAsync(fromCurrency, toCurrency);

            return Ok(new
            {
                from_currency = fromCurrency.ToUpper(),
                to_currency = toCurrency.ToUpper(),
                exchange_rate = rate,
                rate_date = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter taxa de câmbio {FromCurrency}/{ToCurrency}",
                fromCurrency, toCurrency);
            return StatusCode(500, new { error_code = "RATE_ERROR", error_description = "Erro interno ao obter taxa de câmbio" });
        }
    }

    /// <summary>
    /// Obter todas as taxas de câmbio para uma moeda base
    /// </summary>
    [HttpGet("currency/rates/{baseCurrency}")]
    public async Task<IActionResult> GetAllRates(string baseCurrency)
    {
        try
        {
            var rates = await _currencyService.GetAllRatesAsync(baseCurrency);

            return Ok(new
            {
                base_currency = baseCurrency.ToUpper(),
                rates,
                rate_count = rates.Count,
                rate_date = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter todas as taxas para moeda base {BaseCurrency}", baseCurrency);
            return StatusCode(500, new { error_code = "RATES_ERROR", error_description = "Erro interno ao obter taxas de câmbio" });
        }
    }

    /// <summary>
    /// Atualizar taxas de câmbio manualmente
    /// </summary>
    [HttpPost("currency/refresh")]
    public async Task<IActionResult> RefreshRates()
    {
        try
        {
            await _currencyService.RefreshRatesAsync();
            return Ok(new { success = true, message = "Taxas de câmbio atualizadas com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar taxas de câmbio manualmente");
            return StatusCode(500, new { error_code = "REFRESH_ERROR", error_description = "Erro interno ao atualizar taxas" });
        }
    }
}

public class NotificationRequest
{
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}