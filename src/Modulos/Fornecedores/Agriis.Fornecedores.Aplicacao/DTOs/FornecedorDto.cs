using System.Text.Json;

namespace Agriis.Fornecedores.Aplicacao.DTOs;

/// <summary>
/// DTO para dados do fornecedor
/// </summary>
public class FornecedorDto
{
    /// <summary>
    /// ID do fornecedor
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome/Razão social do fornecedor
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// CNPJ do fornecedor
    /// </summary>
    public string Cnpj { get; set; } = string.Empty;
    
    /// <summary>
    /// CNPJ formatado
    /// </summary>
    public string CnpjFormatado { get; set; } = string.Empty;
    
    /// <summary>
    /// Inscrição estadual do fornecedor
    /// </summary>
    public string? InscricaoEstadual { get; set; }
    
    /// <summary>
    /// Endereço completo do fornecedor
    /// </summary>
    public string? Endereco { get; set; }
    
    /// <summary>
    /// Telefone de contato do fornecedor
    /// </summary>
    public string? Telefone { get; set; }
    
    /// <summary>
    /// Email de contato do fornecedor
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// URL da logo do fornecedor
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Moeda padrão do fornecedor (0 = Real, 1 = Dólar)
    /// </summary>
    public int MoedaPadrao { get; set; }
    
    /// <summary>
    /// Nome da moeda padrão
    /// </summary>
    public string MoedaPadraoNome { get; set; } = string.Empty;
    
    /// <summary>
    /// Valor mínimo de pedido
    /// </summary>
    public decimal? PedidoMinimo { get; set; }
    
    /// <summary>
    /// Token para integração Lincros
    /// </summary>
    public string? TokenLincros { get; set; }
    
    /// <summary>
    /// Indica se o fornecedor está ativo
    /// </summary>
    public bool Ativo { get; set; }
    
    /// <summary>
    /// Dados adicionais em formato JSON
    /// </summary>
    public JsonDocument? DadosAdicionais { get; set; }
    
    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime DataCriacao { get; set; }
    
    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? DataAtualizacao { get; set; }
    
    /// <summary>
    /// Lista de usuários associados ao fornecedor
    /// </summary>
    public List<UsuarioFornecedorDto> Usuarios { get; set; } = new();
}


public class FiltrosFornecedorDto
{
    public string? Filtro { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
}