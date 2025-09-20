using Agriis.Pedidos.Aplicacao.DTOs;
using FluentValidation;

namespace Agriis.Pedidos.Aplicacao.Validadores;

/// <summary>
/// Validador para o DTO de criação de proposta
/// </summary>
public class CriarPropostaDtoValidator : AbstractValidator<CriarPropostaDto>
{
    /// <summary>
    /// Construtor do validador
    /// </summary>
    public CriarPropostaDtoValidator()
    {
        RuleFor(x => x.Observacao)
            .MaximumLength(1024)
            .WithMessage("A observação não pode ter mais de 1024 caracteres");
            
        RuleFor(x => x.AcaoComprador)
            .IsInEnum()
            .When(x => x.AcaoComprador.HasValue)
            .WithMessage("Ação do comprador inválida");
    }
}