namespace Agriis.Produtores.Aplicacao.DTOs;

/// <summary>
/// DTO para relacionamento usuário-produtor
/// </summary>
public class UsuarioProdutorDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int ProdutorId { get; set; }
    public bool EhProprietario { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    
    // Dados do usuário
    public string NomeUsuario { get; set; } = string.Empty;
    public string EmailUsuario { get; set; } = string.Empty;
    
    // Dados do produtor
    public string NomeProdutor { get; set; } = string.Empty;
    public string DocumentoProdutor { get; set; } = string.Empty;
}

/// <summary>
/// DTO para criação de relacionamento usuário-produtor
/// </summary>
public class CriarUsuarioProdutorDto
{
    public int UsuarioId { get; set; }
    public int ProdutorId { get; set; }
    public bool EhProprietario { get; set; } = false;
}

/// <summary>
/// DTO para atualização de relacionamento usuário-produtor
/// </summary>
public class AtualizarUsuarioProdutorDto
{
    public bool EhProprietario { get; set; }
    public bool Ativo { get; set; }
}