using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Segmentacoes.Dominio.Entidades;

/// <summary>
/// Representa os descontos por categoria para um grupo de segmentação
/// </summary>
public class GrupoSegmentacao : EntidadeBase
{
    /// <summary>
    /// ID do grupo proprietário
    /// </summary>
    public int GrupoId { get; private set; }
    
    /// <summary>
    /// ID da categoria de produto
    /// </summary>
    public int CategoriaId { get; private set; }
    
    /// <summary>
    /// Percentual de desconto aplicado (0-100)
    /// </summary>
    public decimal PercentualDesconto { get; private set; }
    
    /// <summary>
    /// Indica se o desconto está ativo
    /// </summary>
    public bool Ativo { get; private set; }
    
    /// <summary>
    /// Observações sobre o desconto
    /// </summary>
    public string? Observacoes { get; private set; }
    
    /// <summary>
    /// Grupo proprietário
    /// </summary>
    public virtual Grupo Grupo { get; private set; } = null!;
    
    /// <summary>
    /// Construtor protegido para o Entity Framework
    /// </summary>
    protected GrupoSegmentacao() { }
    
    /// <summary>
    /// Construtor para criar um novo grupo de segmentação
    /// </summary>
    /// <param name="grupoId">ID do grupo</param>
    /// <param name="categoriaId">ID da categoria</param>
    /// <param name="percentualDesconto">Percentual de desconto (0-100)</param>
    /// <param name="observacoes">Observações opcionais</param>
    public GrupoSegmentacao(int grupoId, int categoriaId, decimal percentualDesconto, string? observacoes = null)
    {
        if (grupoId <= 0)
            throw new ArgumentException("ID do grupo deve ser válido", nameof(grupoId));
            
        if (categoriaId <= 0)
            throw new ArgumentException("ID da categoria deve ser válido", nameof(categoriaId));
            
        if (percentualDesconto < 0 || percentualDesconto > 100)
            throw new ArgumentException("Percentual de desconto deve estar entre 0 e 100", nameof(percentualDesconto));
            
        GrupoId = grupoId;
        CategoriaId = categoriaId;
        PercentualDesconto = percentualDesconto;
        Observacoes = observacoes?.Trim();
        Ativo = true;
    }
    
    /// <summary>
    /// Atualiza o percentual de desconto
    /// </summary>
    /// <param name="novoPercentual">Novo percentual de desconto</param>
    public void AtualizarPercentualDesconto(decimal novoPercentual)
    {
        if (novoPercentual < 0 || novoPercentual > 100)
            throw new ArgumentException("Percentual de desconto deve estar entre 0 e 100", nameof(novoPercentual));
            
        PercentualDesconto = novoPercentual;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza as observações
    /// </summary>
    /// <param name="observacoes">Novas observações</param>
    public void AtualizarObservacoes(string? observacoes)
    {
        Observacoes = observacoes?.Trim();
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Ativa o desconto
    /// </summary>
    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Desativa o desconto
    /// </summary>
    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Calcula o valor do desconto para um valor base
    /// </summary>
    /// <param name="valorBase">Valor base para cálculo</param>
    /// <returns>Valor do desconto</returns>
    public decimal CalcularValorDesconto(decimal valorBase)
    {
        if (!Ativo || valorBase <= 0)
            return 0;
            
        return valorBase * (PercentualDesconto / 100);
    }
    
    /// <summary>
    /// Calcula o valor final após aplicar o desconto
    /// </summary>
    /// <param name="valorBase">Valor base</param>
    /// <returns>Valor final com desconto aplicado</returns>
    public decimal CalcularValorComDesconto(decimal valorBase)
    {
        if (!Ativo || valorBase <= 0)
            return valorBase;
            
        var valorDesconto = CalcularValorDesconto(valorBase);
        return valorBase - valorDesconto;
    }
}