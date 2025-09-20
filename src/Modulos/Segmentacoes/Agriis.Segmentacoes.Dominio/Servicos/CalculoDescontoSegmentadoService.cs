using Agriis.Segmentacoes.Dominio.Entidades;
using Agriis.Segmentacoes.Dominio.Interfaces;

namespace Agriis.Segmentacoes.Dominio.Servicos;

/// <summary>
/// Serviço de domínio para cálculo de descontos segmentados
/// Replica a função desconto_segmentado_por_produtor_fornecedor_categoria do sistema Python
/// </summary>
public class CalculoDescontoSegmentadoService
{
    private readonly ISegmentacaoRepository _segmentacaoRepository;
    private readonly IGrupoRepository _grupoRepository;
    private readonly IGrupoSegmentacaoRepository _grupoSegmentacaoRepository;
    
    public CalculoDescontoSegmentadoService(
        ISegmentacaoRepository segmentacaoRepository,
        IGrupoRepository grupoRepository,
        IGrupoSegmentacaoRepository grupoSegmentacaoRepository)
    {
        _segmentacaoRepository = segmentacaoRepository;
        _grupoRepository = grupoRepository;
        _grupoSegmentacaoRepository = grupoSegmentacaoRepository;
    }
    
    /// <summary>
    /// Calcula o desconto segmentado para um produtor, fornecedor e categoria
    /// </summary>
    /// <param name="produtorId">ID do produtor</param>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="categoriaId">ID da categoria</param>
    /// <param name="areaProdutor">Área total do produtor em hectares</param>
    /// <param name="valorBase">Valor base para cálculo do desconto</param>
    /// <returns>Resultado do cálculo de desconto</returns>
    public async Task<ResultadoDescontoSegmentado> CalcularDescontoAsync(
        int produtorId, 
        int fornecedorId, 
        int categoriaId, 
        decimal areaProdutor, 
        decimal valorBase)
    {
        // 1. Buscar segmentação ativa para o fornecedor
        var segmentacao = await ObterSegmentacaoAplicavelAsync(fornecedorId, produtorId);
        
        if (segmentacao == null)
        {
            return new ResultadoDescontoSegmentado
            {
                PercentualDesconto = 0,
                ValorDesconto = 0,
                ValorFinal = valorBase,
                SegmentacaoAplicada = null,
                GrupoAplicado = null,
                Observacoes = "Nenhuma segmentação encontrada para o fornecedor"
            };
        }
        
        // 2. Buscar grupo que se enquadra na área do produtor
        var grupo = await _grupoRepository.ObterPorAreaAsync(segmentacao.Id, areaProdutor);
        
        if (grupo == null)
        {
            return new ResultadoDescontoSegmentado
            {
                PercentualDesconto = 0,
                ValorDesconto = 0,
                ValorFinal = valorBase,
                SegmentacaoAplicada = segmentacao.Nome,
                GrupoAplicado = null,
                Observacoes = $"Nenhum grupo encontrado para área de {areaProdutor} hectares"
            };
        }
        
        // 3. Buscar desconto específico para a categoria
        var grupoSegmentacao = await _grupoSegmentacaoRepository.ObterPorGrupoECategoriaAsync(grupo.Id, categoriaId);
        
        if (grupoSegmentacao == null || !grupoSegmentacao.Ativo)
        {
            return new ResultadoDescontoSegmentado
            {
                PercentualDesconto = 0,
                ValorDesconto = 0,
                ValorFinal = valorBase,
                SegmentacaoAplicada = segmentacao.Nome,
                GrupoAplicado = grupo.Nome,
                Observacoes = "Nenhum desconto configurado para esta categoria"
            };
        }
        
        // 4. Calcular desconto
        var valorDesconto = grupoSegmentacao.CalcularValorDesconto(valorBase);
        var valorFinal = grupoSegmentacao.CalcularValorComDesconto(valorBase);
        
        return new ResultadoDescontoSegmentado
        {
            PercentualDesconto = grupoSegmentacao.PercentualDesconto,
            ValorDesconto = valorDesconto,
            ValorFinal = valorFinal,
            SegmentacaoAplicada = segmentacao.Nome,
            GrupoAplicado = grupo.Nome,
            Observacoes = $"Desconto aplicado: {grupoSegmentacao.PercentualDesconto}% para área de {areaProdutor} hectares"
        };
    }
    
    /// <summary>
    /// Obtém a segmentação aplicável para um fornecedor e produtor
    /// Considera primeiro segmentações específicas, depois a padrão
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="produtorId">ID do produtor</param>
    /// <returns>Segmentação aplicável ou null</returns>
    private async Task<Segmentacao?> ObterSegmentacaoAplicavelAsync(int fornecedorId, int produtorId)
    {
        // Buscar segmentações ativas do fornecedor
        var segmentacoes = await _segmentacaoRepository.ObterAtivasPorFornecedorAsync(fornecedorId);
        
        // TODO: Implementar lógica de verificação territorial quando necessário
        // Por enquanto, usar a segmentação padrão ou a primeira ativa
        
        // Primeiro, tentar encontrar a segmentação padrão
        var segmentacaoPadrao = segmentacoes.FirstOrDefault(s => s.EhPadrao);
        if (segmentacaoPadrao != null)
            return segmentacaoPadrao;
        
        // Se não houver padrão, usar a primeira ativa
        return segmentacoes.FirstOrDefault();
    }
    
    /// <summary>
    /// Valida se uma área se enquadra em algum grupo de uma segmentação
    /// </summary>
    /// <param name="segmentacaoId">ID da segmentação</param>
    /// <param name="area">Área em hectares</param>
    /// <returns>True se a área se enquadra</returns>
    public async Task<bool> ValidarAreaSeEnquadraAsync(int segmentacaoId, decimal area)
    {
        var grupo = await _grupoRepository.ObterPorAreaAsync(segmentacaoId, area);
        return grupo != null;
    }
    
    /// <summary>
    /// Obtém todos os grupos aplicáveis para uma área
    /// </summary>
    /// <param name="segmentacaoId">ID da segmentação</param>
    /// <param name="area">Área em hectares</param>
    /// <returns>Lista de grupos aplicáveis</returns>
    public async Task<IEnumerable<Grupo>> ObterGruposAplicaveisAsync(int segmentacaoId, decimal area)
    {
        var grupos = await _grupoRepository.ObterAtivosPorSegmentacaoAsync(segmentacaoId);
        return grupos.Where(g => g.AreaSeEnquadra(area));
    }
}

/// <summary>
/// Resultado do cálculo de desconto segmentado
/// </summary>
public class ResultadoDescontoSegmentado
{
    /// <summary>
    /// Percentual de desconto aplicado
    /// </summary>
    public decimal PercentualDesconto { get; set; }
    
    /// <summary>
    /// Valor do desconto em moeda
    /// </summary>
    public decimal ValorDesconto { get; set; }
    
    /// <summary>
    /// Valor final após aplicar o desconto
    /// </summary>
    public decimal ValorFinal { get; set; }
    
    /// <summary>
    /// Nome da segmentação aplicada
    /// </summary>
    public string? SegmentacaoAplicada { get; set; }
    
    /// <summary>
    /// Nome do grupo aplicado
    /// </summary>
    public string? GrupoAplicado { get; set; }
    
    /// <summary>
    /// Observações sobre o cálculo
    /// </summary>
    public string? Observacoes { get; set; }
}