using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Referencias.Dominio.Entidades;

/// <summary>
/// Entidade que representa uma Unidade Federativa (UF)
/// </summary>
public class Uf : EntidadeBase
{
    /// <summary>
    /// Código da UF (2 caracteres - SP, RJ, MG, etc.)
    /// </summary>
    public string Codigo { get; private set; } = string.Empty;
    
    /// <summary>
    /// Nome da UF
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// ID do país ao qual a UF pertence
    /// </summary>
    public int PaisId { get; private set; }
    
    /// <summary>
    /// Indica se a UF está ativa no sistema
    /// </summary>
    public bool Ativo { get; private set; }
    
    /// <summary>
    /// País ao qual a UF pertence
    /// </summary>
    public virtual Pais Pais { get; private set; } = null!;
    
    /// <summary>
    /// Coleção de municípios da UF
    /// </summary>
    public virtual ICollection<Municipio> Municipios { get; private set; } = new List<Municipio>();
    
    /// <summary>
    /// Construtor protegido para uso do Entity Framework
    /// </summary>
    protected Uf() { }
    
    /// <summary>
    /// Construtor para criação de uma nova UF
    /// </summary>
    /// <param name="codigo">Código da UF (2 caracteres)</param>
    /// <param name="nome">Nome da UF</param>
    /// <param name="paisId">ID do país</param>
    public Uf(string codigo, string nome, int paisId)
    {
        ValidarCodigo(codigo);
        ValidarNome(nome);
        ValidarPaisId(paisId);
        
        Codigo = codigo.ToUpper();
        Nome = nome;
        PaisId = paisId;
        Ativo = true;
    }
    
    /// <summary>
    /// Ativa a UF
    /// </summary>
    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Desativa a UF
    /// </summary>
    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza as informações da UF
    /// </summary>
    /// <param name="nome">Novo nome</param>
    public void AtualizarInformacoes(string nome)
    {
        ValidarNome(nome);
        
        Nome = nome;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Verifica se a UF possui municípios cadastrados
    /// </summary>
    /// <returns>True se possui municípios</returns>
    public bool PossuiMunicipios()
    {
        return Municipios.Any();
    }
    
    private static void ValidarCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("Código da UF é obrigatório", nameof(codigo));
            
        if (codigo.Length != 2)
            throw new ArgumentException("Código da UF deve ter exatamente 2 caracteres", nameof(codigo));
    }
    
    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da UF é obrigatório", nameof(nome));
            
        if (nome.Length > 100)
            throw new ArgumentException("Nome da UF não pode ter mais de 100 caracteres", nameof(nome));
    }
    
    private static void ValidarPaisId(int paisId)
    {
        if (paisId <= 0)
            throw new ArgumentException("ID do país deve ser maior que zero", nameof(paisId));
    }
}