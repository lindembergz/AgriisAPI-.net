namespace Agriis.Enderecos.Aplicacao.DTOs;

/// <summary>
/// DTO para Endereço
/// </summary>
public class EnderecoDto
{
    /// <summary>
    /// ID do endereço
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// CEP do endereço
    /// </summary>
    public string Cep { get; set; } = string.Empty;
    
    /// <summary>
    /// CEP formatado (00000-000)
    /// </summary>
    public string CepFormatado { get; set; } = string.Empty;
    
    /// <summary>
    /// Logradouro
    /// </summary>
    public string Logradouro { get; set; } = string.Empty;
    
    /// <summary>
    /// Número do endereço
    /// </summary>
    public string? Numero { get; set; }
    
    /// <summary>
    /// Complemento
    /// </summary>
    public string? Complemento { get; set; }
    
    /// <summary>
    /// Bairro
    /// </summary>
    public string Bairro { get; set; } = string.Empty;
    
    /// <summary>
    /// Latitude específica do endereço
    /// </summary>
    public double? Latitude { get; set; }
    
    /// <summary>
    /// Longitude específica do endereço
    /// </summary>
    public double? Longitude { get; set; }
    
    /// <summary>
    /// ID do município
    /// </summary>
    public int MunicipioId { get; set; }
    
    /// <summary>
    /// Dados do município
    /// </summary>
    public MunicipioResumoDto Municipio { get; set; } = new();
    
    /// <summary>
    /// ID do estado
    /// </summary>
    public int EstadoId { get; set; }
    
    /// <summary>
    /// Dados do estado
    /// </summary>
    public EstadoResumoDto Estado { get; set; } = new();
    
    /// <summary>
    /// Endereço formatado completo
    /// </summary>
    public string EnderecoFormatado { get; set; } = string.Empty;
    
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
/// DTO para criação de Endereço
/// </summary>
public class CriarEnderecoDto
{
    /// <summary>
    /// CEP do endereço
    /// </summary>
    public string Cep { get; set; } = string.Empty;
    
    /// <summary>
    /// Logradouro
    /// </summary>
    public string Logradouro { get; set; } = string.Empty;
    
    /// <summary>
    /// Número do endereço
    /// </summary>
    public string? Numero { get; set; }
    
    /// <summary>
    /// Complemento
    /// </summary>
    public string? Complemento { get; set; }
    
    /// <summary>
    /// Bairro
    /// </summary>
    public string Bairro { get; set; } = string.Empty;
    
    /// <summary>
    /// ID do município
    /// </summary>
    public int MunicipioId { get; set; }
    
    /// <summary>
    /// ID do estado
    /// </summary>
    public int EstadoId { get; set; }
    
    /// <summary>
    /// Latitude específica do endereço
    /// </summary>
    public double? Latitude { get; set; }
    
    /// <summary>
    /// Longitude específica do endereço
    /// </summary>
    public double? Longitude { get; set; }
}

/// <summary>
/// DTO para atualização de Endereço
/// </summary>
public class AtualizarEnderecoDto
{
    /// <summary>
    /// CEP do endereço
    /// </summary>
    public string Cep { get; set; } = string.Empty;
    
    /// <summary>
    /// Logradouro
    /// </summary>
    public string Logradouro { get; set; } = string.Empty;
    
    /// <summary>
    /// Número do endereço
    /// </summary>
    public string? Numero { get; set; }
    
    /// <summary>
    /// Complemento
    /// </summary>
    public string? Complemento { get; set; }
    
    /// <summary>
    /// Bairro
    /// </summary>
    public string Bairro { get; set; } = string.Empty;
    
    /// <summary>
    /// Latitude específica do endereço
    /// </summary>
    public double? Latitude { get; set; }
    
    /// <summary>
    /// Longitude específica do endereço
    /// </summary>
    public double? Longitude { get; set; }
}

/// <summary>
/// DTO resumido para Endereço (para uso em listas)
/// </summary>
public class EnderecoResumoDto
{
    /// <summary>
    /// ID do endereço
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// CEP formatado
    /// </summary>
    public string CepFormatado { get; set; } = string.Empty;
    
    /// <summary>
    /// Endereço formatado
    /// </summary>
    public string EnderecoFormatado { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome do município
    /// </summary>
    public string MunicipioNome { get; set; } = string.Empty;
    
    /// <summary>
    /// UF do estado
    /// </summary>
    public string EstadoUf { get; set; } = string.Empty;
}

/// <summary>
/// DTO para consulta de endereços próximos
/// </summary>
public class EnderecoProximoDto
{
    /// <summary>
    /// ID do endereço
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Endereço formatado
    /// </summary>
    public string EnderecoFormatado { get; set; } = string.Empty;
    
    /// <summary>
    /// CEP formatado
    /// </summary>
    public string CepFormatado { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome do município
    /// </summary>
    public string MunicipioNome { get; set; } = string.Empty;
    
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