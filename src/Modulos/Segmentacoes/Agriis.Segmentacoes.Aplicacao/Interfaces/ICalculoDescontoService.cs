using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Segmentacoes.Aplicacao.DTOs;

namespace Agriis.Segmentacoes.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviços de cálculo de desconto segmentado
/// </summary>
public interface ICalculoDescontoService
{
    /// <summary>
    /// Calcula o desconto segmentado para um produtor, fornecedor e categoria
    /// </summary>
    /// <param name="produtorId">ID do produtor</param>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="categoriaId">ID da categoria</param>
    /// <param name="areaProdutor">Área total do produtor em hectares</param>
    /// <param name="valorBase">Valor base para cálculo do desconto</param>
    /// <returns>Resultado do cálculo de desconto</returns>
    Task<Result<ResultadoDescontoSegmentadoDto>> CalcularDescontoSegmentadoAsync(
        int produtorId, 
        int fornecedorId, 
        int categoriaId, 
        decimal areaProdutor, 
        decimal valorBase);
    
    /// <summary>
    /// Valida se uma área se enquadra em algum grupo de uma segmentação
    /// </summary>
    /// <param name="segmentacaoId">ID da segmentação</param>
    /// <param name="area">Área em hectares</param>
    /// <returns>True se a área se enquadra</returns>
    Task<Result<bool>> ValidarAreaSeEnquadraAsync(int segmentacaoId, decimal area);
    
    /// <summary>
    /// Obtém grupos aplicáveis para uma área específica
    /// </summary>
    /// <param name="segmentacaoId">ID da segmentação</param>
    /// <param name="area">Área em hectares</param>
    /// <returns>Lista de grupos aplicáveis</returns>
    Task<Result<IEnumerable<GrupoDto>>> ObterGruposAplicaveisAsync(int segmentacaoId, decimal area);
}