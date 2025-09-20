using Agriis.Combos.Aplicacao.DTOs;
using Agriis.Combos.Dominio.Enums;
using Agriis.Compartilhado.Aplicacao.Resultados;

namespace Agriis.Combos.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviços de combo
/// </summary>
public interface IComboService
{
    /// <summary>
    /// Cria um novo combo
    /// </summary>
    Task<Result<ComboDto>> CriarAsync(CriarComboDto dto);

    /// <summary>
    /// Atualiza um combo existente
    /// </summary>
    Task<Result<ComboDto>> AtualizarAsync(int id, AtualizarComboDto dto);

    /// <summary>
    /// Obtém um combo por ID
    /// </summary>
    Task<Result<ComboDto>> ObterPorIdAsync(int id);

    /// <summary>
    /// Obtém combos por fornecedor
    /// </summary>
    Task<Result<IEnumerable<ComboDto>>> ObterPorFornecedorAsync(int fornecedorId);

    /// <summary>
    /// Obtém combos vigentes
    /// </summary>
    Task<Result<IEnumerable<ComboDto>>> ObterCombosVigentesAsync();

    /// <summary>
    /// Obtém combos válidos para um produtor
    /// </summary>
    Task<Result<IEnumerable<ComboDto>>> ObterCombosValidosParaProdutorAsync(
        int produtorId, 
        decimal hectareProdutor, 
        int municipioId);

    /// <summary>
    /// Atualiza status do combo
    /// </summary>
    Task<Result> AtualizarStatusAsync(int id, StatusCombo status);

    /// <summary>
    /// Remove um combo
    /// </summary>
    Task<Result> RemoverAsync(int id);

    /// <summary>
    /// Adiciona item ao combo
    /// </summary>
    Task<Result<ComboItemDto>> AdicionarItemAsync(int comboId, CriarComboItemDto dto);

    /// <summary>
    /// Atualiza item do combo
    /// </summary>
    Task<Result<ComboItemDto>> AtualizarItemAsync(int comboId, int itemId, AtualizarComboItemDto dto);

    /// <summary>
    /// Remove item do combo
    /// </summary>
    Task<Result> RemoverItemAsync(int comboId, int itemId);

    /// <summary>
    /// Adiciona local de recebimento ao combo
    /// </summary>
    Task<Result<ComboLocalRecebimentoDto>> AdicionarLocalRecebimentoAsync(
        int comboId, 
        CriarComboLocalRecebimentoDto dto);

    /// <summary>
    /// Adiciona desconto por categoria ao combo
    /// </summary>
    Task<Result<ComboCategoriaDescontoDto>> AdicionarCategoriaDescontoAsync(
        int comboId, 
        CriarComboCategoriaDescontoDto dto);

    /// <summary>
    /// Valida se um combo é aplicável para um produtor
    /// </summary>
    Task<Result<bool>> ValidarComboParaProdutorAsync(
        int comboId, 
        int produtorId, 
        decimal hectareProdutor, 
        int municipioId);
}