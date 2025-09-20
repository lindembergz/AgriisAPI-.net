using AutoMapper;
using Agriis.Pedidos.Aplicacao.DTOs;
using Agriis.Pedidos.Dominio.Entidades;

namespace Agriis.Pedidos.Aplicacao.Mapeamentos;

/// <summary>
/// Perfil de mapeamento para entidades de pedidos
/// </summary>
public class PedidoMappingProfile : Profile
{
    public PedidoMappingProfile()
    {
        CreateMap<Pedido, PedidoDto>();
        CreateMap<CriarPedidoDto, Pedido>();
        
        CreateMap<PedidoItem, PedidoItemDto>();
        CreateMap<CriarPedidoItemDto, PedidoItem>();
        
        CreateMap<PedidoItemTransporte, PedidoItemTransporteDto>();
        
        // Mapeamentos para Proposta
        CreateMap<Proposta, PropostaDto>()
            .ForMember(dest => dest.UsuarioFornecedor, opt => opt.Ignore()) // Será mapeado manualmente quando necessário
            .ForMember(dest => dest.UsuarioProdutor, opt => opt.Ignore()); // Será mapeado manualmente quando necessário
    }
}