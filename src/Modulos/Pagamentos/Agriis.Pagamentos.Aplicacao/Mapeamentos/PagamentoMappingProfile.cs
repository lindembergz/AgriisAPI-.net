using AutoMapper;
using Agriis.Pagamentos.Aplicacao.DTOs;
using Agriis.Pagamentos.Dominio.Entidades;

namespace Agriis.Pagamentos.Aplicacao.Mapeamentos;

/// <summary>
/// Perfil de mapeamento para entidades de pagamento
/// </summary>
public class PagamentoMappingProfile : Profile
{
    public PagamentoMappingProfile()
    {
        // FormaPagamento mappings
        CreateMap<FormaPagamento, FormaPagamentoDto>();
        CreateMap<CriarFormaPagamentoDto, FormaPagamento>()
            .ConstructUsing(src => new FormaPagamento(src.Descricao));
        
        // CulturaFormaPagamento mappings
        CreateMap<CulturaFormaPagamento, CulturaFormaPagamentoDto>();
        CreateMap<CriarCulturaFormaPagamentoDto, CulturaFormaPagamento>()
            .ConstructUsing(src => new CulturaFormaPagamento(
                src.FornecedorId, 
                src.CulturaId, 
                src.FormaPagamentoId));
    }
}