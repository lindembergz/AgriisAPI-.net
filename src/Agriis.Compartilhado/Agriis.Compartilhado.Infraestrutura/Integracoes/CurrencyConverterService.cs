using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Agriis.Compartilhado.Infraestrutura.Integracoes;

public interface ICurrencyConverterService
{
    Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
    Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
    Task<Dictionary<string, decimal>> GetAllRatesAsync(string baseCurrency = "USD");
    Task RefreshRatesAsync();
}

public class CurrencyConverterService : ICurrencyConverterService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CurrencyConverterService> _logger;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly TimeSpan _cacheExpiration;

    public CurrencyConverterService(
        HttpClient httpClient, 
        IMemoryCache cache, 
        IConfiguration configuration, 
        ILogger<CurrencyConverterService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _apiKey = configuration["CurrencySettings:ApiKey"] ?? "";
        _baseUrl = configuration["CurrencySettings:BaseUrl"] ?? "https://api.exchangerate-api.com/v4/latest";
        _cacheExpiration = TimeSpan.FromMinutes(configuration.GetValue<int>("CurrencySettings:CacheExpirationMinutes", 60));
    }

    public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
        {
            throw new ArgumentException("Moedas de origem e destino devem ser especificadas");
        }

        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return amount;
        }

        try
        {
            var rate = await GetExchangeRateAsync(fromCurrency, toCurrency);
            var convertedAmount = amount * rate;

            _logger.LogInformation("Conversão realizada: {Amount} {FromCurrency} = {ConvertedAmount} {ToCurrency} (Taxa: {Rate})",
                amount, fromCurrency, convertedAmount, toCurrency, rate);

            return Math.Round(convertedAmount, 4);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao converter {Amount} de {FromCurrency} para {ToCurrency}",
                amount, fromCurrency, toCurrency);
            throw;
        }
    }

    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
        {
            throw new ArgumentException("Moedas de origem e destino devem ser especificadas");
        }

        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return 1.0m;
        }

        var cacheKey = $"exchange_rate_{fromCurrency}_{toCurrency}";
        
        if (_cache.TryGetValue(cacheKey, out decimal cachedRate))
        {
            _logger.LogDebug("Taxa de câmbio obtida do cache: {FromCurrency}/{ToCurrency} = {Rate}",
                fromCurrency, toCurrency, cachedRate);
            return cachedRate;
        }

        try
        {
            var rates = await GetAllRatesAsync(fromCurrency);
            
            if (!rates.TryGetValue(toCurrency.ToUpper(), out decimal rate))
            {
                throw new InvalidOperationException($"Taxa de câmbio não encontrada para {fromCurrency}/{toCurrency}");
            }

            _cache.Set(cacheKey, rate, _cacheExpiration);
            
            _logger.LogInformation("Taxa de câmbio obtida da API: {FromCurrency}/{ToCurrency} = {Rate}",
                fromCurrency, toCurrency, rate);

            return rate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter taxa de câmbio {FromCurrency}/{ToCurrency}",
                fromCurrency, toCurrency);
            
            // Tentar usar taxa padrão em caso de erro
            if (TryGetFallbackRate(fromCurrency, toCurrency, out decimal fallbackRate))
            {
                _logger.LogWarning("Usando taxa de câmbio padrão: {FromCurrency}/{ToCurrency} = {Rate}",
                    fromCurrency, toCurrency, fallbackRate);
                return fallbackRate;
            }
            
            throw;
        }
    }

    public async Task<Dictionary<string, decimal>> GetAllRatesAsync(string baseCurrency = "USD")
    {
        var cacheKey = $"all_rates_{baseCurrency}";
        
        if (_cache.TryGetValue(cacheKey, out Dictionary<string, decimal>? cachedRates) && cachedRates != null)
        {
            _logger.LogDebug("Taxas de câmbio obtidas do cache para base {BaseCurrency}", baseCurrency);
            return cachedRates;
        }

        try
        {
            var url = string.IsNullOrEmpty(_apiKey) 
                ? $"{_baseUrl}/{baseCurrency}"
                : $"{_baseUrl}/{baseCurrency}?access_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var exchangeRateResponse = JsonSerializer.Deserialize<ExchangeRateResponse>(content);

            if (exchangeRateResponse?.Rates == null)
            {
                throw new InvalidOperationException("Resposta inválida da API de câmbio");
            }

            var rates = exchangeRateResponse.Rates;
            _cache.Set(cacheKey, rates, _cacheExpiration);

            _logger.LogInformation("Taxas de câmbio atualizadas para base {BaseCurrency}. {Count} moedas disponíveis",
                baseCurrency, rates.Count);

            return rates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter todas as taxas de câmbio para base {BaseCurrency}", baseCurrency);
            
            // Retornar taxas padrão em caso de erro
            var fallbackRates = GetFallbackRates(baseCurrency);
            if (fallbackRates.Any())
            {
                _logger.LogWarning("Usando taxas de câmbio padrão para base {BaseCurrency}", baseCurrency);
                return fallbackRates;
            }
            
            throw;
        }
    }

    public async Task RefreshRatesAsync()
    {
        try
        {
            _logger.LogInformation("Iniciando atualização manual das taxas de câmbio");

            // Limpar cache
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0);
            }

            // Recarregar taxas principais
            var baseCurrencies = new[] { "USD", "BRL", "EUR" };
            
            foreach (var baseCurrency in baseCurrencies)
            {
                await GetAllRatesAsync(baseCurrency);
            }

            _logger.LogInformation("Atualização manual das taxas de câmbio concluída");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante atualização manual das taxas de câmbio");
            throw;
        }
    }

    private bool TryGetFallbackRate(string fromCurrency, string toCurrency, out decimal rate)
    {
        rate = 0;

        // Taxas padrão para casos de emergência (valores aproximados)
        var fallbackRates = new Dictionary<string, Dictionary<string, decimal>>
        {
            ["USD"] = new()
            {
                ["BRL"] = 5.0m,
                ["EUR"] = 0.85m,
                ["GBP"] = 0.75m
            },
            ["BRL"] = new()
            {
                ["USD"] = 0.20m,
                ["EUR"] = 0.17m,
                ["GBP"] = 0.15m
            },
            ["EUR"] = new()
            {
                ["USD"] = 1.18m,
                ["BRL"] = 5.88m,
                ["GBP"] = 0.88m
            }
        };

        if (fallbackRates.TryGetValue(fromCurrency.ToUpper(), out var fromRates) &&
            fromRates.TryGetValue(toCurrency.ToUpper(), out rate))
        {
            return true;
        }

        return false;
    }

    private Dictionary<string, decimal> GetFallbackRates(string baseCurrency)
    {
        // Taxas padrão básicas para casos de emergência
        return baseCurrency.ToUpper() switch
        {
            "USD" => new Dictionary<string, decimal>
            {
                ["BRL"] = 5.0m,
                ["EUR"] = 0.85m,
                ["GBP"] = 0.75m,
                ["JPY"] = 110.0m,
                ["CAD"] = 1.25m
            },
            "BRL" => new Dictionary<string, decimal>
            {
                ["USD"] = 0.20m,
                ["EUR"] = 0.17m,
                ["GBP"] = 0.15m,
                ["JPY"] = 22.0m,
                ["CAD"] = 0.25m
            },
            _ => new Dictionary<string, decimal>()
        };
    }
}

public class ExchangeRateResponse
{
    public string? Base { get; set; }
    public string? Date { get; set; }
    public Dictionary<string, decimal> Rates { get; set; } = new();
}

// Background service para atualizar taxas periodicamente
public class CurrencyRateUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CurrencyRateUpdateService> _logger;
    private readonly TimeSpan _updateInterval;

    public CurrencyRateUpdateService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<CurrencyRateUpdateService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var intervalMinutes = configuration.GetValue<int>("CurrencySettings:UpdateIntervalMinutes", 60);
        _updateInterval = TimeSpan.FromMinutes(intervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Serviço de atualização de taxas de câmbio iniciado. Intervalo: {Interval}", _updateInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var currencyService = scope.ServiceProvider.GetRequiredService<ICurrencyConverterService>();
                
                await currencyService.RefreshRatesAsync();
                _logger.LogInformation("Taxas de câmbio atualizadas automaticamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante atualização automática das taxas de câmbio");
            }

            await Task.Delay(_updateInterval, stoppingToken);
        }
    }
}