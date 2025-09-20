using Agriis.Combos.Dominio.Enums;

namespace Agriis.Combos.Aplicacao.DTOs;

/// <summary>
/// DTO para representar um combo
/// </summary>
public class ComboDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal HectareMinimo { get; set; }
    public decimal HectareMaximo { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public ModalidadePagamento ModalidadePagamento { get; set; }
    public StatusCombo Status { get; set; }
    public bool PermiteAlteracaoItem { get; set; }
    public bool PermiteExclusaoItem { get; set; }
    public int FornecedorId { get; set; }
    public int SafraId { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public List<ComboItemDto> Itens { get; set; } = new();
    public List<ComboLocalRecebimentoDto> LocaisRecebimento { get; set; } = new();
    public List<ComboCategoriaDescontoDto> CategoriasDesconto { get; set; } = new();
    public object? RestricoesMunicipios { get; set; }
}

/// <summary>
/// DTO para criar um novo combo
/// </summary>
public class CriarComboDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal HectareMinimo { get; set; }
    public decimal HectareMaximo { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public ModalidadePagamento ModalidadePagamento { get; set; }
    public int FornecedorId { get; set; }
    public int SafraId { get; set; }
    public List<int>? MunicipiosPermitidos { get; set; }
    public bool PermiteAlteracaoItem { get; set; } = true;
    public bool PermiteExclusaoItem { get; set; } = true;
}

/// <summary>
/// DTO para atualizar um combo
/// </summary>
public class AtualizarComboDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal HectareMinimo { get; set; }
    public decimal HectareMaximo { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public List<int>? MunicipiosPermitidos { get; set; }
    public bool PermiteAlteracaoItem { get; set; }
    public bool PermiteExclusaoItem { get; set; }
}