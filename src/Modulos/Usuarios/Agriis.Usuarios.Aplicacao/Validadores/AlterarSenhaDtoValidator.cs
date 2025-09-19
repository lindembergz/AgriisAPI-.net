using FluentValidation;
using Agriis.Usuarios.Aplicacao.DTOs;

namespace Agriis.Usuarios.Aplicacao.Validadores;

/// <summary>
/// Validador para o DTO de alteração de senha
/// </summary>
public class AlterarSenhaDtoValidator : AbstractValidator<AlterarSenhaDto>
{
    public AlterarSenhaDtoValidator()
    {
        RuleFor(x => x.SenhaAtual)
            .NotEmpty()
            .WithMessage("Senha atual é obrigatória");
            
        RuleFor(x => x.NovaSenha)
            .NotEmpty()
            .WithMessage("Nova senha é obrigatória")
            .MinimumLength(6)
            .WithMessage("Nova senha deve ter no mínimo 6 caracteres");
    }
}