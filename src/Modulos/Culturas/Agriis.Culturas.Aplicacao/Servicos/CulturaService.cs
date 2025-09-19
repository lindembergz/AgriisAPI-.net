using AutoMapper;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Culturas.Aplicacao.DTOs;
using Agriis.Culturas.Aplicacao.Interfaces;
using Agriis.Culturas.Dominio.Entidades;
using Agriis.Culturas.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.Culturas.Aplicacao.Servicos;

public class CulturaService : ICulturaService
{
    private readonly ICulturaRepository _culturaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CulturaService> _logger;

    public CulturaService(
        ICulturaRepository culturaRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CulturaService> logger)
    {
        _culturaRepository = culturaRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CulturaDto>> ObterPorIdAsync(int id)
    {
        try
        {
            var cultura = await _culturaRepository.ObterPorIdAsync(id);
            if (cultura == null)
            {
                return Result<CulturaDto>.Failure("Cultura não encontrada");
            }

            var dto = _mapper.Map<CulturaDto>(cultura);
            return Result<CulturaDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter cultura por ID {Id}", id);
            return Result<CulturaDto>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<IEnumerable<CulturaDto>>> ObterTodasAsync()
    {
        try
        {
            var culturas = await _culturaRepository.ObterTodosAsync();
            var culturasOrdenadas = culturas.OrderBy(c => c.Nome);
            var dtos = _mapper.Map<IEnumerable<CulturaDto>>(culturasOrdenadas);
            return Result<IEnumerable<CulturaDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter todas as culturas");
            return Result<IEnumerable<CulturaDto>>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<IEnumerable<CulturaDto>>> ObterAtivasAsync()
    {
        try
        {
            var culturas = await _culturaRepository.ObterAtivasAsync();
            var culturasOrdenadas = culturas.OrderBy(c => c.Nome);
            var dtos = _mapper.Map<IEnumerable<CulturaDto>>(culturasOrdenadas);
            return Result<IEnumerable<CulturaDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter culturas ativas");
            return Result<IEnumerable<CulturaDto>>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<CulturaDto>> CriarAsync(CriarCulturaDto dto)
    {
        try
        {
            // Validar se já existe cultura com o mesmo nome
            var existeNome = await _culturaRepository.ExisteComNomeAsync(dto.Nome);
            if (existeNome)
            {
                return Result<CulturaDto>.Failure("Já existe uma cultura com este nome");
            }

            var cultura = new Cultura(dto.Nome, dto.Descricao);
            await _culturaRepository.AdicionarAsync(cultura);
            await _unitOfWork.SalvarAlteracoesAsync();

            var culturaDto = _mapper.Map<CulturaDto>(cultura);
            _logger.LogInformation("Cultura criada com sucesso: {Nome}", cultura.Nome);
            
            return Result<CulturaDto>.Success(culturaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar cultura: {Nome}", dto.Nome);
            return Result<CulturaDto>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<CulturaDto>> AtualizarAsync(int id, AtualizarCulturaDto dto)
    {
        try
        {
            var cultura = await _culturaRepository.ObterPorIdAsync(id);
            if (cultura == null)
            {
                return Result<CulturaDto>.Failure("Cultura não encontrada");
            }

            // Validar se já existe cultura com o mesmo nome (excluindo a atual)
            var existeNome = await _culturaRepository.ExisteComNomeAsync(dto.Nome, id);
            if (existeNome)
            {
                return Result<CulturaDto>.Failure("Já existe uma cultura com este nome");
            }

            cultura.AtualizarNome(dto.Nome);
            cultura.AtualizarDescricao(dto.Descricao);
            
            if (dto.Ativo && !cultura.Ativo)
                cultura.Ativar();
            else if (!dto.Ativo && cultura.Ativo)
                cultura.Desativar();

            await _culturaRepository.AtualizarAsync(cultura);
            await _unitOfWork.SalvarAlteracoesAsync();

            var culturaDto = _mapper.Map<CulturaDto>(cultura);
            _logger.LogInformation("Cultura atualizada com sucesso: {Id} - {Nome}", id, cultura.Nome);
            
            return Result<CulturaDto>.Success(culturaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar cultura: {Id}", id);
            return Result<CulturaDto>.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result> RemoverAsync(int id)
    {
        try
        {
            var cultura = await _culturaRepository.ObterPorIdAsync(id);
            if (cultura == null)
            {
                return Result.Failure("Cultura não encontrada");
            }

            await _culturaRepository.RemoverAsync(id);
            await _unitOfWork.SalvarAlteracoesAsync();

            _logger.LogInformation("Cultura removida com sucesso: {Id} - {Nome}", id, cultura.Nome);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover cultura: {Id}", id);
            return Result.Failure("Erro interno do servidor");
        }
    }

    public async Task<Result<CulturaDto>> ObterPorNomeAsync(string nome)
    {
        try
        {
            var cultura = await _culturaRepository.ObterPorNomeAsync(nome);
            if (cultura == null)
            {
                return Result<CulturaDto>.Failure("Cultura não encontrada");
            }

            var dto = _mapper.Map<CulturaDto>(cultura);
            return Result<CulturaDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter cultura por nome: {Nome}", nome);
            return Result<CulturaDto>.Failure("Erro interno do servidor");
        }
    }
}