using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Enderecos.Dominio.Entidades;

namespace Agriis.Referencias.Dominio.Entidades;

/// <summary>
/// Entidade que representa uma moeda do sistema
/// </summary>
public class Moeda : EntidadeBase
{
    /// <summary>
    /// Código da moeda (3 caracteres - USD, BRL, EUR, etc.)
    /// </summary>
    public string Codigo { get; private set; } = string.Empty;
    
    /// <summary>
    /// Nome da moeda
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// Símbolo da moeda ($, R$, €, etc.)
    /// </summary>
    public string Simbolo { get; private set; } = string.Empty;
    
    /// <summary>
    /// ID do país ao qual a moeda pertence
    /// </summary>
    public int PaisId { get; private set; }
    
    /// <summary>
    /// País ao qual a moeda pertence
    /// </summary>
    public virtual Pais Pais { get; private set; } = null!;
    
    /// <summary>
    /// Indica se a moeda está ativa no sistema
    /// </summary>
    public bool Ativo { get; private set; }
    
    /// <summary>
    /// Construtor protegido para uso do Entity Framework
    /// </summary>
    protected Moeda() { }
    
    /// <summary>
    /// Construtor para criação de uma nova moeda
    /// </summary>
    /// <param name="codigo">Código da moeda (3 caracteres)</param>
    /// <param name="nome">Nome da moeda</param>
    /// <param name="simbolo">Símbolo da moeda</param>
    /// <param name="paisId">ID do país</param>
    public Moeda(string codigo, string nome, string simbolo, int paisId)
    {
        ValidarCodigo(codigo);
        ValidarNome(nome);
        ValidarSimbolo(simbolo);
        ValidarPaisId(paisId);
        
        Codigo = codigo.ToUpper();
        Nome = nome;
        Simbolo = simbolo;
        PaisId = paisId;
        Ativo = true;
    }
    
    /// <summary>
    /// Ativa a moeda
    /// </summary>
    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Desativa a moeda
    /// </summary>
    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza as informações da moeda
    /// </summary>
    /// <param name="nome">Novo nome</param>
    /// <param name="simbolo">Novo símbolo</param>
    /// <param name="paisId">Novo ID do país</param>
    public void AtualizarInformacoes(string nome, string simbolo, int paisId)
    {
        ValidarNome(nome);
        ValidarSimbolo(simbolo);
        ValidarPaisId(paisId);
        
        Nome = nome;
        Simbolo = simbolo;
        PaisId = paisId;
        AtualizarDataModificacao();
    }
    
    private static void ValidarCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("Código da moeda é obrigatório", nameof(codigo));
            
        if (codigo.Length != 3)
            throw new ArgumentException("Código da moeda deve ter exatamente 3 caracteres", nameof(codigo));
    }
    
    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da moeda é obrigatório", nameof(nome));
            
        if (nome.Length > 100)
            throw new ArgumentException("Nome da moeda não pode ter mais de 100 caracteres", nameof(nome));
    }
    
    private static void ValidarSimbolo(string simbolo)
    {
        if (string.IsNullOrWhiteSpace(simbolo))
            throw new ArgumentException("Símbolo da moeda é obrigatório", nameof(simbolo));
            
        if (simbolo.Length > 5)
            throw new ArgumentException("Símbolo da moeda não pode ter mais de 5 caracteres", nameof(simbolo));
    }
    
    private static void ValidarPaisId(int paisId)
    {
        if (paisId <= 0)
            throw new ArgumentException("ID do país deve ser maior que zero", nameof(paisId));
    }
}