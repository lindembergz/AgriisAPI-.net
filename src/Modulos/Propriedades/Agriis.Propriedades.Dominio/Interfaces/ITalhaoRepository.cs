using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Propriedades.Dominio.Entidades;
using NetTopologySuite.Geometries;

namespace Agriis.Propriedades.Dominio.Interfaces;

public interface ITalhaoRepository : IRepository<Talhao>
{
    Task<IEnumerable<Talhao>> ObterPorPropriedadeAsync(int propriedadeId);
    Task<IEnumerable<Talhao>> ObterPorRegiao(Point centro, double raioKm);
    Task<decimal> CalcularAreaTotalPorPropriedadeAsync(int propriedadeId);
    Task<IEnumerable<Talhao>> ObterTalhoesProximosAsync(Point localizacao, double raioKm);
}