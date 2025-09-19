using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Culturas.Aplicacao.DTOs;

namespace Agriis.Culturas.Aplicacao.Interfaces;

public interface ICulturaService
{
    Task<Result<CulturaDto>> ObterPorIdAsync(int id);
    Task<Result<IEnumerable<CulturaDto>>> ObterTodasAsync();
    Task<Result<IEnumerable<CulturaDto>>> ObterAtivasAsync();
    Task<Result<CulturaDto>> CriarAsync(CriarCulturaDto dto);
    Task<Result<CulturaDto>> AtualizarAsync(int id, AtualizarCulturaDto dto);
    Task<Result> RemoverAsync(int id);
    Task<Result<CulturaDto>> ObterPorNomeAsync(string nome);
}