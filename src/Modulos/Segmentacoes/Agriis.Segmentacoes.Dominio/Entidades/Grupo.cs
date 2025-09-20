using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Segmentacoes.Dominio.Entidades;

/// <summary>
/// Representa um grupo de segmentação com faixas de área
/// </summary>
public class Grupo : EntidadeBase
{
    /// <summary>
    /// Nome do grupo
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// Descrição do grupo
    /// </summary>
    public string? Descricao { get; private set; }
    
    /// <summary>
    /// Área mínima em hectares para este grupo
    /// </summary>
    public decimal AreaMinima { get; private set; }
    
    /// <summary>
    /// Área máxima em hectares para este grupo (null = sem limite)
    /// </summary>
    public decimal? AreaMaxima { get; private set; }
    
    /// <summary>
    /// Indica se o grupo está ativo
    /// </summary>
    public bool Ativo { get; private set; }
    
    /// <summary>
    /// ID da segmentação proprietária
    /// </summary>
    public int SegmentacaoId { get; private set; }
    
    /// <summary>
    /// Segmentação proprietária
    /// </summary>
    public virtual Segmentacao Segmentacao { get; private set; } = null!;
    
    /// <summary>
    /// Grupos de segmentação (descontos por categoria) associados
    /// </summary>
    public virtual ICollection<GrupoSegmentacao> GruposSegmentacao { get; private set; } = new List<GrupoSegmentacao>();
    
    /// <summary>
    /// Construtor protegido para o Entity Framework
    /// </summary>
    protected Grupo() { }
    
    /// <summary>
    /// Construtor para criar um novo grupo
    /// </summary>
    /// <param name="nome">Nome do grupo</param>
    /// <param name="segmentacaoId">ID da segmentação</param>
    /// <param name="areaMinima">Área mínima em hectares</param>
    /// <param name="areaMaxima">Área máxima em hectares (opcional)</param>
    /// <param name="descricao">Descrição opcional</param>
    public Grupo(string nome, int segmentacaoId, decimal areaMinima, decimal? areaMaxima = null, string? descricao = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do grupo é obrigatório", nameof(nome));
            
        if (segmentacaoId <= 0)
            throw new ArgumentException("ID da segmentação deve ser válido", nameof(segmentacaoId));
            
        if (areaMinima < 0)
            throw new ArgumentException("Área mínima não pode ser negativa", nameof(areaMinima));
            
        if (areaMaxima.HasValue && areaMaxima.Value < areaMinima)
            throw new ArgumentException("Área máxima deve ser maior que a área mínima", nameof(areaMaxima));
            
        Nome = nome.Trim();
        SegmentacaoId = segmentacaoId;
        AreaMinima = areaMinima;
        AreaMaxima = areaMaxima;
        Descricao = descricao?.Trim();
        Ativo = true;
    }
    
    /// <summary>
    /// Atualiza as informações básicas do grupo
    /// </summary>
    /// <param name="nome">Novo nome</param>
    /// <param name="descricao">Nova descrição</param>
    public void AtualizarInformacoes(string nome, string? descricao = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do grupo é obrigatório", nameof(nome));
            
        Nome = nome.Trim();
        Descricao = descricao?.Trim();
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza as faixas de área do grupo
    /// </summary>
    /// <param name="areaMinima">Nova área mínima</param>
    /// <param name="areaMaxima">Nova área máxima</param>
    public void AtualizarFaixasArea(decimal areaMinima, decimal? areaMaxima = null)
    {
        if (areaMinima < 0)
            throw new ArgumentException("Área mínima não pode ser negativa", nameof(areaMinima));
            
        if (areaMaxima.HasValue && areaMaxima.Value < areaMinima)
            throw new ArgumentException("Área máxima deve ser maior que a área mínima", nameof(areaMaxima));
            
        AreaMinima = areaMinima;
        AreaMaxima = areaMaxima;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Ativa o grupo
    /// </summary>
    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Desativa o grupo
    /// </summary>
    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Verifica se uma área se enquadra neste grupo
    /// </summary>
    /// <param name="area">Área em hectares</param>
    /// <returns>True se a área se enquadra no grupo</returns>
    public bool AreaSeEnquadra(decimal area)
    {
        if (!Ativo)
            return false;
            
        if (area < AreaMinima)
            return false;
            
        if (AreaMaxima.HasValue && area > AreaMaxima.Value)
            return false;
            
        return true;
    }
    
    /// <summary>
    /// Adiciona um grupo de segmentação (desconto por categoria)
    /// </summary>
    /// <param name="grupoSegmentacao">Grupo de segmentação a ser adicionado</param>
    public void AdicionarGrupoSegmentacao(GrupoSegmentacao grupoSegmentacao)
    {
        if (grupoSegmentacao == null)
            throw new ArgumentNullException(nameof(grupoSegmentacao));
            
        if (!GruposSegmentacao.Contains(grupoSegmentacao))
        {
            GruposSegmentacao.Add(grupoSegmentacao);
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Remove um grupo de segmentação
    /// </summary>
    /// <param name="grupoSegmentacao">Grupo de segmentação a ser removido</param>
    public void RemoverGrupoSegmentacao(GrupoSegmentacao grupoSegmentacao)
    {
        if (grupoSegmentacao != null && GruposSegmentacao.Contains(grupoSegmentacao))
        {
            GruposSegmentacao.Remove(grupoSegmentacao);
            AtualizarDataModificacao();
        }
    }
}