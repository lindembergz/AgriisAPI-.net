using AutoMapper;
using Agriis.PontosDistribuicao.Aplicacao.DTOs;
using Agriis.PontosDistribuicao.Dominio.Entidades;

namespace Agriis.PontosDistribuicao.Aplicacao.Mapeamentos;

/// <summary>
/// Perfil de mapeamento para pontos de distribuição
/// </summary>
public class PontoDistribuicaoMappingProfile : Profile
{
    public PontoDistribuicaoMappingProfile()
    {
        CreateMap<PontoDistribuicao, PontoDistribuicaoDto>()
            .ForMember(dest => dest.EstadosAtendidos, opt => opt.MapFrom(src => src.ObterEstadosAtendidos()))
            .ForMember(dest => dest.MunicipiosAtendidos, opt => opt.MapFrom(src => src.ObterMunicipiosAtendidos()))
            .ForMember(dest => dest.HorarioFuncionamento, opt => opt.MapFrom(src => src.ObterHorarioFuncionamento()))
            .ForMember(dest => dest.DistanciaKm, opt => opt.Ignore()); // Será preenchida manualmente quando necessário
        
        CreateMap<CriarPontoDistribuicaoDto, PontoDistribuicao>()
            .ConstructUsing(src => new PontoDistribuicao(
                src.Nome,
                src.FornecedorId,
                src.EnderecoId,
                src.Descricao,
                src.RaioCobertura,
                src.CapacidadeMaxima,
                src.UnidadeCapacidade,
                src.Observacoes))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.Endereco, opt => opt.Ignore())
            .ForMember(dest => dest.CoberturaTerritorios, opt => opt.Ignore())
            .ForMember(dest => dest.HorarioFuncionamento, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                // Configurar cobertura territorial após a criação
                dest.DefinirCoberturaTerritorios(src.EstadosAtendidos, src.MunicipiosAtendidos);
                
                // Configurar horário de funcionamento se fornecido
                if (src.HorarioFuncionamento.Any())
                {
                    dest.DefinirHorarioFuncionamento(src.HorarioFuncionamento);
                }
            });
        
        CreateMap<(int Total, int Ativos, int Inativos), EstatisticasPontosDistribuicaoDto>()
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
            .ForMember(dest => dest.Ativos, opt => opt.MapFrom(src => src.Ativos))
            .ForMember(dest => dest.Inativos, opt => opt.MapFrom(src => src.Inativos));
    }
}