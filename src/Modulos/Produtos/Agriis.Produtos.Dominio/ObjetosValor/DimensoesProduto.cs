using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Produtos.Dominio.Enums;

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
    /// Peso nominal em quilogramas (mantido para compatibilidade)
    /// </summary>
    public decimal PesoNominal { get; private set; }
    
    /// <summary>
    /// Peso da embalagem em quilogramas (usado nos cálculos de frete)
    /// </summary>
    public decimal PesoEmbalagem { get; private set; }
    
    /// <summary>
    /// Peso de mil sementes em gramas (PMS - usado para cálculo de peso de sementes)
    /// </summary>
    public decimal? Pms { get; private set; }
    
    /// <summary>
    /// Quantidade mínima por embalagem
    /// </summary>
    public decimal QuantidadeMinima { get; private set; }
    
    /// <summary>
    /// Tipo de embalagem
    /// </summary>
    public string Embalagem { get; private set; } = string.Empty;
    
    /// <summary>
    /// Densidade inicial para cálculo cúbico (kg/m³)
    /// </summary>
    public decimal? FaixaDensidadeInicial { get; private set; }
    
    /// <summary>
    /// Densidade final para cálculo cúbico (kg/m³)
    /// </summary>
    public decimal? FaixaDensidadeFinal { get; private set; }

    protected DimensoesProduto() { } // EF Constructor

    public DimensoesProduto(decimal altura, decimal largura, decimal comprimento, decimal pesoNominal, 
                           decimal pesoEmbalagem, decimal quantidadeMinima, string embalagem,
                           decimal? pms = null, decimal? faixaDensidadeInicial = null, decimal? faixaDensidadeFinal = null)
    {
        if (altura <= 0)
            throw new ArgumentException("Altura deve ser maior que zero", nameof(altura));
        if (largura <= 0)
            throw new ArgumentException("Largura deve ser maior que zero", nameof(largura));
        if (comprimento <= 0)
            throw new ArgumentException("Comprimento deve ser maior que zero", nameof(comprimento));
        if (pesoNominal <= 0)
            throw new ArgumentException("Peso nominal deve ser maior que zero", nameof(pesoNominal));
        if (pesoEmbalagem <= 0)
            throw new ArgumentException("Peso da embalagem deve ser maior que zero", nameof(pesoEmbalagem));
        if (quantidadeMinima <= 0)
            throw new ArgumentException("Quantidade mínima deve ser maior que zero", nameof(quantidadeMinima));
        if (string.IsNullOrWhiteSpace(embalagem))
            throw new ArgumentException("Embalagem deve ser informada", nameof(embalagem));
        if (pms.HasValue && pms <= 0)
            throw new ArgumentException("PMS deve ser maior que zero", nameof(pms));
        if (faixaDensidadeInicial.HasValue && faixaDensidadeInicial <= 0)
            throw new ArgumentException("Faixa de densidade inicial deve ser maior que zero", nameof(faixaDensidadeInicial));
        if (faixaDensidadeFinal.HasValue && faixaDensidadeFinal <= 0)
            throw new ArgumentException("Faixa de densidade final deve ser maior que zero", nameof(faixaDensidadeFinal));

        Altura = altura;
        Largura = largura;
        Comprimento = comprimento;
        PesoNominal = pesoNominal;
        PesoEmbalagem = pesoEmbalagem;
        Pms = pms;
        QuantidadeMinima = quantidadeMinima;
        Embalagem = embalagem;
        FaixaDensidadeInicial = faixaDensidadeInicial;
        FaixaDensidadeFinal = faixaDensidadeFinal;
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
    /// Calcula o peso cúbico baseado na densidade (replicando lógica Python)
    /// </summary>
    public decimal? CalcularPesoCubado()
    {
        if (!FaixaDensidadeInicial.HasValue)
            return null;

        // Replicar lógica Python: dimensoes = produto.comprimento * produto.largura * produto.profundidade
        var dimensoes = Comprimento * Largura * Altura; // em cm³
        
        // fator_cubagem = produto.faixa_densidade_inicial
        var fatorCubagem = FaixaDensidadeInicial.Value;
        
        // pre_peso_cubado = (dimensoes / 1000000) * fator_cubagem
        var prePesoCubado = (dimensoes / 1_000_000) * fatorCubagem; // converter cm³ para m³
        
        // peso_cubado = pre_peso_cubado if pre_peso_cubado > peso_produto else peso_produto
        return Math.Max(prePesoCubado, PesoEmbalagem);
    }

    /// <summary>
    /// Calcula o peso do produto replicando exatamente a lógica Python
    /// </summary>
    /// <param name="categoriaNome">Nome da categoria do produto</param>
    /// <param name="unidade">Unidade do produto</param>
    /// <returns>Peso do produto em quilogramas</returns>
    public decimal CalcularPesoProduto(string categoriaNome, TipoUnidade unidade)
    {
        // Replicar lógica Python exata:
        // if produto.categoria.nome == 'Sementes' and produto.unidade == TipoUnidade.SEMENTES:
        //     peso_produto = ((produto.pms / 1000) / 1000) * produto.quantidade_minima
        // else:
        //     peso_produto = produto.peso_embalagem
        
        if (categoriaNome == "Sementes" && unidade == TipoUnidade.Sementes && Pms.HasValue)
        {
            return ((Pms.Value / 1000) / 1000) * QuantidadeMinima;
        }
        
        return PesoEmbalagem;
    }

    /// <summary>
    /// Determina qual peso usar para cálculo de frete baseado no tipo de cálculo
    /// </summary>
    /// <param name="tipoCalculoPeso">Tipo de cálculo (PesoNominal ou PesoCubado)</param>
    /// <param name="categoriaNome">Nome da categoria do produto</param>
    /// <param name="unidade">Unidade do produto</param>
    /// <returns>Peso para cálculo de frete</returns>
    public decimal ObterPesoParaFrete(TipoCalculoPeso tipoCalculoPeso, string categoriaNome, TipoUnidade unidade)
    {
        return tipoCalculoPeso switch
        {
            TipoCalculoPeso.PesoCubado => CalcularPesoCubado() ?? PesoEmbalagem,
            TipoCalculoPeso.PesoNominal => CalcularPesoProduto(categoriaNome, unidade),
            _ => PesoEmbalagem
        };
    }

    protected override IEnumerable<object> ObterComponentesIgualdade()
    {
        yield return Altura;
        yield return Largura;
        yield return Comprimento;
        yield return PesoNominal;
        yield return PesoEmbalagem;
        yield return Pms ?? 0;
        yield return QuantidadeMinima;
        yield return Embalagem;
        yield return FaixaDensidadeInicial ?? 0;
        yield return FaixaDensidadeFinal ?? 0;
    }
}