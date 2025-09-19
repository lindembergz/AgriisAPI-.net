using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Propriedades.Aplicacao.DTOs;

namespace Agriis.Propriedades.Aplicacao.Interfaces;

public interface IPropriedadeService
{
    Task<Result<PropriedadeDto>> ObterPorIdAsync(int id);
    Task<Result<IEnumerable<PropriedadeDto>>> ObterPorProdutorAsync(int produtorId);
    Task<Result<IEnumerable<PropriedadeDto>>> ObterPorCulturaAsync(int culturaId);
    Task<Result<PropriedadeDto>> CriarAsync(PropriedadeCreateDto dto);
    Task<Result<PropriedadeDto>> AtualizarAsync(int id, PropriedadeUpdateDto dto);
    Task<Result> RemoverAsync(int id);
    Task<Result<PropriedadeDto>> ObterCompletaAsync(int id);
    Task<Result<decimal>> CalcularAreaTotalPorProdutorAsync(int produtorId);
    Task<Result<IEnumerable<PropriedadeDto>>> BuscarPropriedadesProximasAsync(double latitude, double longitude, double raioKm);
    Task<Result<Dictionary<int, decimal>>> ObterEstatisticasCulturasPorProdutorAsync(int produtorId);
}