namespace Agriis.Autenticacao.Dominio.Interfaces;

/// <summary>
/// Interface para serviços de senha
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Gera o hash de uma senha
    /// </summary>
    /// <param name="senha">Senha em texto plano</param>
    /// <returns>Hash da senha</returns>
    string GerarHash(string senha);
    
    /// <summary>
    /// Verifica se uma senha corresponde ao hash
    /// </summary>
    /// <param name="senha">Senha em texto plano</param>
    /// <param name="hash">Hash armazenado</param>
    /// <returns>True se a senha está correta</returns>
    bool VerificarSenha(string senha, string hash);
    
    /// <summary>
    /// Gera uma senha temporária aleatória
    /// </summary>
    /// <param name="tamanho">Tamanho da senha (padrão: 8)</param>
    /// <returns>Senha temporária</returns>
    string GerarSenhaTemporaria(int tamanho = 8);
    
    /// <summary>
    /// Valida se uma senha atende aos critérios de segurança
    /// </summary>
    /// <param name="senha">Senha a ser validada</param>
    /// <returns>True se a senha é válida</returns>
    bool ValidarForcaSenha(string senha);
}