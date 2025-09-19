using System.Text.RegularExpressions;

namespace Agriis.Compartilhado.Dominio.ObjetosValor;

/// <summary>
/// Objeto de valor para CNPJ (Cadastro Nacional da Pessoa Jurídica)
/// </summary>
public class Cnpj : ObjetoValorBase
{
    /// <summary>
    /// Valor do CNPJ sem formatação
    /// </summary>
    public string Valor { get; private set; } = string.Empty;

    /// <summary>
    /// Construtor protegido para Entity Framework
    /// </summary>
    protected Cnpj() { }

    /// <summary>
    /// Construtor público para criação do CNPJ
    /// </summary>
    /// <param name="valor">CNPJ com ou sem formatação</param>
    /// <exception cref="ArgumentException">Lançada quando o CNPJ é inválido</exception>
    public Cnpj(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("CNPJ não pode ser vazio ou nulo", nameof(valor));

        var cnpjLimpo = LimparCnpj(valor);
        
        if (!ValidarCnpj(cnpjLimpo))
            throw new ArgumentException("CNPJ inválido", nameof(valor));

        Valor = cnpjLimpo;
    }

    /// <summary>
    /// Retorna o CNPJ formatado (XX.XXX.XXX/XXXX-XX)
    /// </summary>
    public string ValorFormatado => FormatarCnpj(Valor);

    /// <summary>
    /// Remove formatação do CNPJ
    /// </summary>
    /// <param name="cnpj">CNPJ com ou sem formatação</param>
    /// <returns>CNPJ apenas com números</returns>
    private static string LimparCnpj(string cnpj)
    {
        return Regex.Replace(cnpj, @"[^\d]", "");
    }

    /// <summary>
    /// Formata o CNPJ com pontos, barra e hífen
    /// </summary>
    /// <param name="cnpj">CNPJ sem formatação</param>
    /// <returns>CNPJ formatado</returns>
    private static string FormatarCnpj(string cnpj)
    {
        if (cnpj.Length != 14)
            return cnpj;

        return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
    }

    /// <summary>
    /// Valida se o CNPJ é válido usando o algoritmo oficial
    /// </summary>
    /// <param name="cnpj">CNPJ sem formatação</param>
    /// <returns>True se válido, False caso contrário</returns>
    private static bool ValidarCnpj(string cnpj)
    {
        // CNPJ deve ter 14 dígitos
        if (cnpj.Length != 14)
            return false;

        // Verifica se todos os dígitos são iguais (CNPJ inválido)
        if (cnpj.All(c => c == cnpj[0]))
            return false;

        // Calcula o primeiro dígito verificador
        var multiplicadores1 = new int[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var soma = 0;

        for (int i = 0; i < 12; i++)
        {
            soma += int.Parse(cnpj[i].ToString()) * multiplicadores1[i];
        }

        var resto = soma % 11;
        var digitoVerificador1 = resto < 2 ? 0 : 11 - resto;

        if (int.Parse(cnpj[12].ToString()) != digitoVerificador1)
            return false;

        // Calcula o segundo dígito verificador
        var multiplicadores2 = new int[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        soma = 0;

        for (int i = 0; i < 13; i++)
        {
            soma += int.Parse(cnpj[i].ToString()) * multiplicadores2[i];
        }

        resto = soma % 11;
        var digitoVerificador2 = resto < 2 ? 0 : 11 - resto;

        return int.Parse(cnpj[13].ToString()) == digitoVerificador2;
    }

    /// <summary>
    /// Retorna os componentes de igualdade
    /// </summary>
    protected override IEnumerable<object?> ObterComponentesIgualdade()
    {
        yield return Valor;
    }

    /// <summary>
    /// Representação em string do CNPJ
    /// </summary>
    public override string ToString() => ValorFormatado;

    /// <summary>
    /// Conversão implícita de string para CNPJ
    /// </summary>
    public static implicit operator Cnpj(string valor) => new(valor);

    /// <summary>
    /// Conversão implícita de CNPJ para string
    /// </summary>
    public static implicit operator string(Cnpj cnpj) => cnpj?.Valor ?? string.Empty;
}