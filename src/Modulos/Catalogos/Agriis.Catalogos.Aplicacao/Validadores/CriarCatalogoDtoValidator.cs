using Agriis.Catalogos.Aplicacao.DTOs;
using FluentValidation;

namespace Agriis.Catalogos.Aplicacao.Validadores;

public class CriarCatalogoDtoValidator : AbstractValidator<CriarCatalogoDto>
{
    public CriarCatalogoDtoValidator()
    {
        RuleFor(x => x.SafraId)
            .GreaterThan(0)
            .WithMessage("SafraId deve ser maior que zero");

        RuleFor(x => x.PontoDistribuicaoId)
            .GreaterThan(0)
            .WithMessage("PontoDistribuicaoId deve ser maior que zero");

        RuleFor(x => x.CulturaId)
            .GreaterThan(0)
            .WithMessage("CulturaId deve ser maior que zero");

        RuleFor(x => x.CategoriaId)
            .GreaterThan(0)
            .WithMessage("CategoriaId deve ser maior que zero");

        RuleFor(x => x.Moeda)
            .IsInEnum()
            .WithMessage("Moeda deve ser um valor válido");

        RuleFor(x => x.DataInicio)
            .NotEmpty()
            .WithMessage("DataInicio é obrigatória");

        RuleFor(x => x.DataFim)
            .GreaterThan(x => x.DataInicio)
            .When(x => x.DataFim.HasValue)
            .WithMessage("DataFim deve ser maior que DataInicio");
    }
}