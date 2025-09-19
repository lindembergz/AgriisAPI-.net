using AutoMapper;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Produtores.Aplicacao.DTOs;
using Agriis.Produtores.Dominio.Entidades;
using Agriis.Produtores.Dominio.Enums;
using Agriis.Produtores.Dominio.Interfaces;

namespace Agriis.Produtores.Aplicacao.Mapeamentos;

/// <summary>
/// Perfil de mapeamento do AutoMapper para Produtores
/// </summary>
public class ProdutorMappingProfile : Profile
{
    public ProdutorMappingProfile()
    {
        // Produtor -> ProdutorDto
        CreateMap<Produtor, ProdutorDto>()
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.Cpf != null ? src.Cpf.Valor : null))
            .ForMember(dest => dest.CpfFormatado, opt => opt.MapFrom(src => src.Cpf != null ? src.Cpf.ValorFormatado : null))
            .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.Cnpj != null ? src.Cnpj.Valor : null))
            .ForMember(dest => dest.CnpjFormatado, opt => opt.MapFrom(src => src.Cnpj != null ? src.Cnpj.ValorFormatado : null))
            .ForMember(dest => dest.AreaPlantio, opt => opt.MapFrom(src => src.AreaPlantio.Valor))
            .ForMember(dest => dest.AreaPlantioFormatada, opt => opt.MapFrom(src => src.AreaPlantio.ValorFormatado))
            .ForMember(dest => dest.StatusDescricao, opt => opt.MapFrom(src => ObterDescricaoStatus(src.Status)))
            .ForMember(dest => dest.EstaAutorizado, opt => opt.MapFrom(src => src.EstaAutorizado()))
            .ForMember(dest => dest.EhPessoaFisica, opt => opt.MapFrom(src => src.EhPessoaFisica()))
            .ForMember(dest => dest.EhPessoaJuridica, opt => opt.MapFrom(src => src.EhPessoaJuridica()))
            .ForMember(dest => dest.DocumentoPrincipal, opt => opt.MapFrom(src => src.ObterDocumentoPrincipal()));

        // CriarProdutorDto -> Produtor
        CreateMap<CriarProdutorDto, Produtor>()
            .ConstructUsing(src => new Produtor(
                src.Nome,
                !string.IsNullOrEmpty(src.Cpf) ? new Cpf(src.Cpf) : null,
                !string.IsNullOrEmpty(src.Cnpj) ? new Cnpj(src.Cnpj) : null,
                src.InscricaoEstadual,
                src.TipoAtividade,
                new AreaPlantio(src.AreaPlantio)))
            .ForMember(dest => dest.Culturas, opt => opt.MapFrom(src => src.Culturas));

        // UsuarioProdutor -> UsuarioProdutorDto
        CreateMap<UsuarioProdutor, UsuarioProdutorDto>()
            .ForMember(dest => dest.NomeUsuario, opt => opt.MapFrom(src => src.Usuario.Nome))
            .ForMember(dest => dest.EmailUsuario, opt => opt.MapFrom(src => src.Usuario.Email))
            .ForMember(dest => dest.NomeProdutor, opt => opt.MapFrom(src => src.Produtor.Nome))
            .ForMember(dest => dest.DocumentoProdutor, opt => opt.MapFrom(src => src.Produtor.ObterDocumentoPrincipal()));

        // CriarUsuarioProdutorDto -> UsuarioProdutor
        CreateMap<CriarUsuarioProdutorDto, UsuarioProdutor>()
            .ConstructUsing(src => new UsuarioProdutor(src.UsuarioId, src.ProdutorId, src.EhProprietario));

        // ProdutorEstatisticas -> ProdutorEstatisticasDto
        CreateMap<ProdutorEstatisticas, ProdutorEstatisticasDto>()
            .ForMember(dest => dest.AreaTotalFormatada, opt => opt.MapFrom(src => $"{src.AreaTotalPlantio:N2} ha"))
            .ForMember(dest => dest.AreaMediaFormatada, opt => opt.MapFrom(src => $"{src.AreaMediaPlantio:N2} ha"));
    }

    /// <summary>
    /// Obtém a descrição do status do produtor
    /// </summary>
    /// <param name="status">Status do produtor</param>
    /// <returns>Descrição do status</returns>
    private static string ObterDescricaoStatus(StatusProdutor status)
    {
        return status switch
        {
            StatusProdutor.PendenteValidacaoAutomatica => "Pendente de Validação Automática",
            StatusProdutor.PendenteValidacaoManual => "Pendente de Validação Manual",
            StatusProdutor.PendenteCnpj => "Pendente de CNPJ",
            StatusProdutor.AutorizadoAutomaticamente => "Autorizado Automaticamente",
            StatusProdutor.AutorizadoManualmente => "Autorizado Manualmente",
            StatusProdutor.Negado => "Negado",
            _ => "Status Desconhecido"
        };
    }
}