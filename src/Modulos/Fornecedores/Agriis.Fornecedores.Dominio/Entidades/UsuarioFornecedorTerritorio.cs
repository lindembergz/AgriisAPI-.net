using System.Text.Json;
using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Fornecedores.Dominio.Entidades;

/// <summary>
/// Entidade que representa o território de atuação de um usuário fornecedor
/// </summary>
public class UsuarioFornecedorTerritorio : EntidadeBase
{
    /// <summary>
    /// ID da associação usuário-fornecedor
    /// </summary>
    public int UsuarioFornecedorId { get; private set; }
    
    /// <summary>
    /// Estados de atuação em formato JSON
    /// Exemplo: ["SP", "MG", "RJ"]
    /// </summary>
    public JsonDocument Estados { get; private set; } = null!;
    
    /// <summary>
    /// Municípios específicos de atuação em formato JSON
    /// Exemplo: [{"estado": "SP", "municipios": ["São Paulo", "Campinas"]}, ...]
    /// </summary>
    public JsonDocument? Municipios { get; private set; }
    
    /// <summary>
    /// Indica se é território padrão (aplicado quando não há configuração específica)
    /// </summary>
    public bool TerritorioPadrao { get; private set; } = false;
    
    /// <summary>
    /// Indica se o território está ativo
    /// </summary>
    public bool Ativo { get; private set; } = true;
    
    // Navigation Properties
    /// <summary>
    /// Associação usuário-fornecedor
    /// </summary>
    public virtual UsuarioFornecedor UsuarioFornecedor { get; private set; } = null!;
    
    /// <summary>
    /// Construtor protegido para Entity Framework
    /// </summary>
    protected UsuarioFornecedorTerritorio() { }
    
    /// <summary>
    /// Construtor para criar um novo território
    /// </summary>
    /// <param name="usuarioFornecedorId">ID da associação usuário-fornecedor</param>
    /// <param name="estados">Estados de atuação</param>
    /// <param name="municipios">Municípios específicos (opcional)</param>
    /// <param name="territorioPadrao">Se é território padrão</param>
    public UsuarioFornecedorTerritorio(
        int usuarioFornecedorId,
        JsonDocument estados,
        JsonDocument? municipios = null,
        bool territorioPadrao = false)
    {
        if (usuarioFornecedorId <= 0)
            throw new ArgumentException("ID da associação usuário-fornecedor deve ser maior que zero", nameof(usuarioFornecedorId));
            
        UsuarioFornecedorId = usuarioFornecedorId;
        Estados = estados ?? throw new ArgumentNullException(nameof(estados));
        Municipios = municipios;
        TerritorioPadrao = territorioPadrao;
        Ativo = true;
    }    
  
  /// <summary>
    /// Atualiza os estados de atuação
    /// </summary>
    /// <param name="estados">Novos estados</param>
    public void AtualizarEstados(JsonDocument estados)
    {
        Estados = estados ?? throw new ArgumentNullException(nameof(estados));
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza os municípios específicos
    /// </summary>
    /// <param name="municipios">Novos municípios</param>
    public void AtualizarMunicipios(JsonDocument? municipios)
    {
        Municipios = municipios;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Define como território padrão
    /// </summary>
    /// <param name="territorioPadrao">Se é território padrão</param>
    public void DefinirTerritorioPadrao(bool territorioPadrao)
    {
        TerritorioPadrao = territorioPadrao;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Ativa o território
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
    /// Desativa o território
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
    /// Verifica se o território inclui um estado específico
    /// </summary>
    /// <param name="uf">UF do estado</param>
    /// <returns>True se inclui o estado</returns>
    public bool IncluiEstado(string uf)
    {
        if (string.IsNullOrWhiteSpace(uf) || Estados == null)
            return false;
            
        try
        {
            var estados = Estados.RootElement.EnumerateArray()
                .Select(e => e.GetString())
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList();
                
            return estados.Contains(uf.ToUpper());
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Verifica se o território inclui um município específico
    /// </summary>
    /// <param name="uf">UF do estado</param>
    /// <param name="municipio">Nome do município</param>
    /// <returns>True se inclui o município</returns>
    public bool IncluiMunicipio(string uf, string municipio)
    {
        if (string.IsNullOrWhiteSpace(uf) || string.IsNullOrWhiteSpace(municipio) || Municipios == null)
            return false;
            
        try
        {
            foreach (var item in Municipios.RootElement.EnumerateArray())
            {
                if (item.TryGetProperty("estado", out var estadoElement) &&
                    estadoElement.GetString()?.Equals(uf, StringComparison.OrdinalIgnoreCase) == true &&
                    item.TryGetProperty("municipios", out var municipiosElement))
                {
                    var municipiosLista = municipiosElement.EnumerateArray()
                        .Select(m => m.GetString())
                        .Where(m => !string.IsNullOrEmpty(m))
                        .ToList();
                        
                    return municipiosLista.Any(m => m.Equals(municipio, StringComparison.OrdinalIgnoreCase));
                }
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }
}