using System.Text.Json;
using Agriis.Combos.Dominio.Enums;
using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Combos.Dominio.Entidades;

/// <summary>
/// Representa um combo promocional de produtos
/// </summary>
public class Combo : EntidadeRaizAgregada
{
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public decimal HectareMinimo { get; private set; }
    public decimal HectareMaximo { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime DataFim { get; private set; }
    public ModalidadePagamento ModalidadePagamento { get; private set; }
    public StatusCombo Status { get; private set; }
    public JsonDocument? RestricoesMunicipios { get; private set; }
    public bool PermiteAlteracaoItem { get; private set; }
    public bool PermiteExclusaoItem { get; private set; }
    public int FornecedorId { get; private set; }
    public int SafraId { get; private set; }

    // Navigation Properties
    public virtual ICollection<ComboItem> Itens { get; private set; } = new List<ComboItem>();
    public virtual ICollection<ComboLocalRecebimento> LocaisRecebimento { get; private set; } = new List<ComboLocalRecebimento>();
    public virtual ICollection<ComboCategoriaDesconto> CategoriasDesconto { get; private set; } = new List<ComboCategoriaDesconto>();

    protected Combo() { } // EF Constructor

    public Combo(
        string nome,
        decimal hectareMinimo,
        decimal hectareMaximo,
        DateTime dataInicio,
        DateTime dataFim,
        ModalidadePagamento modalidadePagamento,
        int fornecedorId,
        int safraId,
        string? descricao = null)
    {
        ValidarParametros(nome, hectareMinimo, hectareMaximo, dataInicio, dataFim);

        Nome = nome;
        Descricao = descricao;
        HectareMinimo = hectareMinimo;
        HectareMaximo = hectareMaximo;
        DataInicio = dataInicio;
        DataFim = dataFim;
        ModalidadePagamento = modalidadePagamento;
        FornecedorId = fornecedorId;
        SafraId = safraId;
        Status = StatusCombo.Ativo;
        PermiteAlteracaoItem = true;
        PermiteExclusaoItem = true;
    }

    public void AtualizarInformacoes(
        string nome,
        decimal hectareMinimo,
        decimal hectareMaximo,
        DateTime dataInicio,
        DateTime dataFim,
        string? descricao = null)
    {
        ValidarParametros(nome, hectareMinimo, hectareMaximo, dataInicio, dataFim);

        Nome = nome;
        Descricao = descricao;
        HectareMinimo = hectareMinimo;
        HectareMaximo = hectareMaximo;
        DataInicio = dataInicio;
        DataFim = dataFim;
        AtualizarDataModificacao();
    }

    public void AtualizarStatus(StatusCombo novoStatus)
    {
        Status = novoStatus;
        AtualizarDataModificacao();
    }

    public void DefinirRestricoesMunicipios(JsonDocument restricoes)
    {
        RestricoesMunicipios = restricoes;
        AtualizarDataModificacao();
    }

    public void ConfigurarPermissoes(bool permiteAlteracao, bool permiteExclusao)
    {
        PermiteAlteracaoItem = permiteAlteracao;
        PermiteExclusaoItem = permiteExclusao;
        AtualizarDataModificacao();
    }

    public void AdicionarItem(ComboItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        Itens.Add(item);
        AtualizarDataModificacao();
    }

    public void RemoverItem(int itemId)
    {
        if (!PermiteExclusaoItem)
            throw new InvalidOperationException("Exclusão de itens não permitida para este combo");

        var item = Itens.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Itens.Remove(item);
            AtualizarDataModificacao();
        }
    }

    public void AdicionarLocalRecebimento(ComboLocalRecebimento local)
    {
        if (local == null)
            throw new ArgumentNullException(nameof(local));

        LocaisRecebimento.Add(local);
        AtualizarDataModificacao();
    }

    public void AdicionarCategoriaDesconto(ComboCategoriaDesconto categoria)
    {
        if (categoria == null)
            throw new ArgumentNullException(nameof(categoria));

        CategoriasDesconto.Add(categoria);
        AtualizarDataModificacao();
    }

    public bool EstaVigente()
    {
        var agora = DateTime.UtcNow;
        return Status == StatusCombo.Ativo && 
               agora >= DataInicio && 
               agora <= DataFim;
    }

    public bool ValidarHectareProdutor(decimal hectareProdutor)
    {
        return hectareProdutor >= HectareMinimo && hectareProdutor <= HectareMaximo;
    }

    private static void ValidarParametros(
        string nome,
        decimal hectareMinimo,
        decimal hectareMaximo,
        DateTime dataInicio,
        DateTime dataFim)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do combo é obrigatório", nameof(nome));

        if (hectareMinimo < 0)
            throw new ArgumentException("Hectare mínimo deve ser maior ou igual a zero", nameof(hectareMinimo));

        if (hectareMaximo <= hectareMinimo)
            throw new ArgumentException("Hectare máximo deve ser maior que o mínimo", nameof(hectareMaximo));

        if (dataFim <= dataInicio)
            throw new ArgumentException("Data fim deve ser posterior à data início", nameof(dataFim));

        if (dataInicio < DateTime.UtcNow.Date)
            throw new ArgumentException("Data início não pode ser no passado", nameof(dataInicio));
    }
}