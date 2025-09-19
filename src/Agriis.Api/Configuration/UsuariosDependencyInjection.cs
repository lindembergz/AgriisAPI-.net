using Agriis.Usuarios.Dominio.Interfaces;
using Agriis.Usuarios.Infraestrutura.Repositorios;
using Agriis.Usuarios.Aplicacao.Interfaces;
using Agriis.Usuarios.Aplicacao.Servicos;
using Agriis.Usuarios.Aplicacao.Validadores;
using Agriis.Usuarios.Aplicacao.DTOs;
using FluentValidation;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de Usuários
/// </summary>
public static class UsuariosDependencyInjection
{
    /// <summary>
    /// Configura os serviços do módulo de Usuários
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços configurada</returns>
    public static IServiceCollection AddUsuariosModule(this IServiceCollection services)
    {
        // Repositórios
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // Serviços de aplicação
        services.AddScoped<IUsuarioService, UsuarioService>();

        // Validadores
        services.AddScoped<IValidator<CriarUsuarioDto>, CriarUsuarioDtoValidator>();
        services.AddScoped<IValidator<AtualizarUsuarioDto>, AtualizarUsuarioDtoValidator>();
        services.AddScoped<IValidator<AlterarEmailDto>, AlterarEmailDtoValidator>();
        services.AddScoped<IValidator<AlterarSenhaDto>, AlterarSenhaDtoValidator>();

        return services;
    }
}