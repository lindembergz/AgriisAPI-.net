using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Segmentacoes.Aplicacao.DTOs;
using Agriis.Segmentacoes.Aplicacao.Interfaces;
using Agriis.Segmentacoes.Dominio.Servicos;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Agriis.Segmentacoes.Aplicacao.Servicos;

/// <summary>
/// Implementação do serviço de cálculo de desconto segmentado
/// </summary>
public class CalculoDescontoService : ICalculoDescontoService
{
    private readonly CalculoDescontoSegmentadoService _calculoDescontoSegmentadoService;
    private readonly IMapper _mapper;
    private readonly ILogger<CalculoDescontoService> _logger;
    
    public CalculoDescontoService(
        CalculoDescontoSegmentadoService calculoDescontoSegmentadoService,
        IMapper mapper,
        ILogger<CalculoDescontoService> logger)
    {
        _calculoDescontoSegmentadoService = calculoDescontoSegmentadoService;
        _mapper = mapper;
        _logger = logger;
    }
    
    /// <summary>
    /// Calcula o desconto segmentado para um produtor, fornecedor e categoria
    /// </summary>
    public async Task<Result<ResultadoDescontoSegmentadoDto>> CalcularDescontoSegmentadoAsync(
        int produtorId, 
        int fornecedorId, 
        int categoriaId, 
        decimal areaProdutor, 
        decimal valorBase)
    {
        try
        {
            if (areaProdutor < 0)
                return Result<ResultadoDescontoSegmentadoDto>.Failure("Área do produtor deve ser positiva");
                
            if (valorBase <= 0)
                return Result<ResultadoDescontoSegmentadoDto>.Failure("Valor base deve ser positivo");
            
            var resultado = await _calculoDescontoSegmentadoService.CalcularDescontoAsync(
                produtorId, fornecedorId, categoriaId, areaProdutor, valorBase);
            
            var resultadoDto = _mapper.Map<ResultadoDescontoSegmentadoDto>(resultado);
            
            _logger.LogInformation(
                "Desconto calculado: Produtor {ProdutorId}, Fornecedor {FornecedorId}, Categoria {CategoriaId}, " +
                "Área {Area}, Valor Base {ValorBase}, Desconto {Desconto}%",
                produtorId, fornecedorId, categoriaId, areaProdutor, valorBase, resultado.PercentualDesconto);
            
            return Result<ResultadoDescontoSegmentadoDto>.Success(resultadoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Erro ao calcular desconto segmentado: Produtor {ProdutorId}, Fornecedor {FornecedorId}, " +
                "Categoria {CategoriaId}, Área {Area}, Valor {ValorBase}",
                produtorId, fornecedorId, categoriaId, areaProdutor, valorBase);
            
            return Result<ResultadoDescontoSegmentadoDto>.Failure("Erro interno do servidor");
        }
    }
    
    /// <summary>
    /// Valida se uma área se enquadra em algum grupo de uma segmentação
    /// </summary>
    public async Task<Result<bool>> ValidarAreaSeEnquadraAsync(int segmentacaoId, decimal area)
    {
        try
        {
            if (area < 0)
                return Result<bool>.Failure("Área deve ser positiva");
            
            var seEnquadra = await _calculoDescontoSegmentadoService.ValidarAreaSeEnquadraAsync(segmentacaoId, area);
            
            return Result<bool>.Success(seEnquadra);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar área {Area} para segmentação {SegmentacaoId}", area, segmentacaoId);
            return Result<bool>.Failure("Erro interno do servidor");
        }
    }
    
    /// <summary>
    /// Obtém grupos aplicáveis para uma área específica
    /// </summary>
    public async Task<Result<IEnumerable<GrupoDto>>> ObterGruposAplicaveisAsync(int segmentacaoId, decimal area)
    {
        try
        {
            if (area < 0)
                return Result<IEnumerable<GrupoDto>>.Failure("Área deve ser positiva");
            
            var grupos = await _calculoDescontoSegmentadoService.ObterGruposAplicaveisAsync(segmentacaoId, area);
            var gruposDto = _mapper.Map<IEnumerable<GrupoDto>>(grupos);
            
            return Result<IEnumerable<GrupoDto>>.Success(gruposDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter grupos aplicáveis para área {Area} na segmentação {SegmentacaoId}", 
                area, segmentacaoId);
            return Result<IEnumerable<GrupoDto>>.Failure("Erro interno do servidor");
        }
    }
}