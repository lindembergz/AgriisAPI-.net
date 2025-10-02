using AutoMapper;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Dominio.Entidades;

namespace Agriis.Referencias.Aplicacao.Mapeamentos;

/// <summary>
/// Perfil de mapeamento para entidades de referência
/// </summary>
public class ReferenciasMappingProfile : Profile
{
    public ReferenciasMappingProfile()
    {
        ConfigurarMapeamentoMoeda();
        ConfigurarMapeamentoAtividadeAgropecuaria();
        ConfigurarMapeamentoUnidadeMedida();
        ConfigurarMapeamentoEmbalagem();
        // REMOVIDO: ConfigurarMapeamentoUf() - migrado para EnderecoMappingProfile
        // REMOVIDO: ConfigurarMapeamentoMunicipio() - migrado para EnderecoMappingProfile
    }

    private void ConfigurarMapeamentoMoeda()
    {
        // Entidade -> DTO de leitura
        CreateMap<Moeda, MoedaDto>();

        // DTO de criação -> Entidade
        CreateMap<CriarMoedaDto, Moeda>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => true)); // Novas moedas são criadas ativas

        // DTO de atualização -> Entidade (preserva campos não editáveis)
        CreateMap<AtualizarMoedaDto, Moeda>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Codigo, opt => opt.Ignore()) // Código não pode ser alterado
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore()); // RowVersion é gerenciado pelo EF
    }





    private void ConfigurarMapeamentoAtividadeAgropecuaria()
    {
        // Entidade -> DTO de leitura
        CreateMap<AtividadeAgropecuaria, AtividadeAgropecuariaDto>()
            .ForMember(dest => dest.TipoDescricao, opt => opt.MapFrom(src => src.Tipo.ToString()));

        // DTO de criação -> Entidade
        CreateMap<CriarAtividadeAgropecuariaDto, AtividadeAgropecuaria>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => true));

        // DTO de atualização -> Entidade
        CreateMap<AtualizarAtividadeAgropecuariaDto, AtividadeAgropecuaria>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Codigo, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());
    }

    private void ConfigurarMapeamentoUnidadeMedida()
    {
        // Entidade -> DTO de leitura
        CreateMap<UnidadeMedida, UnidadeMedidaDto>()
            .ForMember(dest => dest.TipoDescricao, opt => opt.MapFrom(src => src.Tipo.ToString()));

        // DTO de criação -> Entidade
        CreateMap<CriarUnidadeMedidaDto, UnidadeMedida>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => true));

        // DTO de atualização -> Entidade
        CreateMap<AtualizarUnidadeMedidaDto, UnidadeMedida>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Simbolo, opt => opt.Ignore())
            .ForMember(dest => dest.Tipo, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());
    }

    private void ConfigurarMapeamentoEmbalagem()
    {
        // Entidade -> DTO de leitura
        CreateMap<Embalagem, EmbalagemDto>()
            .ForMember(dest => dest.UnidadeMedidaNome, opt => opt.MapFrom(src => src.UnidadeMedida != null ? src.UnidadeMedida.Nome : string.Empty))
            .ForMember(dest => dest.UnidadeMedidaSimbolo, opt => opt.MapFrom(src => src.UnidadeMedida != null ? src.UnidadeMedida.Simbolo : string.Empty));

        // DTO de criação -> Entidade
        CreateMap<CriarEmbalagemDto, Embalagem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.UnidadeMedida, opt => opt.Ignore());

        // DTO de atualização -> Entidade
        CreateMap<AtualizarEmbalagemDto, Embalagem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.UnidadeMedida, opt => opt.Ignore());
    }
}