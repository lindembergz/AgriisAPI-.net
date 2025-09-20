using Agriis.Segmentacoes.Aplicacao.DTOs;
using Agriis.Segmentacoes.Dominio.Entidades;
using Agriis.Segmentacoes.Dominio.Servicos;
using AutoMapper;

namespace Agriis.Segmentacoes.Aplicacao.Mapeamentos;

/// <summary>
/// Perfil de mapeamento para segmentações
/// </summary>
public class SegmentacaoMappingProfile : Profile
{
    public SegmentacaoMappingProfile()
    {
        // Segmentacao
        CreateMap<Segmentacao, SegmentacaoDto>()
            .ForMember(dest => dest.Grupos, opt => opt.MapFrom(src => src.Grupos));
            
        CreateMap<CriarSegmentacaoDto, Segmentacao>()
            .ConstructUsing(src => new Segmentacao(src.Nome, src.FornecedorId, src.Descricao, src.EhPadrao))
            .ForMember(dest => dest.ConfiguracaoTerritorial, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                if (src.ConfiguracaoTerritorial != null)
                    dest.DefinirConfiguracaoTerritorial(src.ConfiguracaoTerritorial);
            });
        
        // Grupo
        CreateMap<Grupo, GrupoDto>()
            .ForMember(dest => dest.Descontos, opt => opt.MapFrom(src => src.GruposSegmentacao));
            
        CreateMap<CriarGrupoDto, Grupo>()
            .ConstructUsing(src => new Grupo(src.Nome, src.SegmentacaoId, src.AreaMinima, src.AreaMaxima, src.Descricao));
        
        // GrupoSegmentacao
        CreateMap<GrupoSegmentacao, GrupoSegmentacaoDto>();
        
        CreateMap<CriarGrupoSegmentacaoDto, GrupoSegmentacao>()
            .ConstructUsing(src => new GrupoSegmentacao(src.GrupoId, src.CategoriaId, src.PercentualDesconto, src.Observacoes));
        
        // ResultadoDescontoSegmentado
        CreateMap<ResultadoDescontoSegmentado, ResultadoDescontoSegmentadoDto>();
    }
}