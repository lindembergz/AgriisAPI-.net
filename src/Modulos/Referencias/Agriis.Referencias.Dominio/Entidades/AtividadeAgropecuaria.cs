using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Referencias.Dominio.Enums;

namespace Agriis.Referencias.Dominio.Entidades;

/// <summary>
/// Entidade que representa uma atividade agropecuária
/// </summary>
public class AtividadeAgropecuaria : EntidadeBase
{
    /// <summary>
    /// Código da atividade agropecuária
    /// </summary>
    public string Codigo { get; private set; } = string.Empty;
    
    /// <summary>
    /// Descrição da atividade agropecuária
    /// </summary>
    public string Descricao { get; private set; } = string.Empty;
    
    /// <summary>
    /// Tipo da atividade agropecuária
    /// </summary>
    public TipoAtividadeAgropecuaria Tipo { get; private set; }
    
    /// <summary>
    /// Indica se a atividade está ativa no sistema
    /// </summary>
    public bool Ativo { get; private set; }
    
    /// <summary>
    /// Construtor protegido para uso do Entity Framework
    /// </summary>
    protected AtividadeAgropecuaria() { }
    
    /// <summary>
    /// Construtor para criação de uma nova atividade agropecuária
    /// </summary>
    /// <param name="codigo">Código da atividade</param>
    /// <param name="descricao">Descrição da atividade</param>
    /// <param name="tipo">Tipo da atividade</param>
    public AtividadeAgropecuaria(string codigo, string descricao, TipoAtividadeAgropecuaria tipo)
    {
        ValidarCodigo(codigo);
        ValidarDescricao(descricao);
        ValidarTipo(tipo);
        
        Codigo = codigo.ToUpper();
        Descricao = descricao;
        Tipo = tipo;
        Ativo = true;
    }
    
    /// <summary>
    /// Ativa a atividade agropecuária
    /// </summary>
    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Desativa a atividade agropecuária
    /// </summary>
    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza as informações da atividade agropecuária
    /// </summary>
    /// <param name="descricao">Nova descrição</param>
    /// <param name="tipo">Novo tipo</param>
    public void AtualizarInformacoes(string descricao, TipoAtividadeAgropecuaria tipo)
    {
        ValidarDescricao(descricao);
        ValidarTipo(tipo);
        
        Descricao = descricao;
        Tipo = tipo;
        AtualizarDataModificacao();
    }
    
    private static void ValidarCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("Código da atividade agropecuária é obrigatório", nameof(codigo));
            
        if (codigo.Length > 20)
            throw new ArgumentException("Código da atividade agropecuária não pode ter mais de 20 caracteres", nameof(codigo));
    }
    
    private static void ValidarDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição da atividade agropecuária é obrigatória", nameof(descricao));
            
        if (descricao.Length > 200)
            throw new ArgumentException("Descrição da atividade agropecuária não pode ter mais de 200 caracteres", nameof(descricao));
    }
    
    private static void ValidarTipo(TipoAtividadeAgropecuaria tipo)
    {
        if (!Enum.IsDefined(typeof(TipoAtividadeAgropecuaria), tipo))
            throw new ArgumentException("Tipo de atividade agropecuária inválido", nameof(tipo));
    }
}