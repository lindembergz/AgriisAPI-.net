using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Segmentacoes.Aplicacao.DTOs;
using Agriis.Segmentacoes.Aplicacao.Interfaces;
using Agriis.Segmentacoes.Dominio.Entidades;
using Agriis.Segmentacoes.Dominio.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Agriis.Segmentacoes.Aplicacao.Servicos;

/// <summary>
/// Implementação do serviço de segmentações
/// </summary>
public class SegmentacaoService : ISegmentacaoService
{
    private readonly ISegmentacaoRepository _segmentacaoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SegmentacaoService> _logger;
    
    public SegmentacaoService(
        ISegmentacaoRepository segmentacaoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SegmentacaoService> logger)
    {
        _segmentacaoRepository = segmentacaoRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }
    
    /// <summary>
    /// Obtém todas as segmentações de um fornecedor
    /// </summary>
    public async Task<Result<IEnumerable<SegmentacaoDto>>> ObterPorFornecedorAsync(int fornecedorId)
    {
        try
        {
            var segmentacoes = await _segmentacaoRepository.ObterPorFornecedorAsync(fornecedorId);
            var segmentacoesDto = _mapper.Map<IEnumerable<SegmentacaoDto>>(segmentacoes);
            
            return Result<IEnumerable<SegmentacaoDto>>.Success(segmentacoesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter segmentações do fornecedor {FornecedorId}", fornecedorId);
            return Result<IEnumerable<SegmentacaoDto>>.Failure("Erro interno do servidor");
        }
    }
    
    /// <summary>
    /// Obtém segmentação por ID
    /// </summary>
    public async Task<Result<SegmentacaoDto>> ObterPorIdAsync(int id)
    {
        try
        {
            var segmentacao = await _segmentacaoRepository.ObterPorIdAsync(id);
            
            if (segmentacao == null)
                return Result<SegmentacaoDto>.Failure("Segmentação não encontrada");
            
            var segmentacaoDto = _mapper.Map<SegmentacaoDto>(segmentacao);
            return Result<SegmentacaoDto>.Success(segmentacaoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter segmentação {Id}", id);
            return Result<SegmentacaoDto>.Failure("Erro interno do servidor");
        }
    }
    
    /// <summary>
    /// Obtém segmentação completa com grupos e descontos
    /// </summary>
    public async Task<Result<SegmentacaoDto>> ObterCompletaAsync(int id)
    {
        try
        {
            var segmentacao = await _segmentacaoRepository.ObterCompletaAsync(id);
            
            if (segmentacao == null)
                return Result<SegmentacaoDto>.Failure("Segmentação não encontrada");
            
            var segmentacaoDto = _mapper.Map<SegmentacaoDto>(segmentacao);
            return Result<SegmentacaoDto>.Success(segmentacaoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter segmentação completa {Id}", id);
            return Result<SegmentacaoDto>.Failure("Erro interno do servidor");
        }
    }
    
    /// <summary>
    /// Obtém a segmentação padrão de um fornecedor
    /// </summary>
    public async Task<Result<SegmentacaoDto>> ObterPadraoAsync(int fornecedorId)
    {
        try
        {
            var segmentacao = await _segmentacaoRepository.ObterPadraoAsync(fornecedorId);
            
            if (segmentacao == null)
                return Result<SegmentacaoDto>.Failure("Segmentação padrão não encontrada");
            
            var segmentacaoDto = _mapper.Map<SegmentacaoDto>(segmentacao);
            return Result<SegmentacaoDto>.Success(segmentacaoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter segmentação padrão do fornecedor {FornecedorId}", fornecedorId);
            return Result<SegmentacaoDto>.Failure("Erro interno do servidor");
        }
    }
    
    /// <summary>
    /// Cria uma nova segmentação
    /// </summary>
    public async Task<Result<SegmentacaoDto>> CriarAsync(CriarSegmentacaoDto dto)
    {
        try
        {
            // Validar se já existe segmentação padrão quando está criando uma nova padrão
            if (dto.EhPadrao)
            {
                var existePadrao = await _segmentacaoRepository.ExistePadraoAsync(dto.FornecedorId);
                if (existePadrao)
                    return Result<SegmentacaoDto>.Failure("Já existe uma segmentação padrão para este fornecedor");
            }
            
            var segmentacao = _mapper.Map<Segmentacao>(dto);
            
            await _segmentacaoRepository.AdicionarAsync(segmentacao);
            await _unitOfWork.SalvarAlteracoesAsync();
            
            var segmentacaoDto = _mapper.Map<SegmentacaoDto>(segmentacao);
            
            _logger.LogInformation("Segmentação {Nome} criada com sucesso para fornecedor {FornecedorId}", 
                segmentacao.Nome, segmentacao.FornecedorId);
            
            return Result<SegmentacaoDto>.Success(segmentacaoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar segmentação {Nome}", dto.Nome);
            return Result<SegmentacaoDto>.Failure("Erro interno do servidor");
        }
    }
    
    /// <summary>
    /// Atualiza uma segmentação
    /// </summary>
    public async Task<Result<SegmentacaoDto>> AtualizarAsync(int id, AtualizarSegmentacaoDto dto)
    {
        try
        {
            var segmentacao = await _segmentacaoRepository.ObterPorIdAsync(id);
            
            if (segmentacao == null)
                return Result<SegmentacaoDto>.Failure("Segmentação não encontrada");
            
            // Validar se já existe segmentação padrão quando está definindo como padrão
            if (dto.EhPadrao && !segmentacao.EhPadrao)
            {
                var existePadrao = await _segmentacaoRepository.ExistePadraoAsync(segmentacao.FornecedorId, id);
                if (existePadrao)
                    return Result<SegmentacaoDto>.Failure("Já existe uma segmentação padrão para este fornecedor");
            }
            
            segmentacao.AtualizarInformacoes(dto.Nome, dto.Descricao);
            
            if (dto.ConfiguracaoTerritorial != null)
                segmentacao.DefinirConfiguracaoTerritorial(dto.ConfiguracaoTerritorial);
            
            if (dto.EhPadrao && !segmentacao.EhPadrao)
                segmentacao.DefinirComoPadrao();
            else if (!dto.EhPadrao && segmentacao.EhPadrao)
                segmentacao.RemoverComoPadrao();
            
            if (dto.Ativo && !segmentacao.Ativo)
                segmentacao.Ativar();
            else if (!dto.Ativo && segmentacao.Ativo)
                segmentacao.Desativar();
            
            await _segmentacaoRepository.AtualizarAsync(segmentacao);
            await _unitOfWork.SalvarAlteracoesAsync();
            
            var segmentacaoDto = _mapper.Map<SegmentacaoDto>(segmentacao);
            
            _logger.LogInformation("Segmentação {Id} atualizada com sucesso", id);
            
            return Result<SegmentacaoDto>.Success(segmentacaoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar segmentação {Id}", id);
            return Result<SegmentacaoDto>.Failure("Erro interno do servidor");
        }
    }
    
    /// <summary>
    /// Remove uma segmentação
    /// </summary>
    public async Task<Result> RemoverAsync(int id)
    {
        try
        {
            var segmentacao = await _segmentacaoRepository.ObterPorIdAsync(id);
            
            if (segmentacao == null)
                return Result.Failure("Segmentação não encontrada");
            
            await _segmentacaoRepository.RemoverAsync(id);
            await _unitOfWork.SalvarAlteracoesAsync();
            
            _logger.LogInformation("Segmentação {Id} removida com sucesso", id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover segmentação {Id}", id);
            return Result.Failure("Erro interno do servidor");
        }
    }
    
    /// <summary>
    /// Ativa uma segmentação
    /// </summary>
    public async Task<Result> AtivarAsync(int id)
    {
        try
        {
            var segmentacao = await _segmentacaoRepository.ObterPorIdAsync(id);
            
            if (segmentacao == null)
                return Result.Failure("Segmentação não encontrada");
            
            segmentacao.Ativar();
            
            await _segmentacaoRepository.AtualizarAsync(segmentacao);
            await _unitOfWork.SalvarAlteracoesAsync();
            
            _logger.LogInformation("Segmentação {Id} ativada com sucesso", id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar segmentação {Id}", id);
            return Result.Failure("Erro interno do servidor");
        }
    }
    
    /// <summary>
    /// Desativa uma segmentação
    /// </summary>
    public async Task<Result> DesativarAsync(int id)
    {
        try
        {
            var segmentacao = await _segmentacaoRepository.ObterPorIdAsync(id);
            
            if (segmentacao == null)
                return Result.Failure("Segmentação não encontrada");
            
            segmentacao.Desativar();
            
            await _segmentacaoRepository.AtualizarAsync(segmentacao);
            await _unitOfWork.SalvarAlteracoesAsync();
            
            _logger.LogInformation("Segmentação {Id} desativada com sucesso", id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar segmentação {Id}", id);
            return Result.Failure("Erro interno do servidor");
        }
    }
    
    /// <summary>
    /// Define uma segmentação como padrão
    /// </summary>
    public async Task<Result> DefinirComoPadraoAsync(int id)
    {
        try
        {
            var segmentacao = await _segmentacaoRepository.ObterPorIdAsync(id);
            
            if (segmentacao == null)
                return Result.Failure("Segmentação não encontrada");
            
            if (segmentacao.EhPadrao)
                return Result.Failure("Segmentação já é padrão");
            
            segmentacao.DefinirComoPadrao();
            
            await _segmentacaoRepository.AtualizarAsync(segmentacao);
            await _unitOfWork.SalvarAlteracoesAsync();
            
            _logger.LogInformation("Segmentação {Id} definida como padrão", id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir segmentação {Id} como padrão", id);
            return Result.Failure("Erro interno do servidor");
        }
    }
}