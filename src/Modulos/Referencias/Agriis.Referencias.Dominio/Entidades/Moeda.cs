using Agriis.Compartilhado.Dominio.Entidades;

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
    public Moeda(string codigo, string nome, string simbolo)
    {
        ValidarCodigo(codigo);
        ValidarNome(nome);
        ValidarSimbolo(simbolo);
        
        Codigo = codigo.ToUpper();
        Nome = nome;
        Simbolo = simbolo;
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
    public void AtualizarInformacoes(string nome, string simbolo)
    {
        ValidarNome(nome);
        ValidarSimbolo(simbolo);
        
        Nome = nome;
        Simbolo = simbolo;
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
}