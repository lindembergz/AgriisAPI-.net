namespace Agriis.Combos.Dominio.Enums;

/// <summary>
/// Modalidades de pagamento para combos
/// </summary>
public enum ModalidadePagamento
{
    Normal = 1,
    Barter = 2
}

/// <summary>
/// Status do combo
/// </summary>
public enum StatusCombo
{
    Ativo = 1,
    Inativo = 2,
    Expirado = 3,
    Suspenso = 4
}

/// <summary>
/// Tipo de desconto do combo
/// </summary>
public enum TipoDesconto
{
    Percentual = 1,
    ValorFixo = 2,
    PorHectare = 3
}