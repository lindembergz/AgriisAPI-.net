using FluentValidation;
using Agriis.Produtos.Aplicacao.DTOs;

namespace Agriis.Produtos.Aplicacao.Validadores;

/// <summary>
/// Validador para atualização de produto
/// </summary>
public class AtualizarProdutoDtoValidator : AbstractValidator<AtualizarProdutoDto>
{
    public AtualizarProdutoDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Codigo)
            .NotEmpty()
            .WithMessage("Código é obrigatório")
            .MaximumLength(50)
            .WithMessage("Código deve ter no máximo 50 caracteres");

        RuleFor(x => x.Marca)
            .MaximumLength(100)
            .WithMessage("Marca deve ter no máximo 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Marca));

        RuleFor(x => x.Descricao)
            .MaximumLength(1000)
            .WithMessage("Descrição deve ter no máximo 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descricao));

        RuleFor(x => x.Unidade)
            .IsInEnum()
            .WithMessage("Unidade inválida");

        RuleFor(x => x.TipoCalculoPeso)
            .IsInEnum()
            .WithMessage("Tipo de cálculo de peso inválido");

        RuleFor(x => x.CategoriaId)
            .GreaterThan(0)
            .WithMessage("Categoria é obrigatória");

        RuleFor(x => x.ObservacoesRestricao)
            .NotEmpty()
            .WithMessage("Observações de restrição são obrigatórias quando o produto é restrito")
            .MaximumLength(500)
            .WithMessage("Observações de restrição devem ter no máximo 500 caracteres")
            .When(x => x.ProdutoRestrito);

        RuleFor(x => x.Dimensoes)
            .NotNull()
            .WithMessage("Dimensões são obrigatórias")
            .SetValidator(new AtualizarDimensoesProdutoDtoValidator());

        RuleFor(x => x.CulturasIds)
            .NotEmpty()
            .WithMessage("Pelo menos uma cultura deve ser associada ao produto");

        RuleForEach(x => x.CulturasIds)
            .GreaterThan(0)
            .WithMessage("IDs de cultura devem ser válidos");
    }
}

/// <summary>
/// Validador para dimensões do produto na atualização
/// </summary>
public class AtualizarDimensoesProdutoDtoValidator : AbstractValidator<AtualizarDimensoesProdutoDto>
{
    public AtualizarDimensoesProdutoDtoValidator()
    {
        RuleFor(x => x.Altura)
            .GreaterThan(0)
            .WithMessage("Altura deve ser maior que zero")
            .LessThanOrEqualTo(1000)
            .WithMessage("Altura deve ser menor ou igual a 1000 cm");

        RuleFor(x => x.Largura)
            .GreaterThan(0)
            .WithMessage("Largura deve ser maior que zero")
            .LessThanOrEqualTo(1000)
            .WithMessage("Largura deve ser menor ou igual a 1000 cm");

        RuleFor(x => x.Comprimento)
            .GreaterThan(0)
            .WithMessage("Comprimento deve ser maior que zero")
            .LessThanOrEqualTo(1000)
            .WithMessage("Comprimento deve ser menor ou igual a 1000 cm");

        RuleFor(x => x.PesoNominal)
            .GreaterThan(0)
            .WithMessage("Peso nominal deve ser maior que zero")
            .LessThanOrEqualTo(10000)
            .WithMessage("Peso nominal deve ser menor ou igual a 10000 kg");

        RuleFor(x => x.PesoEmbalagem)
            .GreaterThan(0)
            .WithMessage("Peso da embalagem deve ser maior que zero")
            .LessThanOrEqualTo(10000)
            .WithMessage("Peso da embalagem deve ser menor ou igual a 10000 kg");

        RuleFor(x => x.QuantidadeMinima)
            .GreaterThan(0)
            .WithMessage("Quantidade mínima deve ser maior que zero")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Quantidade mínima deve ser menor ou igual a 1.000.000");

        RuleFor(x => x.Embalagem)
            .NotEmpty()
            .WithMessage("Embalagem deve ser informada")
            .MaximumLength(100)
            .WithMessage("Embalagem deve ter no máximo 100 caracteres");

        RuleFor(x => x.Pms)
            .GreaterThan(0)
            .WithMessage("PMS deve ser maior que zero")
            .LessThanOrEqualTo(1000000)
            .WithMessage("PMS deve ser menor ou igual a 1.000.000 gramas")
            .When(x => x.Pms.HasValue);

        RuleFor(x => x.FaixaDensidadeInicial)
            .GreaterThan(0)
            .WithMessage("Faixa de densidade inicial deve ser maior que zero")
            .LessThanOrEqualTo(10000)
            .WithMessage("Faixa de densidade inicial deve ser menor ou igual a 10000 kg/m³")
            .When(x => x.FaixaDensidadeInicial.HasValue);

        RuleFor(x => x.FaixaDensidadeFinal)
            .GreaterThan(0)
            .WithMessage("Faixa de densidade final deve ser maior que zero")
            .LessThanOrEqualTo(10000)
            .WithMessage("Faixa de densidade final deve ser menor ou igual a 10000 kg/m³")
            .When(x => x.FaixaDensidadeFinal.HasValue);
    }
}