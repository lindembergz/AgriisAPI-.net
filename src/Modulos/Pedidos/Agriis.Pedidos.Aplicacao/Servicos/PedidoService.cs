using Agriis.Pedidos.Aplicacao.DTOs;
using Agriis.Pedidos.Aplicacao.Interfaces;
using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Enums;
using Agriis.Pedidos.Dominio.Interfaces;
using Agriis.Pedidos.Dominio.Servicos;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Compartilhado.Dominio.Interfaces;
using AutoMapper;

namespace Agriis.Pedidos.Aplicacao.Servicos;

/// <summary>
/// Implementação do serviço de aplicação de pedidos
/// </summary>
public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IPedidoItemRepository _pedidoItemRepository;
    private readonly CarrinhoComprasService _carrinhoService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PedidoService(
        IPedidoRepository pedidoRepository,
        IPedidoItemRepository pedidoItemRepository,
        CarrinhoComprasService carrinhoService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _pedidoRepository = pedidoRepository;
        _pedidoItemRepository = pedidoItemRepository;
        _carrinhoService = carrinhoService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PedidoDto?> ObterPorIdAsync(int id)
    {
        var pedido = await _pedidoRepository.ObterPorIdAsync(id);
        return pedido != null ? _mapper.Map<PedidoDto>(pedido) : null;
    }

    public async Task<PedidoDto?> ObterComItensAsync(int id)
    {
        var pedido = await _pedidoRepository.ObterComItensAsync(id);
        return pedido != null ? _mapper.Map<PedidoDto>(pedido) : null;
    }

    public async Task<IEnumerable<PedidoDto>> ObterPorProdutorAsync(int produtorId)
    {
        var pedidos = await _pedidoRepository.ObterPorProdutorAsync(produtorId);
        return _mapper.Map<IEnumerable<PedidoDto>>(pedidos);
    }

    public async Task<IEnumerable<PedidoDto>> ObterPorFornecedorAsync(int fornecedorId)
    {
        var pedidos = await _pedidoRepository.ObterPorFornecedorAsync(fornecedorId);
        return _mapper.Map<IEnumerable<PedidoDto>>(pedidos);
    }

    public async Task<IEnumerable<PedidoDto>> ObterPorStatusAsync(StatusPedido status)
    {
        var pedidos = await _pedidoRepository.ObterPorStatusAsync(status);
        return _mapper.Map<IEnumerable<PedidoDto>>(pedidos);
    }

    public async Task<PedidoDto> CriarAsync(CriarPedidoDto dto)
    {
        var pedido = new Pedido(
            dto.FornecedorId,
            dto.ProdutorId,
            dto.PermiteContato,
            dto.NegociarPedido,
            dto.DiasLimiteInteracao);

        await _pedidoRepository.AdicionarAsync(pedido);
        await _unitOfWork.SalvarAlteracoesAsync();

        return _mapper.Map<PedidoDto>(pedido);
    }

    public async Task<PedidoDto?> AtualizarAsync(int id, AtualizarPedidoDto dto)
    {
        var pedido = await _pedidoRepository.ObterPorIdAsync(id);
        if (pedido == null)
            return null;

        if (dto.PermiteContato.HasValue)
        {
            // TODO: Implementar método na entidade para atualizar PermiteContato
        }

        if (dto.NegociarPedido.HasValue)
        {
            // TODO: Implementar método na entidade para atualizar NegociarPedido
        }

        if (dto.Totais != null)
        {
            pedido.AtualizarTotais(dto.Totais);
        }

        await _unitOfWork.SalvarAlteracoesAsync();
        return _mapper.Map<PedidoDto>(pedido);
    }

    public async Task<PedidoDto?> FecharPedidoAsync(int id)
    {
        var pedido = await _pedidoRepository.ObterPorIdAsync(id);
        if (pedido == null)
            return null;

        pedido.FecharPedido();
        await _unitOfWork.SalvarAlteracoesAsync();

        return _mapper.Map<PedidoDto>(pedido);
    }

    public async Task<PedidoDto?> CancelarPorCompradorAsync(int id)
    {
        var pedido = await _pedidoRepository.ObterPorIdAsync(id);
        if (pedido == null)
            return null;

        pedido.CancelarPorComprador();
        await _unitOfWork.SalvarAlteracoesAsync();

        return _mapper.Map<PedidoDto>(pedido);
    }

    public async Task<IEnumerable<PedidoDto>> ObterProximosPrazoLimiteAsync(int diasAntes = 1)
    {
        var pedidos = await _pedidoRepository.ObterProximosPrazoLimiteAsync(diasAntes);
        return _mapper.Map<IEnumerable<PedidoDto>>(pedidos);
    }

    public async Task<IEnumerable<PedidoDto>> ObterComPrazoUltrapassadoAsync()
    {
        var pedidos = await _pedidoRepository.ObterComPrazoUltrapassadoAsync();
        return _mapper.Map<IEnumerable<PedidoDto>>(pedidos);
    }

    public async Task<int> CancelarPedidosComPrazoUltrapassadoAsync()
    {
        var pedidos = await _pedidoRepository.ObterComPrazoUltrapassadoAsync();
        var contador = 0;

        foreach (var pedido in pedidos)
        {
            if (pedido.Status == StatusPedido.EmNegociacao)
            {
                pedido.CancelarPorTempoLimite();
                contador++;
            }
        }

        if (contador > 0)
        {
            await _unitOfWork.SalvarAlteracoesAsync();
        }

        return contador;
    }

    /// <summary>
    /// Adiciona um item ao carrinho de compras
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="dto">Dados do item</param>
    /// <param name="catalogoId">ID do catálogo para consulta de preços</param>
    /// <returns>Item adicionado</returns>
    public async Task<PedidoItemDto> AdicionarItemCarrinhoAsync(int pedidoId, CriarPedidoItemDto dto, int catalogoId)
    {
        var pedido = await _pedidoRepository.ObterComItensAsync(pedidoId);
        if (pedido == null)
            throw new InvalidOperationException($"Pedido com ID {pedidoId} não encontrado");

        if (!_carrinhoService.VerificarPrazoLimite(pedido))
            throw new InvalidOperationException("Pedido fora do prazo limite para modificações");

        var item = await _carrinhoService.AdicionarItemAsync(
            pedido, 
            dto.ProdutoId, 
            dto.Quantidade, 
            catalogoId, 
            dto.Observacoes);

        pedido.AdicionarItem(item);

        // Recalcular totais
        var totais = _carrinhoService.CalcularTotais(pedido);
        pedido.AtualizarTotais(totais.ToJsonDocument());

        await _unitOfWork.SalvarAlteracoesAsync();

        return _mapper.Map<PedidoItemDto>(item);
    }

    /// <summary>
    /// Remove um item do carrinho de compras
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="itemId">ID do item</param>
    public async Task RemoverItemCarrinhoAsync(int pedidoId, int itemId)
    {
        var pedido = await _pedidoRepository.ObterComItensAsync(pedidoId);
        if (pedido == null)
            throw new InvalidOperationException($"Pedido com ID {pedidoId} não encontrado");

        if (!_carrinhoService.VerificarPrazoLimite(pedido))
            throw new InvalidOperationException("Pedido fora do prazo limite para modificações");

        pedido.RemoverItem(itemId);

        // Recalcular totais
        var totais = _carrinhoService.CalcularTotais(pedido);
        pedido.AtualizarTotais(totais.ToJsonDocument());

        await _unitOfWork.SalvarAlteracoesAsync();
    }

    /// <summary>
    /// Atualiza a quantidade de um item no carrinho
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="itemId">ID do item</param>
    /// <param name="novaQuantidade">Nova quantidade</param>
    public async Task<PedidoItemDto> AtualizarQuantidadeItemAsync(int pedidoId, int itemId, decimal novaQuantidade)
    {
        var pedido = await _pedidoRepository.ObterComItensAsync(pedidoId);
        if (pedido == null)
            throw new InvalidOperationException($"Pedido com ID {pedidoId} não encontrado");

        if (!_carrinhoService.VerificarPrazoLimite(pedido))
            throw new InvalidOperationException("Pedido fora do prazo limite para modificações");

        var item = pedido.Itens.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException($"Item com ID {itemId} não encontrado no pedido");

        await _carrinhoService.AtualizarQuantidadeItemAsync(item, novaQuantidade, pedido);

        // Recalcular totais
        var totais = _carrinhoService.CalcularTotais(pedido);
        pedido.AtualizarTotais(totais.ToJsonDocument());

        await _unitOfWork.SalvarAlteracoesAsync();

        return _mapper.Map<PedidoItemDto>(item);
    }

    /// <summary>
    /// Recalcula todos os totais do pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Pedido com totais atualizados</returns>
    public async Task<PedidoDto> RecalcularTotaisAsync(int pedidoId)
    {
        var pedido = await _pedidoRepository.ObterComItensAsync(pedidoId);
        if (pedido == null)
            throw new InvalidOperationException($"Pedido com ID {pedidoId} não encontrado");

        var totais = _carrinhoService.CalcularTotais(pedido);
        pedido.AtualizarTotais(totais.ToJsonDocument());

        await _unitOfWork.SalvarAlteracoesAsync();

        return _mapper.Map<PedidoDto>(pedido);
    }

    /// <summary>
    /// Atualiza o prazo limite de interação do pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="novosDias">Novos dias a partir de agora</param>
    public async Task<PedidoDto> AtualizarPrazoLimiteAsync(int pedidoId, int novosDias)
    {
        var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId);
        if (pedido == null)
            throw new InvalidOperationException($"Pedido com ID {pedidoId} não encontrado");

        pedido.AtualizarPrazoLimite(novosDias);
        await _unitOfWork.SalvarAlteracoesAsync();

        return _mapper.Map<PedidoDto>(pedido);
    }
}