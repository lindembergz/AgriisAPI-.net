using AutoMapper;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Safras.Aplicacao.DTOs;
using Agriis.Safras.Aplicacao.Interfaces;
using Agriis.Safras.Dominio.Entidades;
using Agriis.Safras.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.Safras.Aplicacao.Servicos;

/// <summary>
/// Implementação do serviço de Safras
/// </summary>
public class SafraService : ISafraService
{
    private readonly ISafraRepository _safraRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SafraService> _logger;

    public SafraService(
        ISafraRepository safraRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SafraService> logger)
    {
        _safraRepository = safraRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<SafraDto>> ObterPorIdAsync(int id)
    {
        try
        {
            var safra = await _safraRepository.ObterPorIdAsync(id);
            if (safra == null)
            {
                return Result<SafraDto>.Failure("Safra não encontrada");
            }

            var dto = _mapper.Map<SafraDto>(safra);
            return Result<SafraDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter safra por ID {Id}", id);
            return Result<SafraDto>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<IEnumerable<SafraDto>>> ObterTodasAsync()
    {
        try
        {
            var safras = await _safraRepository.ObterTodasOrdenadasAsync();
            var dtos = _mapper.Map<IEnumerable<SafraDto>>(safras);
            return Result<IEnumerable<SafraDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter todas as safras");
            return Result<IEnumerable<SafraDto>>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<SafraAtualDto?>> ObterSafraAtualAsync()
    {
        try
        {
            var safraAtual = await _safraRepository.ObterSafraAtualAsync();
            if (safraAtual == null)
            {
                return Result<SafraAtualDto?>.Success(null);
            }

            var dto = _mapper.Map<SafraAtualDto>(safraAtual);
            return Result<SafraAtualDto?>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter safra atual");
            return Result<SafraAtualDto?>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<IEnumerable<SafraDto>>> ObterPorAnoColheitaAsync(int anoColheita)
    {
        try
        {
            var safras = await _safraRepository.ObterPorAnoColheitaAsync(anoColheita);
            var dtos = _mapper.Map<IEnumerable<SafraDto>>(safras);
            return Result<IEnumerable<SafraDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter safras por ano de colheita {Ano}", anoColheita);
            return Result<IEnumerable<SafraDto>>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<SafraDto>> CriarAsync(CriarSafraDto dto)
    {
        try
        {
            // Validar se já existe safra com o mesmo período
            var existeConflito = await _safraRepository.ExisteConflitoPeriodoAsync(
                dto.PlantioInicial, dto.PlantioFinal, dto.PlantioNome);
            
            if (existeConflito)
            {
                return Result<SafraDto>.Failure("Já existe uma safra com período conflitante");
            }

            var safra = new Safra(dto.PlantioInicial, dto.PlantioFinal, dto.PlantioNome, dto.Descricao);
            await _safraRepository.AdicionarAsync(safra);
            await _unitOfWork.SalvarAlteracoesAsync();

            var safraDto = _mapper.Map<SafraDto>(safra);
            _logger.LogInformation("Safra criada com sucesso: {Descricao}", safra.Descricao);
            
            return Result<SafraDto>.Success(safraDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar safra: {Descricao}", dto.Descricao);
            return Result<SafraDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar safra: {Descricao}", dto.Descricao);
            return Result<SafraDto>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<SafraDto>> AtualizarAsync(int id, AtualizarSafraDto dto)
    {
        try
        {
            var safra = await _safraRepository.ObterPorIdAsync(id);
            if (safra == null)
            {
                return Result<SafraDto>.Failure("Safra não encontrada");
            }

            // Validar se já existe safra com o mesmo período (excluindo a atual)
            var existeConflito = await _safraRepository.ExisteConflitoPeriodoAsync(
                dto.PlantioInicial, dto.PlantioFinal, dto.PlantioNome, id);
            
            if (existeConflito)
            {
                return Result<SafraDto>.Failure("Já existe uma safra com período conflitante");
            }

            safra.Atualizar(dto.PlantioInicial, dto.PlantioFinal, dto.PlantioNome, dto.Descricao);
            
            await _safraRepository.AtualizarAsync(safra);
            await _unitOfWork.SalvarAlteracoesAsync();

            var safraDto = _mapper.Map<SafraDto>(safra);
            _logger.LogInformation("Safra atualizada com sucesso: {Id} - {Descricao}", id, safra.Descricao);
            
            return Result<SafraDto>.Success(safraDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao atualizar safra: {Id}", id);
            return Result<SafraDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar safra: {Id}", id);
            return Result<SafraDto>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result> RemoverAsync(int id)
    {
        try
        {
            var safra = await _safraRepository.ObterPorIdAsync(id);
            if (safra == null)
            {
                return Result.Failure("Safra não encontrada");
            }

            await _safraRepository.RemoverAsync(id);
            await _unitOfWork.SalvarAlteracoesAsync();

            _logger.LogInformation("Safra removida com sucesso: {Id} - {Descricao}", id, safra.Descricao);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover safra: {Id}", id);
            return Result.Failure("Erro interno do servidor");
        }
    }
}