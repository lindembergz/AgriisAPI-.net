using FluentValidation;
using Agriis.Pedidos.Aplicacao.DTOs;

namespace Agriis.Pedidos.Aplicacao.Validadores;

/// <summary>
/// Validador para DTO de adicionar item ao carrinho
/// </summary>
public class AdicionarItemCarrinhoDtoValidator : AbstractValidator<AdicionarItemCarrinhoDto>
{
    public AdicionarItemCarrinhoDtoValidator()
    {
        RuleFor(x => x.ProdutoId)
            .GreaterThan(0)
            .WithMessage("ID do produto deve ser maior que zero");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");

        RuleFor(x => x.CatalogoId)
            .GreaterThan(0)
            .WithMessage("ID do catálogo deve ser maior que zero");

        RuleFor(x => x.Observacoes)
            .MaximumLength(500)
            .WithMessage("Observações não podem exceder 500 caracteres");
    }
}