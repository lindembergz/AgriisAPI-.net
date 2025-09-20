using AutoMapper;
using Agriis.Produtos.Aplicacao.DTOs;
using Agriis.Produtos.Aplicacao.Interfaces;
using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.Enums;
using Agriis.Produtos.Dominio.Interfaces;

namespace Agriis.Produtos.Aplicacao.Servicos;

/// <summary>
/// Serviço de aplicação para categorias
/// </summary>
public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IMapper _mapper;

    public CategoriaService(ICategoriaRepository categoriaRepository, IMapper mapper)
    {
        _categoriaRepository = categoriaRepository;
        _mapper = mapper;
    }

    public async Task<CategoriaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await _categoriaRepository.ObterPorIdAsync(id, cancellationToken);
        return categoria != null ? _mapper.Map<CategoriaDto>(categoria) : null;
    }

    public async Task<CategoriaDto?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        var categoria = await _categoriaRepository.ObterPorNomeAsync(nome, cancellationToken);
        return categoria != null ? _mapper.Map<CategoriaDto>(categoria) : null;
    }

    public async Task<IEnumerable<CategoriaDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var categorias = await _categoriaRepository.ObterTodosAsync(cancellationToken);
        return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
    }

    public async Task<IEnumerable<CategoriaDto>> ObterAtivasAsync(CancellationToken cancellationToken = default)
    {
        var categorias = await _categoriaRepository.ObterAtivasAsync(cancellationToken);
        return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
    }

    public async Task<IEnumerable<CategoriaDto>> ObterPorTipoAsync(CategoriaProduto tipo, CancellationToken cancellationToken = default)
    {
        var categorias = await _categoriaRepository.ObterPorTipoAsync(tipo, cancellationToken);
        return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
    }

    public async Task<IEnumerable<CategoriaDto>> ObterCategoriasRaizAsync(CancellationToken cancellationToken = default)
    {
        var categorias = await _categoriaRepository.ObterCategoriasRaizAsync(cancellationToken);
        return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
    }

    public async Task<IEnumerable<CategoriaDto>> ObterSubCategoriasAsync(int categoriaPaiId, CancellationToken cancellationToken = default)
    {
        var categorias = await _categoriaRepository.ObterSubCategoriasAsync(categoriaPaiId, cancellationToken);
        return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
    }

    public async Task<IEnumerable<CategoriaDto>> ObterComHierarquiaAsync(CancellationToken cancellationToken = default)
    {
        var categorias = await _categoriaRepository.ObterComSubCategoriasAsync(cancellationToken);
        return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
    }

    public async Task<IEnumerable<CategoriaDto>> ObterOrdenadasAsync(CancellationToken cancellationToken = default)
    {
        var categorias = await _categoriaRepository.ObterOrdenadasAsync(cancellationToken);
        return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
    }

    public async Task<CategoriaDto> CriarAsync(CriarCategoriaDto dto, CancellationToken cancellationToken = default)
    {
        // Validar se nome já existe
        if (await _categoriaRepository.ExisteComNomeAsync(dto.Nome, cancellationToken: cancellationToken))
            throw new InvalidOperationException("Já existe uma categoria com este nome");

        // Validar categoria pai se especificada
        if (dto.CategoriaPaiId.HasValue)
        {
            var categoriaPai = await _categoriaRepository.ObterPorIdAsync(dto.CategoriaPaiId.Value, cancellationToken);
            if (categoriaPai == null)
                throw new ArgumentException("Categoria pai não encontrada", nameof(dto.CategoriaPaiId));
        }

        var categoria = new Categoria(
            dto.Nome,
            dto.Tipo,
            dto.Descricao,
            dto.CategoriaPaiId,
            dto.Ordem);

        var categoriaSalva = await _categoriaRepository.AdicionarAsync(categoria, cancellationToken);
        return _mapper.Map<CategoriaDto>(categoriaSalva);
    }

    public async Task<CategoriaDto> AtualizarAsync(int id, AtualizarCategoriaDto dto, CancellationToken cancellationToken = default)
    {
        var categoria = await _categoriaRepository.ObterPorIdAsync(id, cancellationToken);
        if (categoria == null)
            throw new ArgumentException("Categoria não encontrada", nameof(id));

        // Validar se nome já existe (excluindo a categoria atual)
        if (await _categoriaRepository.ExisteComNomeAsync(dto.Nome, id, cancellationToken))
            throw new InvalidOperationException("Já existe uma categoria com este nome");

        // Validar categoria pai se especificada
        if (dto.CategoriaPaiId.HasValue)
        {
            if (dto.CategoriaPaiId.Value == id)
                throw new InvalidOperationException("Uma categoria não pode ser pai de si mesma");

            var categoriaPai = await _categoriaRepository.ObterPorIdAsync(dto.CategoriaPaiId.Value, cancellationToken);
            if (categoriaPai == null)
                throw new ArgumentException("Categoria pai não encontrada", nameof(dto.CategoriaPaiId));

            // Verificar se não está criando referência circular
            if (await VerificarReferenciaCircularAsync(id, dto.CategoriaPaiId.Value, cancellationToken))
                throw new InvalidOperationException("A operação criaria uma referência circular");
        }

        categoria.AtualizarNome(dto.Nome);
        categoria.AtualizarDescricao(dto.Descricao);
        categoria.AtualizarTipo(dto.Tipo);
        categoria.AtualizarOrdem(dto.Ordem);
        categoria.DefinirCategoriaPai(dto.CategoriaPaiId);

        await _categoriaRepository.AtualizarAsync(categoria, cancellationToken);
        return _mapper.Map<CategoriaDto>(categoria);
    }

    public async Task AtivarAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await _categoriaRepository.ObterPorIdAsync(id, cancellationToken);
        if (categoria == null)
            throw new ArgumentException("Categoria não encontrada", nameof(id));

        categoria.Ativar();
        await _categoriaRepository.AtualizarAsync(categoria, cancellationToken);
    }

    public async Task DesativarAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await _categoriaRepository.ObterPorIdAsync(id, cancellationToken);
        if (categoria == null)
            throw new ArgumentException("Categoria não encontrada", nameof(id));

        categoria.Desativar();
        await _categoriaRepository.AtualizarAsync(categoria, cancellationToken);
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await _categoriaRepository.ObterPorIdAsync(id, cancellationToken);
        if (categoria == null)
            throw new ArgumentException("Categoria não encontrada", nameof(id));

        // Verificar se pode ser removida
        if (!await PodeRemoverAsync(id, cancellationToken))
            throw new InvalidOperationException("Não é possível remover uma categoria que possui produtos ou subcategorias");

        await _categoriaRepository.RemoverAsync(categoria, cancellationToken);
    }

    public async Task<bool> ExisteComNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        return await _categoriaRepository.ExisteComNomeAsync(nome, idExcluir, cancellationToken);
    }

    public async Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var temProdutos = await _categoriaRepository.TemProdutosAsync(id, cancellationToken);
        var temSubCategorias = await _categoriaRepository.TemSubCategoriasAsync(id, cancellationToken);
        
        return !temProdutos && !temSubCategorias;
    }

    private async Task<bool> VerificarReferenciaCircularAsync(int categoriaId, int categoriaPaiId, CancellationToken cancellationToken)
    {
        var categoriaPai = await _categoriaRepository.ObterPorIdAsync(categoriaPaiId, cancellationToken);
        
        while (categoriaPai != null)
        {
            if (categoriaPai.Id == categoriaId)
                return true;
                
            if (!categoriaPai.CategoriaPaiId.HasValue)
                break;
                
            categoriaPai = await _categoriaRepository.ObterPorIdAsync(categoriaPai.CategoriaPaiId.Value, cancellationToken);
        }
        
        return false;
    }
}