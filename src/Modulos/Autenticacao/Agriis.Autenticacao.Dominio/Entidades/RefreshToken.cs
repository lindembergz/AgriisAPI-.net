using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Autenticacao.Dominio.Entidades;

/// <summary>
/// Entidade que representa um refresh token para renovação de JWT
/// </summary>
public class RefreshToken : EntidadeBase
{
    /// <summary>
    /// Token de refresh único
    /// </summary>
    public string Token { get; private set; }
    
    /// <summary>
    /// ID do usuário proprietário do token
    /// </summary>
    public int UsuarioId { get; private set; }
    
    /// <summary>
    /// Data de expiração do refresh token (UTC com timezone)
    /// </summary>
    public DateTimeOffset DataExpiracao { get; private set; }
    
    /// <summary>
    /// Indica se o token foi revogado
    /// </summary>
    public bool Revogado { get; private set; }
    
    /// <summary>
    /// Data de revogação do token (UTC com timezone)
    /// </summary>
    public DateTimeOffset? DataRevogacao { get; private set; }
    
    /// <summary>
    /// Endereço IP que criou o token
    /// </summary>
    public string? EnderecoIp { get; private set; }
    
    /// <summary>
    /// User Agent que criou o token
    /// </summary>
    public string? UserAgent { get; private set; }
    
    /// <summary>
    /// Construtor protegido para o Entity Framework
    /// </summary>
    protected RefreshToken()
    {
        Token = string.Empty;
    }
    
    /// <summary>
    /// Construtor para criar um novo refresh token
    /// </summary>
    /// <param name="token">Token único</param>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="dataExpiracao">Data de expiração</param>
    /// <param name="enderecoIp">Endereço IP</param>
    /// <param name="userAgent">User Agent</param>
    public RefreshToken(string token, int usuarioId, DateTimeOffset dataExpiracao, string? enderecoIp = null, string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token é obrigatório", nameof(token));
            
        if (usuarioId <= 0)
            throw new ArgumentException("ID do usuário deve ser maior que zero", nameof(usuarioId));
            
        if (dataExpiracao <= DateTimeOffset.UtcNow)
            throw new ArgumentException("Data de expiração deve ser futura", nameof(dataExpiracao));
            
        Token = token;
        UsuarioId = usuarioId;
        DataExpiracao = dataExpiracao;
        EnderecoIp = enderecoIp;
        UserAgent = userAgent;
        Revogado = false;
    }
    
    /// <summary>
    /// Verifica se o token está válido (não expirado e não revogado)
    /// </summary>
    /// <returns>True se o token está válido</returns>
    public bool EstaValido()
    {
        return !Revogado && DataExpiracao > DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Revoga o refresh token
    /// </summary>
    public void Revogar()
    {
        Revogado = true;
        DataRevogacao = DateTimeOffset.UtcNow;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Verifica se o token expirou
    /// </summary>
    /// <returns>True se o token expirou</returns>
    public bool Expirou()
    {
        return DataExpiracao <= DateTimeOffset.UtcNow;
    }
}