using AutoMapper;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Pagamentos.Aplicacao.DTOs;
using Agriis.Pagamentos.Aplicacao.Interfaces;
using Agriis.Pagamentos.Dominio.Entidades;
using Agriis.Pagamentos.Dominio.Interfaces;

namespace Agriis.Pagamentos.Aplicacao.Servicos;

/// <summary>
/// Serviço de aplicação para associações cultura-fornecedor-forma de pagamento
/// </summary>
public class CulturaFormaPagamentoService : ICulturaFormaPagamentoService
{
    private readonly ICulturaFormaPagamentoRepository _culturaFormaPagamentoRepository;
    private readonly IFormaPagamentoRepository _formaPagamentoRepository;
    private readonly IMapper _mapper;

    public CulturaFormaPagamentoService(
        ICulturaFormaPagamentoRepository culturaFormaPagamentoRepository,
        IFormaPagamentoRepository formaPagamentoRepository,
        IMapper mapper)
    {
        _culturaFormaPagamentoRepository = culturaFormaPagamentoRepository;
        _formaPagamentoRepository = formaPagamentoRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<CulturaFormaPagamentoDto>>> ObterPorFornecedorAsync(int fornecedorId)
    {
        try
        {
            var associacoes = await _culturaFormaPagamentoRepository.ObterPorFornecedorAsync(fornecedorId);
            var dtos = _mapper.Map<IEnumerable<CulturaFormaPagamentoDto>>(associacoes);
            
            return Result<IEnumerable<CulturaFormaPagamentoDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<CulturaFormaPagamentoDto>>.Failure($"Erro ao obter associações do fornecedor: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<FormaPagamentoDto>>> ObterFormasPagamentoPorFornecedorCulturaAsync(
        int fornecedorId, 
        int culturaId)
    {
        try
        {
            var formasPagamento = await _culturaFormaPagamentoRepository
                .ObterFormasPagamentoPorFornecedorCulturaAsync(fornecedorId, culturaId);
            
            var dtos = _mapper.Map<IEnumerable<FormaPagamentoDto>>(formasPagamento);
            return Result<IEnumerable<FormaPagamentoDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<FormaPagamentoDto>>.Failure($"Erro ao obter formas de pagamento: {ex.Message}");
        }
    }

    public async Task<Result<CulturaFormaPagamentoDto>> CriarAsync(CriarCulturaFormaPagamentoDto dto)
    {
        try
        {
            // Verificar se já existe associação
            var associacaoExistente = await _culturaFormaPagamentoRepository
                .ObterPorFornecedorCulturaFormaPagamentoAsync(dto.FornecedorId, dto.CulturaId, dto.FormaPagamentoId);
            
            if (associacaoExistente != null)
                return Result<CulturaFormaPagamentoDto>.Failure("Associação já existe para este fornecedor, cultura e forma de pagamento");

            // Verificar se a forma de pagamento existe e está ativa
            var formaPagamentoExiste = await _formaPagamentoRepository.ExisteAtivaAsync(dto.FormaPagamentoId);
            if (!formaPagamentoExiste)
                return Result<CulturaFormaPagamentoDto>.Failure("Forma de pagamento não encontrada ou inativa");

            var associacao = _mapper.Map<CulturaFormaPagamento>(dto);
            var associacaoCriada = await _culturaFormaPagamentoRepository.AdicionarAsync(associacao);
            
            var resultDto = _mapper.Map<CulturaFormaPagamentoDto>(associacaoCriada);
            return Result<CulturaFormaPagamentoDto>.Success(resultDto);
        }
        catch (Exception ex)
        {
            return Result<CulturaFormaPagamentoDto>.Failure($"Erro ao criar associação: {ex.Message}");
        }
    }

    public async Task<Result> RemoverAsync(int id)
    {
        try
        {
            var existe = await _culturaFormaPagamentoRepository.ExisteAsync(id);
            
            if (!existe)
                return Result.Failure("Associação não encontrada");

            await _culturaFormaPagamentoRepository.RemoverAsync(id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Erro ao remover associação: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ExisteAssociacaoAtivaAsync(int fornecedorId, int culturaId, int formaPagamentoId)
    {
        try
        {
            var existe = await _culturaFormaPagamentoRepository
                .ExisteAssociacaoAtivaAsync(fornecedorId, culturaId, formaPagamentoId);
            
            return Result<bool>.Success(existe);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Erro ao verificar associação: {ex.Message}");
        }
    }
}