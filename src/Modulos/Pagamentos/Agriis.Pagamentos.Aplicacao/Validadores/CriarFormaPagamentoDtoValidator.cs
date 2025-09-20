using FluentValidation;
using Agriis.Pagamentos.Aplicacao.DTOs;

namespace Agriis.Pagamentos.Aplicacao.Validadores;

/// <summary>
/// Validador para criação de forma de pagamento
/// </summary>
public class CriarFormaPagamentoDtoValidator : AbstractValidator<CriarFormaPagamentoDto>
{
    public CriarFormaPagamentoDtoValidator()
    {
        RuleFor(x => x.Descricao)
            .NotEmpty()
            .WithMessage("Descrição da forma de pagamento é obrigatória")
            .MaximumLength(45)
            .WithMessage("Descrição da forma de pagamento deve ter no máximo 45 caracteres");
    }
}