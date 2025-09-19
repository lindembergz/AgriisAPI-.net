using Agriis.Autenticacao.Dominio.Interfaces;
using System.Text.RegularExpressions;

namespace Agriis.Autenticacao.Infraestrutura.Servicos;

/// <summary>
/// Serviço para gerenciamento de senhas
/// </summary>
public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12; // Fator de trabalho do BCrypt

    public string GerarHash(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            throw new ArgumentException("Senha não pode ser vazia", nameof(senha));

        return BCrypt.Net.BCrypt.HashPassword(senha, WorkFactor);
    }

    public bool VerificarSenha(string senha, string hash)
    {
        if (string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(senha, hash);
        }
        catch
        {
            return false;
        }
    }

    public string GerarSenhaTemporaria(int tamanho = 8)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var random = new Random();
        
        return new string(Enumerable.Repeat(chars, tamanho)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public bool ValidarForcaSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            return false;

        // Mínimo 6 caracteres
        if (senha.Length < 6)
            return false;

        // Pelo menos uma letra
        if (!Regex.IsMatch(senha, @"[a-zA-Z]"))
            return false;

        // Pelo menos um número
        if (!Regex.IsMatch(senha, @"\d"))
            return false;

        // Não pode ter espaços
        if (senha.Contains(' '))
            return false;

        return true;
    }
}