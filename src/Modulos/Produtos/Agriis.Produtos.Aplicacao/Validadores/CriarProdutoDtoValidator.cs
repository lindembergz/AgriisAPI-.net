using FluentValidation;
using Agriis.Produtos.Aplicacao.DTOs;
using Agriis.Produtos.Dominio.Enums;

namespace Agriis.Produtos.Aplicacao.Validadores;

/// <summary>
/// Validador para criação de produto
/// </summary>
public class CriarProdutoDtoValidator : AbstractValidator<CriarProdutoDto>
{
    public CriarProdutoDtoValidator()
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

        RuleFor(x => x.Tipo)
            .IsInEnum()
            .WithMessage("Tipo de produto inválido");

        RuleFor(x => x.Unidade)
            .IsInEnum()
            .WithMessage("Unidade inválida");

        RuleFor(x => x.TipoCalculoPeso)
            .IsInEnum()
            .WithMessage("Tipo de cálculo de peso inválido");

        RuleFor(x => x.CategoriaId)
            .GreaterThan(0)
            .WithMessage("Categoria é obrigatória");

        RuleFor(x => x.FornecedorId)
            .GreaterThan(0)
            .WithMessage("Fornecedor é obrigatório");

        RuleFor(x => x.ProdutoPaiId)
            .GreaterThan(0)
            .WithMessage("Produto pai deve ser válido")
            .When(x => x.ProdutoPaiId.HasValue);

        RuleFor(x => x.ObservacoesRestricao)
            .NotEmpty()
            .WithMessage("Observações de restrição são obrigatórias quando o produto é restrito")
            .MaximumLength(500)
            .WithMessage("Observações de restrição devem ter no máximo 500 caracteres")
            .When(x => x.ProdutoRestrito);

        RuleFor(x => x.Dimensoes)
            .NotNull()
            .WithMessage("Dimensões são obrigatórias")
            .SetValidator(new CriarDimensoesProdutoDtoValidator());

        RuleFor(x => x.CulturasIds)
            .NotEmpty()
            .WithMessage("Pelo menos uma cultura deve ser associada ao produto");

        RuleForEach(x => x.CulturasIds)
            .GreaterThan(0)
            .WithMessage("IDs de cultura devem ser válidos");

        // Validação específica para produtos revendedores
        RuleFor(x => x.ProdutoPaiId)
            .NotNull()
            .WithMessage("Produtos revendedores devem ter um produto pai")
            .When(x => x.Tipo == TipoProduto.Revendedor);

        // Validação específica para produtos fabricantes
        RuleFor(x => x.ProdutoPaiId)
            .Null()
            .WithMessage("Produtos fabricantes não podem ter produto pai")
            .When(x => x.Tipo == TipoProduto.Fabricante);
    }
}

/// <summary>
/// Validador para dimensões do produto na criação
/// </summary>
public class CriarDimensoesProdutoDtoValidator : AbstractValidator<CriarDimensoesProdutoDto>
{
    public CriarDimensoesProdutoDtoValidator()
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

        RuleFor(x => x.Densidade)
            .GreaterThan(0)
            .WithMessage("Densidade deve ser maior que zero")
            .LessThanOrEqualTo(10000)
            .WithMessage("Densidade deve ser menor ou igual a 10000 kg/m³")
            .When(x => x.Densidade.HasValue);
    }
}