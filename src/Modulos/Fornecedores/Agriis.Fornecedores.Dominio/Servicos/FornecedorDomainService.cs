using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Fornecedores.Dominio.Entidades;
using Agriis.Fornecedores.Dominio.Interfaces;

namespace Agriis.Fornecedores.Dominio.Servicos;

/// <summary>
/// Serviço de domínio para regras de negócio relacionadas a fornecedores
/// </summary>
public class FornecedorDomainService
{
    private readonly IFornecedorRepository _fornecedorRepository;
    private readonly IUsuarioFornecedorRepository _usuarioFornecedorRepository;
    private readonly IUsuarioFornecedorTerritorioRepository _territorioRepository;

    public FornecedorDomainService(
        IFornecedorRepository fornecedorRepository,
        IUsuarioFornecedorRepository usuarioFornecedorRepository,
        IUsuarioFornecedorTerritorioRepository territorioRepository)
    {
        _fornecedorRepository = fornecedorRepository ?? throw new ArgumentNullException(nameof(fornecedorRepository));
        _usuarioFornecedorRepository = usuarioFornecedorRepository ?? throw new ArgumentNullException(nameof(usuarioFornecedorRepository));
        _territorioRepository = territorioRepository ?? throw new ArgumentNullException(nameof(territorioRepository));
    }

    /// <summary>
    /// Valida se um CNPJ pode ser usado para um fornecedor
    /// </summary>
    /// <param name="cnpj">CNPJ a validar</param>
    /// <param name="fornecedorIdExcluir">ID do fornecedor a excluir da validação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o CNPJ é válido e disponível</returns>
    public async Task<bool> ValidarCnpjDisponivelAsync(Cnpj cnpj, int? fornecedorIdExcluir = null, CancellationToken cancellationToken = default)
    {
        if (cnpj == null)
            return false;

        return !await _fornecedorRepository.ExisteCnpjAsync(cnpj.Valor, fornecedorIdExcluir, cancellationToken);
    }

    /// <summary>
    /// Verifica se um usuário pode ser associado a um fornecedor
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se pode ser associado</returns>
    public async Task<bool> PodeAssociarUsuarioAsync(int usuarioId, int fornecedorId, CancellationToken cancellationToken = default)
    {
        // Verifica se já existe uma associação ativa
        var associacaoExistente = await _usuarioFornecedorRepository.ExisteAssociacaoAtivaAsync(usuarioId, fornecedorId, cancellationToken);
        return !associacaoExistente;
    }

    /// <summary>
    /// Obtém fornecedores que atendem um território específico
    /// </summary>
    /// <param name="uf">UF do estado</param>
    /// <param name="municipio">Nome do município (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de fornecedores que atendem o território</returns>
    public async Task<IEnumerable<Fornecedor>> ObterFornecedoresPorTerritorioAsync(string uf, string? municipio = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uf))
            return Enumerable.Empty<Fornecedor>();

        return await _fornecedorRepository.ObterPorTerritorioAsync(uf, municipio, cancellationToken);
    }
    
/// <summary>
    /// Verifica se um território pode ser definido como padrão
    /// </summary>
    /// <param name="usuarioFornecedorId">ID da associação usuário-fornecedor</param>
    /// <param name="territorioId">ID do território a ser definido como padrão</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se pode ser definido como padrão</returns>
    public async Task<bool> PodeDefinirTerritorioPadraoAsync(int usuarioFornecedorId, int territorioId, CancellationToken cancellationToken = default)
    {
        // Verifica se o território existe e pertence à associação
        var territorio = await _territorioRepository.ObterPorIdAsync(territorioId, cancellationToken);
        return territorio != null && territorio.UsuarioFornecedorId == usuarioFornecedorId && territorio.Ativo;
    }

    /// <summary>
    /// Remove o status de território padrão de outros territórios antes de definir um novo
    /// </summary>
    /// <param name="usuarioFornecedorId">ID da associação usuário-fornecedor</param>
    /// <param name="novoTerritorioPadraoId">ID do novo território padrão</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    public async Task RemoverOutrosTerritoriosPadraoAsync(int usuarioFornecedorId, int novoTerritorioPadraoId, CancellationToken cancellationToken = default)
    {
        var territorios = await _territorioRepository.ObterPorUsuarioFornecedorAsync(usuarioFornecedorId, true, cancellationToken);
        
        foreach (var territorio in territorios.Where(t => t.Id != novoTerritorioPadraoId && t.TerritorioPadrao))
        {
            territorio.DefinirTerritorioPadrao(false);
            await _territorioRepository.AtualizarAsync(territorio, cancellationToken);
        }
    }

    /// <summary>
    /// Valida se um pedido atende ao valor mínimo do fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="valorPedido">Valor total do pedido</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se atende ao valor mínimo</returns>
    public async Task<bool> ValidarPedidoMinimoAsync(int fornecedorId, decimal valorPedido, CancellationToken cancellationToken = default)
    {
        var fornecedor = await _fornecedorRepository.ObterPorIdAsync(fornecedorId, cancellationToken);
        
        if (fornecedor == null || !fornecedor.PedidoMinimo.HasValue)
            return true; // Se não há pedido mínimo definido, qualquer valor é válido
            
        return valorPedido >= fornecedor.PedidoMinimo.Value;
    }

    /// <summary>
    /// Obtém representantes comerciais de um fornecedor para um território específico
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="uf">UF do estado</param>
    /// <param name="municipio">Nome do município (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de representantes que atendem o território</returns>
    public async Task<IEnumerable<UsuarioFornecedor>> ObterRepresentantesPorTerritorioAsync(int fornecedorId, string uf, string? municipio = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uf))
            return Enumerable.Empty<UsuarioFornecedor>();

        // Obtém usuários fornecedores que atendem o território
        var usuariosFornecedores = await _territorioRepository.ObterUsuariosFornecedoresPorTerritorioAsync(uf, municipio, cancellationToken);
        
        // Filtra apenas representantes comerciais do fornecedor específico
        return usuariosFornecedores.Where(uf => uf.FornecedorId == fornecedorId && uf.EhRepresentante() && uf.Ativo);
    }
}