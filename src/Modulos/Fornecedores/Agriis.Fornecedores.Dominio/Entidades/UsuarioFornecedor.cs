using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.Enums;

namespace Agriis.Fornecedores.Dominio.Entidades;

/// <summary>
/// Entidade que representa a associação entre usuário e fornecedor
/// </summary>
public class UsuarioFornecedor : EntidadeBase
{
    /// <summary>
    /// ID do usuário
    /// </summary>
    public int UsuarioId { get; private set; }
    
    /// <summary>
    /// ID do fornecedor
    /// </summary>
    public int FornecedorId { get; private set; }
    
    /// <summary>
    /// Role/perfil do usuário no fornecedor
    /// </summary>
    public Roles Role { get; private set; }
    
    /// <summary>
    /// Indica se o usuário está ativo no fornecedor
    /// </summary>
    public bool Ativo { get; private set; } = true;
    
    /// <summary>
    /// Data de início da associação
    /// </summary>
    public DateTime DataInicio { get; private set; }
    
    /// <summary>
    /// Data de fim da associação (opcional)
    /// </summary>
    public DateTime? DataFim { get; private set; }
    
    // Navigation Properties
    /// <summary>
    /// Usuário associado
    /// </summary>
    public virtual Agriis.Usuarios.Dominio.Entidades.Usuario Usuario { get; private set; } = null!;
    
    /// <summary>
    /// Fornecedor associado
    /// </summary>
    public virtual Fornecedor Fornecedor { get; private set; } = null!;
    
    /// <summary>
    /// Territórios de atuação do usuário
    /// </summary>
    public virtual ICollection<UsuarioFornecedorTerritorio> Territorios { get; private set; } = new List<UsuarioFornecedorTerritorio>();    

    /// <summary>
    /// Construtor protegido para Entity Framework
    /// </summary>
    protected UsuarioFornecedor() { }
    
    /// <summary>
    /// Construtor para criar uma nova associação usuário-fornecedor
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="role">Role do usuário</param>
    /// <param name="dataInicio">Data de início da associação</param>
    public UsuarioFornecedor(
        int usuarioId,
        int fornecedorId,
        Roles role,
        DateTime? dataInicio = null)
    {
        if (usuarioId <= 0)
            throw new ArgumentException("ID do usuário deve ser maior que zero", nameof(usuarioId));
            
        if (fornecedorId <= 0)
            throw new ArgumentException("ID do fornecedor deve ser maior que zero", nameof(fornecedorId));
            
        UsuarioId = usuarioId;
        FornecedorId = fornecedorId;
        Role = role;
        DataInicio = dataInicio ?? DateTime.UtcNow;
        Ativo = true;
        Territorios = new List<UsuarioFornecedorTerritorio>();
    }
    
    /// <summary>
    /// Ativa a associação usuário-fornecedor
    /// </summary>
    public void Ativar()
    {
        if (!Ativo)
        {
            Ativo = true;
            DataFim = null;
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Desativa a associação usuário-fornecedor
    /// </summary>
    /// <param name="dataFim">Data de fim da associação</param>
    public void Desativar(DateTime? dataFim = null)
    {
        if (Ativo)
        {
            Ativo = false;
            DataFim = dataFim ?? DateTime.UtcNow;
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Altera o role do usuário no fornecedor
    /// </summary>
    /// <param name="novoRole">Novo role</param>
    public void AlterarRole(Roles novoRole)
    {
        Role = novoRole;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Verifica se o usuário é administrador do fornecedor
    /// </summary>
    /// <returns>True se é administrador</returns>
    public bool EhAdministrador()
    {
        return Role == Roles.RoleFornecedorWebAdmin;
    }
    
    /// <summary>
    /// Verifica se o usuário é representante comercial
    /// </summary>
    /// <returns>True se é representante</returns>
    public bool EhRepresentante()
    {
        return Role == Roles.RoleFornecedorWebRepresentante;
    }
}