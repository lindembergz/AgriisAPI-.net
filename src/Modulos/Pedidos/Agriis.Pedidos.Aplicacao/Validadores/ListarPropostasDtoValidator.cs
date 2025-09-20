using Agriis.Pedidos.Aplicacao.DTOs;
using FluentValidation;

namespace Agriis.Pedidos.Aplicacao.Validadores;

/// <summary>
/// Validador para o DTO de listagem de propostas
/// </summary>
public class ListarPropostasDtoValidator : AbstractValidator<ListarPropostasDto>
{
    /// <summary>
    /// Construtor do validador
    /// </summary>
    public ListarPropostasDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("A página deve ser maior ou igual a 0");
            
        RuleFor(x => x.MaxPerPage)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("O máximo de itens por página deve estar entre 1 e 100");
            
        RuleFor(x => x.Sorting)
            .Must(BeValidSorting)
            .When(x => !string.IsNullOrWhiteSpace(x.Sorting))
            .WithMessage("Campo de ordenação inválido");
    }
    
    /// <summary>
    /// Valida se o campo de ordenação é válido
    /// </summary>
    /// <param name="sorting">Campo de ordenação</param>
    /// <returns>True se válido</returns>
    private static bool BeValidSorting(string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
            return true;
            
        var validSortings = new[] { "datacriacao", "datacriacao desc" };
        return validSortings.Contains(sorting.ToLower());
    }
}