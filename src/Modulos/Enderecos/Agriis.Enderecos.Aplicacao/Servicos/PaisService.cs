using AutoMapper;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Enderecos.Aplicacao.DTOs;
using Agriis.Enderecos.Aplicacao.Interfaces;
using Agriis.Enderecos.Dominio.Entidades;
using Agriis.Enderecos.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.Enderecos.Aplicacao.Servicos;

/// <summary>
/// Serviço de aplicação para países
/// </summary>
public class PaisService : IPaisService
{
    private readonly IPaisRepository _paisRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PaisService> _logger;

    public PaisService(
        IPaisRepository paisRepository,
        IMapper mapper,
        ILogger<PaisService> logger)
    {
        _paisRepository = paisRepository ?? throw new ArgumentNullException(nameof(paisRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaisDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var pais = await _paisRepository.ObterPorIdAsync(id, cancellationToken);
            return pais != null ? _mapper.Map<PaisDto>(pais) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter país por ID {Id}", id);
            throw;
        }
    }

    public async Task<PaisDto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        try
        {
            var pais = await _paisRepository.ObterPorCodigoAsync(codigo, cancellationToken);
            return pais != null ? _mapper.Map<PaisDto>(pais) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter país por código {Codigo}", codigo);
            throw;
        }
    }

    public async Task<IEnumerable<PaisDto>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var paises = await _paisRepository.ObterAtivosAsync(cancellationToken);
            return _mapper.Map<IEnumerable<PaisDto>>(paises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter países ativos");
            throw;
        }
    }

    public async Task<IEnumerable<PaisDto>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        try
        {
            var paises = await _paisRepository.BuscarPorNomeAsync(nome, cancellationToken);
            return _mapper.Map<IEnumerable<PaisDto>>(paises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar países por nome {Nome}", nome);
            throw;
        }
    }

    public async Task<IEnumerable<PaisDto>> ObterTodosComEstadosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var paises = await _paisRepository.ObterTodosComEstadosAsync(cancellationToken);
            return _mapper.Map<IEnumerable<PaisDto>>(paises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter todos os países com estados");
            throw;
        }
    }

    public async Task<IEnumerable<PaisDto>> ObterAtivosComEstadosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var paises = await _paisRepository.ObterAtivosComEstadosAsync(cancellationToken);
            return _mapper.Map<IEnumerable<PaisDto>>(paises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter países ativos com estados");
            throw;
        }
    }

    public async Task<IEnumerable<PaisComContadorDto>> ObterTodosAsync(int pagina = 1, int tamanhoPagina = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var paises = await _paisRepository.ObterTodosAsync(pagina, tamanhoPagina, cancellationToken);
            return _mapper.Map<IEnumerable<PaisComContadorDto>>(paises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter todos os países com paginação");
            throw;
        }
    }

    public async Task<Result<PaisDto>> CriarAsync(CriarPaisDto criarPaisDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar se já existe país com o mesmo código
            if (await _paisRepository.ExistePorCodigoAsync(criarPaisDto.Codigo, cancellationToken: cancellationToken))
            {
                return Result<PaisDto>.Failure($"Já existe um país com o código '{criarPaisDto.Codigo}'");
            }

            // Validar se já existe país com o mesmo nome
            if (await _paisRepository.ExistePorNomeAsync(criarPaisDto.Nome, cancellationToken: cancellationToken))
            {
                return Result<PaisDto>.Failure($"Já existe um país com o nome '{criarPaisDto.Nome}'");
            }

            var pais = _mapper.Map<Pais>(criarPaisDto);
            pais = await _paisRepository.AdicionarAsync(pais, cancellationToken);
            
            var paisDto = _mapper.Map<PaisDto>(pais);
            
            _logger.LogInformation("País criado com sucesso: {Nome} ({Codigo})", pais.Nome, pais.Codigo);
            
            return Result<PaisDto>.Success(paisDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar país");
            return Result<PaisDto>.Failure("Erro interno do servidor ao criar país");
        }
    }

    public async Task<Result<PaisDto>> AtualizarAsync(int id, AtualizarPaisDto atualizarPaisDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var pais = await _paisRepository.ObterPorIdAsync(id, cancellationToken);
            if (pais == null)
            {
                return Result<PaisDto>.Failure("País não encontrado");
            }

            // Validar se já existe país com o mesmo código (excluindo o atual)
            if (await _paisRepository.ExistePorCodigoAsync(atualizarPaisDto.Codigo, id, cancellationToken))
            {
                return Result<PaisDto>.Failure($"Já existe outro país com o código '{atualizarPaisDto.Codigo}'");
            }

            // Validar se já existe país com o mesmo nome (excluindo o atual)
            if (await _paisRepository.ExistePorNomeAsync(atualizarPaisDto.Nome, id, cancellationToken))
            {
                return Result<PaisDto>.Failure($"Já existe outro país com o nome '{atualizarPaisDto.Nome}'");
            }

            pais.AtualizarInformacoes(atualizarPaisDto.Nome, atualizarPaisDto.Codigo);
            await _paisRepository.AtualizarAsync(pais, cancellationToken);
            
            var paisDto = _mapper.Map<PaisDto>(pais);
            
            _logger.LogInformation("País atualizado com sucesso: {Id} - {Nome}", id, pais.Nome);
            
            return Result<PaisDto>.Success(paisDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar país {Id}", id);
            return Result<PaisDto>.Failure("Erro interno do servidor ao atualizar país");
        }
    }

    public async Task<Result> ExcluirAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var pais = await _paisRepository.ObterPorIdAsync(id, cancellationToken);
            if (pais == null)
            {
                return Result.Failure("País não encontrado");
            }

            // Verificar se o país possui estados associados
            var possuiEstados = await _paisRepository.PossuiEstadosAsync(id, cancellationToken);
            if (possuiEstados)
            {
                return Result.Failure("Não é possível excluir o país pois existem estados associados a ele");
            }

            await _paisRepository.RemoverAsync(pais, cancellationToken);
            
            _logger.LogInformation("País excluído com sucesso: {Id} - {Nome}", id, pais.Nome);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir país {Id}", id);
            return Result.Failure("Erro interno do servidor ao excluir país");
        }
    }

    public async Task<Result> AtivarAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var pais = await _paisRepository.ObterPorIdAsync(id, cancellationToken);
            if (pais == null)
            {
                return Result.Failure("País não encontrado");
            }

            if (pais.Ativo)
            {
                return Result.Failure("País já está ativo");
            }

            pais.Ativar();
            await _paisRepository.AtualizarAsync(pais, cancellationToken);
            
            _logger.LogInformation("País ativado com sucesso: {Id} - {Nome}", id, pais.Nome);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar país {Id}", id);
            return Result.Failure("Erro interno do servidor ao ativar país");
        }
    }

    public async Task<Result> DesativarAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var pais = await _paisRepository.ObterPorIdAsync(id, cancellationToken);
            if (pais == null)
            {
                return Result.Failure("País não encontrado");
            }

            if (!pais.Ativo)
            {
                return Result.Failure("País já está inativo");
            }

            pais.Desativar();
            await _paisRepository.AtualizarAsync(pais, cancellationToken);
            
            _logger.LogInformation("País desativado com sucesso: {Id} - {Nome}", id, pais.Nome);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar país {Id}", id);
            return Result.Failure("Erro interno do servidor ao desativar país");
        }
    }

    public async Task<bool> ExistePorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _paisRepository.ExistePorCodigoAsync(codigo, null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar se existe país por código {Codigo}", codigo);
            throw;
        }
    }

    public async Task<bool> ExistePorNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _paisRepository.ExistePorNomeAsync(nome, idExcluir, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar se existe país por nome {Nome}", nome);
            throw;
        }
    }
}