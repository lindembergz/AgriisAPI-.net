using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Referencias.Dominio.Enums;

namespace Agriis.Referencias.Dominio.Entidades;

/// <summary>
/// Entidade que representa uma unidade de medida
/// </summary>
public class UnidadeMedida : EntidadeBase
{
    /// <summary>
    /// Símbolo da unidade de medida (kg, L, m², un, etc.)
    /// </summary>
    public string Simbolo { get; private set; } = string.Empty;
    
    /// <summary>
    /// Nome da unidade de medida
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// Tipo da unidade de medida
    /// </summary>
    public TipoUnidadeMedida Tipo { get; private set; }
    
    /// <summary>
    /// Fator de conversão para unidade base do tipo (opcional)
    /// </summary>
    public decimal? FatorConversao { get; private set; }
    
    /// <summary>
    /// Indica se a unidade de medida está ativa no sistema
    /// </summary>
    public bool Ativo { get; private set; }
    
    /// <summary>
    /// Coleção de embalagens que utilizam esta unidade de medida
    /// </summary>
    public virtual ICollection<Embalagem> Embalagens { get; private set; } = new List<Embalagem>();
    
    /// <summary>
    /// Construtor protegido para uso do Entity Framework
    /// </summary>
    protected UnidadeMedida() { }
    
    /// <summary>
    /// Construtor para criação de uma nova unidade de medida
    /// </summary>
    /// <param name="simbolo">Símbolo da unidade</param>
    /// <param name="nome">Nome da unidade</param>
    /// <param name="tipo">Tipo da unidade</param>
    /// <param name="fatorConversao">Fator de conversão (opcional)</param>
    public UnidadeMedida(string simbolo, string nome, TipoUnidadeMedida tipo, decimal? fatorConversao = null)
    {
        ValidarSimbolo(simbolo);
        ValidarNome(nome);
        ValidarTipo(tipo);
        ValidarFatorConversao(fatorConversao);
        
        Simbolo = simbolo;
        Nome = nome;
        Tipo = tipo;
        FatorConversao = fatorConversao;
        Ativo = true;
    }
    
    /// <summary>
    /// Ativa a unidade de medida
    /// </summary>
    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Desativa a unidade de medida
    /// </summary>
    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza as informações da unidade de medida
    /// </summary>
    /// <param name="nome">Novo nome</param>
    /// <param name="fatorConversao">Novo fator de conversão</param>
    public void AtualizarInformacoes(string nome, decimal? fatorConversao = null)
    {
        ValidarNome(nome);
        ValidarFatorConversao(fatorConversao);
        
        Nome = nome;
        FatorConversao = fatorConversao;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Verifica se a unidade de medida possui embalagens cadastradas
    /// </summary>
    /// <returns>True se possui embalagens</returns>
    public bool PossuiEmbalagens()
    {
        return Embalagens.Any();
    }
    
    private static void ValidarSimbolo(string simbolo)
    {
        if (string.IsNullOrWhiteSpace(simbolo))
            throw new ArgumentException("Símbolo da unidade de medida é obrigatório", nameof(simbolo));
            
        if (simbolo.Length > 10)
            throw new ArgumentException("Símbolo da unidade de medida não pode ter mais de 10 caracteres", nameof(simbolo));
    }
    
    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da unidade de medida é obrigatório", nameof(nome));
            
        if (nome.Length > 100)
            throw new ArgumentException("Nome da unidade de medida não pode ter mais de 100 caracteres", nameof(nome));
    }
    
    private static void ValidarTipo(TipoUnidadeMedida tipo)
    {
        if (!Enum.IsDefined(typeof(TipoUnidadeMedida), tipo))
            throw new ArgumentException("Tipo de unidade de medida inválido", nameof(tipo));
    }
    
    private static void ValidarFatorConversao(decimal? fatorConversao)
    {
        if (fatorConversao.HasValue && fatorConversao.Value <= 0)
            throw new ArgumentException("Fator de conversão deve ser maior que zero", nameof(fatorConversao));
    }
}