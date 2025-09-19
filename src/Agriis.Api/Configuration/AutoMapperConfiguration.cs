using AutoMapper;
using Agriis.Enderecos.Aplicacao.Mapeamentos;
using Agriis.Usuarios.Aplicacao.Mapeamentos;
using Agriis.Culturas.Aplicacao.Mapeamentos;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração centralizada do AutoMapper
/// </summary>
public static class AutoMapperConfiguration
{
    /// <summary>
    /// Configura o AutoMapper com todos os profiles dos módulos
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços configurada</returns>
    public static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services)
    {
        // Configurar AutoMapper manualmente
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EnderecoMappingProfile>();
            cfg.AddProfile<UsuarioMappingProfile>();
            cfg.AddProfile<CulturaMappingProfile>();
        });

        services.AddSingleton(config);
        services.AddSingleton<IMapper>(provider => new Mapper(config));

        return services;
    }
}