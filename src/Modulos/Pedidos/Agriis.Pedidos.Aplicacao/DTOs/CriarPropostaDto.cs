using Agriis.Pedidos.Dominio.Enums;

namespace Agriis.Pedidos.Aplicacao.DTOs;

/// <summary>
/// DTO para criação de proposta
/// </summary>
public class CriarPropostaDto
{
    /// <summary>
    /// Ação do comprador (quando aplicável)
    /// </summary>
    public AcaoCompradorPedido? AcaoComprador { get; set; }
    
    /// <summary>
    /// Observação da proposta
    /// </summary>
    public string? Observacao { get; set; }
}

/// <summary>
/// DTO para listagem de propostas
/// </summary>
public class ListarPropostasDto
{
    /// <summary>
    /// Número da página
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Máximo de itens por página
    /// </summary>
    public int MaxPerPage { get; set; }
    
    /// <summary>
    /// Campo de ordenação
    /// </summary>
    public string? Sorting { get; set; }
}