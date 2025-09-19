using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Propriedades.Dominio.Entidades;
using Agriis.Propriedades.Dominio.Interfaces;
using NetTopologySuite.Geometries;

namespace Agriis.Propriedades.Dominio.Servicos;

public class PropriedadeDomainService
{
    private readonly IPropriedadeRepository _propriedadeRepository;
    private readonly ITalhaoRepository _talhaoRepository;
    private readonly IPropriedadeCulturaRepository _propriedadeCulturaRepository;

    public PropriedadeDomainService(
        IPropriedadeRepository propriedadeRepository,
        ITalhaoRepository talhaoRepository,
        IPropriedadeCulturaRepository propriedadeCulturaRepository)
    {
        _propriedadeRepository = propriedadeRepository;
        _talhaoRepository = talhaoRepository;
        _propriedadeCulturaRepository = propriedadeCulturaRepository;
    }

    public async Task<AreaPlantio> CalcularAreaTotalProdutorAsync(int produtorId)
    {
        var areaTotal = await _propriedadeRepository.CalcularAreaTotalPorProdutorAsync(produtorId);
        return new AreaPlantio(areaTotal);
    }

    public async Task<AreaPlantio> CalcularAreaTotalCulturaAsync(int culturaId)
    {
        var areaTotal = await _propriedadeCulturaRepository.CalcularAreaTotalPorCulturaAsync(culturaId);
        return new AreaPlantio(areaTotal);
    }

    public async Task<bool> ValidarAreaTalhaoAsync(int propriedadeId, AreaPlantio areaTalhao)
    {
        var propriedade = await _propriedadeRepository.ObterPorIdAsync(propriedadeId);
        if (propriedade == null) return false;

        var areaTotalTalhoes = await _talhaoRepository.CalcularAreaTotalPorPropriedadeAsync(propriedadeId);
        var novaAreaTotal = areaTotalTalhoes + areaTalhao.Valor;

        return novaAreaTotal <= propriedade.AreaTotal.Valor;
    }

    public async Task<bool> ValidarAreaCulturaAsync(int propriedadeId, AreaPlantio areaCultura)
    {
        var propriedade = await _propriedadeRepository.ObterPorIdAsync(propriedadeId);
        if (propriedade == null) return false;

        var areaTotalCulturas = propriedade.CalcularAreaTotalCulturas();
        var novaAreaTotal = areaTotalCulturas.Valor + areaCultura.Valor;

        return novaAreaTotal <= propriedade.AreaTotal.Valor;
    }

    public async Task<IEnumerable<Propriedade>> BuscarPropriedadesProximasAsync(
        Point localizacao, double raioKm)
    {
        return await _propriedadeRepository.ObterPropriedadesProximasAsync(localizacao, raioKm);
    }

    public async Task<IEnumerable<Talhao>> BuscarTalhoesProximosAsync(
        Point localizacao, double raioKm)
    {
        return await _talhaoRepository.ObterTalhoesProximosAsync(localizacao, raioKm);
    }

    public async Task<Dictionary<int, decimal>> ObterEstatisticasCulturasPorProdutorAsync(int produtorId)
    {
        var propriedades = await _propriedadeRepository.ObterPorProdutorAsync(produtorId);
        var estatisticas = new Dictionary<int, decimal>();

        foreach (var propriedade in propriedades)
        {
            foreach (var propriedadeCultura in propriedade.PropriedadeCulturas)
            {
                if (estatisticas.ContainsKey(propriedadeCultura.CulturaId))
                {
                    estatisticas[propriedadeCultura.CulturaId] += propriedadeCultura.Area.Valor;
                }
                else
                {
                    estatisticas[propriedadeCultura.CulturaId] = propriedadeCultura.Area.Valor;
                }
            }
        }

        return estatisticas;
    }

    public async Task<bool> PodeRemoverPropriedadeAsync(int propriedadeId)
    {
        // Verificar se a propriedade tem talhões
        var talhoes = await _talhaoRepository.ObterPorPropriedadeAsync(propriedadeId);
        if (talhoes.Any()) return false;

        // Verificar se a propriedade tem culturas ativas
        var culturas = await _propriedadeCulturaRepository.ObterPorPropriedadeAsync(propriedadeId);
        var culturasAtivas = culturas.Where(c => c.EstaEmPeriodoPlantio());
        
        return !culturasAtivas.Any();
    }
}