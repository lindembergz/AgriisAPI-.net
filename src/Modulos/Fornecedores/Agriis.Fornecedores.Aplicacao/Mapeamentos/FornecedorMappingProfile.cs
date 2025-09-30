using AutoMapper;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Fornecedores.Aplicacao.DTOs;
using Agriis.Fornecedores.Dominio.Entidades;

namespace Agriis.Fornecedores.Aplicacao.Mapeamentos;

/// <summary>
/// Profile do AutoMapper para mapeamento de entidades de fornecedores
/// </summary>
public class FornecedorMappingProfile : Profile
{
    public FornecedorMappingProfile()
    {
        // Mapeamento Fornecedor -> FornecedorDto
        CreateMap<Fornecedor, FornecedorDto>()
            .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.Cnpj.Valor))
            .ForMember(dest => dest.CnpjFormatado, opt => opt.MapFrom(src => src.Cnpj.ValorFormatado))
            .ForMember(dest => dest.MoedaPadrao, opt => opt.MapFrom(src => (int)src.Moeda))
            .ForMember(dest => dest.MoedaPadraoNome, opt => opt.MapFrom(src => ObterNomeMoeda(src.Moeda)))
            .ForMember(dest => dest.UfNome, opt => opt.MapFrom(src => src.Estado != null ? src.Estado.Nome : null))
            .ForMember(dest => dest.UfCodigo, opt => opt.MapFrom(src => src.Estado != null ? src.Estado.Uf : null))
            .ForMember(dest => dest.MunicipioNome, opt => opt.MapFrom(src => src.Municipio != null ? src.Municipio.Nome : null))
            .ForMember(dest => dest.Usuarios, opt => opt.MapFrom(src => src.UsuariosFornecedores));

        // Mapeamento UsuarioFornecedor -> UsuarioFornecedorDto
        CreateMap<UsuarioFornecedor, UsuarioFornecedorDto>()
            .ForMember(dest => dest.UsuarioNome, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Nome : string.Empty))
            .ForMember(dest => dest.UsuarioEmail, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Email : string.Empty))
            .ForMember(dest => dest.FornecedorNome, opt => opt.MapFrom(src => src.Fornecedor != null ? src.Fornecedor.Nome : string.Empty))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => (int)src.Role))
            .ForMember(dest => dest.RoleNome, opt => opt.MapFrom(src => ObterNomeRole(src.Role)))
            .ForMember(dest => dest.Territorios, opt => opt.MapFrom(src => src.Territorios));

        // Mapeamento UsuarioFornecedorTerritorio -> UsuarioFornecedorTerritorioDto
        CreateMap<UsuarioFornecedorTerritorio, UsuarioFornecedorTerritorioDto>()
            .ForMember(dest => dest.EstadosLista, opt => opt.MapFrom(src => ExtrairEstadosLista(src)))
            .ForMember(dest => dest.MunicipiosLista, opt => opt.MapFrom(src => ExtrairMunicipiosLista(src)));
    }

    private static string ObterNomeMoeda(MoedaFinanceira moeda)
    {
        return moeda switch
        {
            MoedaFinanceira.Real => "Real",
            MoedaFinanceira.Dolar => "Dólar",
            MoedaFinanceira.Euro => "Euro",
            _ => "Desconhecido"
        };
    }

    private static string ObterNomeRole(Roles role)
    {
        return role switch
        {
            Roles.RoleFornecedorWebAdmin => "Administrador",
            Roles.RoleFornecedorWebRepresentante => "Representante Comercial",
            Roles.RoleComprador => "Comprador",
            Roles.RoleAdmin => "Administrador do Sistema",
            _ => "Desconhecido"
        };
    }   
 private static List<string> ExtrairEstadosLista(UsuarioFornecedorTerritorio territorio)
    {
        try
        {
            if (territorio.Estados == null)
                return new List<string>();

            return territorio.Estados.RootElement.EnumerateArray()
                .Select(e => e.GetString())
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList()!;
        }
        catch
        {
            return new List<string>();
        }
    }

    private static Dictionary<string, List<string>> ExtrairMunicipiosLista(UsuarioFornecedorTerritorio territorio)
    {
        try
        {
            var resultado = new Dictionary<string, List<string>>();

            if (territorio.Municipios == null)
                return resultado;

            foreach (var item in territorio.Municipios.RootElement.EnumerateArray())
            {
                if (item.TryGetProperty("estado", out var estadoElement) &&
                    item.TryGetProperty("municipios", out var municipiosElement))
                {
                    var estado = estadoElement.GetString();
                    if (!string.IsNullOrEmpty(estado))
                    {
                        var municipios = municipiosElement.EnumerateArray()
                            .Select(m => m.GetString())
                            .Where(m => !string.IsNullOrEmpty(m))
                            .ToList()!;

                        resultado[estado] = municipios;
                    }
                }
            }

            return resultado;
        }
        catch
        {
            return new Dictionary<string, List<string>>();
        }
    }
}