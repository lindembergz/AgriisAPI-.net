using AutoMapper;
using Agriis.Produtos.Aplicacao.DTOs;
using Agriis.Produtos.Aplicacao.Interfaces;
using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.Enums;
using Agriis.Produtos.Dominio.Interfaces;
using Agriis.Produtos.Dominio.ObjetosValor;

namespace Agriis.Produtos.Aplicacao.Servicos;

/// <summary>
/// Serviço de aplicação para produtos
/// </summary>
public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IProdutoCulturaRepository _produtoCulturaRepository;
    private readonly IMapper _mapper;

    public ProdutoService(
        IProdutoRepository produtoRepository,
        ICategoriaRepository categoriaRepository,
        IProdutoCulturaRepository produtoCulturaRepository,
        IMapper mapper)
    {
        _produtoRepository = produtoRepository;
        _categoriaRepository = categoriaRepository;
        _produtoCulturaRepository = produtoCulturaRepository;
        _mapper = mapper;
    }

    public async Task<ProdutoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id, cancellationToken);
        return produto != null ? _mapper.Map<ProdutoDto>(produto) : null;
    }

    public async Task<ProdutoDto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        var produto = await _produtoRepository.ObterPorCodigoAsync(codigo, cancellationToken);
        return produto != null ? _mapper.Map<ProdutoDto>(produto) : null;
    }

    public async Task<IEnumerable<ProdutoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var produtos = await _produtoRepository.ObterTodosAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ProdutoDto>>(produtos);
    }

    public async Task<IEnumerable<ProdutoDto>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        var produtos = await _produtoRepository.ObterAtivosAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ProdutoDto>>(produtos);
    }

    public async Task<IEnumerable<ProdutoDto>> ObterPorFornecedorAsync(int fornecedorId, CancellationToken cancellationToken = default)
    {
        var produtos = await _produtoRepository.ObterPorFornecedorAsync(fornecedorId, cancellationToken);
        return _mapper.Map<IEnumerable<ProdutoDto>>(produtos);
    }

    public async Task<IEnumerable<ProdutoDto>> ObterPorCategoriaAsync(int categoriaId, CancellationToken cancellationToken = default)
    {
        var produtos = await _produtoRepository.ObterPorCategoriaAsync(categoriaId, cancellationToken);
        return _mapper.Map<IEnumerable<ProdutoDto>>(produtos);
    }

    public async Task<IEnumerable<ProdutoDto>> ObterPorCulturaAsync(int culturaId, CancellationToken cancellationToken = default)
    {
        var produtos = await _produtoRepository.ObterPorCulturaAsync(culturaId, cancellationToken);
        return _mapper.Map<IEnumerable<ProdutoDto>>(produtos);
    }

    public async Task<IEnumerable<ProdutoDto>> ObterPorTipoAsync(TipoProduto tipo, CancellationToken cancellationToken = default)
    {
        var produtos = await _produtoRepository.ObterPorTipoAsync(tipo, cancellationToken);
        return _mapper.Map<IEnumerable<ProdutoDto>>(produtos);
    }

    public async Task<IEnumerable<ProdutoDto>> ObterFabricantesAsync(CancellationToken cancellationToken = default)
    {
        var produtos = await _produtoRepository.ObterFabricantesAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ProdutoDto>>(produtos);
    }

    public async Task<IEnumerable<ProdutoDto>> ObterProdutosFilhosAsync(int produtoPaiId, CancellationToken cancellationToken = default)
    {
        var produtos = await _produtoRepository.ObterProdutosFilhosAsync(produtoPaiId, cancellationToken);
        return _mapper.Map<IEnumerable<ProdutoDto>>(produtos);
    }

    public async Task<IEnumerable<ProdutoDto>> ObterRestritosAsync(CancellationToken cancellationToken = default)
    {
        var produtos = await _produtoRepository.ObterRestritosAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ProdutoDto>>(produtos);
    }

    public async Task<IEnumerable<ProdutoDto>> BuscarAsync(string termo, CancellationToken cancellationToken = default)
    {
        var produtos = await _produtoRepository.BuscarPorNomeOuCodigoAsync(termo, cancellationToken);
        return _mapper.Map<IEnumerable<ProdutoDto>>(produtos);
    }

    public async Task<IEnumerable<ProdutoDto>> ObterPorCulturasAsync(IEnumerable<int> culturasIds, CancellationToken cancellationToken = default)
    {
        var produtos = await _produtoRepository.ObterPorCulturasAsync(culturasIds, cancellationToken);
        return _mapper.Map<IEnumerable<ProdutoDto>>(produtos);
    }

    public async Task<ProdutoDto> CriarAsync(CriarProdutoDto dto, CancellationToken cancellationToken = default)
    {
        // Validar se categoria existe
        var categoria = await _categoriaRepository.ObterPorIdAsync(dto.CategoriaId, cancellationToken);
        if (categoria == null)
            throw new ArgumentException("Categoria não encontrada", nameof(dto.CategoriaId));

        // Validar se código já existe
        if (await _produtoRepository.ExisteComCodigoAsync(dto.Codigo, cancellationToken: cancellationToken))
            throw new InvalidOperationException("Já existe um produto com este código");

        // Validar produto pai se especificado
        if (dto.ProdutoPaiId.HasValue)
        {
            var produtoPai = await _produtoRepository.ObterPorIdAsync(dto.ProdutoPaiId.Value, cancellationToken);
            if (produtoPai == null)
                throw new ArgumentException("Produto pai não encontrado", nameof(dto.ProdutoPaiId));
            
            if (produtoPai.Tipo != TipoProduto.Fabricante)
                throw new InvalidOperationException("Produto pai deve ser do tipo Fabricante");
        }

        // Criar dimensões
        var dimensoes = new DimensoesProduto(
            dto.Dimensoes.Altura,
            dto.Dimensoes.Largura,
            dto.Dimensoes.Comprimento,
            dto.Dimensoes.PesoNominal,
            dto.Dimensoes.Densidade);

        // Criar produto
        var produto = new Produto(
            dto.Nome,
            dto.Codigo,
            dto.Tipo,
            dto.Unidade,
            dimensoes,
            dto.CategoriaId,
            dto.FornecedorId,
            dto.Descricao,
            dto.Marca,
            dto.TipoCalculoPeso,
            dto.ProdutoRestrito,
            dto.ObservacoesRestricao,
            dto.ProdutoPaiId);

        // Adicionar culturas
        foreach (var culturaId in dto.CulturasIds)
        {
            produto.AdicionarCultura(culturaId);
        }

        var produtoSalvo = await _produtoRepository.AdicionarAsync(produto, cancellationToken);
        return _mapper.Map<ProdutoDto>(produtoSalvo);
    }

    public async Task<ProdutoDto> AtualizarAsync(int id, AtualizarProdutoDto dto, CancellationToken cancellationToken = default)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id, cancellationToken);
        if (produto == null)
            throw new ArgumentException("Produto não encontrado", nameof(id));

        // Validar se categoria existe
        var categoria = await _categoriaRepository.ObterPorIdAsync(dto.CategoriaId, cancellationToken);
        if (categoria == null)
            throw new ArgumentException("Categoria não encontrada", nameof(dto.CategoriaId));

        // Validar se código já existe (excluindo o produto atual)
        if (await _produtoRepository.ExisteComCodigoAsync(dto.Codigo, id, cancellationToken))
            throw new InvalidOperationException("Já existe um produto com este código");

        // Atualizar informações básicas
        produto.AtualizarInformacoes(dto.Nome, dto.Descricao, dto.Marca);
        produto.AtualizarCodigo(dto.Codigo);
        produto.AtualizarCategoria(dto.CategoriaId);
        produto.AtualizarTipoCalculoPeso(dto.TipoCalculoPeso);
        produto.DefinirRestricao(dto.ProdutoRestrito, dto.ObservacoesRestricao);

        // Atualizar dimensões
        var novasDimensoes = new DimensoesProduto(
            dto.Dimensoes.Altura,
            dto.Dimensoes.Largura,
            dto.Dimensoes.Comprimento,
            dto.Dimensoes.PesoNominal,
            dto.Dimensoes.Densidade);
        produto.AtualizarDimensoes(novasDimensoes);

        // Atualizar culturas
        var culturasAtuais = produto.ObterCulturasCompativeis().ToList();
        var culturasNovas = dto.CulturasIds;

        // Remover culturas que não estão na nova lista
        foreach (var culturaId in culturasAtuais.Except(culturasNovas))
        {
            produto.RemoverCultura(culturaId);
        }

        // Adicionar novas culturas
        foreach (var culturaId in culturasNovas.Except(culturasAtuais))
        {
            produto.AdicionarCultura(culturaId);
        }

        await _produtoRepository.AtualizarAsync(produto, cancellationToken);
        return _mapper.Map<ProdutoDto>(produto);
    }

    public async Task AtivarAsync(int id, CancellationToken cancellationToken = default)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id, cancellationToken);
        if (produto == null)
            throw new ArgumentException("Produto não encontrado", nameof(id));

        produto.Ativar();
        await _produtoRepository.AtualizarAsync(produto, cancellationToken);
    }

    public async Task InativarAsync(int id, CancellationToken cancellationToken = default)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id, cancellationToken);
        if (produto == null)
            throw new ArgumentException("Produto não encontrado", nameof(id));

        produto.Inativar();
        await _produtoRepository.AtualizarAsync(produto, cancellationToken);
    }

    public async Task DescontinuarAsync(int id, CancellationToken cancellationToken = default)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id, cancellationToken);
        if (produto == null)
            throw new ArgumentException("Produto não encontrado", nameof(id));

        produto.Descontinuar();
        await _produtoRepository.AtualizarAsync(produto, cancellationToken);
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id, cancellationToken);
        if (produto == null)
            throw new ArgumentException("Produto não encontrado", nameof(id));

        // Verificar se tem produtos filhos
        if (produto.TemProdutosFilhos())
            throw new InvalidOperationException("Não é possível remover um produto que possui produtos filhos");

        await _produtoRepository.RemoverAsync(produto, cancellationToken);
    }

    public async Task AdicionarCulturaAsync(int produtoId, int culturaId, CancellationToken cancellationToken = default)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(produtoId, cancellationToken);
        if (produto == null)
            throw new ArgumentException("Produto não encontrado", nameof(produtoId));

        produto.AdicionarCultura(culturaId);
        await _produtoRepository.AtualizarAsync(produto, cancellationToken);
    }

    public async Task RemoverCulturaAsync(int produtoId, int culturaId, CancellationToken cancellationToken = default)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(produtoId, cancellationToken);
        if (produto == null)
            throw new ArgumentException("Produto não encontrado", nameof(produtoId));

        produto.RemoverCultura(culturaId);
        await _produtoRepository.AtualizarAsync(produto, cancellationToken);
    }

    public async Task<bool> ExisteComCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        return await _produtoRepository.ExisteComCodigoAsync(codigo, idExcluir, cancellationToken);
    }
}