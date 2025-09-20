using System.Text.Json;
using AutoMapper;
using Agriis.Combos.Aplicacao.DTOs;
using Agriis.Combos.Dominio.Entidades;

namespace Agriis.Combos.Aplicacao.Mapeamentos;

/// <summary>
/// Perfil de mapeamento para entidades de Combo
/// </summary>
public class ComboMappingProfile : Profile
{
    public ComboMappingProfile()
    {
        CreateMap<Combo, ComboDto>()
            .ForMember(dest => dest.RestricoesMunicipios, opt => opt.MapFrom(src => 
                src.RestricoesMunicipios != null ? 
                JsonSerializer.Deserialize<object>(src.RestricoesMunicipios.RootElement.GetRawText(), (JsonSerializerOptions?)null) : 
                null));

        CreateMap<ComboItem, ComboItemDto>();

        CreateMap<ComboLocalRecebimento, ComboLocalRecebimentoDto>();

        CreateMap<ComboCategoriaDesconto, ComboCategoriaDescontoDto>();
    }
}