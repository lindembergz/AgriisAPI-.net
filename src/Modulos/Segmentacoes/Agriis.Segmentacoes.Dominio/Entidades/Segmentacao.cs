using Agriis.Compartilhado.Dominio.Entidades;
using System.Text.Json;

namespace Agriis.Segmentacoes.Dominio.Entidades;

/// <summary>
/// Representa uma segmentação territorial para aplicação de descontos
/// </summary>
public class Segmentacao : EntidadeBase
{
    /// <summary>
    /// Nome da segmentação
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// Descrição da segmentação
    /// </summary>
    public string? Descricao { get; private set; }
    
    /// <summary>
    /// Indica se a segmentação está ativa
    /// </summary>
    public bool Ativo { get; private set; }
    
    /// <summary>
    /// ID do fornecedor proprietário da segmentação
    /// </summary>
    public int FornecedorId { get; private set; }
    
    /// <summary>
    /// Configuração territorial em formato JSON
    /// Contém estados e municípios cobertos pela segmentação
    /// </summary>
    public JsonDocument? ConfiguracaoTerritorial { get; private set; }
    
    /// <summary>
    /// Indica se esta é a segmentação padrão para o fornecedor
    /// </summary>
    public bool EhPadrao { get; private set; }
    
    /// <summary>
    /// Grupos de segmentação associados
    /// </summary>
    public virtual ICollection<Grupo> Grupos { get; private set; } = new List<Grupo>();
    
    /// <summary>
    /// Construtor protegido para o Entity Framework
    /// </summary>
    protected Segmentacao() { }
    
    /// <summary>
    /// Construtor para criar uma nova segmentação
    /// </summary>
    /// <param name="nome">Nome da segmentação</param>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="descricao">Descrição opcional</param>
    /// <param name="ehPadrao">Se é a segmentação padrão</param>
    public Segmentacao(string nome, int fornecedorId, string? descricao = null, bool ehPadrao = false)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da segmentação é obrigatório", nameof(nome));
            
        if (fornecedorId <= 0)
            throw new ArgumentException("ID do fornecedor deve ser válido", nameof(fornecedorId));
            
        Nome = nome.Trim();
        FornecedorId = fornecedorId;
        Descricao = descricao?.Trim();
        EhPadrao = ehPadrao;
        Ativo = true;
    }
    
    /// <summary>
    /// Atualiza as informações básicas da segmentação
    /// </summary>
    /// <param name="nome">Novo nome</param>
    /// <param name="descricao">Nova descrição</param>
    public void AtualizarInformacoes(string nome, string? descricao = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da segmentação é obrigatório", nameof(nome));
            
        Nome = nome.Trim();
        Descricao = descricao?.Trim();
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Define a configuração territorial da segmentação
    /// </summary>
    /// <param name="configuracao">Configuração em formato JSON</param>
    public void DefinirConfiguracaoTerritorial(JsonDocument configuracao)
    {
        ConfiguracaoTerritorial = configuracao;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Ativa a segmentação
    /// </summary>
    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Desativa a segmentação
    /// </summary>
    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Define como segmentação padrão
    /// </summary>
    public void DefinirComoPadrao()
    {
        EhPadrao = true;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Remove como segmentação padrão
    /// </summary>
    public void RemoverComoPadrao()
    {
        EhPadrao = false;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Adiciona um grupo à segmentação
    /// </summary>
    /// <param name="grupo">Grupo a ser adicionado</param>
    public void AdicionarGrupo(Grupo grupo)
    {
        if (grupo == null)
            throw new ArgumentNullException(nameof(grupo));
            
        if (!Grupos.Contains(grupo))
        {
            Grupos.Add(grupo);
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Remove um grupo da segmentação
    /// </summary>
    /// <param name="grupo">Grupo a ser removido</param>
    public void RemoverGrupo(Grupo grupo)
    {
        if (grupo != null && Grupos.Contains(grupo))
        {
            Grupos.Remove(grupo);
            AtualizarDataModificacao();
        }
    }
}