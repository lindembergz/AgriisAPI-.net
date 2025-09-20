namespace Agriis.Pedidos.Aplicacao.DTOs;

/// <summary>
/// DTO para cálculo de frete
/// </summary>
public class CalcularFreteDto
{
    /// <summary>
    /// ID do produto
    /// </summary>
    public int ProdutoId { get; set; }

    /// <summary>
    /// Quantidade do produto
    /// </summary>
    public decimal Quantidade { get; set; }

    /// <summary>
    /// Distância em quilômetros
    /// </summary>
    public decimal DistanciaKm { get; set; }

    /// <summary>
    /// Valor por quilograma por quilômetro (opcional)
    /// </summary>
    public decimal? ValorPorKgKm { get; set; }

    /// <summary>
    /// Valor mínimo de frete (opcional)
    /// </summary>
    public decimal? ValorMinimoFrete { get; set; }
}

/// <summary>
/// DTO para cálculo de frete consolidado
/// </summary>
public class CalcularFreteConsolidadoDto
{
    /// <summary>
    /// Lista de itens para cálculo
    /// </summary>
    public List<ItemFreteDto> Itens { get; set; } = new();

    /// <summary>
    /// Distância em quilômetros
    /// </summary>
    public decimal DistanciaKm { get; set; }

    /// <summary>
    /// Valor por quilograma por quilômetro (opcional)
    /// </summary>
    public decimal? ValorPorKgKm { get; set; }

    /// <summary>
    /// Valor mínimo de frete (opcional)
    /// </summary>
    public decimal? ValorMinimoFrete { get; set; }
}

/// <summary>
/// DTO para item de frete
/// </summary>
public class ItemFreteDto
{
    /// <summary>
    /// ID do produto
    /// </summary>
    public int ProdutoId { get; set; }

    /// <summary>
    /// Quantidade do produto
    /// </summary>
    public decimal Quantidade { get; set; }
}

/// <summary>
/// DTO com resultado do cálculo de frete
/// </summary>
public class CalculoFreteDto
{
    /// <summary>
    /// Peso total em quilogramas
    /// </summary>
    public decimal PesoTotal { get; set; }

    /// <summary>
    /// Volume total em metros cúbicos
    /// </summary>
    public decimal VolumeTotal { get; set; }

    /// <summary>
    /// Peso cúbico total (se aplicável)
    /// </summary>
    public decimal? PesoCubadoTotal { get; set; }

    /// <summary>
    /// Peso utilizado para cálculo do frete
    /// </summary>
    public decimal PesoParaFrete { get; set; }

    /// <summary>
    /// Valor do frete calculado
    /// </summary>
    public decimal ValorFrete { get; set; }

    /// <summary>
    /// Distância utilizada no cálculo
    /// </summary>
    public decimal DistanciaKm { get; set; }

    /// <summary>
    /// Tipo de cálculo utilizado
    /// </summary>
    public string TipoCalculoUtilizado { get; set; } = string.Empty;
}

/// <summary>
/// DTO com resultado do cálculo de frete consolidado
/// </summary>
public class CalculoFreteConsolidadoDto
{
    /// <summary>
    /// Cálculos individuais por item
    /// </summary>
    public List<CalculoFreteDto> CalculosIndividuais { get; set; } = new();

    /// <summary>
    /// Peso total consolidado
    /// </summary>
    public decimal PesoTotalConsolidado { get; set; }

    /// <summary>
    /// Volume total consolidado
    /// </summary>
    public decimal VolumeTotalConsolidado { get; set; }

    /// <summary>
    /// Peso cúbico total consolidado (se aplicável)
    /// </summary>
    public decimal? PesoCubadoTotalConsolidado { get; set; }

    /// <summary>
    /// Valor do frete consolidado
    /// </summary>
    public decimal ValorFreteConsolidado { get; set; }

    /// <summary>
    /// Distância utilizada no cálculo
    /// </summary>
    public decimal DistanciaKm { get; set; }
}