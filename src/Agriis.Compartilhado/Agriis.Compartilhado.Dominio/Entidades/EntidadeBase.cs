namespace Agriis.Compartilhado.Dominio.Entidades;

/// <summary>
/// Classe base para todas as entidades do sistema com auditoria
/// </summary>
public abstract class EntidadeBase
{
    /// <summary>
    /// Identificador único da entidade
    /// </summary>
    public int Id { get; protected set; }
    
    /// <summary>
    /// Data de criação da entidade
    /// </summary>
    public DateTime DataCriacao { get; protected set; }
    
    /// <summary>
    /// Data da última atualização da entidade
    /// </summary>
    public DateTime? DataAtualizacao { get; protected set; }
    
    /// <summary>
    /// Construtor protegido para uso pelas classes filhas
    /// </summary>
    protected EntidadeBase()
    {
        DataCriacao = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Atualiza a data de modificação da entidade
    /// </summary>
    public void AtualizarDataModificacao()
    {
        DataAtualizacao = DateTime.UtcNow;
    }

    public void SetDataCriacao(DateTime dataCriacao)
    {
        DataCriacao = dataCriacao;
    }
    
    /// <summary>
    /// Verifica se a entidade é transitória (não foi persistida ainda)
    /// </summary>
    /// <returns>True se a entidade é transitória</returns>
    public bool EhTransitoria()
    {
        return Id == default;
    }
    
    /// <summary>
    /// Implementação de igualdade baseada no Id
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not EntidadeBase other)
            return false;
            
        if (ReferenceEquals(this, other))
            return true;
            
        if (GetType() != other.GetType())
            return false;
            
        if (EhTransitoria() || other.EhTransitoria())
            return false;
            
        return Id == other.Id;
    }
    
    /// <summary>
    /// Implementação de GetHashCode baseada no Id
    /// </summary>
    public override int GetHashCode()
    {
        if (EhTransitoria())
            return base.GetHashCode();
            
        return Id.GetHashCode();
    }
    
    /// <summary>
    /// Operador de igualdade
    /// </summary>
    public static bool operator ==(EntidadeBase? left, EntidadeBase? right)
    {
        return Equals(left, right);
    }
    
    /// <summary>
    /// Operador de desigualdade
    /// </summary>
    public static bool operator !=(EntidadeBase? left, EntidadeBase? right)
    {
        return !Equals(left, right);
    }
}