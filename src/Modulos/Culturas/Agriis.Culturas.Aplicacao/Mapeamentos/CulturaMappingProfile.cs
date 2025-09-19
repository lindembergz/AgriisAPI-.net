using AutoMapper;
using Agriis.Culturas.Aplicacao.DTOs;
using Agriis.Culturas.Dominio.Entidades;

namespace Agriis.Culturas.Aplicacao.Mapeamentos;

public class CulturaMappingProfile : Profile
{
    public CulturaMappingProfile()
    {
        CreateMap<Cultura, CulturaDto>();
        CreateMap<CriarCulturaDto, Cultura>()
            .ConstructUsing(src => new Cultura(src.Nome, src.Descricao));
    }
}