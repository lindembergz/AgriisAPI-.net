using Agriis.Api.Contexto;
using Agriis.Autenticacao.Aplicacao.Interfaces;
using Agriis.Autenticacao.Aplicacao.Servicos;
using Agriis.Autenticacao.Dominio.Interfaces;
using Agriis.Autenticacao.Infraestrutura.Repositorios;
using Agriis.Autenticacao.Infraestrutura.Servicos;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de autenticação
/// </summary>
public static class AutenticacaoDependencyInjection
{
    /// <summary>
    /// Registra os serviços do módulo de autenticação
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços</returns>
    public static IServiceCollection AddAutenticacaoServices(this IServiceCollection services)
    {
        // Serviços de aplicação
        services.AddScoped<IAutenticacaoService, AutenticacaoService>();

        // Serviços de domínio/infraestrutura
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();

        // Repositórios
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }
}