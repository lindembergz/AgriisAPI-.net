using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Enderecos.Dominio.Entidades;

/// <summary>
/// Entidade que representa um estado brasileiro
/// </summary>
public class Estado : EntidadeBase
{
    /// <summary>
    /// Nome completo do estado
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// Sigla do estado (UF)
    /// </summary>
    public string Uf { get; private set; } = string.Empty;
    
    /// <summary>
    /// Código IBGE do estado
    /// </summary>
    public int CodigoIbge { get; private set; }
    
    /// <summary>
    /// Região do estado (Norte, Nordeste, Centro-Oeste, Sudeste, Sul)
    /// </summary>
    public string Regiao { get; private set; } = string.Empty;
    
    /// <summary>
    /// Municípios do estado
    /// </summary>
    public virtual ICollection<Municipio> Municipios { get; private set; } = new List<Municipio>();
    
    /// <summary>
    /// Endereços do estado
    /// </summary>
    public virtual ICollection<Endereco> Enderecos { get; private set; } = new List<Endereco>();
    
    /// <summary>
    /// Construtor protegido para o Entity Framework
    /// </summary>
    protected Estado() { }
    
    /// <summary>
    /// Construtor para criar um novo estado
    /// </summary>
    /// <param name="nome">Nome completo do estado</param>
    /// <param name="uf">Sigla do estado</param>
    /// <param name="codigoIbge">Código IBGE do estado</param>
    /// <param name="regiao">Região do estado</param>
    public Estado(string nome, string uf, int codigoIbge, string regiao)
    {
        ValidarParametros(nome, uf, codigoIbge, regiao);
        
        Nome = nome;
        Uf = uf.ToUpperInvariant();
        CodigoIbge = codigoIbge;
        Regiao = regiao;
    }
    
    /// <summary>
    /// Atualiza os dados do estado
    /// </summary>
    /// <param name="nome">Nome completo do estado</param>
    /// <param name="uf">Sigla do estado</param>
    /// <param name="codigoIbge">Código IBGE do estado</param>
    /// <param name="regiao">Região do estado</param>
    public void Atualizar(string nome, string uf, int codigoIbge, string regiao)
    {
        ValidarParametros(nome, uf, codigoIbge, regiao);
        
        Nome = nome;
        Uf = uf.ToUpperInvariant();
        CodigoIbge = codigoIbge;
        Regiao = regiao;
        
        AtualizarDataModificacao();
    }
    
    private static void ValidarParametros(string nome, string uf, int codigoIbge, string regiao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do estado é obrigatório", nameof(nome));
            
        if (string.IsNullOrWhiteSpace(uf) || uf.Length != 2)
            throw new ArgumentException("UF deve ter exatamente 2 caracteres", nameof(uf));
            
        if (codigoIbge <= 0)
            throw new ArgumentException("Código IBGE deve ser maior que zero", nameof(codigoIbge));
            
        if (string.IsNullOrWhiteSpace(regiao))
            throw new ArgumentException("Região é obrigatória", nameof(regiao));
    }
}