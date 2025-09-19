using AutoMapper;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Propriedades.Aplicacao.DTOs;
using Agriis.Propriedades.Dominio.Entidades;
using NetTopologySuite.Geometries;
using System.Text.Json;

namespace Agriis.Propriedades.Aplicacao.Mapeamentos;

public class PropriedadeMappingProfile : Profile
{
    public PropriedadeMappingProfile()
    {
        // Propriedade mappings
        CreateMap<Propriedade, PropriedadeDto>()
            .ForMember(dest => dest.AreaTotal, opt => opt.MapFrom(src => src.AreaTotal.Valor))
            .ForMember(dest => dest.DadosAdicionais, opt => opt.MapFrom(src => 
                src.DadosAdicionais != null ? JsonSerializer.Deserialize<object>(src.DadosAdicionais.RootElement.GetRawText(), (JsonSerializerOptions?)null) : null))
            .ForMember(dest => dest.Talhoes, opt => opt.MapFrom(src => src.Talhoes))
            .ForMember(dest => dest.Culturas, opt => opt.MapFrom(src => src.PropriedadeCulturas));

        CreateMap<PropriedadeCreateDto, Propriedade>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.Talhoes, opt => opt.Ignore())
            .ForMember(dest => dest.PropriedadeCulturas, opt => opt.Ignore())
            .ForMember(dest => dest.DadosAdicionais, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                if (src.DadosAdicionais != null)
                {
                    dest.DefinirDadosAdicionais(src.DadosAdicionais);
                }
            });

        // Talhao mappings
        CreateMap<Talhao, TalhaoDto>()
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area.Valor))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Localizacao != null ? src.Localizacao.Y : (double?)null))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Localizacao != null ? src.Localizacao.X : (double?)null));

        CreateMap<TalhaoCreateDto, Talhao>()
            .ConstructUsing(src => new Talhao(
                src.Nome,
                new AreaPlantio(src.Area),
                src.PropriedadeId,
                src.Descricao))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.Localizacao, opt => opt.Ignore())
            .ForMember(dest => dest.Geometria, opt => opt.Ignore())
            .ForMember(dest => dest.Propriedade, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                if (src.Latitude.HasValue && src.Longitude.HasValue)
                {
                    dest.DefinirLocalizacao(src.Latitude.Value, src.Longitude.Value);
                }
            });

        // PropriedadeCultura mappings
        CreateMap<PropriedadeCultura, PropriedadeCulturaDto>()
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area.Valor))
            .ForMember(dest => dest.EstaEmPeriodoPlantio, opt => opt.MapFrom(src => src.EstaEmPeriodoPlantio()));

        CreateMap<PropriedadeCulturaCreateDto, PropriedadeCultura>()
            .ConstructUsing(src => new PropriedadeCultura(
                src.PropriedadeId,
                src.CulturaId,
                new AreaPlantio(src.Area),
                src.SafraId))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.Propriedade, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                if (src.DataPlantio.HasValue || src.DataColheitaPrevista.HasValue)
                {
                    dest.DefinirDatasPlantioColheita(src.DataPlantio, src.DataColheitaPrevista);
                }
                if (!string.IsNullOrEmpty(src.Observacoes))
                {
                    dest.AdicionarObservacoes(src.Observacoes);
                }
            });
    }
}