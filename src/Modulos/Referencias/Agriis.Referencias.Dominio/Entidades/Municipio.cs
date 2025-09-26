using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Referencias.Dominio.Entidades;

/// <summary>
/// Entidade que representa um município (versão simplificada para referências)
/// </summary>
public class Municipio : EntidadeBase
{
    /// <summary>
    /// Nome do município
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// Código IBGE do município
    /// </summary>
    public string CodigoIbge { get; private set; } = string.Empty;
    
    /// <summary>
    /// ID da UF à qual o município pertence
    /// </summary>
    public int UfId { get; private set; }
    
    /// <summary>
    /// Indica se o município está ativo no sistema
    /// </summary>
    public bool Ativo { get; private set; }
    
    /// <summary>
    /// UF à qual o município pertence
    /// </summary>
    public virtual Uf Uf { get; private set; } = null!;
    
    /// <summary>
    /// Construtor protegido para uso do Entity Framework
    /// </summary>
    protected Municipio() { }
    
    /// <summary>
    /// Construtor para criação de um novo município
    /// </summary>
    /// <param name="nome">Nome do município</param>
    /// <param name="codigoIbge">Código IBGE do município</param>
    /// <param name="ufId">ID da UF</param>
    public Municipio(string nome, string codigoIbge, int ufId)
    {
        ValidarNome(nome);
        ValidarCodigoIbge(codigoIbge);
        ValidarUfId(ufId);
        
        Nome = nome;
        CodigoIbge = codigoIbge;
        UfId = ufId;
        Ativo = true;
    }
    
    /// <summary>
    /// Ativa o município
    /// </summary>
    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Desativa o município
    /// </summary>
    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza as informações do município
    /// </summary>
    /// <param name="nome">Novo nome</param>
    /// <param name="codigoIbge">Novo código IBGE</param>
    public void AtualizarInformacoes(string nome, string codigoIbge)
    {
        ValidarNome(nome);
        ValidarCodigoIbge(codigoIbge);
        
        Nome = nome;
        CodigoIbge = codigoIbge;
        AtualizarDataModificacao();
    }
    
    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do município é obrigatório", nameof(nome));
            
        if (nome.Length > 100)
            throw new ArgumentException("Nome do município não pode ter mais de 100 caracteres", nameof(nome));
    }
    
    private static void ValidarCodigoIbge(string codigoIbge)
    {
        if (string.IsNullOrWhiteSpace(codigoIbge))
            throw new ArgumentException("Código IBGE é obrigatório", nameof(codigoIbge));
            
        if (codigoIbge.Length > 10)
            throw new ArgumentException("Código IBGE não pode ter mais de 10 caracteres", nameof(codigoIbge));
    }
    
    private static void ValidarUfId(int ufId)
    {
        if (ufId <= 0)
            throw new ArgumentException("ID da UF deve ser maior que zero", nameof(ufId));
    }
}