using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Produtores.Dominio.Entidades;

/// <summary>
/// Entidade que representa o relacionamento entre usuário e produtor
/// </summary>
public class UsuarioProdutor : EntidadeBase
{
    /// <summary>
    /// ID do usuário
    /// </summary>
    public int UsuarioId { get; private set; }
    
    /// <summary>
    /// ID do produtor
    /// </summary>
    public int ProdutorId { get; private set; }
    
    /// <summary>
    /// Indica se o usuário é o proprietário principal do produtor
    /// </summary>
    public bool EhProprietario { get; private set; }
    
    /// <summary>
    /// Indica se o relacionamento está ativo
    /// </summary>
    public bool Ativo { get; private set; }
    
    // Navigation Properties
    /// <summary>
    /// Usuário associado
    /// </summary>
    public virtual Agriis.Usuarios.Dominio.Entidades.Usuario Usuario { get; private set; } = null!;
    
    /// <summary>
    /// Produtor associado
    /// </summary>
    public virtual Produtor Produtor { get; private set; } = null!;
    
    /// <summary>
    /// Construtor protegido para Entity Framework
    /// </summary>
    protected UsuarioProdutor() { }
    
    /// <summary>
    /// Construtor para criar um novo relacionamento usuário-produtor
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="produtorId">ID do produtor</param>
    /// <param name="ehProprietario">Se é proprietário principal</param>
    public UsuarioProdutor(int usuarioId, int produtorId, bool ehProprietario = false)
    {
        if (usuarioId <= 0)
            throw new ArgumentException("ID do usuário deve ser maior que zero", nameof(usuarioId));
            
        if (produtorId <= 0)
            throw new ArgumentException("ID do produtor deve ser maior que zero", nameof(produtorId));
            
        UsuarioId = usuarioId;
        ProdutorId = produtorId;
        EhProprietario = ehProprietario;
        Ativo = true;
    }
    
    /// <summary>
    /// Ativa o relacionamento
    /// </summary>
    public void Ativar()
    {
        if (!Ativo)
        {
            Ativo = true;
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Desativa o relacionamento
    /// </summary>
    public void Desativar()
    {
        if (Ativo)
        {
            Ativo = false;
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Define como proprietário principal
    /// </summary>
    public void DefinirComoProprietario()
    {
        if (!EhProprietario)
        {
            EhProprietario = true;
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Remove como proprietário principal
    /// </summary>
    public void RemoverComoProprietario()
    {
        if (EhProprietario)
        {
            EhProprietario = false;
            AtualizarDataModificacao();
        }
    }
}