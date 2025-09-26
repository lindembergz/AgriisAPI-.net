using AutoMapper;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Fornecedores.Aplicacao.DTOs;
using Agriis.Fornecedores.Aplicacao.Interfaces;
using Agriis.Fornecedores.Dominio.Entidades;
using Agriis.Fornecedores.Dominio.Interfaces;
using Agriis.Fornecedores.Dominio.Servicos;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Usuarios.Aplicacao.Interfaces;
using Agriis.Usuarios.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;

namespace Agriis.Fornecedores.Aplicacao.Servicos;

/// <summary>
/// Serviço de aplicação para fornecedores
/// </summary>
public class FornecedorService : IFornecedorService
{
    private readonly IFornecedorRepository _fornecedorRepository;
    private readonly FornecedorDomainService _domainService;
    private readonly IMapper _mapper;
    private readonly IUsuarioService _usuarioService;
    private readonly IUsuarioFornecedorRepository _usuarioFornecedorRepository;
    private readonly IMunicipioService _municipioService;

    public FornecedorService(
        IFornecedorRepository fornecedorRepository,
        FornecedorDomainService domainService,
        IMapper mapper,
        IUsuarioService usuarioService,
        IUsuarioFornecedorRepository usuarioFornecedorRepository,
        IMunicipioService municipioService)
    {
        _fornecedorRepository = fornecedorRepository ?? throw new ArgumentNullException(nameof(fornecedorRepository));
        _domainService = domainService ?? throw new ArgumentNullException(nameof(domainService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
        _usuarioFornecedorRepository = usuarioFornecedorRepository ?? throw new ArgumentNullException(nameof(usuarioFornecedorRepository));
        _municipioService = municipioService ?? throw new ArgumentNullException(nameof(municipioService));
    }

    public async Task<PagedResult<FornecedorDto>> ObterPaginadoAsync(FiltrosFornecedorDto filtros)
    {
        var resultado = await _fornecedorRepository.ObterPaginadoAsync(
            filtros.Pagina,
            filtros.TamanhoPagina,
            filtros.Filtro);

        var fornecedoresDto = _mapper.Map<IEnumerable<FornecedorDto>>(resultado.Items);

        return new PagedResult<FornecedorDto>(
            fornecedoresDto,
            resultado.PageNumber,
            resultado.PageSize,
            resultado.TotalCount);
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

        // Validar relacionamento UF-Município
        await ValidarRelacionamentoUfMunicipioAsync(request.UfId, request.MunicipioId, cancellationToken);

        // Criar fornecedor
        var fornecedor = new Fornecedor(
            request.Nome,
            cnpj,
            request.InscricaoEstadual,
            request.Logradouro,
            request.UfId,
            request.MunicipioId,
            request.Cep,
            request.Complemento,
            request.Latitude,
            request.Longitude,
            request.Telefone,
            request.Email,
            (MoedaFinanceira)request.MoedaPadrao);

        if (request.PedidoMinimo.HasValue)
            fornecedor.DefinirPedidoMinimo(request.PedidoMinimo.Value);

        if (!string.IsNullOrWhiteSpace(request.TokenLincros))
            fornecedor.DefinirTokenLincros(request.TokenLincros);

        var fornecedorCriado = await _fornecedorRepository.AdicionarAsync(fornecedor, cancellationToken);
        return _mapper.Map<FornecedorDto>(fornecedorCriado);
    }

    public async Task<FornecedorDto> CriarCompletoAsync(CriarFornecedorCompletoRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // TODO: Implementar transação para garantir consistência
        // using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // 1. Validar e criar fornecedor básico
            var cnpj = new Cnpj(request.CpfCnpj);
            var cnpjDisponivel = await _domainService.ValidarCnpjDisponivelAsync(cnpj, null, cancellationToken);
            if (!cnpjDisponivel)
                throw new InvalidOperationException("CPF/CNPJ já está em uso por outro fornecedor");

            var fornecedor = new Fornecedor(
                request.Nome,
                cnpj,
                request.InscricaoEstadual,
                request.Endereco?.Logradouro,
                null, // UfId - será resolvido posteriormente
                null, // MunicipioId - será resolvido posteriormente
                request.Endereco?.Cep,
                request.Endereco?.Complemento,
                request.Endereco?.Latitude,
                request.Endereco?.Longitude,
                request.Telefone,
                request.Email);

            var fornecedorCriado = await _fornecedorRepository.AdicionarAsync(fornecedor, cancellationToken);

            // 3. Criar usuário master
            var criarUsuarioDto = new CriarUsuarioDto
            {
                Nome = request.UsuarioMaster.Nome,
                Email = request.UsuarioMaster.Email,
                Celular = request.UsuarioMaster.Telefone,
                Cpf = null, // UsuarioMasterRequest não tem CPF
                Senha = request.UsuarioMaster.Senha,
                Roles = new List<Roles> { Roles.RoleFornecedorWebAdmin }
            };
            
            var usuarioCriado = await _usuarioService.CriarAsync(criarUsuarioDto, cancellationToken);

            // 4. Criar relacionamento UsuarioFornecedor
            var usuarioFornecedor = new UsuarioFornecedor(
                usuarioCriado.Id,
                fornecedorCriado.Id,
                Roles.RoleFornecedorWebAdmin);
            
            await _usuarioFornecedorRepository.AdicionarAsync(usuarioFornecedor, cancellationToken);

            // 4. TODO: Criar pontos de distribuição
            // foreach (var pontoRequest in request.PontosDistribuicao)
            // {
            //     var ponto = new PontoDistribuicao(...);
            //     await _pontoDistribuicaoRepository.AdicionarAsync(ponto, cancellationToken);
            // }

            // await transaction.CommitAsync(cancellationToken);
            return _mapper.Map<FornecedorDto>(fornecedorCriado);
        }
        catch
        {
            // await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<FornecedorDto> AtualizarAsync(int id, AtualizarFornecedorRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var fornecedor = await _fornecedorRepository.ObterPorIdAsync(id, cancellationToken);
        if (fornecedor == null)
            throw new InvalidOperationException("Fornecedor não encontrado");

        // Validar relacionamentos (implementar se necessário)

        // Atualizar dados
        fornecedor.AtualizarDados(
            request.Nome,
            request.InscricaoEstadual,
            request.Logradouro,
            null, // UfId - será resolvido posteriormente
            null, // MunicipioId - será resolvido posteriormente
            request.Cep,
            request.Complemento,
            request.Latitude,
            request.Longitude,
            request.Telefone,
            request.Email);

        fornecedor.AlterarMoedaPadrao((MoedaFinanceira)request.MoedaPadrao);
        fornecedor.DefinirPedidoMinimo(request.PedidoMinimo);
        fornecedor.DefinirTokenLincros(request.TokenLincros);

        await _fornecedorRepository.AtualizarAsync(fornecedor, cancellationToken);
        return _mapper.Map<FornecedorDto>(fornecedor);
    }

    public async Task<FornecedorDto> AtualizarCompletoAsync(int id, AtualizarFornecedorCompletoRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // TODO: Implementar transação para garantir consistência
        // using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // 1. Buscar fornecedor existente
            var fornecedor = await _fornecedorRepository.ObterPorIdAsync(id, cancellationToken);
            if (fornecedor == null)
                throw new InvalidOperationException("Fornecedor não encontrado");

            // 2. Validar CNPJ se foi alterado
            var cnpj = new Cnpj(request.CpfCnpj);
            if (fornecedor.Cnpj.Valor != cnpj.Valor)
            {
                var cnpjDisponivel = await _domainService.ValidarCnpjDisponivelAsync(cnpj, id, cancellationToken);
                if (!cnpjDisponivel)
                    throw new InvalidOperationException("CPF/CNPJ já está em uso por outro fornecedor");
            }

            // 3. Atualizar dados básicos do fornecedor
            fornecedor.AtualizarDados(
                request.Nome,
                request.InscricaoEstadual,
                request.Endereco?.Logradouro,
                null, // UfId - será resolvido posteriormente
                null, // MunicipioId - será resolvido posteriormente
                request.Endereco?.Cep,
                request.Endereco?.Complemento,
                request.Endereco?.Latitude,
                request.Endereco?.Longitude,
                request.Telefone,
                request.Email);

            // Atualizar CNPJ se foi alterado
            if (fornecedor.Cnpj.Valor != cnpj.Valor)
            {
                // TODO: Implementar método para atualizar CNPJ na entidade
                // fornecedor.AtualizarCnpj(cnpj);
            }

            await _fornecedorRepository.AtualizarAsync(fornecedor, cancellationToken);

            // 4. TODO: Atualizar endereços
            // - Remover endereços que não estão mais na lista
            // - Atualizar endereços existentes
            // - Criar novos endereços

            // 5. TODO: Atualizar usuário master
            // - Buscar usuário existente
            // - Atualizar dados do usuário

            // 6. TODO: Atualizar pontos de distribuição
            // - Remover pontos que não estão mais na lista
            // - Atualizar pontos existentes
            // - Criar novos pontos

            // await transaction.CommitAsync(cancellationToken);
            return _mapper.Map<FornecedorDto>(fornecedor);
        }
        catch
        {
            // await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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

    /// <summary>
    /// Valida se o município pertence à UF informada
    /// </summary>
    private async Task ValidarRelacionamentoUfMunicipioAsync(int? ufId, int? municipioId, CancellationToken cancellationToken)
    {
        if (!ufId.HasValue || !municipioId.HasValue)
            return; // Se não há UF ou município, não há o que validar

        var municipio = await _municipioService.ObterPorIdAsync(municipioId.Value, cancellationToken);
        if (municipio == null)
            throw new InvalidOperationException("Município não encontrado");

        if (municipio.UfId != ufId.Value)
            throw new InvalidOperationException("O município selecionado não pertence à UF informada");
    }
}