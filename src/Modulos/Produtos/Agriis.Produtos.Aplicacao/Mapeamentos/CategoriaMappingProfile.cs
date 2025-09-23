using AutoMapper;
using Agriis.Produtos.Aplicacao.DTOs;
using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.Enums;

namespace Agriis.Produtos.Aplicacao.Mapeamentos;

/// <summary>
/// Perfil de mapeamento para categorias
/// </summary>
public class CategoriaMappingProfile : Profile
{
    public CategoriaMappingProfile()
    {
        // Categoria -> CategoriaDto
        CreateMap<Categoria, CategoriaDto>()
            .ForMember(dest => dest.CategoriaPaiNome, opt => opt.MapFrom(src => src.CategoriaPai != null ? src.CategoriaPai.Nome : null))
            .ForMember(dest => dest.SubCategorias, opt => opt.MapFrom(src => src.SubCategorias.Where(sc => sc.Ativo).OrderBy(sc => sc.Ordem)))
            .ForMember(dest => dest.QuantidadeProdutos, opt => opt.MapFrom(src => src.Produtos.Count(p => p.Status == StatusProduto.Ativo)));

        // Categoria -> CategoriaResumoDto
        CreateMap<Categoria, CategoriaResumoDto>()
            .ForMember(dest => dest.TemSubCategorias, opt => opt.MapFrom(src => src.SubCategorias.Any(sc => sc.Ativo)))
            .ForMember(dest => dest.QuantidadeProdutos, opt => opt.MapFrom(src => src.Produtos.Count(p => p.Status == StatusProduto.Ativo)));

        // CriarCategoriaDto -> Categoria
        CreateMap<CriarCategoriaDto, Categoria>()
            .ConstructUsing(src => new Categoria(
                src.Nome,
                src.Tipo,
                src.Descricao,
                src.CategoriaPaiId,
                src.Ordem))
            .ForAllMembers(opt => opt.Ignore());
    }
}