using FluentValidation;
using Agriis.Combos.Aplicacao.DTOs;

namespace Agriis.Combos.Aplicacao.Validadores;

/// <summary>
/// Validador para criação de item de combo
/// </summary>
public class CriarComboItemDtoValidator : AbstractValidator<CriarComboItemDto>
{
    public CriarComboItemDtoValidator()
    {
        RuleFor(x => x.ProdutoId)
            .GreaterThan(0)
            .WithMessage("Produto é obrigatório");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");

        RuleFor(x => x.PrecoUnitario)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Preço unitário não pode ser negativo");

        RuleFor(x => x.PercentualDesconto)
            .InclusiveBetween(0, 100)
            .WithMessage("Percentual de desconto deve estar entre 0 e 100");

        RuleFor(x => x.Ordem)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Ordem deve ser maior ou igual a zero");
    }
}