using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Referencias.Dominio.Entidades;

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
    /// Coleção de UFs do país
    /// </summary>
    public virtual ICollection<Uf> Ufs { get; private set; } = new List<Uf>();
    
    /// <summary>
    /// Construtor protegido para uso do Entity Framework
    /// </summary>
    protected Pais() { }
    
    /// <summary>
    /// Construtor para criação de um novo país
    /// </summary>
    /// <param name="codigo">Código do país (ISO)</param>
    /// <param name="nome">Nome do país</param>
    public Pais(string codigo, string nome)
    {
        ValidarCodigo(codigo);
        ValidarNome(nome);
        
        Codigo = codigo.ToUpper();
        Nome = nome;
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
    public void AtualizarInformacoes(string nome)
    {
        ValidarNome(nome);
        
        Nome = nome;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Verifica se o país possui UFs cadastradas
    /// </summary>
    /// <returns>True se possui UFs</returns>
    public bool PossuiUfs()
    {
        return Ufs.Any();
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