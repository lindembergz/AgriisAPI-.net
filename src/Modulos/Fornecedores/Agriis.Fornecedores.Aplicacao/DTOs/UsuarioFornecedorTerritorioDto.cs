using System.Text.Json;

namespace Agriis.Fornecedores.Aplicacao.DTOs;

/// <summary>
/// DTO para território de usuário fornecedor
/// </summary>
public class UsuarioFornecedorTerritorioDto
{
    /// <summary>
    /// ID do território
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// ID da associação usuário-fornecedor
    /// </summary>
    public int UsuarioFornecedorId { get; set; }
    
    /// <summary>
    /// Estados de atuação em formato JSON
    /// </summary>
    public JsonDocument Estados { get; set; } = null!;
    
    /// <summary>
    /// Lista de estados (para facilitar o uso)
    /// </summary>
    public List<string> EstadosLista { get; set; } = new();
    
    /// <summary>
    /// Municípios específicos em formato JSON
    /// </summary>
    public JsonDocument? Municipios { get; set; }
    
    /// <summary>
    /// Lista de municípios por estado (para facilitar o uso)
    /// </summary>
    public Dictionary<string, List<string>> MunicipiosLista { get; set; } = new();
    
    /// <summary>
    /// Indica se é território padrão
    /// </summary>
    public bool TerritorioPadrao { get; set; }
    
    /// <summary>
    /// Indica se o território está ativo
    /// </summary>
    public bool Ativo { get; set; }
    
    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime DataCriacao { get; set; }
    
    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? DataAtualizacao { get; set; }
}