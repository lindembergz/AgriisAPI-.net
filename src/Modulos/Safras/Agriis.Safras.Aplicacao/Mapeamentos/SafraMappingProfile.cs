using AutoMapper;
using Agriis.Safras.Aplicacao.DTOs;
using Agriis.Safras.Dominio.Entidades;

namespace Agriis.Safras.Aplicacao.Mapeamentos;

/// <summary>
/// Profile do AutoMapper para mapeamento de Safra
/// </summary>
public class SafraMappingProfile : Profile
{
    public SafraMappingProfile()
    {
        CreateMap<Safra, SafraDto>()
            .ForMember(dest => dest.SafraFormatada, opt => opt.MapFrom(src => src.ObterSafraFormatada()))
            .ForMember(dest => dest.Atual, opt => opt.MapFrom(src => src.EstaAtiva()));
            
        CreateMap<Safra, SafraAtualDto>()
            .ForMember(dest => dest.Safra, opt => opt.MapFrom(src => src.ObterSafraAnosFormatada()));
            
        CreateMap<CriarSafraDto, Safra>()
            .ConstructUsing(src => new Safra(src.PlantioInicial, src.PlantioFinal, src.PlantioNome, src.Descricao));
    }
}