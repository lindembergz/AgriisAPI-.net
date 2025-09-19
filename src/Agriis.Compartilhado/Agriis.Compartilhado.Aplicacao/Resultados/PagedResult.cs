namespace Agriis.Compartilhado.Aplicacao.Resultados;

/// <summary>
/// Representa um resultado paginado de uma consulta
/// </summary>
/// <typeparam name="T">Tipo dos itens na página</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Itens da página atual
    /// </summary>
    public IReadOnlyList<T> Items { get; private set; }
    
    /// <summary>
    /// Número da página atual (baseado em 1)
    /// </summary>
    public int PageNumber { get; private set; }
    
    /// <summary>
    /// Tamanho da página (número de itens por página)
    /// </summary>
    public int PageSize { get; private set; }
    
    /// <summary>
    /// Total de itens em todas as páginas
    /// </summary>
    public int TotalCount { get; private set; }
    
    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPages { get; private set; }
    
    /// <summary>
    /// Indica se existe uma página anterior
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
    
    /// <summary>
    /// Indica se existe uma próxima página
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
    
    /// <summary>
    /// Indica se é a primeira página
    /// </summary>
    public bool IsFirstPage => PageNumber == 1;
    
    /// <summary>
    /// Indica se é a última página
    /// </summary>
    public bool IsLastPage => PageNumber == TotalPages;
    
    /// <summary>
    /// Número da primeira página
    /// </summary>
    public int FirstPage => 1;
    
    /// <summary>
    /// Número da última página
    /// </summary>
    public int LastPage => TotalPages;
    
    /// <summary>
    /// Número da página anterior (se existir)
    /// </summary>
    public int? PreviousPage => HasPreviousPage ? PageNumber - 1 : null;
    
    /// <summary>
    /// Número da próxima página (se existir)
    /// </summary>
    public int? NextPage => HasNextPage ? PageNumber + 1 : null;
    
    /// <summary>
    /// Índice do primeiro item da página atual (baseado em 0)
    /// </summary>
    public int FirstItemIndex => (PageNumber - 1) * PageSize;
    
    /// <summary>
    /// Índice do último item da página atual (baseado em 0)
    /// </summary>
    public int LastItemIndex => Math.Min(FirstItemIndex + PageSize - 1, TotalCount - 1);
    
    /// <summary>
    /// Número do primeiro item da página atual (baseado em 1)
    /// </summary>
    public int FirstItemNumber => TotalCount == 0 ? 0 : FirstItemIndex + 1;
    
    /// <summary>
    /// Número do último item da página atual (baseado em 1)
    /// </summary>
    public int LastItemNumber => TotalCount == 0 ? 0 : LastItemIndex + 1;
    
    /// <summary>
    /// Construtor
    /// </summary>
    /// <param name="items">Itens da página</param>
    /// <param name="pageNumber">Número da página</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <param name="totalCount">Total de itens</param>
    public PagedResult(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Número da página deve ser maior que zero", nameof(pageNumber));
            
        if (pageSize < 1)
            throw new ArgumentException("Tamanho da página deve ser maior que zero", nameof(pageSize));
            
        if (totalCount < 0)
            throw new ArgumentException("Total de itens não pode ser negativo", nameof(totalCount));
        
        Items = items.ToList().AsReadOnly();
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
    }
    
    /// <summary>
    /// Cria um resultado paginado vazio
    /// </summary>
    /// <param name="pageNumber">Número da página</param>
    /// <param name="pageSize">Tamanho da página</param>
    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10)
    {
        return new PagedResult<T>(Enumerable.Empty<T>(), pageNumber, pageSize, 0);
    }
    
    /// <summary>
    /// Cria um resultado paginado a partir de uma lista completa
    /// </summary>
    /// <param name="source">Lista completa de itens</param>
    /// <param name="pageNumber">Número da página</param>
    /// <param name="pageSize">Tamanho da página</param>
    public static PagedResult<T> FromList(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var sourceList = source.ToList();
        var totalCount = sourceList.Count;
        var items = sourceList.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        
        return new PagedResult<T>(items, pageNumber, pageSize, totalCount);
    }
    
    /// <summary>
    /// Mapeia os itens da página para outro tipo
    /// </summary>
    /// <typeparam name="TNew">Novo tipo</typeparam>
    /// <param name="mapper">Função de mapeamento</param>
    public PagedResult<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        var mappedItems = Items.Select(mapper);
        return new PagedResult<TNew>(mappedItems, PageNumber, PageSize, TotalCount);
    }
    
    /// <summary>
    /// Mapeia os itens da página para outro tipo de forma assíncrona
    /// </summary>
    /// <typeparam name="TNew">Novo tipo</typeparam>
    /// <param name="mapper">Função de mapeamento assíncrona</param>
    public async Task<PagedResult<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapper)
    {
        var mappedItems = await Task.WhenAll(Items.Select(mapper));
        return new PagedResult<TNew>(mappedItems, PageNumber, PageSize, TotalCount);
    }
    
    /// <summary>
    /// Obtém informações de metadados da paginação
    /// </summary>
    public PaginationMetadata GetMetadata()
    {
        return new PaginationMetadata
        {
            PageNumber = PageNumber,
            PageSize = PageSize,
            TotalCount = TotalCount,
            TotalPages = TotalPages,
            HasPreviousPage = HasPreviousPage,
            HasNextPage = HasNextPage,
            FirstItemNumber = FirstItemNumber,
            LastItemNumber = LastItemNumber
        };
    }
}

/// <summary>
/// Metadados de paginação
/// </summary>
public class PaginationMetadata
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public int FirstItemNumber { get; set; }
    public int LastItemNumber { get; set; }
}

/// <summary>
/// Parâmetros para paginação
/// </summary>
public class PaginationParameters
{
    private int _pageNumber = 1;
    private int _pageSize = 10;
    
    /// <summary>
    /// Número da página (mínimo 1)
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }
    
    /// <summary>
    /// Tamanho da página (mínimo 1, máximo 100)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 1,
            > 100 => 100,
            _ => value
        };
    }
    
    /// <summary>
    /// Termo de busca (opcional)
    /// </summary>
    public string? SearchTerm { get; set; }
    
    /// <summary>
    /// Campo para ordenação (opcional)
    /// </summary>
    public string? SortBy { get; set; }
    
    /// <summary>
    /// Direção da ordenação (asc/desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";
    
    /// <summary>
    /// Indica se a ordenação é descendente
    /// </summary>
    public bool IsDescending => SortDirection.ToLower() == "desc";
}