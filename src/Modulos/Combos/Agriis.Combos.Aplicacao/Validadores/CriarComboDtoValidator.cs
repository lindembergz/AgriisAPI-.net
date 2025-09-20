using FluentValidation;
using Agriis.Combos.Aplicacao.DTOs;

namespace Agriis.Combos.Aplicacao.Validadores;

/// <summary>
/// Validador para criação de combo
/// </summary>
public class CriarComboDtoValidator : AbstractValidator<CriarComboDto>
{
    public CriarComboDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome do combo é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome do combo deve ter no máximo 200 caracteres");

        RuleFor(x => x.Descricao)
            .MaximumLength(1000)
            .WithMessage("Descrição deve ter no máximo 1000 caracteres");

        RuleFor(x => x.HectareMinimo)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Hectare mínimo deve ser maior ou igual a zero");

        RuleFor(x => x.HectareMaximo)
            .GreaterThan(x => x.HectareMinimo)
            .WithMessage("Hectare máximo deve ser maior que o mínimo");

        RuleFor(x => x.DataInicio)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Data de início não pode ser no passado");

        RuleFor(x => x.DataFim)
            .GreaterThan(x => x.DataInicio)
            .WithMessage("Data fim deve ser posterior à data início");

        RuleFor(x => x.ModalidadePagamento)
            .IsInEnum()
            .WithMessage("Modalidade de pagamento inválida");

        RuleFor(x => x.FornecedorId)
            .GreaterThan(0)
            .WithMessage("Fornecedor é obrigatório");

        RuleFor(x => x.SafraId)
            .GreaterThan(0)
            .WithMessage("Safra é obrigatória");

        RuleFor(x => x.MunicipiosPermitidos)
            .Must(municipios => municipios == null || municipios.All(m => m > 0))
            .WithMessage("Todos os municípios devem ter ID válido");
    }
}