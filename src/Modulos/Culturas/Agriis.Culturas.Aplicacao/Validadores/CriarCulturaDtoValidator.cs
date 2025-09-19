using FluentValidation;
using Agriis.Culturas.Aplicacao.DTOs;

namespace Agriis.Culturas.Aplicacao.Validadores;

public class CriarCulturaDtoValidator : AbstractValidator<CriarCulturaDto>
{
    public CriarCulturaDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Descricao)
            .MaximumLength(500)
            .WithMessage("Descrição deve ter no máximo 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descricao));
    }
}