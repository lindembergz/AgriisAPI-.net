using AutoMapper;
using Agriis.Catalogos.Aplicacao.DTOs;
using Agriis.Catalogos.Dominio.Entidades;

namespace Agriis.Catalogos.Aplicacao.Mapeamentos;

public class CatalogoMappingProfile : Profile
{
    public CatalogoMappingProfile()
    {
        CreateMap<Catalogo, CatalogoDto>();
        CreateMap<CriarCatalogoDto, Catalogo>()
            .ConstructUsing(src => new Catalogo(
                src.SafraId,
                src.PontoDistribuicaoId,
                src.CulturaId,
                src.CategoriaId,
                src.Moeda,
                src.DataInicio,
                src.DataFim));
        
        CreateMap<CatalogoItem, CatalogoItemDto>();
        CreateMap<CriarCatalogoItemDto, CatalogoItem>()
            .ConstructUsing((src, context) => new CatalogoItem(
                0, // CatalogoId será definido no serviço
                src.ProdutoId,
                src.EstruturaPrecosJson,
                src.PrecoBase));
    }
}