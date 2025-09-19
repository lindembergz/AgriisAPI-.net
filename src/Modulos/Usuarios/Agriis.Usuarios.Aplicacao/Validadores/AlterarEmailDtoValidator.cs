using FluentValidation;
using Agriis.Usuarios.Aplicacao.DTOs;

namespace Agriis.Usuarios.Aplicacao.Validadores;

/// <summary>
/// Validador para o DTO de alteração de email
/// </summary>
public class AlterarEmailDtoValidator : AbstractValidator<AlterarEmailDto>
{
    public AlterarEmailDtoValidator()
    {
        RuleFor(x => x.NovoEmail)
            .NotEmpty()
            .WithMessage("Novo email é obrigatório")
            .EmailAddress()
            .WithMessage("Novo email deve ter um formato válido")
            .MaximumLength(255)
            .WithMessage("Novo email deve ter no máximo 255 caracteres");
    }
}