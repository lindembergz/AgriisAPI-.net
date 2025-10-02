namespace Agriis.Enderecos.Aplicacao.DTOs;

/// <summary>
/// DTO para Município compatível com o frontend
/// </summary>
public class MunicipioFrontendDto
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
    /// Código IBGE do município (como string para compatibilidade com frontend)
    /// </summary>
    public string CodigoIbge { get; set; } = string.Empty;
    
    /// <summary>
    /// ID do estado (ufId para compatibilidade com frontend)
    /// </summary>
    public int UfId { get; set; }
    
    /// <summary>
    /// Nome da UF
    /// </summary>
    public string? UfNome { get; set; }
    
    /// <summary>
    /// Código da UF
    /// </summary>
    public string? UfCodigo { get; set; }
    
    /// <summary>
    /// Dados da UF
    /// </summary>
    public UfFrontendDto? Uf { get; set; }
    
    /// <summary>
    /// Status ativo
    /// </summary>
    public bool Ativo { get; set; } = true;
    
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
/// DTO para UF compatível com o frontend
/// </summary>
public class UfFrontendDto
{
    /// <summary>
    /// ID da UF
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome da UF
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Código da UF
    /// </summary>
    public string Uf { get; set; } = string.Empty;
    
    /// <summary>
    /// Código IBGE da UF
    /// </summary>
    public string CodigoIbge { get; set; } = string.Empty;
    
    /// <summary>
    /// Região
    /// </summary>
    public string Regiao { get; set; } = string.Empty;
    
    /// <summary>
    /// ID do país
    /// </summary>
    public int PaisId { get; set; }
    
    /// <summary>
    /// Status ativo
    /// </summary>
    public bool Ativo { get; set; } = true;
    
    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime DataCriacao { get; set; }
    
    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? DataAtualizacao { get; set; }
}