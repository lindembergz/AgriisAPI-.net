namespace Agriis.Enderecos.Aplicacao.DTOs;

/// <summary>
/// DTO para Município
/// </summary>
public class MunicipioDto
{
    /// <summary>
    /// ID do município
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome do município
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Código IBGE do município
    /// </summary>
    public int CodigoIbge { get; set; }
    
    /// <summary>
    /// CEP principal do município
    /// </summary>
    public string? CepPrincipal { get; set; }
    
    /// <summary>
    /// Latitude do centro do município
    /// </summary>
    public double? Latitude { get; set; }
    
    /// <summary>
    /// Longitude do centro do município
    /// </summary>
    public double? Longitude { get; set; }
    
    /// <summary>
    /// ID do estado
    /// </summary>
    public int EstadoId { get; set; }
    
    /// <summary>
    /// Dados do estado
    /// </summary>
    public EstadoResumoDto Estado { get; set; } = new();
    
    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime DataCriacao { get; set; }
    
    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? DataAtualizacao { get; set; }
}

/// <summary>
/// DTO para criação de Município
/// </summary>
public class CriarMunicipioDto
{
    /// <summary>
    /// Nome do município
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Código IBGE do município
    /// </summary>
    public int CodigoIbge { get; set; }
    
    /// <summary>
    /// ID do estado
    /// </summary>
    public int EstadoId { get; set; }
    
    /// <summary>
    /// CEP principal do município
    /// </summary>
    public string? CepPrincipal { get; set; }
    
    /// <summary>
    /// Latitude do centro do município
    /// </summary>
    public double? Latitude { get; set; }
    
    /// <summary>
    /// Longitude do centro do município
    /// </summary>
    public double? Longitude { get; set; }
}

/// <summary>
/// DTO para atualização de Município
/// </summary>
public class AtualizarMunicipioDto
{
    /// <summary>
    /// Nome do município
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Código IBGE do município
    /// </summary>
    public int CodigoIbge { get; set; }
    
    /// <summary>
    /// CEP principal do município
    /// </summary>
    public string? CepPrincipal { get; set; }
    
    /// <summary>
    /// Latitude do centro do município
    /// </summary>
    public double? Latitude { get; set; }
    
    /// <summary>
    /// Longitude do centro do município
    /// </summary>
    public double? Longitude { get; set; }
}

/// <summary>
/// DTO resumido para Município (para uso em listas)
/// </summary>
public class MunicipioResumoDto
{
    /// <summary>
    /// ID do município
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome do município
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Código IBGE do município
    /// </summary>
    public int CodigoIbge { get; set; }
    
    /// <summary>
    /// UF do estado
    /// </summary>
    public string EstadoUf { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome do estado
    /// </summary>
    public string EstadoNome { get; set; } = string.Empty;
}

/// <summary>
/// DTO para consulta de municípios próximos
/// </summary>
public class MunicipioProximoDto
{
    /// <summary>
    /// ID do município
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome do município
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// UF do estado
    /// </summary>
    public string EstadoUf { get; set; } = string.Empty;
    
    /// <summary>
    /// Latitude
    /// </summary>
    public double? Latitude { get; set; }
    
    /// <summary>
    /// Longitude
    /// </summary>
    public double? Longitude { get; set; }
    
    /// <summary>
    /// Distância em quilômetros do ponto de referência
    /// </summary>
    public double? DistanciaKm { get; set; }
}