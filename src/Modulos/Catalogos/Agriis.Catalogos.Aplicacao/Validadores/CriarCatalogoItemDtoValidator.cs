using Agriis.Catalogos.Aplicacao.DTOs;
using FluentValidation;

namespace Agriis.Catalogos.Aplicacao.Validadores;

public class CriarCatalogoItemDtoValidator : AbstractValidator<CriarCatalogoItemDto>
{
    public CriarCatalogoItemDtoValidator()
    {
        RuleFor(x => x.ProdutoId)
            .GreaterThan(0)
            .WithMessage("ProdutoId deve ser maior que zero");

        RuleFor(x => x.EstruturaPrecosJson)
            .NotNull()
            .WithMessage("EstruturaPrecosJson é obrigatória");

        RuleFor(x => x.PrecoBase)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PrecoBase.HasValue)
            .WithMessage("PrecoBase deve ser maior ou igual a zero");
    }
}