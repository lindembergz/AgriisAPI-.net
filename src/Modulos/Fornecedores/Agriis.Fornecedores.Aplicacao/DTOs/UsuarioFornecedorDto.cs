namespace Agriis.Fornecedores.Aplicacao.DTOs;

/// <summary>
/// DTO para associação usuário-fornecedor
/// </summary>
public class UsuarioFornecedorDto
{
    /// <summary>
    /// ID da associação
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// ID do usuário
    /// </summary>
    public int UsuarioId { get; set; }
    
    /// <summary>
    /// Nome do usuário
    /// </summary>
    public string UsuarioNome { get; set; } = string.Empty;
    
    /// <summary>
    /// Email do usuário
    /// </summary>
    public string UsuarioEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// ID do fornecedor
    /// </summary>
    public int FornecedorId { get; set; }
    
    /// <summary>
    /// Nome do fornecedor
    /// </summary>
    public string FornecedorNome { get; set; } = string.Empty;
    
    /// <summary>
    /// Role/perfil do usuário no fornecedor
    /// </summary>
    public int Role { get; set; }
    
    /// <summary>
    /// Nome do role
    /// </summary>
    public string RoleNome { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica se o usuário está ativo no fornecedor
    /// </summary>
    public bool Ativo { get; set; }
    
    /// <summary>
    /// Data de início da associação
    /// </summary>
    public DateTime DataInicio { get; set; }
    
    /// <summary>
    /// Data de fim da associação
    /// </summary>
    public DateTime? DataFim { get; set; }
    
    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime DataCriacao { get; set; }
    
    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? DataAtualizacao { get; set; }
    
    /// <summary>
    /// Lista de territórios de atuação
    /// </summary>
    public List<UsuarioFornecedorTerritorioDto> Territorios { get; set; } = new();
}