using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Culturas.Dominio.Entidades;

namespace Agriis.Culturas.Dominio.Interfaces;

public interface ICulturaRepository : IRepository<Cultura>
{
    Task<Cultura?> ObterPorNomeAsync(string nome);
    Task<IEnumerable<Cultura>> ObterAtivasAsync();
    Task<IEnumerable<Cultura>> ObterPorNomesAsync(IEnumerable<string> nomes);
    Task<bool> ExisteComNomeAsync(string nome, int? idExcluir = null);
}