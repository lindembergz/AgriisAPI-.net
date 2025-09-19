using Agriis.Compartilhado.Dominio.Enums;

namespace Agriis.Usuarios.Aplicacao.DTOs;

/// <summary>
/// DTO para representar um usuário
/// </summary>
public class UsuarioDto
{
    /// <summary>
    /// ID do usuário
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Email do usuário
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Número de celular do usuário
    /// </summary>
    public string? Celular { get; set; }
    
    /// <summary>
    /// CPF do usuário
    /// </summary>
    public string? Cpf { get; set; }
    
    /// <summary>
    /// Indica se o usuário está ativo
    /// </summary>
    public bool Ativo { get; set; }
    
    /// <summary>
    /// Data do último login
    /// </summary>
    public DateTime? UltimoLogin { get; set; }
    
    /// <summary>
    /// URL da logo/avatar do usuário
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime DataCriacao { get; set; }
    
    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? DataAtualizacao { get; set; }
    
    /// <summary>
    /// Roles do usuário
    /// </summary>
    public List<Roles> Roles { get; set; } = new();
}

/// <summary>
/// DTO para criar um novo usuário
/// </summary>
public class CriarUsuarioDto
{
    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Email do usuário
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Número de celular do usuário
    /// </summary>
    public string? Celular { get; set; }
    
    /// <summary>
    /// CPF do usuário
    /// </summary>
    public string? Cpf { get; set; }
    
    /// <summary>
    /// Senha do usuário
    /// </summary>
    public string? Senha { get; set; }
    
    /// <summary>
    /// Roles iniciais do usuário
    /// </summary>
    public List<Roles> Roles { get; set; } = new();
}

/// <summary>
/// DTO para atualizar um usuário
/// </summary>
public class AtualizarUsuarioDto
{
    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Número de celular do usuário
    /// </summary>
    public string? Celular { get; set; }
    
    /// <summary>
    /// CPF do usuário
    /// </summary>
    public string? Cpf { get; set; }
    
    /// <summary>
    /// URL da logo/avatar do usuário
    /// </summary>
    public string? LogoUrl { get; set; }
}

/// <summary>
/// DTO para alterar email do usuário
/// </summary>
public class AlterarEmailDto
{
    /// <summary>
    /// Novo email do usuário
    /// </summary>
    public string NovoEmail { get; set; } = string.Empty;
}

/// <summary>
/// DTO para alterar senha do usuário
/// </summary>
public class AlterarSenhaDto
{
    /// <summary>
    /// Senha atual
    /// </summary>
    public string SenhaAtual { get; set; } = string.Empty;
    
    /// <summary>
    /// Nova senha
    /// </summary>
    public string NovaSenha { get; set; } = string.Empty;
}

/// <summary>
/// DTO para gerenciar roles do usuário
/// </summary>
public class GerenciarRolesDto
{
    /// <summary>
    /// Roles a serem atribuídas ao usuário
    /// </summary>
    public List<Roles> Roles { get; set; } = new();
}

/// <summary>
/// DTO para resultado paginado de usuários
/// </summary>
public class UsuariosPaginadosDto
{
    /// <summary>
    /// Lista de usuários
    /// </summary>
    public List<UsuarioDto> Usuarios { get; set; } = new();
    
    /// <summary>
    /// Total de registros
    /// </summary>
    public int Total { get; set; }
    
    /// <summary>
    /// Página atual
    /// </summary>
    public int Pagina { get; set; }
    
    /// <summary>
    /// Tamanho da página
    /// </summary>
    public int TamanhoPagina { get; set; }
    
    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPaginas => (int)Math.Ceiling((double)Total / TamanhoPagina);
}