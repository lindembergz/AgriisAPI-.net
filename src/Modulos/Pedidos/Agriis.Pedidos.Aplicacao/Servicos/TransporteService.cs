using AutoMapper;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Pedidos.Aplicacao.DTOs;
using Agriis.Pedidos.Aplicacao.Interfaces;
using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Interfaces;
using Agriis.Pedidos.Dominio.Servicos;
using Agriis.Produtos.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.Pedidos.Aplicacao.Servicos;

/// <summary>
/// Serviço de aplicação para gerenciamento de transportes
/// </summary>
public class TransporteService : ITransporteService
{
    private readonly IPedidoItemRepository _pedidoItemRepository;
    private readonly IPedidoItemTransporteRepository _transporteRepository;
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly FreteCalculoService _freteCalculoService;
    private readonly TransporteAgendamentoService _transporteAgendamentoService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TransporteService> _logger;

    public TransporteService(
        IPedidoItemRepository pedidoItemRepository,
        IPedidoItemTransporteRepository transporteRepository,
        IPedidoRepository pedidoRepository,
        IProdutoRepository produtoRepository,
        FreteCalculoService freteCalculoService,
        TransporteAgendamentoService transporteAgendamentoService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<TransporteService> logger)
    {
        _pedidoItemRepository = pedidoItemRepository ?? throw new ArgumentNullException(nameof(pedidoItemRepository));
        _transporteRepository = transporteRepository ?? throw new ArgumentNullException(nameof(transporteRepository));
        _pedidoRepository = pedidoRepository ?? throw new ArgumentNullException(nameof(pedidoRepository));
        _produtoRepository = produtoRepository ?? throw new ArgumentNullException(nameof(produtoRepository));
        _freteCalculoService = freteCalculoService ?? throw new ArgumentNullException(nameof(freteCalculoService));
        _transporteAgendamentoService = transporteAgendamentoService ?? throw new ArgumentNullException(nameof(transporteAgendamentoService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<CalculoFreteDto>> CalcularFreteAsync(CalcularFreteDto dto)
    {
        try
        {
            _logger.LogInformation("Calculando frete para produto {ProdutoId}, quantidade {Quantidade}, distância {DistanciaKm}km",
                dto.ProdutoId, dto.Quantidade, dto.DistanciaKm);

            var produto = await _produtoRepository.ObterPorIdAsync(dto.ProdutoId);
            if (produto == null)
            {
                return Result<CalculoFreteDto>.Failure("Produto não encontrado");
            }

            var valorPorKgKm = dto.ValorPorKgKm ?? 0.05m;
            var valorMinimoFrete = dto.ValorMinimoFrete ?? 50.00m;

            var resultado = _freteCalculoService.CalcularFrete(
                produto, 
                dto.Quantidade, 
                dto.DistanciaKm,
                valorPorKgKm,
                valorMinimoFrete);

            var calculoDto = new CalculoFreteDto
            {
                PesoTotal = resultado.PesoTotal,
                VolumeTotal = resultado.VolumeTotal,
                PesoCubadoTotal = resultado.PesoCubadoTotal,
                PesoParaFrete = resultado.PesoParaFrete,
                ValorFrete = resultado.ValorFrete,
                DistanciaKm = resultado.DistanciaKm,
                TipoCalculoUtilizado = resultado.TipoCalculoUtilizado.ToString()
            };

            _logger.LogInformation("Frete calculado: R$ {ValorFrete} para peso {PesoParaFrete}kg",
                calculoDto.ValorFrete, calculoDto.PesoParaFrete);

            return Result<CalculoFreteDto>.Success(calculoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular frete para produto {ProdutoId}", dto.ProdutoId);
            return Result<CalculoFreteDto>.Failure($"Erro ao calcular frete: {ex.Message}");
        }
    }

    public async Task<Result<CalculoFreteConsolidadoDto>> CalcularFreteConsolidadoAsync(CalcularFreteConsolidadoDto dto)
    {
        try
        {
            _logger.LogInformation("Calculando frete consolidado para {TotalItens} itens, distância {DistanciaKm}km",
                dto.Itens.Count, dto.DistanciaKm);

            var itensComProdutos = new List<(Produtos.Dominio.Entidades.Produto produto, decimal quantidade)>();

            foreach (var item in dto.Itens)
            {
                var produto = await _produtoRepository.ObterPorIdAsync(item.ProdutoId);
                if (produto == null)
                {
                    return Result<CalculoFreteConsolidadoDto>.Failure($"Produto {item.ProdutoId} não encontrado");
                }
                itensComProdutos.Add((produto, item.Quantidade));
            }

            var valorPorKgKm = dto.ValorPorKgKm ?? 0.05m;
            var valorMinimoFrete = dto.ValorMinimoFrete ?? 50.00m;

            var resultado = _freteCalculoService.CalcularFreteConsolidado(
                itensComProdutos,
                dto.DistanciaKm,
                valorPorKgKm,
                valorMinimoFrete);

            var calculoDto = new CalculoFreteConsolidadoDto
            {
                CalculosIndividuais = resultado.CalculosIndividuais.Select(c => new CalculoFreteDto
                {
                    PesoTotal = c.PesoTotal,
                    VolumeTotal = c.VolumeTotal,
                    PesoCubadoTotal = c.PesoCubadoTotal,
                    PesoParaFrete = c.PesoParaFrete,
                    ValorFrete = c.ValorFrete,
                    DistanciaKm = c.DistanciaKm,
                    TipoCalculoUtilizado = c.TipoCalculoUtilizado.ToString()
                }).ToList(),
                PesoTotalConsolidado = resultado.PesoTotalConsolidado,
                VolumeTotalConsolidado = resultado.VolumeTotalConsolidado,
                PesoCubadoTotalConsolidado = resultado.PesoCubadoTotalConsolidado,
                ValorFreteConsolidado = resultado.ValorFreteConsolidado,
                DistanciaKm = resultado.DistanciaKm
            };

            _logger.LogInformation("Frete consolidado calculado: R$ {ValorFrete} para peso total {PesoTotal}kg",
                calculoDto.ValorFreteConsolidado, calculoDto.PesoTotalConsolidado);

            return Result<CalculoFreteConsolidadoDto>.Success(calculoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular frete consolidado");
            return Result<CalculoFreteConsolidadoDto>.Failure($"Erro ao calcular frete consolidado: {ex.Message}");
        }
    }

    public async Task<Result<PedidoItemTransporteDto>> AgendarTransporteAsync(AgendarTransporteDto dto)
    {
        try
        {
            _logger.LogInformation("Agendando transporte para item {PedidoItemId}, quantidade {Quantidade}, data {DataAgendamento}",
                dto.PedidoItemId, dto.Quantidade, dto.DataAgendamento);

            var pedidoItem = await _pedidoItemRepository.ObterComTransportesAsync(dto.PedidoItemId);
            if (pedidoItem == null)
            {
                return Result<PedidoItemTransporteDto>.Failure("Item de pedido não encontrado");
            }

            var transporte = _transporteAgendamentoService.CriarAgendamentoTransporte(
                pedidoItem,
                dto.Quantidade,
                dto.DataAgendamento,
                dto.EnderecoOrigem,
                dto.EnderecoDestino,
                dto.DistanciaKm ?? 0,
                dto.Observacoes);

            await _transporteRepository.AdicionarAsync(transporte);
            await _unitOfWork.SalvarAlteracoesAsync();

            var transporteDto = _mapper.Map<PedidoItemTransporteDto>(transporte);

            _logger.LogInformation("Transporte agendado com sucesso. ID: {TransporteId}", transporte.Id);

            return Result<PedidoItemTransporteDto>.Success(transporteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao agendar transporte para item {PedidoItemId}", dto.PedidoItemId);
            return Result<PedidoItemTransporteDto>.Failure($"Erro ao agendar transporte: {ex.Message}");
        }
    }

    public async Task<Result<PedidoItemTransporteDto>> ReagendarTransporteAsync(int transporteId, ReagendarTransporteDto dto)
    {
        try
        {
            _logger.LogInformation("Reagendando transporte {TransporteId} para {NovaDataAgendamento}",
                transporteId, dto.NovaDataAgendamento);

            var transporte = await _transporteRepository.ObterPorIdAsync(transporteId);
            if (transporte == null)
            {
                return Result<PedidoItemTransporteDto>.Failure("Transporte não encontrado");
            }

            _transporteAgendamentoService.ReagendarTransporte(
                transporte,
                dto.NovaDataAgendamento,
                dto.Observacoes);

            await _transporteRepository.AtualizarAsync(transporte);
            await _unitOfWork.SalvarAlteracoesAsync();

            var transporteDto = _mapper.Map<PedidoItemTransporteDto>(transporte);

            _logger.LogInformation("Transporte reagendado com sucesso. ID: {TransporteId}", transporteId);

            return Result<PedidoItemTransporteDto>.Success(transporteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reagendar transporte {TransporteId}", transporteId);
            return Result<PedidoItemTransporteDto>.Failure($"Erro ao reagendar transporte: {ex.Message}");
        }
    }

    public async Task<Result<PedidoItemTransporteDto>> AtualizarValorFreteAsync(int transporteId, AtualizarValorFreteDto dto)
    {
        try
        {
            _logger.LogInformation("Atualizando valor do frete do transporte {TransporteId} para R$ {NovoValor}",
                transporteId, dto.NovoValorFrete);

            var transporte = await _transporteRepository.ObterPorIdAsync(transporteId);
            if (transporte == null)
            {
                return Result<PedidoItemTransporteDto>.Failure("Transporte não encontrado");
            }

            _transporteAgendamentoService.AtualizarValorFrete(
                transporte,
                dto.NovoValorFrete,
                dto.Motivo);

            await _transporteRepository.AtualizarAsync(transporte);
            await _unitOfWork.SalvarAlteracoesAsync();

            var transporteDto = _mapper.Map<PedidoItemTransporteDto>(transporte);

            _logger.LogInformation("Valor do frete atualizado com sucesso. ID: {TransporteId}", transporteId);

            return Result<PedidoItemTransporteDto>.Success(transporteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar valor do frete do transporte {TransporteId}", transporteId);
            return Result<PedidoItemTransporteDto>.Failure($"Erro ao atualizar valor do frete: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PedidoItemTransporteDto>>> ListarTransportesPedidoAsync(int pedidoId)
    {
        try
        {
            _logger.LogInformation("Listando transportes do pedido {PedidoId}", pedidoId);

            var transportes = await _transporteRepository.ObterPorPedidoIdAsync(pedidoId);
            var transportesDto = _mapper.Map<IEnumerable<PedidoItemTransporteDto>>(transportes);

            return Result<IEnumerable<PedidoItemTransporteDto>>.Success(transportesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar transportes do pedido {PedidoId}", pedidoId);
            return Result<IEnumerable<PedidoItemTransporteDto>>.Failure($"Erro ao listar transportes: {ex.Message}");
        }
    }

    public async Task<Result<PedidoItemTransporteDto>> ObterTransportePorIdAsync(int transporteId)
    {
        try
        {
            var transporte = await _transporteRepository.ObterPorIdAsync(transporteId);
            if (transporte == null)
            {
                return Result<PedidoItemTransporteDto>.Failure("Transporte não encontrado");
            }

            var transporteDto = _mapper.Map<PedidoItemTransporteDto>(transporte);
            return Result<PedidoItemTransporteDto>.Success(transporteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter transporte {TransporteId}", transporteId);
            return Result<PedidoItemTransporteDto>.Failure($"Erro ao obter transporte: {ex.Message}");
        }
    }

    public async Task<Result<ValidacaoAgendamentoDto>> ValidarMultiplosAgendamentosAsync(ValidarAgendamentosDto dto)
    {
        try
        {
            _logger.LogInformation("Validando {TotalAgendamentos} agendamentos", dto.Agendamentos.Count);

            var solicitacoes = new List<SolicitacaoAgendamento>();

            foreach (var agendamento in dto.Agendamentos)
            {
                var pedidoItem = await _pedidoItemRepository.ObterComTransportesAsync(agendamento.PedidoItemId);
                if (pedidoItem == null)
                {
                    return Result<ValidacaoAgendamentoDto>.Failure($"Item de pedido {agendamento.PedidoItemId} não encontrado");
                }

                solicitacoes.Add(new SolicitacaoAgendamento(
                    pedidoItem,
                    agendamento.Quantidade,
                    agendamento.DataAgendamento,
                    agendamento.EnderecoOrigem,
                    agendamento.EnderecoDestino,
                    agendamento.DistanciaKm ?? 0,
                    agendamento.Observacoes));
            }

            var resultado = _transporteAgendamentoService.ValidarMultiplosAgendamentos(solicitacoes);

            var validacaoDto = new ValidacaoAgendamentoDto
            {
                EhValido = resultado.EhValido,
                Erros = resultado.Erros.ToList()
            };

            return Result<ValidacaoAgendamentoDto>.Success(validacaoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar múltiplos agendamentos");
            return Result<ValidacaoAgendamentoDto>.Failure($"Erro ao validar agendamentos: {ex.Message}");
        }
    }

    public async Task<Result<ResumoTransportePedidoDto>> ObterResumoTransporteAsync(int pedidoId)
    {
        try
        {
            _logger.LogInformation("Obtendo resumo de transporte do pedido {PedidoId}", pedidoId);

            var pedido = await _pedidoRepository.ObterComItensETransportesAsync(pedidoId);
            if (pedido == null)
            {
                return Result<ResumoTransportePedidoDto>.Failure("Pedido não encontrado");
            }

            var resumo = _transporteAgendamentoService.CalcularResumoTransporte(pedido);

            var resumoDto = new ResumoTransportePedidoDto
            {
                TotalItens = resumo.TotalItens,
                ItensComTransporte = resumo.ItensComTransporte,
                TotalTransportes = resumo.TotalTransportes,
                TransportesAgendados = resumo.TransportesAgendados,
                PesoTotal = resumo.PesoTotal,
                VolumeTotal = resumo.VolumeTotal,
                ValorFreteTotal = resumo.ValorFreteTotal,
                ProximoAgendamento = resumo.ProximoAgendamento
            };

            return Result<ResumoTransportePedidoDto>.Success(resumoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter resumo de transporte do pedido {PedidoId}", pedidoId);
            return Result<ResumoTransportePedidoDto>.Failure($"Erro ao obter resumo de transporte: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CancelarTransporteAsync(int transporteId, string? motivo = null)
    {
        try
        {
            _logger.LogInformation("Cancelando transporte {TransporteId}. Motivo: {Motivo}", transporteId, motivo);

            var transporte = await _transporteRepository.ObterPorIdAsync(transporteId);
            if (transporte == null)
            {
                return Result<bool>.Failure("Transporte não encontrado");
            }

            // Adicionar observação sobre cancelamento
            var observacaoCancelamento = $"Transporte cancelado em {DateTime.UtcNow:dd/MM/yyyy HH:mm}";
            if (!string.IsNullOrWhiteSpace(motivo))
            {
                observacaoCancelamento += $" - Motivo: {motivo}";
            }

            var observacoesAtuais = transporte.Observacoes ?? string.Empty;
            var observacoesCombinadas = string.IsNullOrWhiteSpace(observacoesAtuais) 
                ? observacaoCancelamento 
                : $"{observacoesAtuais}\n{observacaoCancelamento}";

            transporte.AtualizarObservacoes(observacoesCombinadas);

            // Remover o transporte
            await _transporteRepository.RemoverAsync(transporteId);
            await _unitOfWork.SalvarAlteracoesAsync();

            _logger.LogInformation("Transporte {TransporteId} cancelado com sucesso", transporteId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar transporte {TransporteId}", transporteId);
            return Result<bool>.Failure($"Erro ao cancelar transporte: {ex.Message}");
        }
    }
}