using Agriis.Pedidos.Dominio.Entidades;
using System.Text.Json;

namespace Agriis.Pedidos.Dominio.Servicos;

/// <summary>
/// Serviço de domínio para agendamento de transportes
/// </summary>
public class TransporteAgendamentoService
{
    private readonly FreteCalculoService _freteCalculoService;

    public TransporteAgendamentoService(FreteCalculoService freteCalculoService)
    {
        _freteCalculoService = freteCalculoService ?? throw new ArgumentNullException(nameof(freteCalculoService));
    }

    /// <summary>
    /// Cria um agendamento de transporte para um item de pedido
    /// </summary>
    /// <param name="pedidoItem">Item do pedido</param>
    /// <param name="quantidade">Quantidade a ser transportada</param>
    /// <param name="dataAgendamento">Data do agendamento</param>
    /// <param name="enderecoOrigem">Endereço de origem</param>
    /// <param name="enderecoDestino">Endereço de destino</param>
    /// <param name="distanciaKm">Distância em quilômetros</param>
    /// <param name="observacoes">Observações do transporte</param>
    /// <returns>Transporte criado</returns>
    public PedidoItemTransporte CriarAgendamentoTransporte(
        PedidoItem pedidoItem,
        decimal quantidade,
        DateTime dataAgendamento,
        string? enderecoOrigem = null,
        string? enderecoDestino = null,
        decimal distanciaKm = 0,
        string? observacoes = null)
    {
        if (pedidoItem == null)
            throw new ArgumentNullException(nameof(pedidoItem));

        // Validar disponibilidade de quantidade
        if (!_freteCalculoService.ValidarDisponibilidadeQuantidade(pedidoItem, quantidade))
        {
            var quantidadeDisponivel = _freteCalculoService.CalcularQuantidadeDisponivel(pedidoItem);
            throw new InvalidOperationException(
                $"Quantidade solicitada ({quantidade}) excede a disponível ({quantidadeDisponivel})");
        }

        // Validar data de agendamento
        ValidarDataAgendamento(dataAgendamento);

        // Calcular frete se distância foi informada
        decimal valorFrete = 0;
        if (distanciaKm > 0 && pedidoItem.Produto != null)
        {
            var calculoFrete = _freteCalculoService.CalcularFrete(
                pedidoItem.Produto, 
                quantidade, 
                distanciaKm);
            valorFrete = calculoFrete.ValorFrete;
        }

        // Criar transporte
        var transporte = new PedidoItemTransporte(
            pedidoItem.Id,
            quantidade,
            valorFrete,
            enderecoOrigem,
            enderecoDestino);

        // Agendar data
        transporte.AgendarTransporte(dataAgendamento);

        // Adicionar observações se fornecidas
        if (!string.IsNullOrWhiteSpace(observacoes))
        {
            transporte.AtualizarObservacoes(observacoes);
        }

        // Calcular e atualizar peso/volume se produto disponível
        if (pedidoItem.Produto != null)
        {
            var calculoFrete = _freteCalculoService.CalcularFrete(
                pedidoItem.Produto, 
                quantidade, 
                distanciaKm > 0 ? distanciaKm : 1); // Usar 1km como padrão para cálculo de peso/volume

            transporte.AtualizarPesoVolume(calculoFrete.PesoTotal, calculoFrete.VolumeTotal);

            // Adicionar informações detalhadas do cálculo
            var informacoesTransporte = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                calculo_frete = new
                {
                    peso_total = calculoFrete.PesoTotal,
                    volume_total = calculoFrete.VolumeTotal,
                    peso_cubado_total = calculoFrete.PesoCubadoTotal,
                    peso_para_frete = calculoFrete.PesoParaFrete,
                    distancia_km = calculoFrete.DistanciaKm,
                    tipo_calculo = calculoFrete.TipoCalculoUtilizado.ToString()
                },
                agendamento = new
                {
                    data_criacao = DateTime.UtcNow,
                    data_agendamento = dataAgendamento,
                    endereco_origem = enderecoOrigem,
                    endereco_destino = enderecoDestino
                }
            }));

            transporte.AtualizarInformacoesTransporte(informacoesTransporte);
        }

        return transporte;
    }

    /// <summary>
    /// Reagenda um transporte existente
    /// </summary>
    /// <param name="transporte">Transporte a ser reagendado</param>
    /// <param name="novaDataAgendamento">Nova data de agendamento</param>
    /// <param name="observacoes">Observações sobre o reagendamento</param>
    public void ReagendarTransporte(
        PedidoItemTransporte transporte, 
        DateTime novaDataAgendamento,
        string? observacoes = null)
    {
        if (transporte == null)
            throw new ArgumentNullException(nameof(transporte));

        ValidarDataAgendamento(novaDataAgendamento);

        transporte.AgendarTransporte(novaDataAgendamento);

        // Atualizar observações com histórico de reagendamento
        var observacoesAtuais = transporte.Observacoes ?? string.Empty;
        var novaObservacao = $"Reagendado para {novaDataAgendamento:dd/MM/yyyy HH:mm}";
        
        if (!string.IsNullOrWhiteSpace(observacoes))
        {
            novaObservacao += $" - {observacoes}";
        }

        var observacoesCombinadas = string.IsNullOrWhiteSpace(observacoesAtuais) 
            ? novaObservacao 
            : $"{observacoesAtuais}\n{novaObservacao}";

        transporte.AtualizarObservacoes(observacoesCombinadas);

        // Atualizar informações de transporte com histórico
        if (transporte.InformacoesTransporte != null)
        {
            var informacoesAtuais = JsonSerializer.Deserialize<Dictionary<string, object>>(
                transporte.InformacoesTransporte.RootElement.GetRawText()) ?? new Dictionary<string, object>();

            if (!informacoesAtuais.ContainsKey("historico_reagendamentos"))
            {
                informacoesAtuais["historico_reagendamentos"] = new List<object>();
            }

            var historico = JsonSerializer.Deserialize<List<object>>(
                JsonSerializer.Serialize(informacoesAtuais["historico_reagendamentos"])) ?? new List<object>();

            historico.Add(new
            {
                data_reagendamento = DateTime.UtcNow,
                nova_data_agendamento = novaDataAgendamento,
                observacoes = observacoes
            });

            informacoesAtuais["historico_reagendamentos"] = historico;

            var novasInformacoes = JsonDocument.Parse(JsonSerializer.Serialize(informacoesAtuais));
            transporte.AtualizarInformacoesTransporte(novasInformacoes);
        }
    }

    /// <summary>
    /// Atualiza o valor do frete de um transporte
    /// </summary>
    /// <param name="transporte">Transporte a ser atualizado</param>
    /// <param name="novoValorFrete">Novo valor do frete</param>
    /// <param name="motivo">Motivo da alteração</param>
    public void AtualizarValorFrete(
        PedidoItemTransporte transporte, 
        decimal novoValorFrete,
        string? motivo = null)
    {
        if (transporte == null)
            throw new ArgumentNullException(nameof(transporte));

        var valorAnterior = transporte.ValorFrete;
        transporte.AtualizarValorFrete(novoValorFrete);

        // Registrar alteração nas observações
        var observacao = $"Valor do frete alterado de R$ {valorAnterior:F2} para R$ {novoValorFrete:F2}";
        if (!string.IsNullOrWhiteSpace(motivo))
        {
            observacao += $" - Motivo: {motivo}";
        }

        var observacoesAtuais = transporte.Observacoes ?? string.Empty;
        var observacoesCombinadas = string.IsNullOrWhiteSpace(observacoesAtuais) 
            ? observacao 
            : $"{observacoesAtuais}\n{observacao}";

        transporte.AtualizarObservacoes(observacoesCombinadas);
    }

    /// <summary>
    /// Valida se múltiplos transportes podem ser agendados para as datas especificadas
    /// </summary>
    /// <param name="agendamentos">Lista de agendamentos solicitados</param>
    /// <returns>Resultado da validação</returns>
    public ValidacaoAgendamentoResult ValidarMultiplosAgendamentos(
        IEnumerable<SolicitacaoAgendamento> agendamentos)
    {
        if (agendamentos == null || !agendamentos.Any())
            return new ValidacaoAgendamentoResult(true, new List<string>());

        var erros = new List<string>();

        foreach (var agendamento in agendamentos)
        {
            try
            {
                ValidarDataAgendamento(agendamento.DataAgendamento);

                if (!_freteCalculoService.ValidarDisponibilidadeQuantidade(
                    agendamento.PedidoItem, agendamento.Quantidade))
                {
                    var quantidadeDisponivel = _freteCalculoService.CalcularQuantidadeDisponivel(
                        agendamento.PedidoItem);
                    erros.Add($"Item {agendamento.PedidoItem.Id}: Quantidade solicitada ({agendamento.Quantidade}) " +
                             $"excede a disponível ({quantidadeDisponivel})");
                }
            }
            catch (Exception ex)
            {
                erros.Add($"Item {agendamento.PedidoItem.Id}: {ex.Message}");
            }
        }

        return new ValidacaoAgendamentoResult(erros.Count == 0, erros);
    }

    /// <summary>
    /// Calcula o resumo de transporte para um pedido
    /// </summary>
    /// <param name="pedido">Pedido para calcular resumo</param>
    /// <returns>Resumo do transporte</returns>
    public ResumoTransportePedido CalcularResumoTransporte(Pedido pedido)
    {
        if (pedido == null)
            throw new ArgumentNullException(nameof(pedido));

        var itensComTransporte = pedido.Itens.Where(i => i.Transportes.Any()).ToList();
        var totalTransportes = pedido.Itens.SelectMany(i => i.Transportes).ToList();

        var pesoTotal = totalTransportes.Sum(t => t.PesoTotal ?? 0);
        var volumeTotal = totalTransportes.Sum(t => t.VolumeTotal ?? 0);
        var valorFreteTotal = totalTransportes.Sum(t => t.ValorFrete);

        var transportesAgendados = totalTransportes.Where(t => t.DataAgendamento.HasValue).ToList();
        var proximoAgendamento = transportesAgendados
            .Where(t => t.DataAgendamento > DateTime.UtcNow)
            .OrderBy(t => t.DataAgendamento)
            .FirstOrDefault();

        return new ResumoTransportePedido(
            pedido.Itens.Count,
            itensComTransporte.Count,
            totalTransportes.Count,
            transportesAgendados.Count,
            pesoTotal,
            volumeTotal,
            valorFreteTotal,
            proximoAgendamento?.DataAgendamento
        );
    }

    private static void ValidarDataAgendamento(DateTime dataAgendamento)
    {
        if (dataAgendamento <= DateTime.UtcNow)
            throw new ArgumentException("Data de agendamento deve ser futura", nameof(dataAgendamento));

        // Validar se não é fim de semana (opcional - pode ser configurável)
        if (dataAgendamento.DayOfWeek == DayOfWeek.Sunday || dataAgendamento.DayOfWeek == DayOfWeek.Saturday)
        {
            // Por enquanto apenas um aviso, não um erro
            // throw new ArgumentException("Agendamentos não são permitidos em fins de semana", nameof(dataAgendamento));
        }

        // Validar se não é muito distante no futuro (ex: máximo 90 dias)
        if (dataAgendamento > DateTime.UtcNow.AddDays(90))
            throw new ArgumentException("Data de agendamento não pode ser superior a 90 dias", nameof(dataAgendamento));
    }
}

/// <summary>
/// Solicitação de agendamento de transporte
/// </summary>
public record SolicitacaoAgendamento(
    PedidoItem PedidoItem,
    decimal Quantidade,
    DateTime DataAgendamento,
    string? EnderecoOrigem = null,
    string? EnderecoDestino = null,
    decimal DistanciaKm = 0,
    string? Observacoes = null
);

/// <summary>
/// Resultado da validação de agendamentos
/// </summary>
public record ValidacaoAgendamentoResult(
    bool EhValido,
    IEnumerable<string> Erros
);

/// <summary>
/// Resumo de transporte para um pedido
/// </summary>
public record ResumoTransportePedido(
    int TotalItens,
    int ItensComTransporte,
    int TotalTransportes,
    int TransportesAgendados,
    decimal PesoTotal,
    decimal VolumeTotal,
    decimal ValorFreteTotal,
    DateTime? ProximoAgendamento
);