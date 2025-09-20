using FluentValidation;
using Agriis.Pedidos.Aplicacao.DTOs;

namespace Agriis.Pedidos.Aplicacao.Validadores;

/// <summary>
/// Validador para o DTO de criação de pedido
/// </summary>
public class CriarPedidoDtoValidator : AbstractValidator<CriarPedidoDto>
{
    public CriarPedidoDtoValidator()
    {
        RuleFor(x => x.FornecedorId)
            .GreaterThan(0)
            .WithMessage("ID do fornecedor deve ser maior que zero");
            
        RuleFor(x => x.ProdutorId)
            .GreaterThan(0)
            .WithMessage("ID do produtor deve ser maior que zero");
            
        RuleFor(x => x.DiasLimiteInteracao)
            .GreaterThan(0)
            .WithMessage("Dias limite de interação deve ser maior que zero")
            .LessThanOrEqualTo(365)
            .WithMessage("Dias limite de interação não pode ser maior que 365 dias");
    }
}

/// <summary>
/// Validador para o DTO de criação de item de pedido
/// </summary>
public class CriarPedidoItemDtoValidator : AbstractValidator<CriarPedidoItemDto>
{
    public CriarPedidoItemDtoValidator()
    {
        RuleFor(x => x.PedidoId)
            .GreaterThan(0)
            .WithMessage("ID do pedido deve ser maior que zero");
            
        RuleFor(x => x.ProdutoId)
            .GreaterThan(0)
            .WithMessage("ID do produto deve ser maior que zero");
            
        RuleFor(x => x.Quantidade)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");
            
        RuleFor(x => x.PrecoUnitario)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Preço unitário não pode ser negativo");
            
        RuleFor(x => x.PercentualDesconto)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Percentual de desconto não pode ser negativo")
            .LessThanOrEqualTo(100)
            .WithMessage("Percentual de desconto não pode ser maior que 100%");
            
        RuleFor(x => x.Observacoes)
            .MaximumLength(1000)
            .WithMessage("Observações não podem ter mais que 1000 caracteres");
    }
}

/// <summary>
/// Validador para o DTO de criação de transporte de item
/// </summary>
public class CriarPedidoItemTransporteDtoValidator : AbstractValidator<CriarPedidoItemTransporteDto>
{
    public CriarPedidoItemTransporteDtoValidator()
    {
        RuleFor(x => x.PedidoItemId)
            .GreaterThan(0)
            .WithMessage("ID do item de pedido deve ser maior que zero");
            
        RuleFor(x => x.Quantidade)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");
            
        RuleFor(x => x.ValorFrete)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Valor do frete não pode ser negativo");
            
        RuleFor(x => x.DataAgendamento)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Data de agendamento deve ser futura")
            .When(x => x.DataAgendamento.HasValue);
            
        RuleFor(x => x.EnderecoOrigem)
            .MaximumLength(500)
            .WithMessage("Endereço de origem não pode ter mais que 500 caracteres");
            
        RuleFor(x => x.EnderecoDestino)
            .MaximumLength(500)
            .WithMessage("Endereço de destino não pode ter mais que 500 caracteres");
    }
}