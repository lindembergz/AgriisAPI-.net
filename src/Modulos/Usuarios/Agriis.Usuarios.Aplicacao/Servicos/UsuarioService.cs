using AutoMapper;
using FluentValidation;
using Agriis.Usuarios.Aplicacao.DTOs;
using Agriis.Usuarios.Aplicacao.Interfaces;
using Agriis.Usuarios.Dominio.Entidades;
using Agriis.Usuarios.Dominio.Interfaces;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Compartilhado.Dominio.Interfaces;

namespace Agriis.Usuarios.Aplicacao.Servicos;

/// <summary>
/// Serviço de aplicação para usuários
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CriarUsuarioDto> _criarUsuarioValidator;
    private readonly IValidator<AtualizarUsuarioDto> _atualizarUsuarioValidator;
    private readonly IValidator<AlterarEmailDto> _alterarEmailValidator;
    private readonly IValidator<AlterarSenhaDto> _alterarSenhaValidator;
    
    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CriarUsuarioDto> criarUsuarioValidator,
        IValidator<AtualizarUsuarioDto> atualizarUsuarioValidator,
        IValidator<AlterarEmailDto> alterarEmailValidator,
        IValidator<AlterarSenhaDto> alterarSenhaValidator)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _criarUsuarioValidator = criarUsuarioValidator;
        _atualizarUsuarioValidator = atualizarUsuarioValidator;
        _alterarEmailValidator = alterarEmailValidator;
        _alterarSenhaValidator = alterarSenhaValidator;
    }
    
    public async Task<UsuarioDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterComRolesAsync(id, cancellationToken);
        return usuario != null ? _mapper.Map<UsuarioDto>(usuario) : null;
    }
    
    public async Task<UsuarioDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorEmailComRolesAsync(email, cancellationToken);
        return usuario != null ? _mapper.Map<UsuarioDto>(usuario) : null;
    }
    
    public async Task<UsuariosPaginadosDto> ObterPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? filtro = null, 
        bool apenasAtivos = true, 
        CancellationToken cancellationToken = default)
    {
        var (usuarios, total) = await _usuarioRepository.ObterPaginadoAsync(
            pagina, tamanhoPagina, filtro, apenasAtivos, cancellationToken);
        
        return new UsuariosPaginadosDto
        {
            Usuarios = _mapper.Map<List<UsuarioDto>>(usuarios),
            Total = total,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        };
    }
    
    public async Task<IEnumerable<UsuarioDto>> ObterPorRoleAsync(Roles role, CancellationToken cancellationToken = default)
    {
        var usuarios = await _usuarioRepository.ObterPorRoleAsync(role, cancellationToken);
        return _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);
    }
    
    public async Task<UsuarioDto> CriarAsync(CriarUsuarioDto criarUsuarioDto, CancellationToken cancellationToken = default)
    {
        // Validar DTO
        await _criarUsuarioValidator.ValidateAndThrowAsync(criarUsuarioDto, cancellationToken);
        
        // Verificar se email já existe
        if (await _usuarioRepository.ExisteEmailAsync(criarUsuarioDto.Email, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Email já está em uso");
        }
        
        // Verificar se CPF já existe (se fornecido)
        if (!string.IsNullOrWhiteSpace(criarUsuarioDto.Cpf) && 
            await _usuarioRepository.ExisteCpfAsync(criarUsuarioDto.Cpf, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("CPF já está em uso");
        }
        
        // Criar usuário
        var usuario = new Usuario(
            criarUsuarioDto.Nome,
            criarUsuarioDto.Email,
            criarUsuarioDto.Celular,
            !string.IsNullOrWhiteSpace(criarUsuarioDto.Cpf) ? new Cpf(criarUsuarioDto.Cpf) : null);
        
        // Definir senha se fornecida
        if (!string.IsNullOrWhiteSpace(criarUsuarioDto.Senha))
        {
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(criarUsuarioDto.Senha);
            usuario.DefinirSenha(senhaHash);
        }
        
        // Adicionar roles
        foreach (var role in criarUsuarioDto.Roles)
        {
            usuario.AdicionarRole(role);
        }
        
        // Salvar
        await _usuarioRepository.AdicionarAsync(usuario, cancellationToken);
        await _unitOfWork.SalvarAlteracoesAsync();
        
        return _mapper.Map<UsuarioDto>(usuario);
    }
    
    public async Task<UsuarioDto> AtualizarAsync(int id, AtualizarUsuarioDto atualizarUsuarioDto, CancellationToken cancellationToken = default)
    {
        // Validar DTO
        await _atualizarUsuarioValidator.ValidateAndThrowAsync(atualizarUsuarioDto, cancellationToken);
        
        // Obter usuário
        var usuario = await _usuarioRepository.ObterPorIdAsync(id, cancellationToken);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }
        
        // Verificar se CPF já existe (se fornecido e diferente do atual)
        if (!string.IsNullOrWhiteSpace(atualizarUsuarioDto.Cpf) && 
            await _usuarioRepository.ExisteCpfAsync(atualizarUsuarioDto.Cpf, id, cancellationToken))
        {
            throw new InvalidOperationException("CPF já está em uso");
        }
        
        // Atualizar dados
        var cpf = !string.IsNullOrWhiteSpace(atualizarUsuarioDto.Cpf) ? new Cpf(atualizarUsuarioDto.Cpf) : null;
        usuario.AtualizarDados(atualizarUsuarioDto.Nome, atualizarUsuarioDto.Celular, cpf);
        
        // Atualizar logo se fornecida
        if (atualizarUsuarioDto.LogoUrl != null)
        {
            usuario.AtualizarLogo(atualizarUsuarioDto.LogoUrl);
        }
        
        // Salvar
        await _usuarioRepository.AtualizarAsync(usuario, cancellationToken);
        await _unitOfWork.SalvarAlteracoesAsync();
        
        return _mapper.Map<UsuarioDto>(usuario);
    }
    
    public async Task<UsuarioDto> AlterarEmailAsync(int id, AlterarEmailDto alterarEmailDto, CancellationToken cancellationToken = default)
    {
        // Validar DTO
        await _alterarEmailValidator.ValidateAndThrowAsync(alterarEmailDto, cancellationToken);
        
        // Obter usuário
        var usuario = await _usuarioRepository.ObterPorIdAsync(id, cancellationToken);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }
        
        // Verificar se email já existe
        if (await _usuarioRepository.ExisteEmailAsync(alterarEmailDto.NovoEmail, id, cancellationToken))
        {
            throw new InvalidOperationException("Email já está em uso");
        }
        
        // Atualizar email
        usuario.AtualizarEmail(alterarEmailDto.NovoEmail);
        
        // Salvar
        await _usuarioRepository.AtualizarAsync(usuario, cancellationToken);
        await _unitOfWork.SalvarAlteracoesAsync();
        
        return _mapper.Map<UsuarioDto>(usuario);
    }
    
    public async Task AlterarSenhaAsync(int id, AlterarSenhaDto alterarSenhaDto, CancellationToken cancellationToken = default)
    {
        // Validar DTO
        await _alterarSenhaValidator.ValidateAndThrowAsync(alterarSenhaDto, cancellationToken);
        
        // Obter usuário
        var usuario = await _usuarioRepository.ObterPorIdAsync(id, cancellationToken);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }
        
        // Verificar senha atual
        if (usuario.SenhaHash == null || !BCrypt.Net.BCrypt.Verify(alterarSenhaDto.SenhaAtual, usuario.SenhaHash))
        {
            throw new InvalidOperationException("Senha atual incorreta");
        }
        
        // Definir nova senha
        var novaSenhaHash = BCrypt.Net.BCrypt.HashPassword(alterarSenhaDto.NovaSenha);
        usuario.DefinirSenha(novaSenhaHash);
        
        // Salvar
        await _usuarioRepository.AtualizarAsync(usuario, cancellationToken);
        await _unitOfWork.SalvarAlteracoesAsync();
    }
    
    public async Task<UsuarioDto> GerenciarRolesAsync(int id, GerenciarRolesDto gerenciarRolesDto, CancellationToken cancellationToken = default)
    {
        // Obter usuário
        var usuario = await _usuarioRepository.ObterComRolesAsync(id, cancellationToken);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }
        
        // Remover todas as roles atuais
        var rolesAtuais = usuario.ObterRoles().ToList();
        foreach (var role in rolesAtuais)
        {
            usuario.RemoverRole(role);
        }
        
        // Adicionar novas roles
        foreach (var role in gerenciarRolesDto.Roles)
        {
            usuario.AdicionarRole(role);
        }
        
        // Salvar
        await _usuarioRepository.AtualizarAsync(usuario, cancellationToken);
        await _unitOfWork.SalvarAlteracoesAsync();
        
        return _mapper.Map<UsuarioDto>(usuario);
    }
    
    public async Task<UsuarioDto> AtivarAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id, cancellationToken);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }
        
        usuario.Ativar();
        
        await _usuarioRepository.AtualizarAsync(usuario, cancellationToken);
        await _unitOfWork.SalvarAlteracoesAsync();
        
        return _mapper.Map<UsuarioDto>(usuario);
    }
    
    public async Task<UsuarioDto> DesativarAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id, cancellationToken);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }
        
        usuario.Desativar();
        
        await _usuarioRepository.AtualizarAsync(usuario, cancellationToken);
        await _unitOfWork.SalvarAlteracoesAsync();
        
        return _mapper.Map<UsuarioDto>(usuario);
    }
    
    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id, cancellationToken);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }
        
        await _usuarioRepository.RemoverAsync(usuario, cancellationToken);
        await _unitOfWork.SalvarAlteracoesAsync();
    }
    
    public async Task RegistrarLoginAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id, cancellationToken);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }
        
        usuario.RegistrarLogin();
        
        await _usuarioRepository.AtualizarAsync(usuario, cancellationToken);
        await _unitOfWork.SalvarAlteracoesAsync();
    }
    
    public async Task<bool> EmailJaExisteAsync(string email, int? usuarioIdExcluir = null, CancellationToken cancellationToken = default)
    {
        return await _usuarioRepository.ExisteEmailAsync(email, usuarioIdExcluir, cancellationToken);
    }
    
    public async Task<bool> CpfJaExisteAsync(string cpf, int? usuarioIdExcluir = null, CancellationToken cancellationToken = default)
    {
        return await _usuarioRepository.ExisteCpfAsync(cpf, usuarioIdExcluir, cancellationToken);
    }
}