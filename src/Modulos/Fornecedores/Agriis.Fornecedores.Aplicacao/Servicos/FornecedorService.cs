using AutoMapper;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Fornecedores.Aplicacao.DTOs;
using Agriis.Fornecedores.Aplicacao.Interfaces;
using Agriis.Fornecedores.Dominio.Entidades;
using Agriis.Fornecedores.Dominio.Interfaces;
using Agriis.Fornecedores.Dominio.Servicos;

namespace Agriis.Fornecedores.Aplicacao.Servicos;

/// <summary>
/// Serviço de aplicação para fornecedores
/// </summary>
public class FornecedorService : IFornecedorService
{
    private readonly IFornecedorRepository _fornecedorRepository;
    private readonly FornecedorDomainService _domainService;
    private readonly IMapper _mapper;

    public FornecedorService(
        IFornecedorRepository fornecedorRepository,
        FornecedorDomainService domainService,
        IMapper mapper)
    {
        _fornecedorRepository = fornecedorRepository ?? throw new ArgumentNullException(nameof(fornecedorRepository));
        _domainService = domainService ?? throw new ArgumentNullException(nameof(domainService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<FornecedorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var fornecedores = await _fornecedorRepository.ObterTodosAsync(cancellationToken);
        return _mapper.Map<IEnumerable<FornecedorDto>>(fornecedores);
    }

    public async Task<FornecedorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var fornecedor = await _fornecedorRepository.ObterPorIdAsync(id, cancellationToken);
        return fornecedor != null ? _mapper.Map<FornecedorDto>(fornecedor) : null;
    }

    public async Task<FornecedorDto?> ObterPorCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return null;

        var fornecedor = await _fornecedorRepository.ObterPorCnpjAsync(cnpj, cancellationToken);
        return fornecedor != null ? _mapper.Map<FornecedorDto>(fornecedor) : null;
    }

    public async Task<IEnumerable<FornecedorDto>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        var fornecedores = await _fornecedorRepository.ObterAtivosAsync(cancellationToken);
        return _mapper.Map<IEnumerable<FornecedorDto>>(fornecedores);
    }

    public async Task<IEnumerable<FornecedorDto>> ObterPorTerritorioAsync(string uf, string? municipio = null, CancellationToken cancellationToken = default)
    {
        var fornecedores = await _domainService.ObterFornecedoresPorTerritorioAsync(uf, municipio, cancellationToken);
        return _mapper.Map<IEnumerable<FornecedorDto>>(fornecedores);
    }

    public async Task<IEnumerable<FornecedorDto>> ObterComFiltrosAsync(
        string? nome = null,
        string? cnpj = null,
        bool? ativo = null,
        int? moedaPadrao = null,
        CancellationToken cancellationToken = default)
    {
        var fornecedores = await _fornecedorRepository.ObterComFiltrosAsync(nome, cnpj, ativo, moedaPadrao, cancellationToken);
        return _mapper.Map<IEnumerable<FornecedorDto>>(fornecedores);
    }

    public async Task<FornecedorDto> CriarAsync(CriarFornecedorRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Validar CNPJ
        var cnpj = new Cnpj(request.Cnpj);
        var cnpjDisponivel = await _domainService.ValidarCnpjDisponivelAsync(cnpj, null, cancellationToken);
        if (!cnpjDisponivel)
            throw new InvalidOperationException("CNPJ já está em uso por outro fornecedor");

        // Criar fornecedor
        var fornecedor = new Fornecedor(
            request.Nome,
            cnpj,
            request.InscricaoEstadual,
            request.Endereco,
            request.Telefone,
            request.Email,
            (Moeda)request.MoedaPadrao);

        if (request.PedidoMinimo.HasValue)
            fornecedor.DefinirPedidoMinimo(request.PedidoMinimo.Value);

        if (!string.IsNullOrWhiteSpace(request.TokenLincros))
            fornecedor.DefinirTokenLincros(request.TokenLincros);

        var fornecedorCriado = await _fornecedorRepository.AdicionarAsync(fornecedor, cancellationToken);
        return _mapper.Map<FornecedorDto>(fornecedorCriado);
    }

    public async Task<FornecedorDto> AtualizarAsync(int id, AtualizarFornecedorRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var fornecedor = await _fornecedorRepository.ObterPorIdAsync(id, cancellationToken);
        if (fornecedor == null)
            throw new InvalidOperationException("Fornecedor não encontrado");

        // Atualizar dados
        fornecedor.AtualizarDados(
            request.Nome,
            request.InscricaoEstadual,
            request.Endereco,
            request.Telefone,
            request.Email);

        fornecedor.AlterarMoedaPadrao((Moeda)request.MoedaPadrao);
        fornecedor.DefinirPedidoMinimo(request.PedidoMinimo);
        fornecedor.DefinirTokenLincros(request.TokenLincros);

        await _fornecedorRepository.AtualizarAsync(fornecedor, cancellationToken);
        return _mapper.Map<FornecedorDto>(fornecedor);
    }   
 public async Task AtivarAsync(int id, CancellationToken cancellationToken = default)
    {
        var fornecedor = await _fornecedorRepository.ObterPorIdAsync(id, cancellationToken);
        if (fornecedor == null)
            throw new InvalidOperationException("Fornecedor não encontrado");

        fornecedor.Ativar();
        await _fornecedorRepository.AtualizarAsync(fornecedor, cancellationToken);
    }

    public async Task DesativarAsync(int id, CancellationToken cancellationToken = default)
    {
        var fornecedor = await _fornecedorRepository.ObterPorIdAsync(id, cancellationToken);
        if (fornecedor == null)
            throw new InvalidOperationException("Fornecedor não encontrado");

        fornecedor.Desativar();
        await _fornecedorRepository.AtualizarAsync(fornecedor, cancellationToken);
    }

    public async Task DefinirLogoAsync(int id, string? logoUrl, CancellationToken cancellationToken = default)
    {
        var fornecedor = await _fornecedorRepository.ObterPorIdAsync(id, cancellationToken);
        if (fornecedor == null)
            throw new InvalidOperationException("Fornecedor não encontrado");

        fornecedor.DefinirLogo(logoUrl);
        await _fornecedorRepository.AtualizarAsync(fornecedor, cancellationToken);
    }

    public async Task<bool> VerificarCnpjDisponivelAsync(string cnpj, int? fornecedorIdExcluir = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        try
        {
            var cnpjObj = new Cnpj(cnpj);
            return await _domainService.ValidarCnpjDisponivelAsync(cnpjObj, fornecedorIdExcluir, cancellationToken);
        }
        catch
        {
            return false; // CNPJ inválido
        }
    }
}