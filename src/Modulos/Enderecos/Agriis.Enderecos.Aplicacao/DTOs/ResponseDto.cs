namespace Agriis.Enderecos.Aplicacao.DTOs;

/// <summary>
/// DTO genérico para respostas paginadas
/// </summary>
/// <typeparam name="T">Tipo dos itens da resposta</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Lista de itens da página atual
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();
    
    /// <summary>
    /// Total de itens disponíveis
    /// </summary>
    public int TotalItems { get; set; }
    
    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// Página atual
    /// </summary>
    public int CurrentPage { get; set; }
    
    /// <summary>
    /// Tamanho da página
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Indica se há página anterior
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;
    
    /// <summary>
    /// Indica se há próxima página
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;
}

/// <summary>
/// DTO para país com contadores
/// </summary>
public class PaisComContadorDto
{
    /// <summary>
    /// ID do país
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome do país
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Código do país
    /// </summary>
    public string Codigo { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica se o país está ativo
    /// </summary>
    public bool Ativo { get; set; }
    
    /// <summary>
    /// Quantidade de UFs/Estados do país
    /// </summary>
    public int UfsCount { get; set; }
    
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
/// DTO para resposta de validação
/// </summary>
public class ValidationResponseDto
{
    /// <summary>
    /// Indica se o valor é válido/único
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Mensagem de validação (opcional)
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// DTO para resposta de contagem
/// </summary>
public class CountResponseDto
{
    /// <summary>
    /// Quantidade de itens
    /// </summary>
    public int Count { get; set; }
}

/// <summary>
/// DTO para resposta de verificação de existência
/// </summary>
public class ExistenceResponseDto
{
    /// <summary>
    /// Indica se o item existe
    /// </summary>
    public bool Exists { get; set; }
    
    /// <summary>
    /// Indica se tem dependências (para verificações de remoção)
    /// </summary>
    public bool HasDependencies { get; set; }
}