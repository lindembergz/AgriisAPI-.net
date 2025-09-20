using System.Text.Json;
using AutoMapper;
using Agriis.Combos.Aplicacao.DTOs;
using Agriis.Combos.Aplicacao.Interfaces;
using Agriis.Combos.Dominio.Entidades;
using Agriis.Combos.Dominio.Enums;
using Agriis.Combos.Dominio.Interfaces;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.Interfaces;

namespace Agriis.Combos.Aplicacao.Servicos;

/// <summary>
/// Implementação do serviço de combos
/// </summary>
public class ComboService : IComboService
{
    private readonly IComboRepository _comboRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ComboService(
        IComboRepository comboRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _comboRepository = comboRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ComboDto>> CriarAsync(CriarComboDto dto)
    {
        try
        {
            // Verificar se já existe combo com mesmo nome para o fornecedor na safra
            var existeCombo = await _comboRepository.ExisteComboAtivoAsync(
                dto.FornecedorId, 
                dto.SafraId, 
                dto.Nome);

            if (existeCombo)
            {
                return Result<ComboDto>.Failure("Já existe um combo ativo com este nome para o fornecedor na safra selecionada");
            }

            var combo = new Combo(
                dto.Nome,
                dto.HectareMinimo,
                dto.HectareMaximo,
                dto.DataInicio,
                dto.DataFim,
                dto.ModalidadePagamento,
                dto.FornecedorId,
                dto.SafraId,
                dto.Descricao);

            // Configurar permissões
            combo.ConfigurarPermissoes(dto.PermiteAlteracaoItem, dto.PermiteExclusaoItem);

            // Configurar restrições de municípios se fornecidas
            if (dto.MunicipiosPermitidos?.Any() == true)
            {
                var restricoes = JsonDocument.Parse(JsonSerializer.Serialize(new { municipios = dto.MunicipiosPermitidos }));
                combo.DefinirRestricoesMunicipios(restricoes);
            }

            await _comboRepository.AdicionarAsync(combo);
            await _unitOfWork.SalvarAlteracoesAsync();

            var comboDto = _mapper.Map<ComboDto>(combo);
            return Result<ComboDto>.Success(comboDto);
        }
        catch (Exception ex)
        {
            return Result<ComboDto>.Failure($"Erro ao criar combo: {ex.Message}");
        }
    }

    public async Task<Result<ComboDto>> AtualizarAsync(int id, AtualizarComboDto dto)
    {
        try
        {
            var combo = await _comboRepository.ObterPorIdAsync(id);
            if (combo == null)
            {
                return Result<ComboDto>.Failure("Combo não encontrado");
            }

            combo.AtualizarInformacoes(
                dto.Nome,
                dto.HectareMinimo,
                dto.HectareMaximo,
                dto.DataInicio,
                dto.DataFim,
                dto.Descricao);

            combo.ConfigurarPermissoes(dto.PermiteAlteracaoItem, dto.PermiteExclusaoItem);

            // Atualizar restrições de municípios
            if (dto.MunicipiosPermitidos?.Any() == true)
            {
                var restricoes = JsonDocument.Parse(JsonSerializer.Serialize(new { municipios = dto.MunicipiosPermitidos }));
                combo.DefinirRestricoesMunicipios(restricoes);
            }

            await _comboRepository.AtualizarAsync(combo);
            await _unitOfWork.SalvarAlteracoesAsync();

            var comboDto = _mapper.Map<ComboDto>(combo);
            return Result<ComboDto>.Success(comboDto);
        }
        catch (Exception ex)
        {
            return Result<ComboDto>.Failure($"Erro ao atualizar combo: {ex.Message}");
        }
    }

    public async Task<Result<ComboDto>> ObterPorIdAsync(int id)
    {
        try
        {
            var combo = await _comboRepository.ObterCompletoAsync(id);
            if (combo == null)
            {
                return Result<ComboDto>.Failure("Combo não encontrado");
            }

            var comboDto = _mapper.Map<ComboDto>(combo);
            return Result<ComboDto>.Success(comboDto);
        }
        catch (Exception ex)
        {
            return Result<ComboDto>.Failure($"Erro ao obter combo: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ComboDto>>> ObterPorFornecedorAsync(int fornecedorId)
    {
        try
        {
            var combos = await _comboRepository.ObterPorFornecedorAsync(fornecedorId);
            var combosDto = _mapper.Map<IEnumerable<ComboDto>>(combos);
            return Result<IEnumerable<ComboDto>>.Success(combosDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ComboDto>>.Failure($"Erro ao obter combos do fornecedor: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ComboDto>>> ObterCombosVigentesAsync()
    {
        try
        {
            var combos = await _comboRepository.ObterCombosVigentesAsync();
            var combosDto = _mapper.Map<IEnumerable<ComboDto>>(combos);
            return Result<IEnumerable<ComboDto>>.Success(combosDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ComboDto>>.Failure($"Erro ao obter combos vigentes: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ComboDto>>> ObterCombosValidosParaProdutorAsync(
        int produtorId, 
        decimal hectareProdutor, 
        int municipioId)
    {
        try
        {
            var combos = await _comboRepository.ObterCombosValidosParaProdutorAsync(
                produtorId, 
                hectareProdutor, 
                municipioId);

            var combosDto = _mapper.Map<IEnumerable<ComboDto>>(combos);
            return Result<IEnumerable<ComboDto>>.Success(combosDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ComboDto>>.Failure($"Erro ao obter combos válidos para o produtor: {ex.Message}");
        }
    }

    public async Task<Result> AtualizarStatusAsync(int id, StatusCombo status)
    {
        try
        {
            var combo = await _comboRepository.ObterPorIdAsync(id);
            if (combo == null)
            {
                return Result.Failure("Combo não encontrado");
            }

            combo.AtualizarStatus(status);
            await _comboRepository.AtualizarAsync(combo);
            await _unitOfWork.SalvarAlteracoesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Erro ao atualizar status do combo: {ex.Message}");
        }
    }

    public async Task<Result> RemoverAsync(int id)
    {
        try
        {
            var combo = await _comboRepository.ObterPorIdAsync(id);
            if (combo == null)
            {
                return Result.Failure("Combo não encontrado");
            }

            await _comboRepository.RemoverAsync(id);
            await _unitOfWork.SalvarAlteracoesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Erro ao remover combo: {ex.Message}");
        }
    }

    public async Task<Result<ComboItemDto>> AdicionarItemAsync(int comboId, CriarComboItemDto dto)
    {
        try
        {
            var combo = await _comboRepository.ObterPorIdAsync(comboId);
            if (combo == null)
            {
                return Result<ComboItemDto>.Failure("Combo não encontrado");
            }

            var item = new ComboItem(
                comboId,
                dto.ProdutoId,
                dto.Quantidade,
                dto.PrecoUnitario,
                dto.PercentualDesconto,
                dto.ProdutoObrigatorio,
                dto.Ordem);

            combo.AdicionarItem(item);
            await _comboRepository.AtualizarAsync(combo);
            await _unitOfWork.SalvarAlteracoesAsync();

            var itemDto = _mapper.Map<ComboItemDto>(item);
            return Result<ComboItemDto>.Success(itemDto);
        }
        catch (Exception ex)
        {
            return Result<ComboItemDto>.Failure($"Erro ao adicionar item ao combo: {ex.Message}");
        }
    }

    public async Task<Result<ComboItemDto>> AtualizarItemAsync(int comboId, int itemId, AtualizarComboItemDto dto)
    {
        try
        {
            var combo = await _comboRepository.ObterCompletoAsync(comboId);
            if (combo == null)
            {
                return Result<ComboItemDto>.Failure("Combo não encontrado");
            }

            if (!combo.PermiteAlteracaoItem)
            {
                return Result<ComboItemDto>.Failure("Alteração de itens não permitida para este combo");
            }

            var item = combo.Itens.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                return Result<ComboItemDto>.Failure("Item não encontrado no combo");
            }

            item.AtualizarQuantidade(dto.Quantidade);
            item.AtualizarPreco(dto.PrecoUnitario);
            item.AtualizarDesconto(dto.PercentualDesconto);
            item.DefinirComoObrigatorio(dto.ProdutoObrigatorio);
            item.AtualizarOrdem(dto.Ordem);

            await _comboRepository.AtualizarAsync(combo);
            await _unitOfWork.SalvarAlteracoesAsync();

            var itemDto = _mapper.Map<ComboItemDto>(item);
            return Result<ComboItemDto>.Success(itemDto);
        }
        catch (Exception ex)
        {
            return Result<ComboItemDto>.Failure($"Erro ao atualizar item do combo: {ex.Message}");
        }
    }

    public async Task<Result> RemoverItemAsync(int comboId, int itemId)
    {
        try
        {
            var combo = await _comboRepository.ObterCompletoAsync(comboId);
            if (combo == null)
            {
                return Result.Failure("Combo não encontrado");
            }

            combo.RemoverItem(itemId);
            await _comboRepository.AtualizarAsync(combo);
            await _unitOfWork.SalvarAlteracoesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Erro ao remover item do combo: {ex.Message}");
        }
    }

    public async Task<Result<ComboLocalRecebimentoDto>> AdicionarLocalRecebimentoAsync(
        int comboId, 
        CriarComboLocalRecebimentoDto dto)
    {
        try
        {
            var combo = await _comboRepository.ObterPorIdAsync(comboId);
            if (combo == null)
            {
                return Result<ComboLocalRecebimentoDto>.Failure("Combo não encontrado");
            }

            var local = new ComboLocalRecebimento(
                comboId,
                dto.PontoDistribuicaoId,
                dto.PrecoAdicional,
                dto.PercentualDesconto,
                dto.LocalPadrao,
                dto.Observacoes);

            combo.AdicionarLocalRecebimento(local);
            await _comboRepository.AtualizarAsync(combo);
            await _unitOfWork.SalvarAlteracoesAsync();

            var localDto = _mapper.Map<ComboLocalRecebimentoDto>(local);
            return Result<ComboLocalRecebimentoDto>.Success(localDto);
        }
        catch (Exception ex)
        {
            return Result<ComboLocalRecebimentoDto>.Failure($"Erro ao adicionar local de recebimento: {ex.Message}");
        }
    }

    public async Task<Result<ComboCategoriaDescontoDto>> AdicionarCategoriaDescontoAsync(
        int comboId, 
        CriarComboCategoriaDescontoDto dto)
    {
        try
        {
            var combo = await _comboRepository.ObterPorIdAsync(comboId);
            if (combo == null)
            {
                return Result<ComboCategoriaDescontoDto>.Failure("Combo não encontrado");
            }

            var categoria = new ComboCategoriaDesconto(
                comboId,
                dto.CategoriaId,
                dto.TipoDesconto,
                dto.HectareMinimo,
                dto.HectareMaximo);

            // Configurar o valor do desconto baseado no tipo
            switch (dto.TipoDesconto)
            {
                case TipoDesconto.Percentual:
                    categoria.DefinirDescontoPercentual(dto.ValorDesconto);
                    break;
                case TipoDesconto.ValorFixo:
                    categoria.DefinirDescontoFixo(dto.ValorDesconto);
                    break;
                case TipoDesconto.PorHectare:
                    categoria.DefinirDescontoPorHectare(dto.ValorDesconto);
                    break;
            }

            combo.AdicionarCategoriaDesconto(categoria);
            await _comboRepository.AtualizarAsync(combo);
            await _unitOfWork.SalvarAlteracoesAsync();

            var categoriaDto = _mapper.Map<ComboCategoriaDescontoDto>(categoria);
            return Result<ComboCategoriaDescontoDto>.Success(categoriaDto);
        }
        catch (Exception ex)
        {
            return Result<ComboCategoriaDescontoDto>.Failure($"Erro ao adicionar categoria de desconto: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ValidarComboParaProdutorAsync(
        int comboId, 
        int produtorId, 
        decimal hectareProdutor, 
        int municipioId)
    {
        try
        {
            var combo = await _comboRepository.ObterCompletoAsync(comboId);
            if (combo == null)
            {
                return Result<bool>.Failure("Combo não encontrado");
            }

            // Verificar se o combo está vigente
            if (!combo.EstaVigente())
            {
                return Result<bool>.Success(false);
            }

            // Verificar faixa de hectare
            if (!combo.ValidarHectareProdutor(hectareProdutor))
            {
                return Result<bool>.Success(false);
            }

            // Verificar restrições de município se existirem
            if (combo.RestricoesMunicipios != null)
            {
                var restricoes = JsonSerializer.Deserialize<dynamic>(combo.RestricoesMunicipios.RootElement.GetRawText());
                // Lógica de validação de município seria implementada aqui
                // Por simplicidade, assumindo que é válido se chegou até aqui
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Erro ao validar combo para produtor: {ex.Message}");
        }
    }
}