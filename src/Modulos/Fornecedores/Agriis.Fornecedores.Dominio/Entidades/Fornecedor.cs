using System.Text.Json;
using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Compartilhado.Dominio.Enums;

namespace Agriis.Fornecedores.Dominio.Entidades;

/// <summary>
/// Entidade que representa um fornecedor de insumos agrícolas
/// </summary>
public class Fornecedor : EntidadeRaizAgregada
{
    /// <summary>
    /// Nome/Razão social do fornecedor
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// CNPJ do fornecedor
    /// </summary>
    public Cnpj Cnpj { get; private set; } = null!;
    
    /// <summary>
    /// Inscrição estadual do fornecedor
    /// </summary>
    public string? InscricaoEstadual { get; private set; }
    
    /// <summary>
    /// Endereço completo do fornecedor
    /// </summary>
    public string? Endereco { get; private set; }
    
    /// <summary>
    /// Telefone de contato do fornecedor
    /// </summary>
    public string? Telefone { get; private set; }
    
    /// <summary>
    /// Email de contato do fornecedor
    /// </summary>
    public string? Email { get; private set; }
    
    /// <summary>
    /// URL da logo do fornecedor (AWS S3)
    /// </summary>
    public string? LogoUrl { get; private set; }
    
    /// <summary>
    /// Moeda padrão do fornecedor
    /// </summary>
    public Moeda MoedaPadrao { get; private set; } = Moeda.Real;    
 
   /// <summary>
    /// Valor mínimo de pedido para este fornecedor
    /// </summary>
    public decimal? PedidoMinimo { get; private set; }
    
    /// <summary>
    /// Token para integração com sistema Lincros
    /// </summary>
    public string? TokenLincros { get; private set; }
    
    /// <summary>
    /// Indica se o fornecedor está ativo
    /// </summary>
    public bool Ativo { get; private set; } = true;
    
    /// <summary>
    /// Dados adicionais em formato JSON
    /// </summary>
    public JsonDocument? DadosAdicionais { get; private set; }
    
    // Navigation Properties
    /// <summary>
    /// Usuários associados ao fornecedor
    /// </summary>
    public virtual ICollection<UsuarioFornecedor> UsuariosFornecedores { get; private set; } = new List<UsuarioFornecedor>();
    
    /// <summary>
    /// Construtor protegido para Entity Framework
    /// </summary>
    protected Fornecedor() { }
    
    /// <summary>
    /// Construtor para criar um novo fornecedor
    /// </summary>
    /// <param name="nome">Nome/Razão social</param>
    /// <param name="cnpj">CNPJ do fornecedor</param>
    /// <param name="inscricaoEstadual">Inscrição estadual</param>
    /// <param name="endereco">Endereço</param>
    /// <param name="telefone">Telefone</param>
    /// <param name="email">Email</param>
    /// <param name="moedaPadrao">Moeda padrão</param>
    public Fornecedor(
        string nome,
        Cnpj cnpj,
        string? inscricaoEstadual = null,
        string? endereco = null,
        string? telefone = null,
        string? email = null,
        Moeda moedaPadrao = Moeda.Real)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do fornecedor é obrigatório", nameof(nome));
            
        Nome = nome.Trim();
        Cnpj = cnpj ?? throw new ArgumentNullException(nameof(cnpj));
        InscricaoEstadual = inscricaoEstadual?.Trim();
        Endereco = endereco?.Trim();
        Telefone = telefone?.Trim();
        Email = email?.Trim();
        MoedaPadrao = moedaPadrao;
        Ativo = true;
        UsuariosFornecedores = new List<UsuarioFornecedor>();
    }    

    /// <summary>
    /// Atualiza os dados básicos do fornecedor
    /// </summary>
    /// <param name="nome">Nome/Razão social</param>
    /// <param name="inscricaoEstadual">Inscrição estadual</param>
    /// <param name="endereco">Endereço</param>
    /// <param name="telefone">Telefone</param>
    /// <param name="email">Email</param>
    public void AtualizarDados(
        string nome,
        string? inscricaoEstadual = null,
        string? endereco = null,
        string? telefone = null,
        string? email = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do fornecedor é obrigatório", nameof(nome));
            
        Nome = nome.Trim();
        InscricaoEstadual = inscricaoEstadual?.Trim();
        Endereco = endereco?.Trim();
        Telefone = telefone?.Trim();
        Email = email?.Trim();
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Define a URL da logo do fornecedor
    /// </summary>
    /// <param name="logoUrl">URL da logo</param>
    public void DefinirLogo(string? logoUrl)
    {
        LogoUrl = logoUrl?.Trim();
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Define o pedido mínimo do fornecedor
    /// </summary>
    /// <param name="pedidoMinimo">Valor mínimo do pedido</param>
    public void DefinirPedidoMinimo(decimal? pedidoMinimo)
    {
        if (pedidoMinimo.HasValue && pedidoMinimo.Value < 0)
            throw new ArgumentException("Pedido mínimo não pode ser negativo", nameof(pedidoMinimo));
            
        PedidoMinimo = pedidoMinimo;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Define o token Lincros para integração
    /// </summary>
    /// <param name="tokenLincros">Token de integração</param>
    public void DefinirTokenLincros(string? tokenLincros)
    {
        TokenLincros = tokenLincros?.Trim();
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Ativa o fornecedor
    /// </summary>
    public void Ativar()
    {
        if (!Ativo)
        {
            Ativo = true;
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Desativa o fornecedor
    /// </summary>
    public void Desativar()
    {
        if (Ativo)
        {
            Ativo = false;
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Armazena dados adicionais em formato JSON
    /// </summary>
    /// <param name="dadosAdicionais">Dados em formato JSON</param>
    public void ArmazenarDadosAdicionais(JsonDocument? dadosAdicionais)
    {
        DadosAdicionais = dadosAdicionais;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Altera a moeda padrão do fornecedor
    /// </summary>
    /// <param name="moedaPadrao">Nova moeda padrão</param>
    public void AlterarMoedaPadrao(Moeda moedaPadrao)
    {
        MoedaPadrao = moedaPadrao;
        AtualizarDataModificacao();
    }
}