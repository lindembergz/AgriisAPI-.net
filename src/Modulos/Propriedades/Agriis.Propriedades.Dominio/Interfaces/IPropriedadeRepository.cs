using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Propriedades.Dominio.Entidades;
using NetTopologySuite.Geometries;

namespace Agriis.Propriedades.Dominio.Interfaces;

public interface IPropriedadeRepository : IRepository<Propriedade>
{
    Task<IEnumerable<Propriedade>> ObterPorProdutorAsync(int produtorId);
    Task<IEnumerable<Propriedade>> ObterPorCulturaAsync(int culturaId);
    Task<IEnumerable<Propriedade>> ObterPorRegiao(Point centro, double raioKm);
    Task<Propriedade?> ObterComTalhoesAsync(int propriedadeId);
    Task<Propriedade?> ObterComCulturasAsync(int propriedadeId);
    Task<Propriedade?> ObterCompletaAsync(int propriedadeId);
    Task<decimal> CalcularAreaTotalPorProdutorAsync(int produtorId);
    Task<decimal> CalcularAreaTotalPorCulturaAsync(int culturaId);
    Task<IEnumerable<Propriedade>> ObterPropriedadesProximasAsync(Point localizacao, double raioKm);
}