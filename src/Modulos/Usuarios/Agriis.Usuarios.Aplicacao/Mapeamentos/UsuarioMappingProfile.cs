using AutoMapper;
using Agriis.Usuarios.Dominio.Entidades;
using Agriis.Usuarios.Aplicacao.DTOs;
using Agriis.Compartilhado.Dominio.ObjetosValor;

namespace Agriis.Usuarios.Aplicacao.Mapeamentos;

/// <summary>
/// Profile do AutoMapper para mapeamento de usuários
/// </summary>
public class UsuarioMappingProfile : Profile
{
    public UsuarioMappingProfile()
    {
        // Mapeamento de Usuario para UsuarioDto
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.Cpf != null ? src.Cpf.Valor : null))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UsuarioRoles.Select(ur => ur.Role).ToList()));
        
        // Mapeamento de CriarUsuarioDto para Usuario
        CreateMap<CriarUsuarioDto, Usuario>()
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.Cpf) ? new Cpf(src.Cpf) : null))
            .ForMember(dest => dest.UsuarioRoles, opt => opt.Ignore()) // Será tratado separadamente
            .ConstructUsing(src => new Usuario(src.Nome, src.Email, src.Celular, 
                !string.IsNullOrWhiteSpace(src.Cpf) ? new Cpf(src.Cpf) : null));
        
        // Mapeamento de AtualizarUsuarioDto (não cria instância, apenas para referência)
        CreateMap<AtualizarUsuarioDto, Usuario>()
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.Cpf) ? new Cpf(src.Cpf) : null))
            .ForAllMembers(opt => opt.Ignore()); // Será aplicado manualmente no serviço
    }
}