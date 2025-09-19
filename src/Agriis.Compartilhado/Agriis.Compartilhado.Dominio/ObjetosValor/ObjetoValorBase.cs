namespace Agriis.Compartilhado.Dominio.ObjetosValor;

/// <summary>
/// Classe base para objetos de valor (Value Objects) no DDD
/// Implementa igualdade baseada em valor ao invés de referência
/// </summary>
public abstract class ObjetoValorBase : IEquatable<ObjetoValorBase>
{
    /// <summary>
    /// Retorna os componentes que definem a igualdade do objeto de valor
    /// </summary>
    /// <returns>Enumeração dos componentes de igualdade</returns>
    protected abstract IEnumerable<object?> ObterComponentesIgualdade();
    
    /// <summary>
    /// Implementação de igualdade baseada nos componentes
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;
            
        return Equals((ObjetoValorBase)obj);
    }
    
    /// <summary>
    /// Implementação de igualdade tipada
    /// </summary>
    public bool Equals(ObjetoValorBase? other)
    {
        if (other == null)
            return false;
            
        return ObterComponentesIgualdade().SequenceEqual(other.ObterComponentesIgualdade());
    }
    
    /// <summary>
    /// Implementação de GetHashCode baseada nos componentes
    /// </summary>
    public override int GetHashCode()
    {
        return ObterComponentesIgualdade()
            .Where(x => x != null)
            .Aggregate(1, (current, obj) =>
            {
                unchecked
                {
                    return current * 23 + obj!.GetHashCode();
                }
            });
    }
    
    /// <summary>
    /// Operador de igualdade
    /// </summary>
    public static bool operator ==(ObjetoValorBase? left, ObjetoValorBase? right)
    {
        return Equals(left, right);
    }
    
    /// <summary>
    /// Operador de desigualdade
    /// </summary>
    public static bool operator !=(ObjetoValorBase? left, ObjetoValorBase? right)
    {
        return !Equals(left, right);
    }
    
    /// <summary>
    /// Cria uma cópia do objeto de valor
    /// </summary>
    /// <returns>Nova instância com os mesmos valores</returns>
    public virtual ObjetoValorBase Clonar()
    {
        return (ObjetoValorBase)MemberwiseClone();
    }
}