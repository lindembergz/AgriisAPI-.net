using System.Text.Json;
using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Enderecos.Dominio.Entidades;

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
    /// Logradouro do fornecedor
    /// </summary>
    public string? Logradouro { get; private set; }
    
    /// <summary>
    /// ID da UF do fornecedor
    /// </summary>
    public int? UfId { get; private set; }
    
    /// <summary>
    /// Estado do fornecedor
    /// </summary>
    public virtual Estado? Estado { get; private set; }
    
    /// <summary>
    /// ID do município do fornecedor
    /// </summary>
    public int? MunicipioId { get; private set; }
    
    /// <summary>
    /// Município do fornecedor
    /// </summary>
    public virtual Municipio? Municipio { get; private set; }
    
    /// <summary>
    /// CEP do fornecedor
    /// </summary>
    public string? Cep { get; private set; }
    
    /// <summary>
    /// Complemento do endereço
    /// </summary>
    public string? Complemento { get; private set; }
    
    /// <summary>
    /// Latitude da localização
    /// </summary>
    public decimal? Latitude { get; private set; }
    
    /// <summary>
    /// Longitude da localização
    /// </summary>
    public decimal? Longitude { get; private set; }
    
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
    public MoedaFinanceira Moeda { get; private set; } = MoedaFinanceira.Real;    
 
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
    /// <param name="logradouro">Logradouro</param>
    /// <param name="ufId">ID da UF</param>
    /// <param name="municipioId">ID do município</param>
    /// <param name="cep">CEP</param>
    /// <param name="complemento">Complemento</param>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    /// <param name="telefone">Telefone</param>
    /// <param name="email">Email</param>
    /// <param name="moedaPadrao">Moeda padrão</param>
    public Fornecedor(
        string nome,
        Cnpj cnpj,
        string? inscricaoEstadual = null,
        string? logradouro = null,
        int? ufId = null,
        int? municipioId = null,
        string? cep = null,
        string? complemento = null,
        decimal? latitude = null,
        decimal? longitude = null,
        string? telefone = null,
        string? email = null,
        MoedaFinanceira moedaPadrao = MoedaFinanceira.Real)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do fornecedor é obrigatório", nameof(nome));
            
        Nome = nome.Trim();
        Cnpj = cnpj ?? throw new ArgumentNullException(nameof(cnpj));
        InscricaoEstadual = inscricaoEstadual?.Trim();
        Logradouro = logradouro?.Trim();
        UfId = ufId;
        MunicipioId = municipioId;
        Cep = cep?.Trim();
        Complemento = complemento?.Trim();
        Latitude = latitude;
        Longitude = longitude;
        Telefone = telefone?.Trim();
        Email = email?.Trim();
        Moeda = moedaPadrao;
        Ativo = true;
        UsuariosFornecedores = new List<UsuarioFornecedor>();
    }    

    /// <summary>
    /// Atualiza os dados básicos do fornecedor
    /// </summary>
    /// <param name="nome">Nome/Razão social</param>
    /// <param name="inscricaoEstadual">Inscrição estadual</param>
    /// <param name="logradouro">Logradouro</param>
    /// <param name="ufId">ID da UF</param>
    /// <param name="municipioId">ID do município</param>
    /// <param name="cep">CEP</param>
    /// <param name="complemento">Complemento</param>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    /// <param name="telefone">Telefone</param>
    /// <param name="email">Email</param>
    public void AtualizarDados(
        string nome,
        string? inscricaoEstadual = null,
        string? logradouro = null,
        int? ufId = null,
        int? municipioId = null,
        string? cep = null,
        string? complemento = null,
        decimal? latitude = null,
        decimal? longitude = null,
        string? telefone = null,
        string? email = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do fornecedor é obrigatório", nameof(nome));
            
        Nome = nome.Trim();
        InscricaoEstadual = inscricaoEstadual?.Trim();
        Logradouro = logradouro?.Trim();
        UfId = ufId;
        MunicipioId = municipioId;
        Cep = cep?.Trim();
        Complemento = complemento?.Trim();
        Latitude = latitude;
        Longitude = longitude;
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
    public void AlterarMoedaPadrao(MoedaFinanceira moedaPadrao)
    {
        Moeda = moedaPadrao;
        AtualizarDataModificacao();
    }
}