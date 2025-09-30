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
        ConfigurarMapeamentoPais();
        ConfigurarMapeamentoUf();
        ConfigurarMapeamentoMunicipio();
        ConfigurarMapeamentoAtividadeAgropecuaria();
        ConfigurarMapeamentoUnidadeMedida();
        ConfigurarMapeamentoEmbalagem();
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

    private void ConfigurarMapeamentoPais()
    {
        // Entidade -> DTO de leitura
        CreateMap<Pais, PaisDto>()
            .ForMember(dest => dest.QuantidadeUfs, opt => opt.MapFrom(src => src.Ufs != null ? src.Ufs.Count : 0));

        // DTO de criação -> Entidade
        CreateMap<CriarPaisDto, Pais>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Ufs, opt => opt.Ignore());

        // DTO de atualização -> Entidade
        CreateMap<AtualizarPaisDto, Pais>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Codigo, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Ufs, opt => opt.Ignore());
    }

    private void ConfigurarMapeamentoUf()
    {
        // Entidade -> DTO de leitura
        CreateMap<Uf, UfDto>()
            .ForMember(dest => dest.PaisNome, opt => opt.MapFrom(src => src.Pais != null ? src.Pais.Nome : string.Empty))
            .ForMember(dest => dest.QuantidadeMunicipios, opt => opt.MapFrom(src => src.Municipios != null ? src.Municipios.Count : 0));

        // DTO de criação -> Entidade
        CreateMap<CriarUfDto, Uf>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Pais, opt => opt.Ignore())
            .ForMember(dest => dest.Municipios, opt => opt.Ignore());

        // DTO de atualização -> Entidade
        CreateMap<AtualizarUfDto, Uf>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Codigo, opt => opt.Ignore())
            .ForMember(dest => dest.PaisId, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Pais, opt => opt.Ignore())
            .ForMember(dest => dest.Municipios, opt => opt.Ignore());
    }

    private void ConfigurarMapeamentoMunicipio()
    {
        // Entidade -> DTO de leitura
        CreateMap<Municipio, MunicipioDto>()
            .ForMember(dest => dest.UfNome, opt => opt.MapFrom(src => string.Empty)) // Relacionamento ignorado temporariamente
            .ForMember(dest => dest.UfCodigo, opt => opt.MapFrom(src => string.Empty)); // Relacionamento ignorado temporariamente

        // DTO de criação -> Entidade
        CreateMap<CriarMunicipioDto, Municipio>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Uf, opt => opt.Ignore());

        // DTO de atualização -> Entidade
        CreateMap<AtualizarMunicipioDto, Municipio>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CodigoIbge, opt => opt.Ignore())
            .ForMember(dest => dest.UfId, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Uf, opt => opt.Ignore());
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