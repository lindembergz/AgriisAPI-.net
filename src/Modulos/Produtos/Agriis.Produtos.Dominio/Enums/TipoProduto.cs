namespace Agriis.Produtos.Dominio.Enums;

/// <summary>
/// Tipos de produto no sistema
/// </summary>
public enum TipoProduto
{
    /// <summary>
    /// Produto fabricado pela própria empresa
    /// </summary>
    Fabricante = 1,
    
    /// <summary>
    /// Produto revendido de terceiros
    /// </summary>
    Revendedor = 2
}

/// <summary>
/// Status do produto
/// </summary>
public enum StatusProduto
{
    /// <summary>
    /// Produto ativo e disponível
    /// </summary>
    Ativo = 1,
    
    /// <summary>
    /// Produto inativo
    /// </summary>
    Inativo = 2,
    
    /// <summary>
    /// Produto descontinuado
    /// </summary>
    Descontinuado = 3
}

/// <summary>
/// Tipos de cálculo de peso para frete
/// </summary>
public enum TipoCalculoPeso
{
    /// <summary>
    /// Usar peso nominal do produto
    /// </summary>
    PesoNominal = 1,
    
    /// <summary>
    /// Usar peso cúbico calculado
    /// </summary>
    PesoCubado = 2
}

/// <summary>
/// Categorias de produtos agrícolas
/// </summary>
public enum CategoriaProduto
{
    /// <summary>
    /// Sementes
    /// </summary>
    Sementes = 1,
    
    /// <summary>
    /// Fertilizantes
    /// </summary>
    Fertilizantes = 2,
    
    /// <summary>
    /// Defensivos
    /// </summary>
    Defensivos = 3,
    
    /// <summary>
    /// Inoculantes
    /// </summary>
    Inoculantes = 4,
    
    /// <summary>
    /// Adjuvantes
    /// </summary>
    Adjuvantes = 5,
    
    /// <summary>
    /// Micronutrientes
    /// </summary>
    Micronutrientes = 6,
    
    /// <summary>
    /// Outros produtos
    /// </summary>
    Outros = 99
}