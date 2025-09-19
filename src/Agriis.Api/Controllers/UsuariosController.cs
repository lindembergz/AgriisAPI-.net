using Microsoft.AspNetCore.Mvc;
using Agriis.Usuarios.Aplicacao.Interfaces;
using Agriis.Usuarios.Aplicacao.DTOs;
using Agriis.Compartilhado.Dominio.Enums;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    
    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }
    
    /// <summary>
    /// Obtém um usuário por ID
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do usuário</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> ObterPorId(int id, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioService.ObterPorIdAsync(id, cancellationToken);
        
        if (usuario == null)
        {
            return NotFound(new { error_code = "USUARIO_NAO_ENCONTRADO", error_description = "Usuário não encontrado" });
        }
        
        return Ok(usuario);
    }
    
    /// <summary>
    /// Obtém um usuário por email
    /// </summary>
    /// <param name="email">Email do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do usuário</returns>
    [HttpGet("email/{email}")]
    public async Task<ActionResult<UsuarioDto>> ObterPorEmail(string email, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioService.ObterPorEmailAsync(email, cancellationToken);
        
        if (usuario == null)
        {
            return NotFound(new { error_code = "USUARIO_NAO_ENCONTRADO", error_description = "Usuário não encontrado" });
        }
        
        return Ok(usuario);
    }
    
    /// <summary>
    /// Obtém usuários com paginação
    /// </summary>
    /// <param name="pagina">Número da página (padrão: 1)</param>
    /// <param name="tamanhoPagina">Tamanho da página (padrão: 20)</param>
    /// <param name="filtro">Filtro de busca por nome ou email</param>
    /// <param name="apenasAtivos">Se deve filtrar apenas usuários ativos (padrão: true)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de usuários</returns>
    [HttpGet]
    public async Task<ActionResult<UsuariosPaginadosDto>> ObterPaginado(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        [FromQuery] string? filtro = null,
        [FromQuery] bool apenasAtivos = true,
        CancellationToken cancellationToken = default)
    {
        if (pagina < 1) pagina = 1;
        if (tamanhoPagina < 1 || tamanhoPagina > 100) tamanhoPagina = 20;
        
        var resultado = await _usuarioService.ObterPaginadoAsync(pagina, tamanhoPagina, filtro, apenasAtivos, cancellationToken);
        return Ok(resultado);
    }
    
    /// <summary>
    /// Obtém usuários por role
    /// </summary>
    /// <param name="role">Role a ser filtrada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de usuários</returns>
    [HttpGet("role/{role}")]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> ObterPorRole(Roles role, CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioService.ObterPorRoleAsync(role, cancellationToken);
        return Ok(usuarios);
    }
    
    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    /// <param name="criarUsuarioDto">Dados do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do usuário criado</returns>
    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Criar([FromBody] CriarUsuarioDto criarUsuarioDto, CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await _usuarioService.CriarAsync(criarUsuarioDto, cancellationToken);
            return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error_code = "DADOS_INVALIDOS", error_description = ex.Message });
        }
    }
    
    /// <summary>
    /// Atualiza um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="atualizarUsuarioDto">Dados de atualização</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do usuário atualizado</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Atualizar(int id, [FromBody] AtualizarUsuarioDto atualizarUsuarioDto, CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await _usuarioService.AtualizarAsync(id, atualizarUsuarioDto, cancellationToken);
            return Ok(usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error_code = "DADOS_INVALIDOS", error_description = ex.Message });
        }
    }
    
    /// <summary>
    /// Altera o email de um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="alterarEmailDto">Dados do novo email</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do usuário atualizado</returns>
    [HttpPut("{id:int}/email")]
    public async Task<ActionResult<UsuarioDto>> AlterarEmail(int id, [FromBody] AlterarEmailDto alterarEmailDto, CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await _usuarioService.AlterarEmailAsync(id, alterarEmailDto, cancellationToken);
            return Ok(usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error_code = "DADOS_INVALIDOS", error_description = ex.Message });
        }
    }
    
    /// <summary>
    /// Altera a senha de um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="alterarSenhaDto">Dados da nova senha</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Confirmação da alteração</returns>
    [HttpPut("{id:int}/senha")]
    public async Task<ActionResult> AlterarSenha(int id, [FromBody] AlterarSenhaDto alterarSenhaDto, CancellationToken cancellationToken)
    {
        try
        {
            await _usuarioService.AlterarSenhaAsync(id, alterarSenhaDto, cancellationToken);
            return Ok(new { message = "Senha alterada com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error_code = "DADOS_INVALIDOS", error_description = ex.Message });
        }
    }
    
    /// <summary>
    /// Gerencia as roles de um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="gerenciarRolesDto">Roles a serem atribuídas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do usuário atualizado</returns>
    [HttpPut("{id:int}/roles")]
    public async Task<ActionResult<UsuarioDto>> GerenciarRoles(int id, [FromBody] GerenciarRolesDto gerenciarRolesDto, CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await _usuarioService.GerenciarRolesAsync(id, gerenciarRolesDto, cancellationToken);
            return Ok(usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error_code = "DADOS_INVALIDOS", error_description = ex.Message });
        }
    }
    
    /// <summary>
    /// Ativa um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do usuário atualizado</returns>
    [HttpPut("{id:int}/ativar")]
    public async Task<ActionResult<UsuarioDto>> Ativar(int id, CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await _usuarioService.AtivarAsync(id, cancellationToken);
            return Ok(usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error_code = "DADOS_INVALIDOS", error_description = ex.Message });
        }
    }
    
    /// <summary>
    /// Desativa um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do usuário atualizado</returns>
    [HttpPut("{id:int}/desativar")]
    public async Task<ActionResult<UsuarioDto>> Desativar(int id, CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await _usuarioService.DesativarAsync(id, cancellationToken);
            return Ok(usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error_code = "DADOS_INVALIDOS", error_description = ex.Message });
        }
    }
    
    /// <summary>
    /// Remove um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Confirmação da remoção</returns>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _usuarioService.RemoverAsync(id, cancellationToken);
            return Ok(new { message = "Usuário removido com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error_code = "DADOS_INVALIDOS", error_description = ex.Message });
        }
    }
    
    /// <summary>
    /// Registra o login de um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Confirmação do registro</returns>
    [HttpPost("{id:int}/login")]
    public async Task<ActionResult> RegistrarLogin(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _usuarioService.RegistrarLoginAsync(id, cancellationToken);
            return Ok(new { message = "Login registrado com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error_code = "DADOS_INVALIDOS", error_description = ex.Message });
        }
    }
    
    /// <summary>
    /// Verifica se um email já está em uso
    /// </summary>
    /// <param name="email">Email a ser verificado</param>
    /// <param name="usuarioIdExcluir">ID do usuário a ser excluído da verificação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da verificação</returns>
    [HttpGet("verificar-email")]
    public async Task<ActionResult<bool>> VerificarEmail(
        [FromQuery] string email,
        [FromQuery] int? usuarioIdExcluir = null,
        CancellationToken cancellationToken = default)
    {
        var existe = await _usuarioService.EmailJaExisteAsync(email, usuarioIdExcluir, cancellationToken);
        return Ok(new { existe });
    }
    
    /// <summary>
    /// Verifica se um CPF já está em uso
    /// </summary>
    /// <param name="cpf">CPF a ser verificado</param>
    /// <param name="usuarioIdExcluir">ID do usuário a ser excluído da verificação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da verificação</returns>
    [HttpGet("verificar-cpf")]
    public async Task<ActionResult<bool>> VerificarCpf(
        [FromQuery] string cpf,
        [FromQuery] int? usuarioIdExcluir = null,
        CancellationToken cancellationToken = default)
    {
        var existe = await _usuarioService.CpfJaExisteAsync(cpf, usuarioIdExcluir, cancellationToken);
        return Ok(new { existe });
    }
}