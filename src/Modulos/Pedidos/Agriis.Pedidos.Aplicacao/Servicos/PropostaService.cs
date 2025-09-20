using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Pedidos.Aplicacao.DTOs;
using Agriis.Pedidos.Aplicacao.Interfaces;
using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Enums;
using Agriis.Pedidos.Dominio.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Agriis.Pedidos.Aplicacao.Servicos;

/// <summary>
/// Serviço de propostas
/// </summary>
public class PropostaService : IPropostaService
{
    private readonly IPropostaRepository _propostaRepository;
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PropostaService> _logger;
    
    // Constantes para identificação de clientes
    private const string PRODUTOR_MOBILE = "PRODUTOR_MOBILE";
    private const string FORNECEDOR_WEB = "FORNECEDOR_WEB";
    
    /// <summary>
    /// Construtor do serviço de propostas
    /// </summary>
    public PropostaService(
        IPropostaRepository propostaRepository,
        IPedidoRepository pedidoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PropostaService> logger)
    {
        _propostaRepository = propostaRepository;
        _pedidoRepository = pedidoRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }
    
    /// <summary>
    /// Cria uma nova proposta
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="clientId">ID do cliente (PRODUTOR_MOBILE ou FORNECEDOR_WEB)</param>
    /// <param name="dto">Dados da proposta</param>
    /// <returns>Resultado da operação</returns>
    public async Task<Result> CriarPropostaAsync(int pedidoId, int usuarioId, string clientId, CriarPropostaDto dto)
    {
        try
        {
            // Buscar o pedido
            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId);
            if (pedido == null)
            {
                return Result.Failure("O pedido não pode ser encontrado");
            }
            
            // Validações de negócio
            if (pedido.Status == StatusPedido.CanceladoPeloComprador || pedido.Status == StatusPedido.CanceladoPorTempoLimite)
            {
                return Result.Failure("Não é possível continuar com a proposta, pois este pedido encontra-se cancelado.");
            }
            
            if (pedido.Status == StatusPedido.Fechado)
            {
                return Result.Failure("Não é possível continuar com a proposta, pois este pedido encontra-se negociado.");
            }
            
            if (clientId == FORNECEDOR_WEB && pedido.Status != StatusPedido.EmNegociacao)
            {
                return Result.Failure("A negociação só pode ser iniciada pelo comprador.");
            }
            
            await _unitOfWork.IniciarTransacaoAsync();
            
            try
            {
                if (clientId == PRODUTOR_MOBILE)
                {
                    await AlterarPropostaProdutorAsync(dto.AcaoComprador, pedido, usuarioId);
                }
                else if (clientId == FORNECEDOR_WEB)
                {
                    await AlterarPropostaFornecedorAsync(pedido, usuarioId, dto.Observacao);
                }
                else
                {
                    return Result.Failure($"Tipo de cliente não implementado: {clientId}");
                }
                
                await _unitOfWork.SalvarAlteracoesAsync();
                await _unitOfWork.ConfirmarTransacaoAsync();
                
                _logger.LogInformation("Proposta criada com sucesso para pedido {PedidoId} por usuário {UsuarioId}", pedidoId, usuarioId);
                return Result.Success();
            }
            catch
            {
                await _unitOfWork.ReverterTransacaoAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar proposta para pedido {PedidoId}", pedidoId);
            return Result.Failure("Erro interno do servidor");
        }
    }
    
    /// <summary>
    /// Lista todas as propostas de um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="dto">Parâmetros de listagem</param>
    /// <returns>Lista paginada de propostas</returns>
    public async Task<Result<PagedResult<PropostaDto>>> ListarPropostasAsync(int pedidoId, ListarPropostasDto dto)
    {
        try
        {
            var propostas = await _propostaRepository.ListarPorPedidoAsync(pedidoId, dto.Page, dto.MaxPerPage, dto.Sorting);
            var propostasDto = _mapper.Map<PagedResult<PropostaDto>>(propostas);
            
            return Result<PagedResult<PropostaDto>>.Success(propostasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar propostas do pedido {PedidoId}", pedidoId);
            return Result<PagedResult<PropostaDto>>.Failure("Erro interno do servidor");
        }
    }
    
    /// <summary>
    /// Obtém a última proposta de um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Última proposta ou null</returns>
    public async Task<Result<PropostaDto?>> ObterUltimaPropostaAsync(int pedidoId)
    {
        try
        {
            var proposta = await _propostaRepository.ObterUltimaPorPedidoAsync(pedidoId);
            var propostaDto = _mapper.Map<PropostaDto?>(proposta);
            
            return Result<PropostaDto?>.Success(propostaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter última proposta do pedido {PedidoId}", pedidoId);
            return Result<PropostaDto?>.Failure("Erro interno do servidor");
        }
    }
    
    /// <summary>
    /// Altera proposta do produtor (comprador)
    /// </summary>
    private async Task AlterarPropostaProdutorAsync(AcaoCompradorPedido? acaoComprador, Pedido pedido, int usuarioId)
    {
        var ultimaProposta = await _propostaRepository.ObterUltimaPorPedidoAsync(pedido.Id);
        
        if (ultimaProposta != null && acaoComprador == null)
        {
            throw new ArgumentException("Informar uma ação.");
        }
        
        var acao = acaoComprador;
        string? observacao = null;
        
        if (ultimaProposta == null)
        {
            // Inicia proposta
            pedido.AtualizarStatus(StatusPedido.EmNegociacao);
            acao = AcaoCompradorPedido.Iniciou;
            observacao = "Iniciou a negociação";
        }
        else if (acaoComprador == AcaoCompradorPedido.Aceitou)
        {
            pedido.FecharPedido();
        }
        else if (acaoComprador == AcaoCompradorPedido.Cancelou)
        {
            pedido.CancelarPorComprador();
        }
        
        if (acao == null)
        {
            throw new ArgumentException("Não foi possível identificar a ação informada");
        }
        
        // Evitar inserir muitos registros com a mesma ação de forma contínua
        if (ultimaProposta == null || ultimaProposta.AcaoComprador != acao)
        {
            var proposta = new Proposta(pedido.Id, acao.Value, usuarioId, observacao);
            await SalvarPropostaENotificarAsync(proposta, usuarioId, pedido.ProdutorId);
            await _pedidoRepository.AtualizarAsync(pedido);
        }
    }
    
    /// <summary>
    /// Altera proposta do fornecedor
    /// </summary>
    private async Task AlterarPropostaFornecedorAsync(Pedido pedido, int usuarioId, string? observacao)
    {
        if (string.IsNullOrWhiteSpace(observacao))
        {
            throw new ArgumentException("Informar uma observação");
        }
        
        var proposta = new Proposta(pedido.Id, observacao, usuarioId);
        await SalvarPropostaENotificarAsync(proposta, usuarioId, null, pedido.FornecedorId);
        await _pedidoRepository.AtualizarAsync(pedido);
    }
    
    /// <summary>
    /// Salva proposta e envia notificação
    /// </summary>
    private async Task SalvarPropostaENotificarAsync(Proposta proposta, int usuarioId, int? produtorId = null, int? fornecedorId = null)
    {
        // TODO: Implementar validação de permissões do usuário
        // TODO: Implementar notificações por email/push
        
        await _propostaRepository.AdicionarAsync(proposta);
        
        _logger.LogInformation("Proposta salva para pedido {PedidoId} por usuário {UsuarioId}", proposta.PedidoId, usuarioId);
    }
}