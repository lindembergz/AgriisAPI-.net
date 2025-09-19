using Agriis.Usuarios.Aplicacao.DTOs;
using Agriis.Compartilhado.Dominio.Enums;

namespace Agriis.Usuarios.Aplicacao.Interfaces;

/// <summary>
/// Interface do serviço de aplicação de usuários
/// </summary>
public interface IUsuarioService
{
    /// <summary>
    /// Obtém um usuário por ID
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>DTO do usuário ou null</returns>
    Task<UsuarioDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém um usuário por email
    /// </summary>
    /// <param name="email">Email do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>DTO do usuário ou null</returns>
    Task<UsuarioDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém usuários com paginação
    /// </summary>
    /// <param name="pagina">Número da página</param>
    /// <param name="tamanhoPagina">Tamanho da página</param>
    /// <param name="filtro">Filtro de busca</param>
    /// <param name="apenasAtivos">Se deve filtrar apenas usuários ativos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado paginado</returns>
    Task<UsuariosPaginadosDto> ObterPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? filtro = null, 
        bool apenasAtivos = true, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém usuários por role
    /// </summary>
    /// <param name="role">Role a ser filtrada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de usuários</returns>
    Task<IEnumerable<UsuarioDto>> ObterPorRoleAsync(Roles role, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    /// <param name="criarUsuarioDto">Dados do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>DTO do usuário criado</returns>
    Task<UsuarioDto> CriarAsync(CriarUsuarioDto criarUsuarioDto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="atualizarUsuarioDto">Dados de atualização</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>DTO do usuário atualizado</returns>
    Task<UsuarioDto> AtualizarAsync(int id, AtualizarUsuarioDto atualizarUsuarioDto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Altera o email de um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="alterarEmailDto">Dados do novo email</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>DTO do usuário atualizado</returns>
    Task<UsuarioDto> AlterarEmailAsync(int id, AlterarEmailDto alterarEmailDto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Altera a senha de um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="alterarSenhaDto">Dados da nova senha</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task AlterarSenhaAsync(int id, AlterarSenhaDto alterarSenhaDto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gerencia as roles de um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="gerenciarRolesDto">Roles a serem atribuídas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>DTO do usuário atualizado</returns>
    Task<UsuarioDto> GerenciarRolesAsync(int id, GerenciarRolesDto gerenciarRolesDto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ativa um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>DTO do usuário atualizado</returns>
    Task<UsuarioDto> AtivarAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Desativa um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>DTO do usuário atualizado</returns>
    Task<UsuarioDto> DesativarAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registra o login de um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task RegistrarLoginAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se um email já está em uso
    /// </summary>
    /// <param name="email">Email a ser verificado</param>
    /// <param name="usuarioIdExcluir">ID do usuário a ser excluído da verificação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o email já está em uso</returns>
    Task<bool> EmailJaExisteAsync(string email, int? usuarioIdExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se um CPF já está em uso
    /// </summary>
    /// <param name="cpf">CPF a ser verificado</param>
    /// <param name="usuarioIdExcluir">ID do usuário a ser excluído da verificação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o CPF já está em uso</returns>
    Task<bool> CpfJaExisteAsync(string cpf, int? usuarioIdExcluir = null, CancellationToken cancellationToken = default);
}