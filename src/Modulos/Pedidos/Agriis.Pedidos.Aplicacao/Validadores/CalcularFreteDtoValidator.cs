using FluentValidation;
using Agriis.Pedidos.Aplicacao.DTOs;

namespace Agriis.Pedidos.Aplicacao.Validadores;

/// <summary>
/// Validador para CalcularFreteDto
/// </summary>
public class CalcularFreteDtoValidator : AbstractValidator<CalcularFreteDto>
{
    public CalcularFreteDtoValidator()
    {
        RuleFor(x => x.ProdutoId)
            .GreaterThan(0)
            .WithMessage("ID do produto deve ser maior que zero");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");

        RuleFor(x => x.DistanciaKm)
            .GreaterThan(0)
            .WithMessage("Distância deve ser maior que zero");

        RuleFor(x => x.ValorPorKgKm)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorPorKgKm.HasValue)
            .WithMessage("Valor por kg/km não pode ser negativo");

        RuleFor(x => x.ValorMinimoFrete)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorMinimoFrete.HasValue)
            .WithMessage("Valor mínimo de frete não pode ser negativo");
    }
}

/// <summary>
/// Validador para CalcularFreteConsolidadoDto
/// </summary>
public class CalcularFreteConsolidadoDtoValidator : AbstractValidator<CalcularFreteConsolidadoDto>
{
    public CalcularFreteConsolidadoDtoValidator()
    {
        RuleFor(x => x.Itens)
            .NotEmpty()
            .WithMessage("Lista de itens não pode estar vazia");

        RuleForEach(x => x.Itens)
            .SetValidator(new ItemFreteDtoValidator());

        RuleFor(x => x.DistanciaKm)
            .GreaterThan(0)
            .WithMessage("Distância deve ser maior que zero");

        RuleFor(x => x.ValorPorKgKm)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorPorKgKm.HasValue)
            .WithMessage("Valor por kg/km não pode ser negativo");

        RuleFor(x => x.ValorMinimoFrete)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorMinimoFrete.HasValue)
            .WithMessage("Valor mínimo de frete não pode ser negativo");
    }
}

/// <summary>
/// Validador para ItemFreteDto
/// </summary>
public class ItemFreteDtoValidator : AbstractValidator<ItemFreteDto>
{
    public ItemFreteDtoValidator()
    {
        RuleFor(x => x.ProdutoId)
            .GreaterThan(0)
            .WithMessage("ID do produto deve ser maior que zero");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");
    }
}