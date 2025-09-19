using MediatR;

namespace Agriis.Compartilhado.Dominio.Entidades;

/// <summary>
/// Classe base para entidades que são raízes de agregado no DDD
/// Suporta eventos de domínio através do MediatR
/// </summary>
public abstract class EntidadeRaizAgregada : EntidadeBase
{
    private readonly List<INotification> _eventosdominio = new();
    
    /// <summary>
    /// Lista de eventos de domínio pendentes
    /// </summary>
    public IReadOnlyCollection<INotification> EventosDominio => _eventosdominio.AsReadOnly();
    
    /// <summary>
    /// Adiciona um evento de domínio à lista de eventos pendentes
    /// </summary>
    /// <param name="eventoDominio">Evento a ser adicionado</param>
    protected void AdicionarEventoDominio(INotification eventoDominio)
    {
        _eventosdominio.Add(eventoDominio);
    }
    
    /// <summary>
    /// Remove um evento de domínio específico da lista
    /// </summary>
    /// <param name="eventoDominio">Evento a ser removido</param>
    protected void RemoverEventoDominio(INotification eventoDominio)
    {
        _eventosdominio.Remove(eventoDominio);
    }
    
    /// <summary>
    /// Limpa todos os eventos de domínio pendentes
    /// </summary>
    public void LimparEventosDominio()
    {
        _eventosdominio.Clear();
    }
    
    /// <summary>
    /// Verifica se existem eventos de domínio pendentes
    /// </summary>
    /// <returns>True se existem eventos pendentes</returns>
    public bool TemEventosPendentes()
    {
        return _eventosdominio.Count > 0;
    }
}