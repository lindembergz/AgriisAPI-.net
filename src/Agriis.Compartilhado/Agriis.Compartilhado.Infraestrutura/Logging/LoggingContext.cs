using System.Collections.Concurrent;

namespace Agriis.Compartilhado.Infraestrutura.Logging;

/// <summary>
/// Implementação do contexto de logging estruturado
/// </summary>
public class LoggingContext : ILoggingContext
{
    private readonly ConcurrentDictionary<string, object?> _properties = new();

    public string? CorrelationId
    {
        get => GetProperty<string>(nameof(CorrelationId));
        set => SetProperty(nameof(CorrelationId), value);
    }

    public string? UserId
    {
        get => GetProperty<string>(nameof(UserId));
        set => SetProperty(nameof(UserId), value);
    }

    public string? UserEmail
    {
        get => GetProperty<string>(nameof(UserEmail));
        set => SetProperty(nameof(UserEmail), value);
    }

    public string? RequestPath
    {
        get => GetProperty<string>(nameof(RequestPath));
        set => SetProperty(nameof(RequestPath), value);
    }

    public string? RequestMethod
    {
        get => GetProperty<string>(nameof(RequestMethod));
        set => SetProperty(nameof(RequestMethod), value);
    }

    public string? RemoteIpAddress
    {
        get => GetProperty<string>(nameof(RemoteIpAddress));
        set => SetProperty(nameof(RemoteIpAddress), value);
    }

    public string? UserAgent
    {
        get => GetProperty<string>(nameof(UserAgent));
        set => SetProperty(nameof(UserAgent), value);
    }

    public void AddProperty(string key, object? value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        SetProperty(key, value);
    }

    public void RemoveProperty(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        _properties.TryRemove(key, out _);
    }

    public IDictionary<string, object?> GetProperties()
    {
        return new Dictionary<string, object?>(_properties);
    }

    public void Clear()
    {
        _properties.Clear();
    }

    private T? GetProperty<T>(string key)
    {
        if (_properties.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;

        return default;
    }

    private void SetProperty(string key, object? value)
    {
        if (value == null)
        {
            _properties.TryRemove(key, out _);
        }
        else
        {
            _properties.AddOrUpdate(key, value, (_, _) => value);
        }
    }
}