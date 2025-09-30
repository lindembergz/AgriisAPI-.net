using Agriis.Autenticacao.Aplicacao.DTOs;
using Agriis.Autenticacao.Aplicacao.Interfaces;
using Agriis.Autenticacao.Dominio.Entidades;
using Agriis.Autenticacao.Dominio.Interfaces;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Usuarios.Dominio.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Agriis.Autenticacao.Aplicacao.Servicos;

/// <summary>
/// Serviço de autenticação
/// </summary>
public class AutenticacaoService : IAutenticacaoService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AutenticacaoService> _logger;

    public AutenticacaoService(
        IUsuarioRepository usuarioRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<AutenticacaoService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request, string? enderecoIp = null, string? userAgent = null)
    {
        try
        {
            _logger.LogInformation("Tentativa de login para email: {Email}", request.Email);

            // Buscar usuário por email
            var usuario = await _usuarioRepository.ObterPorEmailAsync(request.Email);
            if (usuario == null)
            {
                _logger.LogWarning("Tentativa de login com email inexistente: {Email}", request.Email);
                return Result<LoginResponseDto>.Failure("Email ou senha inválidos");
            }

            // Verificar se usuário está ativo
            if (!usuario.Ativo)
            {
                _logger.LogWarning("Tentativa de login com usuário inativo: {Email}", request.Email);
                return Result<LoginResponseDto>.Failure("Usuário inativo");
            }

            // Verificar senha
            if (string.IsNullOrEmpty(usuario.SenhaHash) || !_passwordService.VerificarSenha(request.Senha, usuario.SenhaHash))
            {
                _logger.LogWarning("Tentativa de login com senha incorreta para: {Email}", request.Email);
                return Result<LoginResponseDto>.Failure("Email ou senha inválidos");
            }

            // Gerar tokens
            var roles = usuario.ObterRoles();
            var accessToken = _tokenService.GerarToken(usuario.Id, usuario.Email, usuario.Nome, roles);
            var refreshTokenValue = _tokenService.GerarRefreshToken();

            // Configurar expiração do refresh token
            var refreshTokenExpirationDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationInDays", 60);
            var refreshTokenExpiration = DateTimeOffset.UtcNow.AddDays(refreshTokenExpirationDays);

            // Criar refresh token
            var refreshToken = new RefreshToken(refreshTokenValue, usuario.Id, refreshTokenExpiration, enderecoIp, userAgent);
            await _refreshTokenRepository.AdicionarAsync(refreshToken);

            // Atualizar último login
            usuario.RegistrarLogin();
            await _usuarioRepository.AtualizarAsync(usuario);

            await _unitOfWork.SalvarAlteracoesAsync();

            // Preparar resposta
            var tokenExpirationDays = _configuration.GetValue<int>("JwtSettings:TokenExpirationInDays", 100);
            var response = new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                TokenType = "Bearer",
                ExpiresIn = (int)TimeSpan.FromDays(tokenExpirationDays).TotalSeconds,
                Usuario = new UsuarioLogadoDto
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Roles = roles.Select(r => r.ToString()),
                    LogoUrl = usuario.LogoUrl,
                    UltimoLogin = usuario.UltimoLogin
                }
            };

            _logger.LogInformation("Login realizado com sucesso para: {Email}", request.Email);
            return Result<LoginResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar login para: {Email}", request.Email);
            return Result<LoginResponseDto>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request, string? enderecoIp = null, string? userAgent = null)
    {
        try
        {
            _logger.LogInformation("Tentativa de renovação de token");

            // Validar refresh token
            var refreshToken = await _refreshTokenRepository.ObterPorTokenAsync(request.RefreshToken);
            if (refreshToken == null || !refreshToken.EstaValido())
            {
                _logger.LogWarning("Tentativa de renovação com refresh token inválido");
                return Result<LoginResponseDto>.Failure("Refresh token inválido ou expirado");
            }

            // Buscar usuário
            var usuario = await _usuarioRepository.ObterPorIdAsync(refreshToken.UsuarioId);
            if (usuario == null || !usuario.Ativo)
            {
                _logger.LogWarning("Tentativa de renovação para usuário inexistente ou inativo: {UsuarioId}", refreshToken.UsuarioId);
                return Result<LoginResponseDto>.Failure("Usuário inválido");
            }

            // Revogar refresh token atual
            refreshToken.Revogar();
            await _refreshTokenRepository.AtualizarAsync(refreshToken);

            // Gerar novos tokens
            var roles = usuario.ObterRoles();
            var accessToken = _tokenService.GerarToken(usuario.Id, usuario.Email, usuario.Nome, roles);
            var newRefreshTokenValue = _tokenService.GerarRefreshToken();

            // Configurar expiração do novo refresh token
            var refreshTokenExpirationDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationInDays", 60);
            var refreshTokenExpiration = DateTimeOffset.UtcNow.AddDays(refreshTokenExpirationDays);

            // Criar novo refresh token
            var newRefreshToken = new RefreshToken(newRefreshTokenValue, usuario.Id, refreshTokenExpiration, enderecoIp, userAgent);
            await _refreshTokenRepository.AdicionarAsync(newRefreshToken);

            await _unitOfWork.SalvarAlteracoesAsync();

            // Preparar resposta
            var tokenExpirationDays = _configuration.GetValue<int>("JwtSettings:TokenExpirationInDays", 100);
            var response = new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshTokenValue,
                TokenType = "Bearer",
                ExpiresIn = (int)TimeSpan.FromDays(tokenExpirationDays).TotalSeconds,
                Usuario = new UsuarioLogadoDto
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Roles = roles.Select(r => r.ToString()),
                    LogoUrl = usuario.LogoUrl,
                    UltimoLogin = usuario.UltimoLogin
                }
            };

            _logger.LogInformation("Token renovado com sucesso para usuário: {UsuarioId}", usuario.Id);
            return Result<LoginResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renovar token");
            return Result<LoginResponseDto>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<bool>> LogoutAsync(int usuarioId, string? refreshToken = null)
    {
        try
        {
            _logger.LogInformation("Logout para usuário: {UsuarioId}", usuarioId);

            if (!string.IsNullOrEmpty(refreshToken))
            {
                // Revogar refresh token específico
                var token = await _refreshTokenRepository.ObterPorTokenAsync(refreshToken);
                if (token != null && token.UsuarioId == usuarioId)
                {
                    token.Revogar();
                    await _refreshTokenRepository.AtualizarAsync(token);
                }
            }
            else
            {
                // Revogar todos os refresh tokens do usuário
                await _refreshTokenRepository.RevogarTodosTokensUsuarioAsync(usuarioId);
            }

            await _unitOfWork.SalvarAlteracoesAsync();

            _logger.LogInformation("Logout realizado com sucesso para usuário: {UsuarioId}", usuarioId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar logout para usuário: {UsuarioId}", usuarioId);
            return Result<bool>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<bool>> AlterarSenhaAsync(int usuarioId, AlterarSenhaRequestDto request)
    {
        try
        {
            _logger.LogInformation("Alteração de senha para usuário: {UsuarioId}", usuarioId);

            // Buscar usuário
            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
            if (usuario == null)
            {
                return Result<bool>.Failure("Usuário não encontrado");
            }

            // Verificar senha atual
            if (string.IsNullOrEmpty(usuario.SenhaHash) || !_passwordService.VerificarSenha(request.SenhaAtual, usuario.SenhaHash))
            {
                _logger.LogWarning("Tentativa de alteração de senha com senha atual incorreta para usuário: {UsuarioId}", usuarioId);
                return Result<bool>.Failure("Senha atual incorreta");
            }

            // Validar força da nova senha
            if (!_passwordService.ValidarForcaSenha(request.NovaSenha))
            {
                return Result<bool>.Failure("Nova senha não atende aos critérios de segurança");
            }

            // Gerar hash da nova senha
            var novoHash = _passwordService.GerarHash(request.NovaSenha);
            usuario.DefinirSenha(novoHash);

            await _usuarioRepository.AtualizarAsync(usuario);

            // Revogar todos os refresh tokens para forçar novo login
            await _refreshTokenRepository.RevogarTodosTokensUsuarioAsync(usuarioId);

            await _unitOfWork.SalvarAlteracoesAsync();

            _logger.LogInformation("Senha alterada com sucesso para usuário: {UsuarioId}", usuarioId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar senha para usuário: {UsuarioId}", usuarioId);
            return Result<bool>.Failure("Erro interno do servidor");
        }
    }

    public async Task<bool> ValidarTokenAsync(string token)
    {
        try
        {
            var principal = _tokenService.ValidarToken(token);
            return principal != null && !_tokenService.TokenExpirou(token);
        }
        catch
        {
            return false;
        }
    }

    public async Task<Result<bool>> RevogarTodosTokensAsync(int usuarioId)
    {
        try
        {
            _logger.LogInformation("Revogando todos os tokens para usuário: {UsuarioId}", usuarioId);

            await _refreshTokenRepository.RevogarTodosTokensUsuarioAsync(usuarioId);
            await _unitOfWork.SalvarAlteracoesAsync();

            _logger.LogInformation("Todos os tokens revogados para usuário: {UsuarioId}", usuarioId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao revogar tokens para usuário: {UsuarioId}", usuarioId);
            return Result<bool>.Failure("Erro interno do servidor");
        }
    }
}