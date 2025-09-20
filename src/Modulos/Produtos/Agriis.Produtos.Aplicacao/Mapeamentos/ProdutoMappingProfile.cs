using AutoMapper;
using Agriis.Produtos.Aplicacao.DTOs;
using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.ObjetosValor;

namespace Agriis.Produtos.Aplicacao.Mapeamentos;

/// <summary>
/// Perfil de mapeamento para produtos
/// </summary>
public class ProdutoMappingProfile : Profile
{
    public ProdutoMappingProfile()
    {
        // Produto -> ProdutoDto
        CreateMap<Produto, ProdutoDto>()
            .ForMember(dest => dest.CategoriaNome, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nome : null))
            .ForMember(dest => dest.ProdutoPaiNome, opt => opt.MapFrom(src => src.ProdutoPai != null ? src.ProdutoPai.Nome : null))
            .ForMember(dest => dest.CulturasIds, opt => opt.MapFrom(src => src.ProdutosCulturas.Where(pc => pc.Ativo).Select(pc => pc.CulturaId)))
            .ForMember(dest => dest.CulturasNomes, opt => opt.Ignore()) // Será preenchido no serviço
            .ForMember(dest => dest.FornecedorNome, opt => opt.Ignore()); // Será preenchido no serviço

        // Produto -> ProdutoResumoDto
        CreateMap<Produto, ProdutoResumoDto>()
            .ForMember(dest => dest.CategoriaNome, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nome : null))
            .ForMember(dest => dest.FornecedorNome, opt => opt.Ignore()); // Será preenchido no serviço

        // CriarProdutoDto -> Produto
        CreateMap<CriarProdutoDto, Produto>()
            .ConstructUsing(src => new Produto(
                src.Nome,
                src.Codigo,
                src.Tipo,
                src.Unidade,
                new DimensoesProduto(
                    src.Dimensoes.Altura,
                    src.Dimensoes.Largura,
                    src.Dimensoes.Comprimento,
                    src.Dimensoes.PesoNominal,
                    src.Dimensoes.Densidade),
                src.CategoriaId,
                src.FornecedorId,
                src.Descricao,
                src.Marca,
                src.TipoCalculoPeso,
                src.ProdutoRestrito,
                src.ObservacoesRestricao,
                src.ProdutoPaiId))
            .ForAllMembers(opt => opt.Ignore());

        // DimensoesProduto -> DimensoesProdutoDto
        CreateMap<DimensoesProduto, DimensoesProdutoDto>()
            .ForMember(dest => dest.Volume, opt => opt.MapFrom(src => src.CalcularVolume()))
            .ForMember(dest => dest.PesoCubado, opt => opt.MapFrom(src => src.CalcularPesoCubado()))
            .ForMember(dest => dest.PesoParaFrete, opt => opt.MapFrom(src => src.ObterPesoParaFrete()));

        // CriarDimensoesProdutoDto -> DimensoesProduto
        CreateMap<CriarDimensoesProdutoDto, DimensoesProduto>()
            .ConstructUsing(src => new DimensoesProduto(
                src.Altura,
                src.Largura,
                src.Comprimento,
                src.PesoNominal,
                src.Densidade))
            .ForAllMembers(opt => opt.Ignore());

        // AtualizarDimensoesProdutoDto -> DimensoesProduto
        CreateMap<AtualizarDimensoesProdutoDto, DimensoesProduto>()
            .ConstructUsing(src => new DimensoesProduto(
                src.Altura,
                src.Largura,
                src.Comprimento,
                src.PesoNominal,
                src.Densidade))
            .ForAllMembers(opt => opt.Ignore());
    }
}