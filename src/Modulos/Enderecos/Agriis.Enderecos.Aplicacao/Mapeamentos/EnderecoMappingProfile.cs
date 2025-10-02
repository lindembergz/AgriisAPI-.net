using AutoMapper;
using Agriis.Enderecos.Aplicacao.DTOs;
using Agriis.Enderecos.Dominio.Entidades;

namespace Agriis.Enderecos.Aplicacao.Mapeamentos;

/// <summary>
/// Perfil de mapeamento para entidades do módulo de Endereços
/// </summary>
public class EnderecoMappingProfile : Profile
{
    public EnderecoMappingProfile()
    {
        ConfigurarMapeamentosPais();
        ConfigurarMapeamentosEstado();
        ConfigurarMapeamentosMunicipio();
        ConfigurarMapeamentosEndereco();
        ConfigurarMapeamentosResposta();
    }

    private void ConfigurarMapeamentosPais()
    {
        // Pais -> PaisDto
        CreateMap<Pais, PaisDto>();
    }

    private void ConfigurarMapeamentosEstado()
    {
        // Estado -> EstadoDto
        CreateMap<Estado, EstadoDto>();
        
        // Estado -> EstadoResumoDto
        CreateMap<Estado, EstadoResumoDto>();
        
        // CriarEstadoDto -> Estado
        CreateMap<CriarEstadoDto, Estado>()
            .ConstructUsing(dto => new Estado(dto.Nome, dto.Uf, dto.CodigoIbge, dto.Regiao, dto.PaisId));
    }

    private void ConfigurarMapeamentosMunicipio()
    {
        // Municipio -> MunicipioDto
        CreateMap<Municipio, MunicipioDto>()
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado));
        
        // Municipio -> MunicipioResumoDto
        CreateMap<Municipio, MunicipioResumoDto>()
            .ForMember(dest => dest.EstadoUf, opt => opt.MapFrom(src => src.Estado.Uf))
            .ForMember(dest => dest.EstadoNome, opt => opt.MapFrom(src => src.Estado.Nome));
        
        // Municipio -> MunicipioProximoDto
        CreateMap<Municipio, MunicipioProximoDto>()
            .ForMember(dest => dest.EstadoUf, opt => opt.MapFrom(src => src.Estado.Uf))
            .ForMember(dest => dest.DistanciaKm, opt => opt.Ignore()); // Será preenchido manualmente
        
        // Municipio -> DropdownMunicipioDto
        CreateMap<Municipio, DropdownMunicipioDto>();
        
        // CriarMunicipioDto -> Municipio
        CreateMap<CriarMunicipioDto, Municipio>()
            .ConstructUsing(dto => new Municipio(dto.Nome, dto.CodigoIbge, dto.EstadoId, 
                dto.CepPrincipal, dto.Latitude, dto.Longitude));
        
        // Municipio -> MunicipioFrontendDto (compatibilidade com frontend)
        CreateMap<Municipio, MunicipioFrontendDto>()
            .ForMember(dest => dest.CodigoIbge, opt => opt.MapFrom(src => src.CodigoIbge.ToString()))
            .ForMember(dest => dest.UfId, opt => opt.MapFrom(src => src.EstadoId))
            .ForMember(dest => dest.UfNome, opt => opt.MapFrom(src => src.Estado.Nome))
            .ForMember(dest => dest.UfCodigo, opt => opt.MapFrom(src => src.Estado.Uf))
            .ForMember(dest => dest.Uf, opt => opt.MapFrom(src => src.Estado))
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => true)); // Todos os municípios são ativos por padrão
        
        // Estado -> UfFrontendDto (compatibilidade com frontend)
        CreateMap<Estado, UfFrontendDto>()
            .ForMember(dest => dest.CodigoIbge, opt => opt.MapFrom(src => src.CodigoIbge.ToString()))
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => true)); // Todos os estados são ativos por padrão
    }

    private void ConfigurarMapeamentosEndereco()
    {
        // Endereco -> EnderecoDto
        CreateMap<Endereco, EnderecoDto>()
            .ForMember(dest => dest.CepFormatado, opt => opt.MapFrom(src => src.ObterCepFormatado()))
            .ForMember(dest => dest.EnderecoFormatado, opt => opt.MapFrom(src => src.ObterEnderecoFormatado()))
            .ForMember(dest => dest.Municipio, opt => opt.MapFrom(src => src.Municipio))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado));
        
        // Endereco -> EnderecoResumoDto
        CreateMap<Endereco, EnderecoResumoDto>()
            .ForMember(dest => dest.CepFormatado, opt => opt.MapFrom(src => src.ObterCepFormatado()))
            .ForMember(dest => dest.EnderecoFormatado, opt => opt.MapFrom(src => src.ObterEnderecoFormatado()))
            .ForMember(dest => dest.MunicipioNome, opt => opt.MapFrom(src => src.Municipio.Nome))
            .ForMember(dest => dest.EstadoUf, opt => opt.MapFrom(src => src.Estado.Uf));
        
        // Endereco -> EnderecoProximoDto
        CreateMap<Endereco, EnderecoProximoDto>()
            .ForMember(dest => dest.CepFormatado, opt => opt.MapFrom(src => src.ObterCepFormatado()))
            .ForMember(dest => dest.EnderecoFormatado, opt => opt.MapFrom(src => src.ObterEnderecoFormatado()))
            .ForMember(dest => dest.MunicipioNome, opt => opt.MapFrom(src => src.Municipio.Nome))
            .ForMember(dest => dest.EstadoUf, opt => opt.MapFrom(src => src.Estado.Uf))
            .ForMember(dest => dest.DistanciaKm, opt => opt.Ignore()); // Será preenchido manualmente
        
        // CriarEnderecoDto -> Endereco
        CreateMap<CriarEnderecoDto, Endereco>()
            .ConstructUsing(dto => new Endereco(dto.Cep, dto.Logradouro, dto.Bairro, 
                dto.MunicipioId, dto.EstadoId, dto.Numero, dto.Complemento, 
                dto.Latitude, dto.Longitude));
    }

    private void ConfigurarMapeamentosResposta()
    {
        // Configurações para DTOs de resposta genéricos
        // Estes DTOs são criados manualmente nos controllers, então não precisam de mapeamento automático
        
        // Mas podemos configurar mapeamentos úteis se necessário
        CreateMap<(IEnumerable<Municipio> Items, int TotalCount), PaginatedResponse<MunicipioDto>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalCount))
            .ForMember(dest => dest.TotalPages, opt => opt.Ignore()) // Calculado manualmente
            .ForMember(dest => dest.CurrentPage, opt => opt.Ignore()) // Definido manualmente
            .ForMember(dest => dest.PageSize, opt => opt.Ignore()); // Definido manualmente
    }
}