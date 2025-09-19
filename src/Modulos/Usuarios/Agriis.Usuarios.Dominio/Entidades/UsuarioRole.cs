using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.Enums;

namespace Agriis.Usuarios.Dominio.Entidades;

/// <summary>
/// Entidade que representa a associação entre usuário e role
/// </summary>
public class UsuarioRole : EntidadeBase
{
    /// <summary>
    /// ID do usuário
    /// </summary>
    public int UsuarioId { get; private set; }
    
    /// <summary>
    /// Role associada ao usuário
    /// </summary>
    public Roles Role { get; private set; }
    
    /// <summary>
    /// Data de atribuição da role
    /// </summary>
    public DateTime DataAtribuicao { get; private set; }
    
    /// <summary>
    /// Usuário associado
    /// </summary>
    public virtual Usuario Usuario { get; private set; } = null!;
    
    /// <summary>
    /// Construtor protegido para o Entity Framework
    /// </summary>
    protected UsuarioRole()
    {
    }
    
    /// <summary>
    /// Construtor para criar uma nova associação usuário-role
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="role">Role a ser associada</param>
    public UsuarioRole(int usuarioId, Roles role)
    {
        UsuarioId = usuarioId;
        Role = role;
        DataAtribuicao = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Verifica se a role é de administrador
    /// </summary>
    /// <returns>True se é role de admin</returns>
    public bool EhRoleAdmin()
    {
        return Role == Roles.RoleAdmin;
    }
    
    /// <summary>
    /// Verifica se a role é de fornecedor
    /// </summary>
    /// <returns>True se é role de fornecedor</returns>
    public bool EhRoleFornecedor()
    {
        return Role == Roles.RoleFornecedorWebAdmin || 
               Role == Roles.RoleFornecedorWebRepresentante;
    }
    
    /// <summary>
    /// Verifica se a role é de comprador/produtor
    /// </summary>
    /// <returns>True se é role de comprador</returns>
    public bool EhRoleComprador()
    {
        return Role == Roles.RoleComprador;
    }
}