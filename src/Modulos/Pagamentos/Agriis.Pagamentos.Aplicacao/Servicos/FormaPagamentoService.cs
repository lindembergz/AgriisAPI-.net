using AutoMapper;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Pagamentos.Aplicacao.DTOs;
using Agriis.Pagamentos.Aplicacao.Interfaces;
using Agriis.Pagamentos.Dominio.Entidades;
using Agriis.Pagamentos.Dominio.Interfaces;

namespace Agriis.Pagamentos.Aplicacao.Servicos;

/// <summary>
/// Serviço de aplicação para formas de pagamento
/// </summary>
public class FormaPagamentoService : IFormaPagamentoService
{
    private readonly IFormaPagamentoRepository _formaPagamentoRepository;
    private readonly IMapper _mapper;

    public FormaPagamentoService(
        IFormaPagamentoRepository formaPagamentoRepository,
        IMapper mapper)
    {
        _formaPagamentoRepository = formaPagamentoRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<FormaPagamentoDto>>> ObterAtivasAsync()
    {
        try
        {
            var formasPagamento = await _formaPagamentoRepository.ObterAtivasAsync();
            var dtos = _mapper.Map<IEnumerable<FormaPagamentoDto>>(formasPagamento);
            
            return Result<IEnumerable<FormaPagamentoDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<FormaPagamentoDto>>.Failure($"Erro ao obter formas de pagamento: {ex.Message}");
        }
    }

    public async Task<Result<FormaPagamentoDto>> ObterPorIdAsync(int id)
    {
        try
        {
            var formaPagamento = await _formaPagamentoRepository.ObterPorIdAsync(id);
            
            if (formaPagamento == null)
                return Result<FormaPagamentoDto>.Failure("Forma de pagamento não encontrada");

            var dto = _mapper.Map<FormaPagamentoDto>(formaPagamento);
            return Result<FormaPagamentoDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<FormaPagamentoDto>.Failure($"Erro ao obter forma de pagamento: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<FormaPagamentoPedidoDto>>> ObterPorPedidoAsync(int pedidoId)
    {
        try
        {
            var formasPagamento = await _formaPagamentoRepository.ObterPorPedidoIdAsync(pedidoId);
            
            // Mapear para DTO específico de pedido (implementação simplificada)
            var dtos = formasPagamento.Select(fp => new FormaPagamentoPedidoDto
            {
                Id = fp.Id,
                Descricao = fp.Descricao,
                CulturaFormaPagamentoId = 0 // Será preenchido pela consulta específica
            });
            
            return Result<IEnumerable<FormaPagamentoPedidoDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<FormaPagamentoPedidoDto>>.Failure($"Erro ao obter formas de pagamento do pedido: {ex.Message}");
        }
    }

    public async Task<Result<FormaPagamentoDto>> CriarAsync(CriarFormaPagamentoDto dto)
    {
        try
        {
            var formaPagamento = _mapper.Map<FormaPagamento>(dto);
            var formaPagamentoCriada = await _formaPagamentoRepository.AdicionarAsync(formaPagamento);
            
            var resultDto = _mapper.Map<FormaPagamentoDto>(formaPagamentoCriada);
            return Result<FormaPagamentoDto>.Success(resultDto);
        }
        catch (Exception ex)
        {
            return Result<FormaPagamentoDto>.Failure($"Erro ao criar forma de pagamento: {ex.Message}");
        }
    }

    public async Task<Result<FormaPagamentoDto>> AtualizarAsync(int id, AtualizarFormaPagamentoDto dto)
    {
        try
        {
            var formaPagamento = await _formaPagamentoRepository.ObterPorIdAsync(id);
            
            if (formaPagamento == null)
                return Result<FormaPagamentoDto>.Failure("Forma de pagamento não encontrada");

            formaPagamento.AtualizarDescricao(dto.Descricao);
            
            if (dto.Ativo)
                formaPagamento.Ativar();
            else
                formaPagamento.Desativar();

            await _formaPagamentoRepository.AtualizarAsync(formaPagamento);
            
            var resultDto = _mapper.Map<FormaPagamentoDto>(formaPagamento);
            return Result<FormaPagamentoDto>.Success(resultDto);
        }
        catch (Exception ex)
        {
            return Result<FormaPagamentoDto>.Failure($"Erro ao atualizar forma de pagamento: {ex.Message}");
        }
    }

    public async Task<Result> RemoverAsync(int id)
    {
        try
        {
            var existe = await _formaPagamentoRepository.ExisteAsync(id);
            
            if (!existe)
                return Result.Failure("Forma de pagamento não encontrada");

            await _formaPagamentoRepository.RemoverAsync(id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Erro ao remover forma de pagamento: {ex.Message}");
        }
    }
}