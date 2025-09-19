using FluentValidation;
using Agriis.Usuarios.Aplicacao.DTOs;
using Agriis.Compartilhado.Dominio.Validadores;

namespace Agriis.Usuarios.Aplicacao.Validadores;

/// <summary>
/// Validador para o DTO de atualização de usuário
/// </summary>
public class AtualizarUsuarioDtoValidator : AbstractValidator<AtualizarUsuarioDto>
{
    public AtualizarUsuarioDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres");
            
        RuleFor(x => x.Celular)
            .Must(ValidarTelefone)
            .When(x => !string.IsNullOrWhiteSpace(x.Celular))
            .WithMessage("Celular deve ter um formato válido");
            
        RuleFor(x => x.Cpf)
            .Must(ValidarCpf)
            .When(x => !string.IsNullOrWhiteSpace(x.Cpf))
            .WithMessage("CPF deve ter um formato válido");
    }
    
    private static bool ValidarTelefone(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return true;
            
        // Remove caracteres não numéricos
        var numeroLimpo = new string(telefone.Where(char.IsDigit).ToArray());
        
        // Valida se tem 10 ou 11 dígitos (telefone fixo ou celular)
        return numeroLimpo.Length is 10 or 11;
    }
    
    private static bool ValidarCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return true;
            
        return ValidadorDocumentosBrasileiros.ValidarCpf(cpf);
    }
}