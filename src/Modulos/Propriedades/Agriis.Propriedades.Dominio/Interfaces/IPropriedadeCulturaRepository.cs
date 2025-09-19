using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Propriedades.Dominio.Entidades;

namespace Agriis.Propriedades.Dominio.Interfaces;

public interface IPropriedadeCulturaRepository : IRepository<PropriedadeCultura>
{
    Task<IEnumerable<PropriedadeCultura>> ObterPorPropriedadeAsync(int propriedadeId);
    Task<IEnumerable<PropriedadeCultura>> ObterPorCulturaAsync(int culturaId);
    Task<IEnumerable<PropriedadeCultura>> ObterPorSafraAsync(int safraId);
    Task<PropriedadeCultura?> ObterPorPropriedadeECulturaAsync(int propriedadeId, int culturaId);
    Task<decimal> CalcularAreaTotalPorCulturaAsync(int culturaId);
    Task<IEnumerable<PropriedadeCultura>> ObterEmPeriodoPlantioAsync();
}