using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Interfaces;
using Agriis.Pedidos.Dominio.Enums;
using Agriis.Segmentacoes.Dominio.Servicos;
using Agriis.Produtos.Aplicacao.Interfaces;
using Agriis.Produtores.Aplicacao.Interfaces;
using Agriis.Catalogos.Aplicacao.Interfaces;
using System.Text.Json;

namespace Agriis.Pedidos.Dominio.Servicos;

/// <summary>
/// Serviço de domínio para lógica de carrinho de compras
/// </summary>
public class CarrinhoComprasService
{
    private readonly CalculoDescontoSegmentadoService _calculoDescontoService;
    private readonly IProdutoService _produtoService;
    private readonly IProdutorService _produtorService;
    private readonly ICatalogoService _catalogoService;
    private readonly IPropostaRepository _propostaRepository;

    public CarrinhoComprasService(
        CalculoDescontoSegmentadoService calculoDescontoService,
        IProdutoService produtoService,
        IProdutorService produtorService,
        ICatalogoService catalogoService,
        IPropostaRepository propostaRepository)
    {
        _calculoDescontoService = calculoDescontoService;
        _produtoService = produtoService;
        _produtorService = produtorService;
        _catalogoService = catalogoService;
        _propostaRepository = propostaRepository;
    }

    /// <summary>
    /// Adiciona um item ao carrinho com cálculo de preços e descontos
    /// </summary>
    /// <param name="pedido">Pedido (carrinho)</param>
    /// <param name="produtoId">ID do produto</param>
    /// <param name="quantidade">Quantidade solicitada</param>
    /// <param name="catalogoId">ID do catálogo para consulta de preços</param>
    /// <param name="observacoes">Observações opcionais</param>
    /// <returns>Item criado com preços calculados</returns>
    public async Task<PedidoItem> AdicionarItemAsync(
        Pedido pedido, 
        int produtoId, 
        decimal quantidade, 
        int catalogoId,
        string? observacoes = null)
    {
        // Validar se o produto existe e está ativo
        var produto = await _produtoService.ObterPorIdAsync(produtoId);
        if (produto == null)
            throw new InvalidOperationException($"Produto com ID {produtoId} não encontrado");

        // Validar quantidade mínima
        await ValidarQuantidadeMinimaAsync(produtoId, quantidade);

        // Obter preço do catálogo
        var precoResult = await _catalogoService.ConsultarPrecoAsync(catalogoId, produtoId, new Catalogos.Aplicacao.DTOs.ConsultarPrecoDto
        {
            Data = DateTime.UtcNow,
            Uf = string.Empty // TODO: Obter do endereço do produtor se necessário
        });

        if (!precoResult.IsSuccess || !precoResult.Value.HasValue)
            throw new InvalidOperationException($"Preço não encontrado para o produto {produtoId} no catálogo {catalogoId}");

        var precoUnitario = precoResult.Value.Value;

        // Calcular desconto segmentado
        var produtor = await _produtorService.ObterPorIdAsync(pedido.ProdutorId);
        if (produtor == null)
            throw new InvalidOperationException($"Produtor com ID {pedido.ProdutorId} não encontrado");

        var resultadoDesconto = await _calculoDescontoService.CalcularDescontoAsync(
            pedido.ProdutorId,
            pedido.FornecedorId,
            produto.CategoriaId,
            produtor.AreaPlantio,
            precoUnitario * quantidade);

        // Criar item com valores calculados
        var item = new PedidoItem(
            pedido.Id,
            produtoId,
            quantidade,
            precoUnitario,
            resultadoDesconto.PercentualDesconto,
            observacoes);

        // Adicionar dados adicionais sobre o desconto
        var dadosDesconto = JsonSerializer.SerializeToDocument(new
        {
            segmentacao_aplicada = resultadoDesconto.SegmentacaoAplicada,
            grupo_aplicado = resultadoDesconto.GrupoAplicado,
            observacoes_desconto = resultadoDesconto.Observacoes,
            area_produtor = produtor.AreaPlantio,
            categoria_id = produto.CategoriaId
        });

        item.AtualizarDadosAdicionais(dadosDesconto);

        // Criar proposta de alteração do carrinho se o pedido estiver em negociação
        if (pedido.Status == StatusPedido.EmNegociacao)
        {
            await CriarPropostaAlteracaoCarrinhoAsync(pedido, $"Adicionou o produto ({produtoId}) - {produto.Nome} no pedido.");
        }

        return item;
    }

    /// <summary>
    /// Atualiza a quantidade de um item e recalcula preços
    /// </summary>
    /// <param name="item">Item a ser atualizado</param>
    /// <param name="novaQuantidade">Nova quantidade</param>
    /// <param name="pedido">Pedido para recálculo de descontos</param>
    public async Task AtualizarQuantidadeItemAsync(PedidoItem item, decimal novaQuantidade, Pedido pedido)
    {
        // Validar quantidade mínima
        await ValidarQuantidadeMinimaAsync(item.ProdutoId, novaQuantidade);

        // Obter dados do produto e produtor para recálculo
        var produto = await _produtoService.ObterPorIdAsync(item.ProdutoId);
        var produtor = await _produtorService.ObterPorIdAsync(pedido.ProdutorId);

        if (produto == null || produtor == null)
            throw new InvalidOperationException("Dados necessários para recálculo não encontrados");

        // Recalcular desconto com nova quantidade
        var resultadoDesconto = await _calculoDescontoService.CalcularDescontoAsync(
            pedido.ProdutorId,
            pedido.FornecedorId,
            produto.CategoriaId,
            produtor.AreaPlantio,
            item.PrecoUnitario * novaQuantidade);

        // Atualizar item
        item.AtualizarQuantidade(novaQuantidade);
        item.AtualizarDesconto(resultadoDesconto.PercentualDesconto);

        // Atualizar dados adicionais
        var dadosDesconto = JsonSerializer.SerializeToDocument(new
        {
            segmentacao_aplicada = resultadoDesconto.SegmentacaoAplicada,
            grupo_aplicado = resultadoDesconto.GrupoAplicado,
            observacoes_desconto = resultadoDesconto.Observacoes,
            area_produtor = produtor.AreaPlantio,
            categoria_id = produto.CategoriaId
        });

        item.AtualizarDadosAdicionais(dadosDesconto);

        // Criar proposta de alteração do carrinho se o pedido estiver em negociação
        if (pedido.Status == StatusPedido.EmNegociacao)
        {
            await CriarPropostaAlteracaoCarrinhoAsync(pedido, $"Alterou o produto ({item.ProdutoId}) - {produto.Nome} no pedido.");
        }
    }

    /// <summary>
    /// Calcula os totais do pedido
    /// </summary>
    /// <param name="pedido">Pedido para cálculo</param>
    /// <returns>Totais calculados</returns>
    public TotaisPedido CalcularTotais(Pedido pedido)
    {
        var totais = new TotaisPedido();

        foreach (var item in pedido.Itens)
        {
            totais.ValorBruto += item.ValorTotal;
            totais.ValorDesconto += item.ValorDesconto;
            totais.ValorLiquido += item.ValorFinal;
            totais.QuantidadeItens++;
        }

        totais.PercentualDescontoMedio = totais.ValorBruto > 0 
            ? (totais.ValorDesconto / totais.ValorBruto) * 100 
            : 0;

        return totais;
    }

    /// <summary>
    /// Valida se a quantidade atende ao mínimo exigido pelo produto
    /// </summary>
    /// <param name="produtoId">ID do produto</param>
    /// <param name="quantidade">Quantidade solicitada</param>
    private async Task ValidarQuantidadeMinimaAsync(int produtoId, decimal quantidade)
    {
        var produto = await _produtoService.ObterPorIdAsync(produtoId);
        if (produto == null)
            throw new InvalidOperationException($"Produto com ID {produtoId} não encontrado");

        // TODO: Implementar validação de quantidade mínima quando o campo estiver disponível no produto
        // Por enquanto, apenas validar que a quantidade é positiva
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantidade));
    }

    /// <summary>
    /// Verifica se o pedido ainda está dentro do prazo limite para modificações
    /// </summary>
    /// <param name="pedido">Pedido a ser verificado</param>
    /// <returns>True se está dentro do prazo</returns>
    public bool VerificarPrazoLimite(Pedido pedido)
    {
        return pedido.EstaDentroPrazoLimite();
    }

    /// <summary>
    /// Atualiza a data limite de interação do pedido
    /// </summary>
    /// <param name="pedido">Pedido a ser atualizado</param>
    /// <param name="novosDias">Novos dias a partir de agora</param>
    public void AtualizarPrazoLimite(Pedido pedido, int novosDias)
    {
        if (novosDias <= 0)
            throw new ArgumentException("Dias deve ser maior que zero", nameof(novosDias));

        pedido.AtualizarPrazoLimite(novosDias);
    }

    /// <summary>
    /// Remove um item do carrinho e cria proposta se necessário
    /// </summary>
    /// <param name="pedido">Pedido</param>
    /// <param name="itemId">ID do item a ser removido</param>
    /// <param name="usuarioId">ID do usuário que está removendo</param>
    public async Task RemoverItemAsync(Pedido pedido, int itemId, int usuarioId)
    {
        var item = pedido.Itens.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException($"Item com ID {itemId} não encontrado no pedido");

        var produto = await _produtoService.ObterPorIdAsync(item.ProdutoId);
        
        pedido.RemoverItem(itemId);

        // Criar proposta de alteração do carrinho se o pedido estiver em negociação
        if (pedido.Status == StatusPedido.EmNegociacao)
        {
            var nomeProduto = produto?.Nome ?? "Produto não encontrado";
            await CriarPropostaAlteracaoCarrinhoAsync(pedido, $"Removeu o produto ({item.ProdutoId}) - {nomeProduto} do pedido.", usuarioId);
        }
    }

    /// <summary>
    /// Cria uma proposta de alteração do carrinho
    /// </summary>
    /// <param name="pedido">Pedido alterado</param>
    /// <param name="observacao">Observação da alteração</param>
    /// <param name="usuarioId">ID do usuário (opcional)</param>
    private async Task CriarPropostaAlteracaoCarrinhoAsync(Pedido pedido, string observacao, int? usuarioId = null)
    {
        if (usuarioId.HasValue)
        {
            var proposta = new Proposta(pedido.Id, AcaoCompradorPedido.AlterouCarrinho, usuarioId.Value, observacao);
            await _propostaRepository.AdicionarAsync(proposta);
        }
    }
}

/// <summary>
/// Classe para representar os totais calculados de um pedido
/// </summary>
public class TotaisPedido
{
    /// <summary>
    /// Valor bruto total (sem descontos)
    /// </summary>
    public decimal ValorBruto { get; set; }

    /// <summary>
    /// Valor total de descontos aplicados
    /// </summary>
    public decimal ValorDesconto { get; set; }

    /// <summary>
    /// Valor líquido total (com descontos)
    /// </summary>
    public decimal ValorLiquido { get; set; }

    /// <summary>
    /// Quantidade total de itens
    /// </summary>
    public int QuantidadeItens { get; set; }

    /// <summary>
    /// Percentual médio de desconto aplicado
    /// </summary>
    public decimal PercentualDescontoMedio { get; set; }

    /// <summary>
    /// Converte para JsonDocument para armazenamento
    /// </summary>
    /// <returns>JsonDocument com os totais</returns>
    public JsonDocument ToJsonDocument()
    {
        return JsonSerializer.SerializeToDocument(new
        {
            valor_bruto = ValorBruto,
            valor_desconto = ValorDesconto,
            valor_liquido = ValorLiquido,
            quantidade_itens = QuantidadeItens,
            percentual_desconto_medio = PercentualDescontoMedio,
            data_calculo = DateTime.UtcNow
        });
    }
}