using FluentValidation;
using Agriis.Pedidos.Aplicacao.DTOs;

namespace Agriis.Pedidos.Aplicacao.Validadores;

/// <summary>
/// Validador para DTO de atualizar quantidade de item
/// </summary>
public class AtualizarQuantidadeItemDtoValidator : AbstractValidator<AtualizarQuantidadeItemDto>
{
    public AtualizarQuantidadeItemDtoValidator()
    {
        RuleFor(x => x.Quantidade)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");
    }
}