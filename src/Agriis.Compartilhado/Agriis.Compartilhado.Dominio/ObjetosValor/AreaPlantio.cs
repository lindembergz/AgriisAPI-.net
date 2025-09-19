namespace Agriis.Compartilhado.Dominio.ObjetosValor;

/// <summary>
/// Objeto de valor para área de plantio em hectares
/// </summary>
public class AreaPlantio : ObjetoValorBase
{
    /// <summary>
    /// Valor da área em hectares
    /// </summary>
    public decimal Valor { get; private set; }

    /// <summary>
    /// Construtor protegido para Entity Framework
    /// </summary>
    protected AreaPlantio() { }

    /// <summary>
    /// Construtor público para criação da área de plantio
    /// </summary>
    /// <param name="valor">Área em hectares</param>
    /// <exception cref="ArgumentException">Lançada quando a área é inválida</exception>
    public AreaPlantio(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentException("Área de plantio não pode ser negativa", nameof(valor));

        if (valor > 1_000_000) // Limite máximo razoável para área de plantio
            throw new ArgumentException("Área de plantio excede o limite máximo permitido", nameof(valor));

        Valor = Math.Round(valor, 4); // Precisão de 4 casas decimais para hectares
    }

    /// <summary>
    /// Retorna a área formatada com unidade
    /// </summary>
    public string ValorFormatado => $"{Valor:N2} ha";

    /// <summary>
    /// Converte hectares para metros quadrados
    /// </summary>
    public decimal EmMetrosQuadrados => Valor * 10000;

    /// <summary>
    /// Converte hectares para alqueires paulistas (1 alqueire = 2,42 hectares)
    /// </summary>
    public decimal EmAlqueiresPaulistas => Valor / 2.42m;

    /// <summary>
    /// Converte hectares para alqueires mineiros (1 alqueire = 4,84 hectares)
    /// </summary>
    public decimal EmAlqueiresMineiros => Valor / 4.84m;

    /// <summary>
    /// Soma duas áreas de plantio
    /// </summary>
    public static AreaPlantio operator +(AreaPlantio area1, AreaPlantio area2)
    {
        if (area1 == null) throw new ArgumentNullException(nameof(area1));
        if (area2 == null) throw new ArgumentNullException(nameof(area2));
        
        return new AreaPlantio(area1.Valor + area2.Valor);
    }

    /// <summary>
    /// Subtrai duas áreas de plantio
    /// </summary>
    public static AreaPlantio operator -(AreaPlantio area1, AreaPlantio area2)
    {
        if (area1 == null) throw new ArgumentNullException(nameof(area1));
        if (area2 == null) throw new ArgumentNullException(nameof(area2));
        
        return new AreaPlantio(area1.Valor - area2.Valor);
    }

    /// <summary>
    /// Multiplica área por um fator
    /// </summary>
    public static AreaPlantio operator *(AreaPlantio area, decimal fator)
    {
        if (area == null) throw new ArgumentNullException(nameof(area));
        
        return new AreaPlantio(area.Valor * fator);
    }

    /// <summary>
    /// Divide área por um fator
    /// </summary>
    public static AreaPlantio operator /(AreaPlantio area, decimal fator)
    {
        if (area == null) throw new ArgumentNullException(nameof(area));
        if (fator == 0) throw new DivideByZeroException("Não é possível dividir por zero");
        
        return new AreaPlantio(area.Valor / fator);
    }

    /// <summary>
    /// Compara se uma área é maior que outra
    /// </summary>
    public static bool operator >(AreaPlantio area1, AreaPlantio area2)
    {
        if (area1 == null || area2 == null) return false;
        return area1.Valor > area2.Valor;
    }

    /// <summary>
    /// Compara se uma área é menor que outra
    /// </summary>
    public static bool operator <(AreaPlantio area1, AreaPlantio area2)
    {
        if (area1 == null || area2 == null) return false;
        return area1.Valor < area2.Valor;
    }

    /// <summary>
    /// Compara se uma área é maior ou igual a outra
    /// </summary>
    public static bool operator >=(AreaPlantio area1, AreaPlantio area2)
    {
        if (area1 == null || area2 == null) return false;
        return area1.Valor >= area2.Valor;
    }

    /// <summary>
    /// Compara se uma área é menor ou igual a outra
    /// </summary>
    public static bool operator <=(AreaPlantio area1, AreaPlantio area2)
    {
        if (area1 == null || area2 == null) return false;
        return area1.Valor <= area2.Valor;
    }

    /// <summary>
    /// Retorna os componentes de igualdade
    /// </summary>
    protected override IEnumerable<object?> ObterComponentesIgualdade()
    {
        yield return Valor;
    }

    /// <summary>
    /// Representação em string da área
    /// </summary>
    public override string ToString() => ValorFormatado;

    /// <summary>
    /// Conversão implícita de decimal para AreaPlantio
    /// </summary>
    public static implicit operator AreaPlantio(decimal valor) => new(valor);

    /// <summary>
    /// Conversão implícita de AreaPlantio para decimal
    /// </summary>
    public static implicit operator decimal(AreaPlantio area) => area?.Valor ?? 0;

    /// <summary>
    /// Cria uma área de plantio a partir de metros quadrados
    /// </summary>
    public static AreaPlantio DeMetrosQuadrados(decimal metrosQuadrados)
    {
        return new AreaPlantio(metrosQuadrados / 10000);
    }

    /// <summary>
    /// Cria uma área de plantio a partir de alqueires paulistas
    /// </summary>
    public static AreaPlantio DeAlqueiresPaulistas(decimal alqueires)
    {
        return new AreaPlantio(alqueires * 2.42m);
    }

    /// <summary>
    /// Cria uma área de plantio a partir de alqueires mineiros
    /// </summary>
    public static AreaPlantio DeAlqueiresMineiros(decimal alqueires)
    {
        return new AreaPlantio(alqueires * 4.84m);
    }
}