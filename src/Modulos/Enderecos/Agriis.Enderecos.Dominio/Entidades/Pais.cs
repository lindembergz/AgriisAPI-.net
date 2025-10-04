using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Enderecos.Dominio.Entidades;

/// <summary>
/// Entidade que representa um país
/// </summary>
public class Pais : EntidadeBase
{
    /// <summary>
    /// Código do país (ISO 2-3 caracteres)
    /// </summary>
    public string Codigo { get; private set; } = string.Empty;
    
    /// <summary>
    /// Nome do país
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// Indica se o país está ativo no sistema
    /// </summary>
    public bool Ativo { get; private set; }
    
    /// <summary>
    /// Coleção de Estados do país
    /// </summary>
    public virtual ICollection<Estado> Estados { get; private set; } = new List<Estado>();
    
    /// <summary>
    /// Construtor protegido para uso do Entity Framework
    /// </summary>
    protected Pais() { }
    
    /// <summary>
    /// Construtor para criação de um novo país
    /// </summary>
    /// <param name="nome">Nome do país</param>
    /// <param name="codigo">Código do país (ISO)</param>
    public Pais(string nome, string codigo)
    {
        ValidarNome(nome);
        ValidarCodigo(codigo);
        
        Nome = nome;
        Codigo = codigo.ToUpper();
        Ativo = true;
    }
    
    /// <summary>
    /// Ativa o país
    /// </summary>
    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Desativa o país
    /// </summary>
    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza as informações do país
    /// </summary>
    /// <param name="nome">Novo nome</param>
    /// <param name="codigo">Novo código</param>
    public void AtualizarInformacoes(string nome, string codigo)
    {
        ValidarNome(nome);
        ValidarCodigo(codigo);
        
        Nome = nome;
        Codigo = codigo.ToUpper();
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Verifica se o país possui Estados cadastrados
    /// </summary>
    /// <returns>True se possui Estados</returns>
    public bool PossuiEstados()
    {
        return Estados.Any();
    }
    
    private static void ValidarCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("Código do país é obrigatório", nameof(codigo));
            
        if (codigo.Length < 2 || codigo.Length > 3)
            throw new ArgumentException("Código do país deve ter entre 2 e 3 caracteres", nameof(codigo));
    }
    
    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do país é obrigatório", nameof(nome));
            
        if (nome.Length > 100)
            throw new ArgumentException("Nome do país não pode ter mais de 100 caracteres", nameof(nome));
    }
}