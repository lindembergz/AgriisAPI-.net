using Agriis.Compartilhado.Dominio.ObjetosValor;

namespace Agriis.Produtos.Dominio.ObjetosValor;

/// <summary>
/// Objeto de valor que representa as dimensões físicas de um produto
/// </summary>
public class DimensoesProduto : ObjetoValorBase
{
    /// <summary>
    /// Altura em centímetros
    /// </summary>
    public decimal Altura { get; private set; }
    
    /// <summary>
    /// Largura em centímetros
    /// </summary>
    public decimal Largura { get; private set; }
    
    /// <summary>
    /// Comprimento em centímetros
    /// </summary>
    public decimal Comprimento { get; private set; }
    
    /// <summary>
    /// Peso nominal em quilogramas
    /// </summary>
    public decimal PesoNominal { get; private set; }
    
    /// <summary>
    /// Densidade do produto (kg/m³)
    /// </summary>
    public decimal? Densidade { get; private set; }

    protected DimensoesProduto() { } // EF Constructor

    public DimensoesProduto(decimal altura, decimal largura, decimal comprimento, decimal pesoNominal, decimal? densidade = null)
    {
        if (altura <= 0)
            throw new ArgumentException("Altura deve ser maior que zero", nameof(altura));
        if (largura <= 0)
            throw new ArgumentException("Largura deve ser maior que zero", nameof(largura));
        if (comprimento <= 0)
            throw new ArgumentException("Comprimento deve ser maior que zero", nameof(comprimento));
        if (pesoNominal <= 0)
            throw new ArgumentException("Peso nominal deve ser maior que zero", nameof(pesoNominal));
        if (densidade.HasValue && densidade <= 0)
            throw new ArgumentException("Densidade deve ser maior que zero", nameof(densidade));

        Altura = altura;
        Largura = largura;
        Comprimento = comprimento;
        PesoNominal = pesoNominal;
        Densidade = densidade;
    }

    /// <summary>
    /// Calcula o volume em metros cúbicos
    /// </summary>
    public decimal CalcularVolume()
    {
        // Converter de cm³ para m³
        return (Altura * Largura * Comprimento) / 1_000_000;
    }

    /// <summary>
    /// Calcula o peso cúbico baseado na densidade
    /// </summary>
    public decimal? CalcularPesoCubado()
    {
        if (!Densidade.HasValue)
            return null;

        return CalcularVolume() * Densidade.Value;
    }

    /// <summary>
    /// Determina qual peso usar para cálculo de frete
    /// </summary>
    public decimal ObterPesoParaFrete()
    {
        var pesoCubado = CalcularPesoCubado();
        
        // Se não tem densidade, usa peso nominal
        if (!pesoCubado.HasValue)
            return PesoNominal;
        
        // Usa o maior entre peso nominal e peso cúbico
        return Math.Max(PesoNominal, pesoCubado.Value);
    }

    protected override IEnumerable<object> ObterComponentesIgualdade()
    {
        yield return Altura;
        yield return Largura;
        yield return Comprimento;
        yield return PesoNominal;
        yield return Densidade ?? 0;
    }
}