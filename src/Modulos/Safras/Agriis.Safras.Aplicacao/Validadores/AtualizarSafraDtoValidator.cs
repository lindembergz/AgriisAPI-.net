using FluentValidation;
using Agriis.Safras.Aplicacao.DTOs;

namespace Agriis.Safras.Aplicacao.Validadores;

/// <summary>
/// Validador para atualização de Safra
/// </summary>
public class AtualizarSafraDtoValidator : AbstractValidator<AtualizarSafraDto>
{
    public AtualizarSafraDtoValidator()
    {
        RuleFor(x => x.PlantioInicial)
            .NotEmpty()
            .WithMessage("Data inicial do plantio é obrigatória")
            .GreaterThan(new DateTime(1900, 1, 1))
            .WithMessage("Data inicial do plantio deve ser posterior a 1900")
            .LessThan(DateTime.Now.AddYears(10))
            .WithMessage("Data inicial do plantio não pode ser superior a 10 anos no futuro");

        RuleFor(x => x.PlantioFinal)
            .NotEmpty()
            .WithMessage("Data final do plantio é obrigatória")
            .GreaterThan(x => x.PlantioInicial)
            .WithMessage("Data final do plantio deve ser posterior à data inicial")
            .LessThan(DateTime.Now.AddYears(10))
            .WithMessage("Data final do plantio não pode ser superior a 10 anos no futuro");

        RuleFor(x => x.PlantioNome)
            .NotEmpty()
            .WithMessage("Nome do plantio é obrigatório")
            .MaximumLength(256)
            .WithMessage("Nome do plantio deve ter no máximo 256 caracteres");

        RuleFor(x => x.Descricao)
            .NotEmpty()
            .WithMessage("Descrição é obrigatória")
            .MaximumLength(64)
            .WithMessage("Descrição deve ter no máximo 64 caracteres");
    }
}