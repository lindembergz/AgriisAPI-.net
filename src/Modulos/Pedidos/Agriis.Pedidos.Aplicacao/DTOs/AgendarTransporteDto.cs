namespace Agriis.Pedidos.Aplicacao.DTOs;

/// <summary>
/// DTO para agendamento de transporte
/// </summary>
public class AgendarTransporteDto
{
    /// <summary>
    /// ID do item de pedido
    /// </summary>
    public int PedidoItemId { get; set; }

    /// <summary>
    /// Quantidade a ser transportada
    /// </summary>
    public decimal Quantidade { get; set; }

    /// <summary>
    /// Data e hora do agendamento
    /// </summary>
    public DateTime DataAgendamento { get; set; }

    /// <summary>
    /// Endereço de origem (opcional)
    /// </summary>
    public string? EnderecoOrigem { get; set; }

    /// <summary>
    /// Endereço de destino (opcional)
    /// </summary>
    public string? EnderecoDestino { get; set; }

    /// <summary>
    /// Distância em quilômetros (opcional)
    /// </summary>
    public decimal? DistanciaKm { get; set; }

    /// <summary>
    /// Observações sobre o transporte (opcional)
    /// </summary>
    public string? Observacoes { get; set; }
}

/// <summary>
/// DTO para reagendamento de transporte
/// </summary>
public class ReagendarTransporteDto
{
    /// <summary>
    /// Nova data e hora do agendamento
    /// </summary>
    public DateTime NovaDataAgendamento { get; set; }

    /// <summary>
    /// Observações sobre o reagendamento (opcional)
    /// </summary>
    public string? Observacoes { get; set; }
}

/// <summary>
/// DTO para atualização de valor de frete
/// </summary>
public class AtualizarValorFreteDto
{
    /// <summary>
    /// Novo valor do frete
    /// </summary>
    public decimal NovoValorFrete { get; set; }

    /// <summary>
    /// Motivo da alteração (opcional)
    /// </summary>
    public string? Motivo { get; set; }
}

/// <summary>
/// DTO para validação de múltiplos agendamentos
/// </summary>
public class ValidarAgendamentosDto
{
    /// <summary>
    /// Lista de agendamentos a serem validados
    /// </summary>
    public List<SolicitacaoAgendamentoDto> Agendamentos { get; set; } = new();
}

/// <summary>
/// DTO para solicitação de agendamento
/// </summary>
public class SolicitacaoAgendamentoDto
{
    /// <summary>
    /// ID do item de pedido
    /// </summary>
    public int PedidoItemId { get; set; }

    /// <summary>
    /// Quantidade a ser transportada
    /// </summary>
    public decimal Quantidade { get; set; }

    /// <summary>
    /// Data do agendamento
    /// </summary>
    public DateTime DataAgendamento { get; set; }

    /// <summary>
    /// Endereço de origem (opcional)
    /// </summary>
    public string? EnderecoOrigem { get; set; }

    /// <summary>
    /// Endereço de destino (opcional)
    /// </summary>
    public string? EnderecoDestino { get; set; }

    /// <summary>
    /// Distância em quilômetros (opcional)
    /// </summary>
    public decimal? DistanciaKm { get; set; }

    /// <summary>
    /// Observações (opcional)
    /// </summary>
    public string? Observacoes { get; set; }
}

/// <summary>
/// DTO com resultado da validação de agendamentos
/// </summary>
public class ValidacaoAgendamentoDto
{
    /// <summary>
    /// Indica se a validação foi bem-sucedida
    /// </summary>
    public bool EhValido { get; set; }

    /// <summary>
    /// Lista de erros encontrados
    /// </summary>
    public List<string> Erros { get; set; } = new();
}

/// <summary>
/// DTO com resumo de transporte de um pedido
/// </summary>
public class ResumoTransportePedidoDto
{
    /// <summary>
    /// Total de itens no pedido
    /// </summary>
    public int TotalItens { get; set; }

    /// <summary>
    /// Itens que possuem transporte agendado
    /// </summary>
    public int ItensComTransporte { get; set; }

    /// <summary>
    /// Total de transportes agendados
    /// </summary>
    public int TotalTransportes { get; set; }

    /// <summary>
    /// Transportes com data agendada
    /// </summary>
    public int TransportesAgendados { get; set; }

    /// <summary>
    /// Peso total de todos os transportes
    /// </summary>
    public decimal PesoTotal { get; set; }

    /// <summary>
    /// Volume total de todos os transportes
    /// </summary>
    public decimal VolumeTotal { get; set; }

    /// <summary>
    /// Valor total do frete
    /// </summary>
    public decimal ValorFreteTotal { get; set; }

    /// <summary>
    /// Data do próximo agendamento
    /// </summary>
    public DateTime? ProximoAgendamento { get; set; }
}