using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Propriedades.Aplicacao.DTOs;

namespace Agriis.Propriedades.Aplicacao.Interfaces;

public interface IPropriedadeCulturaService
{
    Task<Result<PropriedadeCulturaDto>> ObterPorIdAsync(int id);
    Task<Result<IEnumerable<PropriedadeCulturaDto>>> ObterPorPropriedadeAsync(int propriedadeId);
    Task<Result<IEnumerable<PropriedadeCulturaDto>>> ObterPorCulturaAsync(int culturaId);
    Task<Result<PropriedadeCulturaDto>> CriarAsync(PropriedadeCulturaCreateDto dto);
    Task<Result<PropriedadeCulturaDto>> AtualizarAsync(int id, PropriedadeCulturaUpdateDto dto);
    Task<Result> RemoverAsync(int id);
    Task<Result<decimal>> CalcularAreaTotalPorCulturaAsync(int culturaId);
    Task<Result<IEnumerable<PropriedadeCulturaDto>>> ObterEmPeriodoPlantioAsync();
}