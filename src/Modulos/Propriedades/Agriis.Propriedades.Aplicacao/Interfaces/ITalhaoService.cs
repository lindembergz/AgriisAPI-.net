using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Propriedades.Aplicacao.DTOs;

namespace Agriis.Propriedades.Aplicacao.Interfaces;

public interface ITalhaoService
{
    Task<Result<TalhaoDto>> ObterPorIdAsync(int id);
    Task<Result<IEnumerable<TalhaoDto>>> ObterPorPropriedadeAsync(int propriedadeId);
    Task<Result<TalhaoDto>> CriarAsync(TalhaoCreateDto dto);
    Task<Result<TalhaoDto>> AtualizarAsync(int id, TalhaoUpdateDto dto);
    Task<Result> RemoverAsync(int id);
    Task<Result<decimal>> CalcularAreaTotalPorPropriedadeAsync(int propriedadeId);
    Task<Result<IEnumerable<TalhaoDto>>> BuscarTalhoesProximosAsync(double latitude, double longitude, double raioKm);
}