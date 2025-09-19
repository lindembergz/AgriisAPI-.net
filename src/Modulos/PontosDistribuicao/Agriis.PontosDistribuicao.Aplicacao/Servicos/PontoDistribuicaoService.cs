using AutoMapper;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.PontosDistribuicao.Aplicacao.DTOs;
using Agriis.PontosDistribuicao.Dominio.Entidades;
using Agriis.PontosDistribuicao.Dominio.Interfaces;
using Agriis.Enderecos.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.PontosDistribuicao.Aplicacao.Servicos;

/// <summary>
/// Serviço de aplicação para pontos de distribuição
/// </summary>
public class PontoDistribuicaoService
{
    private readonly IPontoDistribuicaoRepository _pontoDistribuicaoRepository;
    private readonly IEnderecoRepository _enderecoRepository;
    private readonly IMunicipioRepository _municipioRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PontoDistribuicaoService> _logger;
    
    public PontoDistribuicaoService(
        IPontoDistribuicaoRepository pontoDistribuicaoRepository,
        IEnderecoRepository enderecoRepository,
        IMunicipioRepository municipioRepository,
        IMapper mapper,
        ILogger<PontoDistribuicaoService> logger)
    {
        _pontoDistribuicaoRepository = pontoDistribuicaoRepository;
        _enderecoRepository = enderecoRepository;
        _municipioRepository = municipioRepository;
        _mapper = mapper;
        _logger = logger;
    }
    
    /// <summary>
    /// Cria um novo ponto de distribuição
    /// </summary>
    /// <param name="dto">Dados do ponto de distribuição</param>
    /// <returns>Ponto de distribuição criado</returns>
    public async Task<Result<PontoDistribuicaoDto>> CriarAsync(CriarPontoDistribuicaoDto dto)
    {
        try
        {
            _logger.LogInformation("Criando ponto de distribuição {Nome} para fornecedor {FornecedorId}", 
                                 dto.Nome, dto.FornecedorId);
            
            // Verificar se já existe ponto com o mesmo nome para o fornecedor
            var existeNome = await _pontoDistribuicaoRepository.ExisteComMesmoNomeAsync(dto.FornecedorId, dto.Nome);
            if (existeNome)
            {
                return Result<PontoDistribuicaoDto>.Failure("Já existe um ponto de distribuição com este nome para o fornecedor");
            }
            
            // Verificar se o endereço existe
            var endereco = await _enderecoRepository.ObterPorIdAsync(dto.EnderecoId);
            if (endereco == null)
            {
                return Result<PontoDistribuicaoDto>.Failure("Endereço não encontrado");
            }
            
            // Criar o ponto de distribuição
            var ponto = _mapper.Map<PontoDistribuicao>(dto);
            
            // Salvar no repositório
            var pontoSalvo = await _pontoDistribuicaoRepository.AdicionarAsync(ponto);
            
            // Carregar o ponto com relacionamentos para retorno
            var pontoCompleto = await _pontoDistribuicaoRepository.ObterPorIdAsync(pontoSalvo.Id);
            var pontoDto = _mapper.Map<PontoDistribuicaoDto>(pontoCompleto);
            
            _logger.LogInformation("Ponto de distribuição {Id} criado com sucesso", pontoSalvo.Id);
            
            return Result<PontoDistribuicaoDto>.Success(pontoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar ponto de distribuição {Nome}", dto.Nome);
            return Result<PontoDistribuicaoDto>.Failure("Erro interno ao criar ponto de distribuição");
        }
    }
    
    /// <summary>
    /// Atualiza um ponto de distribuição
    /// </summary>
    /// <param name="id">ID do ponto de distribuição</param>
    /// <param name="dto">Dados atualizados</param>
    /// <returns>Ponto de distribuição atualizado</returns>
    public async Task<Result<PontoDistribuicaoDto>> AtualizarAsync(int id, AtualizarPontoDistribuicaoDto dto)
    {
        try
        {
            _logger.LogInformation("Atualizando ponto de distribuição {Id}", id);
            
            var ponto = await _pontoDistribuicaoRepository.ObterPorIdAsync(id);
            if (ponto == null)
            {
                return Result<PontoDistribuicaoDto>.Failure("Ponto de distribuição não encontrado");
            }
            
            // Verificar se já existe ponto com o mesmo nome para o fornecedor (excluindo o atual)
            var existeNome = await _pontoDistribuicaoRepository.ExisteComMesmoNomeAsync(
                ponto.FornecedorId, dto.Nome, id);
            if (existeNome)
            {
                return Result<PontoDistribuicaoDto>.Failure("Já existe um ponto de distribuição com este nome para o fornecedor");
            }
            
            // Atualizar dados básicos
            ponto.Atualizar(dto.Nome, dto.Descricao, dto.RaioCobertura, 
                           dto.CapacidadeMaxima, dto.UnidadeCapacidade, dto.Observacoes);
            
            // Atualizar cobertura territorial
            ponto.DefinirCoberturaTerritorios(dto.EstadosAtendidos, dto.MunicipiosAtendidos);
            
            // Atualizar horário de funcionamento
            if (dto.HorarioFuncionamento.Any())
            {
                ponto.DefinirHorarioFuncionamento(dto.HorarioFuncionamento);
            }
            
            // Salvar alterações
            await _pontoDistribuicaoRepository.AtualizarAsync(ponto);
            
            // Retornar ponto atualizado
            var pontoAtualizado = await _pontoDistribuicaoRepository.ObterPorIdAsync(id);
            var pontoDto = _mapper.Map<PontoDistribuicaoDto>(pontoAtualizado);
            
            _logger.LogInformation("Ponto de distribuição {Id} atualizado com sucesso", id);
            
            return Result<PontoDistribuicaoDto>.Success(pontoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar ponto de distribuição {Id}", id);
            return Result<PontoDistribuicaoDto>.Failure("Erro interno ao atualizar ponto de distribuição");
        }
    }
    
    /// <summary>
    /// Obtém um ponto de distribuição por ID
    /// </summary>
    /// <param name="id">ID do ponto de distribuição</param>
    /// <returns>Ponto de distribuição</returns>
    public async Task<Result<PontoDistribuicaoDto>> ObterPorIdAsync(int id)
    {
        try
        {
            var ponto = await _pontoDistribuicaoRepository.ObterPorIdAsync(id);
            if (ponto == null)
            {
                return Result<PontoDistribuicaoDto>.Failure("Ponto de distribuição não encontrado");
            }
            
            var pontoDto = _mapper.Map<PontoDistribuicaoDto>(ponto);
            return Result<PontoDistribuicaoDto>.Success(pontoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter ponto de distribuição {Id}", id);
            return Result<PontoDistribuicaoDto>.Failure("Erro interno ao obter ponto de distribuição");
        }
    }
    
    /// <summary>
    /// Obtém pontos de distribuição por fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="apenasAtivos">Se deve retornar apenas pontos ativos</param>
    /// <returns>Lista de pontos de distribuição</returns>
    public async Task<Result<IEnumerable<PontoDistribuicaoDto>>> ObterPorFornecedorAsync(int fornecedorId, bool apenasAtivos = true)
    {
        try
        {
            var pontos = await _pontoDistribuicaoRepository.ObterPorFornecedorAsync(fornecedorId, apenasAtivos);
            var pontosDto = _mapper.Map<IEnumerable<PontoDistribuicaoDto>>(pontos);
            
            return Result<IEnumerable<PontoDistribuicaoDto>>.Success(pontosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter pontos de distribuição do fornecedor {FornecedorId}", fornecedorId);
            return Result<IEnumerable<PontoDistribuicaoDto>>.Failure("Erro interno ao obter pontos de distribuição");
        }
    }
    
    /// <summary>
    /// Busca pontos de distribuição por localização
    /// </summary>
    /// <param name="consulta">Parâmetros da consulta</param>
    /// <returns>Lista de pontos de distribuição com distâncias</returns>
    public async Task<Result<IEnumerable<PontoDistribuicaoDto>>> BuscarPorLocalizacaoAsync(ConsultaPontosPorLocalizacaoDto consulta)
    {
        try
        {
            _logger.LogInformation("Buscando pontos de distribuição por localização");
            
            IEnumerable<PontoDistribuicao> pontos;
            
            // Se tem coordenadas, buscar por proximidade
            if (consulta.Latitude.HasValue && consulta.Longitude.HasValue)
            {
                pontos = await _pontoDistribuicaoRepository.ObterProximosAsync(
                    consulta.Latitude.Value, consulta.Longitude.Value, consulta.RaioKm, consulta.ApenasAtivos);
            }
            // Se tem estado e município, buscar por cobertura territorial
            else if (consulta.EstadoId.HasValue && consulta.MunicipioId.HasValue)
            {
                pontos = await _pontoDistribuicaoRepository.ObterQueAtendemLocalizacaoAsync(
                    consulta.EstadoId.Value, consulta.MunicipioId.Value, null, consulta.ApenasAtivos);
            }
            // Se tem apenas estado, buscar por estado
            else if (consulta.EstadoId.HasValue)
            {
                pontos = await _pontoDistribuicaoRepository.ObterPorEstadoAsync(consulta.EstadoId.Value, consulta.ApenasAtivos);
            }
            // Se tem apenas município, buscar por município
            else if (consulta.MunicipioId.HasValue)
            {
                pontos = await _pontoDistribuicaoRepository.ObterPorMunicipioAsync(consulta.MunicipioId.Value, consulta.ApenasAtivos);
            }
            else
            {
                return Result<IEnumerable<PontoDistribuicaoDto>>.Failure("Parâmetros de localização insuficientes");
            }
            
            // Filtrar por fornecedor se especificado
            if (consulta.FornecedorId.HasValue)
            {
                pontos = pontos.Where(p => p.FornecedorId == consulta.FornecedorId.Value);
            }
            
            var pontosDto = _mapper.Map<IEnumerable<PontoDistribuicaoDto>>(pontos);
            
            // Calcular distâncias se tem coordenadas
            if (consulta.Latitude.HasValue && consulta.Longitude.HasValue)
            {
                var pontosIds = pontosDto.Select(p => p.Id).ToList();
                var distancias = await _pontoDistribuicaoRepository.CalcularDistanciasAsync(
                    pontosIds, consulta.Latitude.Value, consulta.Longitude.Value);
                
                foreach (var pontoDto in pontosDto)
                {
                    if (distancias.TryGetValue(pontoDto.Id, out var distancia))
                    {
                        pontoDto.DistanciaKm = distancia;
                    }
                }
                
                // Ordenar por distância
                pontosDto = pontosDto.OrderBy(p => p.DistanciaKm ?? double.MaxValue);
            }
            
            return Result<IEnumerable<PontoDistribuicaoDto>>.Success(pontosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pontos de distribuição por localização");
            return Result<IEnumerable<PontoDistribuicaoDto>>.Failure("Erro interno ao buscar pontos de distribuição");
        }
    }
    
    /// <summary>
    /// Ativa um ponto de distribuição
    /// </summary>
    /// <param name="id">ID do ponto de distribuição</param>
    /// <returns>Resultado da operação</returns>
    public async Task<Result<bool>> AtivarAsync(int id)
    {
        try
        {
            var ponto = await _pontoDistribuicaoRepository.ObterPorIdAsync(id);
            if (ponto == null)
            {
                return Result<bool>.Failure("Ponto de distribuição não encontrado");
            }
            
            ponto.Ativar();
            await _pontoDistribuicaoRepository.AtualizarAsync(ponto);
            
            _logger.LogInformation("Ponto de distribuição {Id} ativado", id);
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar ponto de distribuição {Id}", id);
            return Result<bool>.Failure("Erro interno ao ativar ponto de distribuição");
        }
    }
    
    /// <summary>
    /// Desativa um ponto de distribuição
    /// </summary>
    /// <param name="id">ID do ponto de distribuição</param>
    /// <returns>Resultado da operação</returns>
    public async Task<Result<bool>> DesativarAsync(int id)
    {
        try
        {
            var ponto = await _pontoDistribuicaoRepository.ObterPorIdAsync(id);
            if (ponto == null)
            {
                return Result<bool>.Failure("Ponto de distribuição não encontrado");
            }
            
            ponto.Desativar();
            await _pontoDistribuicaoRepository.AtualizarAsync(ponto);
            
            _logger.LogInformation("Ponto de distribuição {Id} desativado", id);
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar ponto de distribuição {Id}", id);
            return Result<bool>.Failure("Erro interno ao desativar ponto de distribuição");
        }
    }
    
    /// <summary>
    /// Remove um ponto de distribuição
    /// </summary>
    /// <param name="id">ID do ponto de distribuição</param>
    /// <returns>Resultado da operação</returns>
    public async Task<Result<bool>> RemoverAsync(int id)
    {
        try
        {
            var ponto = await _pontoDistribuicaoRepository.ObterPorIdAsync(id);
            if (ponto == null)
            {
                return Result<bool>.Failure("Ponto de distribuição não encontrado");
            }
            
            await _pontoDistribuicaoRepository.RemoverAsync(id);
            
            _logger.LogInformation("Ponto de distribuição {Id} removido", id);
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover ponto de distribuição {Id}", id);
            return Result<bool>.Failure("Erro interno ao remover ponto de distribuição");
        }
    }
    
    /// <summary>
    /// Obtém estatísticas de pontos de distribuição por fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Estatísticas dos pontos</returns>
    public async Task<Result<EstatisticasPontosDistribuicaoDto>> ObterEstatisticasAsync(int fornecedorId)
    {
        try
        {
            var estatisticas = await _pontoDistribuicaoRepository.ObterEstatisticasPorFornecedorAsync(fornecedorId);
            var estatisticasDto = _mapper.Map<EstatisticasPontosDistribuicaoDto>(estatisticas);
            
            return Result<EstatisticasPontosDistribuicaoDto>.Success(estatisticasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas de pontos do fornecedor {FornecedorId}", fornecedorId);
            return Result<EstatisticasPontosDistribuicaoDto>.Failure("Erro interno ao obter estatísticas");
        }
    }
}