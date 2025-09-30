using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Compartilhado.Dominio.ObjetosValor;

namespace Agriis.Usuarios.Dominio.Entidades;

/// <summary>
/// Entidade que representa um usuário do sistema
/// </summary>
public class Usuario : EntidadeRaizAgregada
{
    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    public string Nome { get; private set; }
    
    /// <summary>
    /// Email do usuário (único no sistema)
    /// </summary>
    public string Email { get; private set; }
    
    /// <summary>
    /// Número de celular do usuário
    /// </summary>
    public string? Celular { get; private set; }
    
    /// <summary>
    /// CPF do usuário
    /// </summary>
    public Cpf? Cpf { get; private set; }
    
    /// <summary>
    /// Hash da senha do usuário
    /// </summary>
    public string? SenhaHash { get; private set; }
    
    /// <summary>
    /// Indica se o usuário está ativo
    /// </summary>
    public bool Ativo { get; private set; }
    
    /// <summary>
    /// Data do último login (UTC com timezone)
    /// </summary>
    public DateTimeOffset? UltimoLogin { get; private set; }
    
    /// <summary>
    /// URL da logo/avatar do usuário (AWS S3)
    /// </summary>
    public string? LogoUrl { get; private set; }
    
    /// <summary>
    /// Roles/perfis associados ao usuário
    /// </summary>
    public virtual ICollection<UsuarioRole> UsuarioRoles { get; private set; }
    
    /// <summary>
    /// Construtor protegido para o Entity Framework
    /// </summary>
    protected Usuario()
    {
        Nome = string.Empty;
        Email = string.Empty;
        UsuarioRoles = new List<UsuarioRole>();
    }
    
    /// <summary>
    /// Construtor para criar um novo usuário
    /// </summary>
    /// <param name="nome">Nome completo do usuário</param>
    /// <param name="email">Email do usuário</param>
    /// <param name="celular">Número de celular (opcional)</param>
    /// <param name="cpf">CPF do usuário (opcional)</param>
    public Usuario(string nome, string email, string? celular = null, Cpf? cpf = null)
    {
        ValidarDadosObrigatorios(nome, email);
        
        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        Celular = celular?.Trim();
        Cpf = cpf;
        Ativo = true;
        UsuarioRoles = new List<UsuarioRole>();
    }
    
    /// <summary>
    /// Atualiza os dados básicos do usuário
    /// </summary>
    /// <param name="nome">Novo nome</param>
    /// <param name="celular">Novo celular</param>
    /// <param name="cpf">Novo CPF</param>
    public void AtualizarDados(string nome, string? celular = null, Cpf? cpf = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório", nameof(nome));
            
        Nome = nome.Trim();
        Celular = celular?.Trim();
        Cpf = cpf;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza o email do usuário
    /// </summary>
    /// <param name="novoEmail">Novo email</param>
    public void AtualizarEmail(string novoEmail)
    {
        if (string.IsNullOrWhiteSpace(novoEmail))
            throw new ArgumentException("Email é obrigatório", nameof(novoEmail));
            
        if (!ValidarEmail(novoEmail))
            throw new ArgumentException("Email inválido", nameof(novoEmail));
            
        Email = novoEmail.Trim().ToLowerInvariant();
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Define a senha do usuário
    /// </summary>
    /// <param name="senhaHash">Hash da senha</param>
    public void DefinirSenha(string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("Hash da senha é obrigatório", nameof(senhaHash));
            
        SenhaHash = senhaHash;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Ativa o usuário
    /// </summary>
    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Desativa o usuário
    /// </summary>
    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Registra o último login do usuário
    /// </summary>
    public void RegistrarLogin()
    {
        UltimoLogin = DateTimeOffset.UtcNow;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza a URL da logo do usuário
    /// </summary>
    /// <param name="logoUrl">Nova URL da logo</param>
    public void AtualizarLogo(string? logoUrl)
    {
        LogoUrl = logoUrl;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Adiciona uma role ao usuário
    /// </summary>
    /// <param name="role">Role a ser adicionada</param>
    public void AdicionarRole(Roles role)
    {
        if (UsuarioRoles.Any(ur => ur.Role == role))
            return; // Role já existe
            
        var usuarioRole = new UsuarioRole(Id, role);
        UsuarioRoles.Add(usuarioRole);
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Remove uma role do usuário
    /// </summary>
    /// <param name="role">Role a ser removida</param>
    public void RemoverRole(Roles role)
    {
        var usuarioRole = UsuarioRoles.FirstOrDefault(ur => ur.Role == role);
        if (usuarioRole != null)
        {
            UsuarioRoles.Remove(usuarioRole);
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Verifica se o usuário possui uma role específica
    /// </summary>
    /// <param name="role">Role a ser verificada</param>
    /// <returns>True se o usuário possui a role</returns>
    public bool PossuiRole(Roles role)
    {
        return UsuarioRoles.Any(ur => ur.Role == role);
    }
    
    /// <summary>
    /// Obtém todas as roles do usuário
    /// </summary>
    /// <returns>Lista de roles</returns>
    public IEnumerable<Roles> ObterRoles()
    {
        return UsuarioRoles.Select(ur => ur.Role);
    }
    
    /// <summary>
    /// Valida os dados obrigatórios
    /// </summary>
    private static void ValidarDadosObrigatorios(string nome, string email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório", nameof(nome));
            
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email é obrigatório", nameof(email));
            
        if (!ValidarEmail(email))
            throw new ArgumentException("Email inválido", nameof(email));
    }
    
    /// <summary>
    /// Valida o formato do email
    /// </summary>
    /// <param name="email">Email a ser validado</param>
    /// <returns>True se o email é válido</returns>
    private static bool ValidarEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}