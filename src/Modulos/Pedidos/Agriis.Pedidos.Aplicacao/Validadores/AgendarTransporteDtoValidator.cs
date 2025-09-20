using FluentValidation;
using Agriis.Pedidos.Aplicacao.DTOs;

namespace Agriis.Pedidos.Aplicacao.Validadores;

/// <summary>
/// Validador para AgendarTransporteDto
/// </summary>
public class AgendarTransporteDtoValidator : AbstractValidator<AgendarTransporteDto>
{
    public AgendarTransporteDtoValidator()
    {
        RuleFor(x => x.PedidoItemId)
            .GreaterThan(0)
            .WithMessage("ID do item de pedido deve ser maior que zero");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");

        RuleFor(x => x.DataAgendamento)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Data de agendamento deve ser futura");

        RuleFor(x => x.DistanciaKm)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DistanciaKm.HasValue)
            .WithMessage("Distância não pode ser negativa");

        RuleFor(x => x.EnderecoOrigem)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.EnderecoOrigem))
            .WithMessage("Endereço de origem não pode ter mais que 500 caracteres");

        RuleFor(x => x.EnderecoDestino)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.EnderecoDestino))
            .WithMessage("Endereço de destino não pode ter mais que 500 caracteres");

        RuleFor(x => x.Observacoes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacoes))
            .WithMessage("Observações não podem ter mais que 1000 caracteres");
    }
}

/// <summary>
/// Validador para ReagendarTransporteDto
/// </summary>
public class ReagendarTransporteDtoValidator : AbstractValidator<ReagendarTransporteDto>
{
    public ReagendarTransporteDtoValidator()
    {
        RuleFor(x => x.NovaDataAgendamento)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Nova data de agendamento deve ser futura");

        RuleFor(x => x.Observacoes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacoes))
            .WithMessage("Observações não podem ter mais que 1000 caracteres");
    }
}

/// <summary>
/// Validador para AtualizarValorFreteDto
/// </summary>
public class AtualizarValorFreteDtoValidator : AbstractValidator<AtualizarValorFreteDto>
{
    public AtualizarValorFreteDtoValidator()
    {
        RuleFor(x => x.NovoValorFrete)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Valor do frete não pode ser negativo");

        RuleFor(x => x.Motivo)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Motivo))
            .WithMessage("Motivo não pode ter mais que 500 caracteres");
    }
}

/// <summary>
/// Validador para ValidarAgendamentosDto
/// </summary>
public class ValidarAgendamentosDtoValidator : AbstractValidator<ValidarAgendamentosDto>
{
    public ValidarAgendamentosDtoValidator()
    {
        RuleFor(x => x.Agendamentos)
            .NotEmpty()
            .WithMessage("Lista de agendamentos não pode estar vazia");

        RuleForEach(x => x.Agendamentos)
            .SetValidator(new SolicitacaoAgendamentoDtoValidator());
    }
}

/// <summary>
/// Validador para SolicitacaoAgendamentoDto
/// </summary>
public class SolicitacaoAgendamentoDtoValidator : AbstractValidator<SolicitacaoAgendamentoDto>
{
    public SolicitacaoAgendamentoDtoValidator()
    {
        RuleFor(x => x.PedidoItemId)
            .GreaterThan(0)
            .WithMessage("ID do item de pedido deve ser maior que zero");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");

        RuleFor(x => x.DataAgendamento)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Data de agendamento deve ser futura");

        RuleFor(x => x.DistanciaKm)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DistanciaKm.HasValue)
            .WithMessage("Distância não pode ser negativa");

        RuleFor(x => x.EnderecoOrigem)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.EnderecoOrigem))
            .WithMessage("Endereço de origem não pode ter mais que 500 caracteres");

        RuleFor(x => x.EnderecoDestino)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.EnderecoDestino))
            .WithMessage("Endereço de destino não pode ter mais que 500 caracteres");

        RuleFor(x => x.Observacoes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacoes))
            .WithMessage("Observações não podem ter mais que 1000 caracteres");
    }
}