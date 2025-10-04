namespace Agriis.Enderecos.Aplicacao.DTOs;

/// <summary>
/// DTO para Estado
/// </summary>
public class EstadoDto
{
    /// <summary>
    /// ID do estado
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome completo do estado
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Sigla do estado (UF)
    /// </summary>
    public string Uf { get; set; } = string.Empty;
    
    /// <summary>
    /// Código IBGE do estado
    /// </summary>
    public int CodigoIbge { get; set; }
    
    /// <summary>
    /// Região do estado
    /// </summary>
    public string Regiao { get; set; } = string.Empty;
    
    /// <summary>
    /// ID do país ao qual o estado pertence
    /// </summary>
    public int PaisId { get; set; } = 1;
    
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
/// DTO para criação de Estado
/// </summary>
public class CriarEstadoDto
{
    /// <summary>
    /// Nome completo do estado
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Código/Sigla do estado (UF) - aceita tanto 'codigo' quanto 'uf'
    /// </summary>
    public string Codigo { get; set; } = string.Empty;
    
    /// <summary>
    /// Sigla do estado (UF) - propriedade para compatibilidade
    /// </summary>
    public string Uf 
    { 
        get => Codigo; 
        set => Codigo = value; 
    }
    
    /// <summary>
    /// Código IBGE do estado
    /// </summary>
    public int CodigoIbge { get; set; }
    
    /// <summary>
    /// Região do estado
    /// </summary>
    public string Regiao { get; set; } = string.Empty;
    
    /// <summary>
    /// ID do país ao qual o estado pertence (padrão: 1 para Brasil)
    /// </summary>
    public int PaisId { get; set; } = 1;
}

/// <summary>
/// DTO para atualização de Estado
/// </summary>
public class AtualizarEstadoDto
{
    /// <summary>
    /// Nome completo do estado
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Código/Sigla do estado (UF) - aceita tanto 'codigo' quanto 'uf'
    /// </summary>
    public string Codigo { get; set; } = string.Empty;
    
    /// <summary>
    /// Sigla do estado (UF) - propriedade para compatibilidade
    /// </summary>
    public string Uf 
    { 
        get => Codigo; 
        set => Codigo = value; 
    }
    
    /// <summary>
    /// Código IBGE do estado
    /// </summary>
    public int CodigoIbge { get; set; }
    
    /// <summary>
    /// Região do estado
    /// </summary>
    public string Regiao { get; set; } = string.Empty;
    
    /// <summary>
    /// ID do país ao qual o estado pertence
    /// </summary>
    public int PaisId { get; set; } = 1;
}

/// <summary>
/// DTO resumido para Estado (para uso em listas)
/// </summary>
public class EstadoResumoDto
{
    /// <summary>
    /// ID do estado
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome completo do estado
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Sigla do estado (UF)
    /// </summary>
    public string Uf { get; set; } = string.Empty;
    
    /// <summary>
    /// Região do estado
    /// </summary>
    public string Regiao { get; set; } = string.Empty;
}