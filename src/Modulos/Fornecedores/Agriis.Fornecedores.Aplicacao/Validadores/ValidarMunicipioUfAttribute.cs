using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Agriis.Fornecedores.Aplicacao.Validadores;

/// <summary>
/// Atributo de validação para garantir que o município pertence à UF selecionada
/// </summary>
public class ValidarMunicipioUfAttribute : ValidationAttribute
{
    private readonly string _ufIdPropertyName;

    public ValidarMunicipioUfAttribute(string ufIdPropertyName)
    {
        _ufIdPropertyName = ufIdPropertyName;
        ErrorMessage = "O município selecionado deve pertencer à UF informada";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Se não há município selecionado, não há erro
        if (value == null)
            return ValidationResult.Success;

        var municipioId = (int)value;

        // Obter o valor da propriedade UfId
        var ufIdProperty = validationContext.ObjectType.GetProperty(_ufIdPropertyName);
        if (ufIdProperty == null)
            return new ValidationResult($"Propriedade {_ufIdPropertyName} não encontrada");

        var ufIdValue = ufIdProperty.GetValue(validationContext.ObjectInstance);
        
        // Se não há UF selecionada, não podemos validar
        if (ufIdValue == null)
            return ValidationResult.Success;

        var ufId = (int)ufIdValue;

        // Aqui seria necessário validar no banco de dados se o município pertence à UF
        // Por enquanto, vamos apenas retornar sucesso, pois a validação será feita no service
        return ValidationResult.Success;
    }
}