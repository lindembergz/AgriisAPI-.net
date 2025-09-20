using FluentValidation;
using Agriis.Produtos.Aplicacao.DTOs;

namespace Agriis.Produtos.Aplicacao.Validadores;

/// <summary>
/// Validador para criação de categoria
/// </summary>
public class CriarCategoriaDtoValidator : AbstractValidator<CriarCategoriaDto>
{
    public CriarCategoriaDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Descricao)
            .MaximumLength(500)
            .WithMessage("Descrição deve ter no máximo 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descricao));

        RuleFor(x => x.Tipo)
            .IsInEnum()
            .WithMessage("Tipo de categoria inválido");

        RuleFor(x => x.CategoriaPaiId)
            .GreaterThan(0)
            .WithMessage("Categoria pai deve ser válida")
            .When(x => x.CategoriaPaiId.HasValue);

        RuleFor(x => x.Ordem)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Ordem deve ser maior ou igual a zero");
    }
}