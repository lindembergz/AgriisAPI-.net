using System.Text.RegularExpressions;

namespace Agriis.Compartilhado.Dominio.Validadores;

/// <summary>
/// Validador para documentos brasileiros (CPF, CNPJ, etc.)
/// </summary>
public static class ValidadorDocumentosBrasileiros
{
    /// <summary>
    /// Valida se um CPF é válido
    /// </summary>
    /// <param name="cpf">CPF com ou sem formatação</param>
    /// <returns>True se válido, False caso contrário</returns>
    public static bool ValidarCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        // Remove formatação
        cpf = Regex.Replace(cpf, @"[^\d]", "");

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
    /// Valida se um CNPJ é válido
    /// </summary>
    /// <param name="cnpj">CNPJ com ou sem formatação</param>
    /// <returns>True se válido, False caso contrário</returns>
    public static bool ValidarCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        // Remove formatação
        cnpj = Regex.Replace(cnpj, @"[^\d]", "");

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
    /// Valida se uma Inscrição Estadual é válida (validação básica)
    /// </summary>
    /// <param name="inscricaoEstadual">Inscrição Estadual</param>
    /// <param name="uf">UF do estado</param>
    /// <returns>True se válida, False caso contrário</returns>
    public static bool ValidarInscricaoEstadual(string inscricaoEstadual, string uf)
    {
        if (string.IsNullOrWhiteSpace(inscricaoEstadual) || string.IsNullOrWhiteSpace(uf))
            return false;

        // Remove formatação
        inscricaoEstadual = Regex.Replace(inscricaoEstadual, @"[^\d]", "");

        // Verifica se é "ISENTO"
        if (inscricaoEstadual.ToUpper() == "ISENTO")
            return true;

        // Validação básica por tamanho (cada estado tem suas regras específicas)
        return uf.ToUpper() switch
        {
            "AC" => inscricaoEstadual.Length == 13,
            "AL" => inscricaoEstadual.Length == 9,
            "AP" => inscricaoEstadual.Length == 9,
            "AM" => inscricaoEstadual.Length == 9,
            "BA" => inscricaoEstadual.Length == 8 || inscricaoEstadual.Length == 9,
            "CE" => inscricaoEstadual.Length == 9,
            "DF" => inscricaoEstadual.Length == 13,
            "ES" => inscricaoEstadual.Length == 9,
            "GO" => inscricaoEstadual.Length == 9,
            "MA" => inscricaoEstadual.Length == 9,
            "MT" => inscricaoEstadual.Length == 11,
            "MS" => inscricaoEstadual.Length == 9,
            "MG" => inscricaoEstadual.Length == 13,
            "PA" => inscricaoEstadual.Length == 9,
            "PB" => inscricaoEstadual.Length == 9,
            "PR" => inscricaoEstadual.Length == 10,
            "PE" => inscricaoEstadual.Length == 9,
            "PI" => inscricaoEstadual.Length == 9,
            "RJ" => inscricaoEstadual.Length == 8,
            "RN" => inscricaoEstadual.Length == 9 || inscricaoEstadual.Length == 10,
            "RS" => inscricaoEstadual.Length == 10,
            "RO" => inscricaoEstadual.Length == 9 || inscricaoEstadual.Length == 14,
            "RR" => inscricaoEstadual.Length == 9,
            "SC" => inscricaoEstadual.Length == 9,
            "SP" => inscricaoEstadual.Length == 12,
            "SE" => inscricaoEstadual.Length == 9,
            "TO" => inscricaoEstadual.Length == 11,
            _ => false
        };
    }

    /// <summary>
    /// Valida se um CEP é válido
    /// </summary>
    /// <param name="cep">CEP com ou sem formatação</param>
    /// <returns>True se válido, False caso contrário</returns>
    public static bool ValidarCep(string cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
            return false;

        // Remove formatação
        cep = Regex.Replace(cep, @"[^\d]", "");

        // CEP deve ter 8 dígitos
        return cep.Length == 8 && cep.All(char.IsDigit);
    }

    /// <summary>
    /// Valida se um telefone brasileiro é válido
    /// </summary>
    /// <param name="telefone">Telefone com ou sem formatação</param>
    /// <returns>True se válido, False caso contrário</returns>
    public static bool ValidarTelefone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return false;

        // Remove formatação
        telefone = Regex.Replace(telefone, @"[^\d]", "");

        // Telefone deve ter 10 ou 11 dígitos (com DDD)
        // 10 dígitos: telefone fixo (XX) XXXX-XXXX
        // 11 dígitos: celular (XX) 9XXXX-XXXX
        return telefone.Length == 10 || telefone.Length == 11;
    }

    /// <summary>
    /// Valida se um email é válido
    /// </summary>
    /// <param name="email">Email a ser validado</param>
    /// <returns>True se válido, False caso contrário</returns>
    public static bool ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return emailRegex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Remove formatação de CPF
    /// </summary>
    /// <param name="cpf">CPF formatado</param>
    /// <returns>CPF sem formatação</returns>
    public static string LimparCpf(string cpf)
    {
        return string.IsNullOrWhiteSpace(cpf) ? string.Empty : Regex.Replace(cpf, @"[^\d]", "");
    }

    /// <summary>
    /// Remove formatação de CNPJ
    /// </summary>
    /// <param name="cnpj">CNPJ formatado</param>
    /// <returns>CNPJ sem formatação</returns>
    public static string LimparCnpj(string cnpj)
    {
        return string.IsNullOrWhiteSpace(cnpj) ? string.Empty : Regex.Replace(cnpj, @"[^\d]", "");
    }

    /// <summary>
    /// Formata CPF
    /// </summary>
    /// <param name="cpf">CPF sem formatação</param>
    /// <returns>CPF formatado</returns>
    public static string FormatarCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf) || cpf.Length != 11)
            return cpf;

        return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
    }

    /// <summary>
    /// Formata CNPJ
    /// </summary>
    /// <param name="cnpj">CNPJ sem formatação</param>
    /// <returns>CNPJ formatado</returns>
    public static string FormatarCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj) || cnpj.Length != 14)
            return cnpj;

        return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
    }

    /// <summary>
    /// Formata CEP
    /// </summary>
    /// <param name="cep">CEP sem formatação</param>
    /// <returns>CEP formatado</returns>
    public static string FormatarCep(string cep)
    {
        if (string.IsNullOrWhiteSpace(cep) || cep.Length != 8)
            return cep;

        return $"{cep.Substring(0, 5)}-{cep.Substring(5, 3)}";
    }

    /// <summary>
    /// Formata telefone
    /// </summary>
    /// <param name="telefone">Telefone sem formatação</param>
    /// <returns>Telefone formatado</returns>
    public static string FormatarTelefone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return telefone;

        telefone = Regex.Replace(telefone, @"[^\d]", "");

        return telefone.Length switch
        {
            10 => $"({telefone.Substring(0, 2)}) {telefone.Substring(2, 4)}-{telefone.Substring(6, 4)}",
            11 => $"({telefone.Substring(0, 2)}) {telefone.Substring(2, 5)}-{telefone.Substring(7, 4)}",
            _ => telefone
        };
    }
}