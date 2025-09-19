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
        ConfigurarMapeamentosEstado();
        ConfigurarMapeamentosMunicipio();
        ConfigurarMapeamentosEndereco();
    }

    private void ConfigurarMapeamentosEstado()
    {
        // Estado -> EstadoDto
        CreateMap<Estado, EstadoDto>();
        
        // Estado -> EstadoResumoDto
        CreateMap<Estado, EstadoResumoDto>();
        
        // CriarEstadoDto -> Estado
        CreateMap<CriarEstadoDto, Estado>()
            .ConstructUsing(dto => new Estado(dto.Nome, dto.Uf, dto.CodigoIbge, dto.Regiao));
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
        
        // CriarMunicipioDto -> Municipio
        CreateMap<CriarMunicipioDto, Municipio>()
            .ConstructUsing(dto => new Municipio(dto.Nome, dto.CodigoIbge, dto.EstadoId, 
                dto.CepPrincipal, dto.Latitude, dto.Longitude));
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
}