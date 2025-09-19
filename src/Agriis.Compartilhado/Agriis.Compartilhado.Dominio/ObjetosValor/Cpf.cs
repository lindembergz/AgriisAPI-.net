using System.Text.RegularExpressions;

namespace Agriis.Compartilhado.Dominio.ObjetosValor;

/// <summary>
/// Objeto de valor para CPF (Cadastro de Pessoa Física)
/// </summary>
public class Cpf : ObjetoValorBase
{
    /// <summary>
    /// Valor do CPF sem formatação
    /// </summary>
    public string Valor { get; private set; } = string.Empty;

    /// <summary>
    /// Construtor protegido para Entity Framework
    /// </summary>
    protected Cpf() { }

    /// <summary>
    /// Construtor público para criação do CPF
    /// </summary>
    /// <param name="valor">CPF com ou sem formatação</param>
    /// <exception cref="ArgumentException">Lançada quando o CPF é inválido</exception>
    public Cpf(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("CPF não pode ser vazio ou nulo", nameof(valor));

        var cpfLimpo = LimparCpf(valor);
        
        if (!ValidarCpf(cpfLimpo))
            throw new ArgumentException("CPF inválido", nameof(valor));

        Valor = cpfLimpo;
    }

    /// <summary>
    /// Retorna o CPF formatado (XXX.XXX.XXX-XX)
    /// </summary>
    public string ValorFormatado => FormatarCpf(Valor);

    /// <summary>
    /// Remove formatação do CPF
    /// </summary>
    /// <param name="cpf">CPF com ou sem formatação</param>
    /// <returns>CPF apenas com números</returns>
    private static string LimparCpf(string cpf)
    {
        return Regex.Replace(cpf, @"[^\d]", "");
    }

    /// <summary>
    /// Formata o CPF com pontos e hífen
    /// </summary>
    /// <param name="cpf">CPF sem formatação</param>
    /// <returns>CPF formatado</returns>
    private static string FormatarCpf(string cpf)
    {
        if (cpf.Length != 11)
            return cpf;

        return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
    }

    /// <summary>
    /// Valida se o CPF é válido usando o algoritmo oficial
    /// </summary>
    /// <param name="cpf">CPF sem formatação</param>
    /// <returns>True se válido, False caso contrário</returns>
    private static bool ValidarCpf(string cpf)
    {
        // CPF deve ter 11 dígitos
        if (cpf.Length != 11)
            return false;

        // Verifica se todos os dígitos são iguais (CPF inválido)
        if (cpf.All(c => c == cpf[0]))
            return false;

        // Calcula o primeiro dígito verificador
        var soma = 0;
        for (int i = 0; i < 9; i++)
        {
            soma += int.Parse(cpf[i].ToString()) * (10 - i);
        }

        var resto = soma % 11;
        var digitoVerificador1 = resto < 2 ? 0 : 11 - resto;

        if (int.Parse(cpf[9].ToString()) != digitoVerificador1)
            return false;

        // Calcula o segundo dígito verificador
        soma = 0;
        for (int i = 0; i < 10; i++)
        {
            soma += int.Parse(cpf[i].ToString()) * (11 - i);
        }

        resto = soma % 11;
        var digitoVerificador2 = resto < 2 ? 0 : 11 - resto;

        return int.Parse(cpf[10].ToString()) == digitoVerificador2;
    }

    /// <summary>
    /// Retorna os componentes de igualdade
    /// </summary>
    protected override IEnumerable<object?> ObterComponentesIgualdade()
    {
        yield return Valor;
    }

    /// <summary>
    /// Representação em string do CPF
    /// </summary>
    public override string ToString() => ValorFormatado;

    /// <summary>
    /// Conversão implícita de string para CPF
    /// </summary>
    public static implicit operator Cpf(string valor) => new(valor);

    /// <summary>
    /// Conversão implícita de CPF para string
    /// </summary>
    public static implicit operator string(Cpf cpf) => cpf?.Valor ?? string.Empty;
}