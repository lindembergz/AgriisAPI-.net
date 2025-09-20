using FluentValidation;
using Agriis.Pagamentos.Aplicacao.DTOs;

namespace Agriis.Pagamentos.Aplicacao.Validadores;

/// <summary>
/// Validador para criação de associação cultura-fornecedor-forma de pagamento
/// </summary>
public class CriarCulturaFormaPagamentoDtoValidator : AbstractValidator<CriarCulturaFormaPagamentoDto>
{
    public CriarCulturaFormaPagamentoDtoValidator()
    {
        RuleFor(x => x.FornecedorId)
            .GreaterThan(0)
            .WithMessage("ID do fornecedor deve ser maior que zero");
            
        RuleFor(x => x.CulturaId)
            .GreaterThan(0)
            .WithMessage("ID da cultura deve ser maior que zero");
            
        RuleFor(x => x.FormaPagamentoId)
            .GreaterThan(0)
            .WithMessage("ID da forma de pagamento deve ser maior que zero");
    }
}