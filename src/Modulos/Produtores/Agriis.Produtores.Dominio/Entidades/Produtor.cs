using System.Text.Json;
using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Produtores.Dominio.Enums;

namespace Agriis.Produtores.Dominio.Entidades;

/// <summary>
/// Entidade que representa um produtor rural no sistema
/// </summary>
public class Produtor : EntidadeRaizAgregada
{
    /// <summary>
    /// Nome completo do produtor
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// CPF do produtor (pessoa física)
    /// </summary>
    public Cpf? Cpf { get; private set; }
    
    /// <summary>
    /// CNPJ do produtor (pessoa jurídica)
    /// </summary>
    public Cnpj? Cnpj { get; private set; }
    
    /// <summary>
    /// Inscrição estadual do produtor
    /// </summary>
    public string? InscricaoEstadual { get; private set; }
    
    /// <summary>
    /// Tipo de atividade desenvolvida pelo produtor
    /// </summary>
    public string? TipoAtividade { get; private set; }
    
    /// <summary>
    /// Área total de plantio do produtor em hectares
    /// </summary>
    public AreaPlantio AreaPlantio { get; private set; } = new(0);
    
    /// <summary>
    /// Data de autorização do produtor
    /// </summary>
    public DateTime DataAutorizacao { get; private set; }
    
    /// <summary>
    /// Status atual do produtor
    /// </summary>
    public StatusProdutor Status { get; private set; }
    
    /// <summary>
    /// Retornos das APIs de validação (SERPRO) em formato JSON
    /// </summary>
    public JsonDocument? RetornosApiCheckProdutor { get; private set; }
    
    /// <summary>
    /// ID do usuário que autorizou o produtor (quando autorização manual)
    /// </summary>
    public int? UsuarioAutorizacaoId { get; private set; }
    
    /// <summary>
    /// Lista de IDs das culturas associadas ao produtor
    /// </summary>
    public List<int> Culturas { get; private set; } = new();
    
    // Navigation Properties
    /// <summary>
    /// Usuário que autorizou o produtor
    /// </summary>
    public virtual Agriis.Usuarios.Dominio.Entidades.Usuario? UsuarioAutorizacao { get; private set; }
    
    /// <summary>
    /// Relacionamentos com usuários do produtor
    /// </summary>
    public virtual ICollection<UsuarioProdutor> UsuariosProdutores { get; private set; } = new List<UsuarioProdutor>();
    
    /// <summary>
    /// Construtor protegido para Entity Framework
    /// </summary>
    protected Produtor() { }
    
    /// <summary>
    /// Construtor para criar um novo produtor
    /// </summary>
    /// <param name="nome">Nome do produtor</param>
    /// <param name="cpf">CPF do produtor (opcional)</param>
    /// <param name="cnpj">CNPJ do produtor (opcional)</param>
    /// <param name="inscricaoEstadual">Inscrição estadual</param>
    /// <param name="tipoAtividade">Tipo de atividade</param>
    /// <param name="areaPlantio">Área de plantio</param>
    public Produtor(
        string nome, 
        Cpf? cpf = null, 
        Cnpj? cnpj = null, 
        string? inscricaoEstadual = null,
        string? tipoAtividade = null, 
        AreaPlantio? areaPlantio = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do produtor é obrigatório", nameof(nome));
            
        if (cpf == null && cnpj == null)
            throw new ArgumentException("CPF ou CNPJ deve ser informado");
            
        Nome = nome.Trim();
        Cpf = cpf;
        Cnpj = cnpj;
        InscricaoEstadual = inscricaoEstadual?.Trim();
        TipoAtividade = tipoAtividade?.Trim();
        AreaPlantio = areaPlantio ?? new AreaPlantio(0);
        Status = StatusProdutor.PendenteValidacaoAutomatica;
        DataAutorizacao = DateTime.UtcNow;
        Culturas = new List<int>();
        UsuariosProdutores = new List<UsuarioProdutor>();
    }
    
    /// <summary>
    /// Atualiza o status do produtor
    /// </summary>
    /// <param name="novoStatus">Novo status</param>
    /// <param name="usuarioAutorizacaoId">ID do usuário que autorizou (opcional)</param>
    public void AtualizarStatus(StatusProdutor novoStatus, int? usuarioAutorizacaoId = null)
    {
        Status = novoStatus;
        
        if (usuarioAutorizacaoId.HasValue)
            UsuarioAutorizacaoId = usuarioAutorizacaoId.Value;
            
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Adiciona uma cultura à lista de culturas do produtor
    /// </summary>
    /// <param name="culturaId">ID da cultura</param>
    public void AdicionarCultura(int culturaId)
    {
        if (culturaId <= 0)
            throw new ArgumentException("ID da cultura deve ser maior que zero", nameof(culturaId));
            
        if (!Culturas.Contains(culturaId))
        {
            Culturas.Add(culturaId);
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Remove uma cultura da lista de culturas do produtor
    /// </summary>
    /// <param name="culturaId">ID da cultura</param>
    public void RemoverCultura(int culturaId)
    {
        if (Culturas.Remove(culturaId))
        {
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Atualiza a área de plantio do produtor
    /// </summary>
    /// <param name="novaArea">Nova área de plantio</param>
    public void AtualizarAreaPlantio(AreaPlantio novaArea)
    {
        AreaPlantio = novaArea ?? throw new ArgumentNullException(nameof(novaArea));
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Armazena os retornos das APIs de validação
    /// </summary>
    /// <param name="retornos">Dados JSON dos retornos das APIs</param>
    public void ArmazenarRetornosApiCheck(JsonDocument retornos)
    {
        RetornosApiCheckProdutor = retornos;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Verifica se o produtor está autorizado
    /// </summary>
    /// <returns>True se autorizado</returns>
    public bool EstaAutorizado()
    {
        return Status == StatusProdutor.AutorizadoAutomaticamente || 
               Status == StatusProdutor.AutorizadoManualmente;
    }
    
    /// <summary>
    /// Verifica se o produtor é pessoa física
    /// </summary>
    /// <returns>True se pessoa física</returns>
    public bool EhPessoaFisica()
    {
        return Cpf != null;
    }
    
    /// <summary>
    /// Verifica se o produtor é pessoa jurídica
    /// </summary>
    /// <returns>True se pessoa jurídica</returns>
    public bool EhPessoaJuridica()
    {
        return Cnpj != null;
    }
    
    /// <summary>
    /// Obtém o documento principal (CPF ou CNPJ)
    /// </summary>
    /// <returns>Documento formatado</returns>
    public string ObterDocumentoPrincipal()
    {
        return EhPessoaFisica() ? Cpf!.ValorFormatado : Cnpj!.ValorFormatado;
    }
}