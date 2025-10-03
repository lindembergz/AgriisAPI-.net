using FluentValidation;
using Agriis.Fornecedores.Aplicacao.DTOs;
using Agriis.Fornecedores.Dominio.Constantes;

namespace Agriis.Fornecedores.Aplicacao.Validadores;

/// <summary>
/// Validador para FornecedorDto usando FluentValidation
/// </summary>
public class FornecedorDtoValidator : AbstractValidator<FornecedorDto>
{
    public FornecedorDtoValidator()
    {
        // Validação do Nome (obrigatório)
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome do fornecedor é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome do fornecedor não pode exceder 200 caracteres");

        // Validação do Nome Fantasia (opcional, mas com limite de caracteres)
        RuleFor(x => x.NomeFantasia)
            .MaximumLength(200)
            .WithMessage("Nome fantasia não pode exceder 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.NomeFantasia));

        // Validação do CNPJ (obrigatório e formato válido)
        RuleFor(x => x.Cnpj)
            .NotEmpty()
            .WithMessage("CNPJ é obrigatório")
            .Must(BeValidCnpj)
            .WithMessage("CNPJ deve ter formato válido (apenas números, 14 dígitos)");

        // Validação dos Ramos de Atividade
        RuleFor(x => x.RamosAtividade)
            .Must(ramos => ramos == null || ramos.All(r => RamosAtividadeConstants.IsRamoValido(r)))
            .WithMessage("Todos os ramos de atividade devem estar na lista pré-definida")
            .Must(ramos => ramos == null || ramos.Count <= 10)
            .WithMessage("Máximo de 10 ramos de atividade permitidos");

        // Validação do Endereço de Correspondência
        RuleFor(x => x.EnderecoCorrespondencia)
            .Must(e => e == "MesmoFaturamento" || e == "DiferenteFaturamento")
            .WithMessage("Endereço de correspondência deve ser 'MesmoFaturamento' ou 'DiferenteFaturamento'");

        // Validações de campos opcionais com limites
        RuleFor(x => x.InscricaoEstadual)
            .MaximumLength(50)
            .WithMessage("Inscrição estadual não pode exceder 50 caracteres")
            .When(x => !string.IsNullOrEmpty(x.InscricaoEstadual));

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Email deve ter formato válido")
            .MaximumLength(100)
            .WithMessage("Email não pode exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Telefone)
            .MaximumLength(20)
            .WithMessage("Telefone não pode exceder 20 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Telefone));

        RuleFor(x => x.Logradouro)
            .MaximumLength(500)
            .WithMessage("Logradouro não pode exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Logradouro));

        RuleFor(x => x.Bairro)
            .MaximumLength(100)
            .WithMessage("Bairro não pode exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Bairro));

        RuleFor(x => x.Cep)
            .MaximumLength(10)
            .WithMessage("CEP não pode exceder 10 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Cep));

        RuleFor(x => x.Complemento)
            .MaximumLength(200)
            .WithMessage("Complemento não pode exceder 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Complemento));

        // Validação de coordenadas geográficas
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude deve estar entre -90 e 90")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude deve estar entre -180 e 180")
            .When(x => x.Longitude.HasValue);

        // Validação de pedido mínimo
        RuleFor(x => x.PedidoMinimo)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Pedido mínimo não pode ser negativo")
            .When(x => x.PedidoMinimo.HasValue);

        // Validação de moeda padrão
        RuleFor(x => x.MoedaPadrao)
            .InclusiveBetween(0, 2)
            .WithMessage("Moeda padrão deve ser um valor válido (0=Real, 1=Dólar, 2=Euro)");
    }

    /// <summary>
    /// Valida se o CNPJ tem formato básico válido (14 dígitos numéricos)
    /// </summary>
    /// <param name="cnpj">CNPJ a ser validado</param>
    /// <returns>True se válido, false caso contrário</returns>
    private static bool BeValidCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        // Remove caracteres não numéricos
        var cnpjNumeros = new string(cnpj.Where(char.IsDigit).ToArray());
        
        // Deve ter exatamente 14 dígitos
        return cnpjNumeros.Length == 14;
    }
}

/// <summary>
/// Validador para filtros de fornecedor
/// </summary>
public class FiltrosFornecedorDtoValidator : AbstractValidator<FiltrosFornecedorDto>
{
    public FiltrosFornecedorDtoValidator()
    {
        // Validação dos ramos de atividade no filtro
        RuleFor(x => x.RamosAtividade)
            .Must(ramos => ramos == null || ramos.All(r => RamosAtividadeConstants.IsRamoValido(r)))
            .WithMessage("Todos os ramos de atividade devem estar na lista pré-definida");

        // Validação do endereço de correspondência no filtro
        RuleFor(x => x.EnderecoCorrespondencia)
            .Must(e => string.IsNullOrEmpty(e) || e == "MesmoFaturamento" || e == "DiferenteFaturamento")
            .WithMessage("Endereço de correspondência deve ser 'MesmoFaturamento' ou 'DiferenteFaturamento'");

        // Validação de paginação
        RuleFor(x => x.Pagina)
            .GreaterThan(0)
            .WithMessage("Página deve ser maior que zero");

        RuleFor(x => x.TamanhoPagina)
            .InclusiveBetween(1, 100)
            .WithMessage("Tamanho da página deve estar entre 1 e 100");
    }
}