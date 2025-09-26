using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Referencias.Dominio.Entidades;

/// <summary>
/// Entidade que representa um tipo de embalagem
/// </summary>
public class Embalagem : EntidadeBase
{
    /// <summary>
    /// Nome da embalagem (Saco, Caixa, Tambor, etc.)
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// Descrição da embalagem (opcional)
    /// </summary>
    public string? Descricao { get; private set; }
    
    /// <summary>
    /// ID da unidade de medida associada à embalagem
    /// </summary>
    public int UnidadeMedidaId { get; private set; }
    
    /// <summary>
    /// Indica se a embalagem está ativa no sistema
    /// </summary>
    public bool Ativo { get; private set; }
    
    /// <summary>
    /// Unidade de medida associada à embalagem
    /// </summary>
    public virtual UnidadeMedida UnidadeMedida { get; private set; } = null!;
    
    /// <summary>
    /// Construtor protegido para uso do Entity Framework
    /// </summary>
    protected Embalagem() { }
    
    /// <summary>
    /// Construtor para criação de uma nova embalagem
    /// </summary>
    /// <param name="nome">Nome da embalagem</param>
    /// <param name="unidadeMedidaId">ID da unidade de medida</param>
    /// <param name="descricao">Descrição da embalagem (opcional)</param>
    public Embalagem(string nome, int unidadeMedidaId, string? descricao = null)
    {
        ValidarNome(nome);
        ValidarUnidadeMedidaId(unidadeMedidaId);
        ValidarDescricao(descricao);
        
        Nome = nome;
        UnidadeMedidaId = unidadeMedidaId;
        Descricao = descricao;
        Ativo = true;
    }
    
    /// <summary>
    /// Ativa a embalagem
    /// </summary>
    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Desativa a embalagem
    /// </summary>
    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza as informações da embalagem
    /// </summary>
    /// <param name="nome">Novo nome</param>
    /// <param name="descricao">Nova descrição</param>
    public void AtualizarInformacoes(string nome, string? descricao = null)
    {
        ValidarNome(nome);
        ValidarDescricao(descricao);
        
        Nome = nome;
        Descricao = descricao;
        AtualizarDataModificacao();
    }
    
    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da embalagem é obrigatório", nameof(nome));
            
        if (nome.Length > 100)
            throw new ArgumentException("Nome da embalagem não pode ter mais de 100 caracteres", nameof(nome));
    }
    
    private static void ValidarUnidadeMedidaId(int unidadeMedidaId)
    {
        if (unidadeMedidaId <= 0)
            throw new ArgumentException("ID da unidade de medida deve ser maior que zero", nameof(unidadeMedidaId));
    }
    
    private static void ValidarDescricao(string? descricao)
    {
        if (!string.IsNullOrEmpty(descricao) && descricao.Length > 500)
            throw new ArgumentException("Descrição da embalagem não pode ter mais de 500 caracteres", nameof(descricao));
    }
}