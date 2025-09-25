using AutoMapper;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Produtores.Aplicacao.DTOs;
using Agriis.Produtores.Aplicacao.Interfaces;
using Agriis.Produtores.Dominio.Entidades;
using Agriis.Produtores.Dominio.Enums;
using Agriis.Produtores.Dominio.Interfaces;
using Agriis.Produtores.Dominio.Servicos;
using Agriis.Usuarios.Aplicacao.Interfaces;
using Agriis.Usuarios.Aplicacao.DTOs;

namespace Agriis.Produtores.Aplicacao.Servicos;

/// <summary>
/// Serviço de aplicação para produtores
/// </summary>
public class ProdutorService : IProdutorService
{
    private readonly IProdutorRepository _produtorRepository;
    private readonly IMapper _mapper;
    private readonly ProdutorDomainService _domainService;
    private readonly IUsuarioService _usuarioService;
    private readonly IUsuarioProdutorRepository _usuarioProdutorRepository;

    public ProdutorService(
        IProdutorRepository produtorRepository,
        IMapper mapper,
        ProdutorDomainService domainService,
        IUsuarioService usuarioService,
        IUsuarioProdutorRepository usuarioProdutorRepository)
    {
        _produtorRepository = produtorRepository ?? throw new ArgumentNullException(nameof(produtorRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _domainService = domainService ?? throw new ArgumentNullException(nameof(domainService));
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
        _usuarioProdutorRepository = usuarioProdutorRepository ?? throw new ArgumentNullException(nameof(usuarioProdutorRepository));
    }

    /// <inheritdoc />
    public async Task<ProdutorDto?> ObterPorIdAsync(int id)
    {
        var produtor = await _produtorRepository.ObterPorIdAsync(id);
        return produtor != null ? _mapper.Map<ProdutorDto>(produtor) : null;
    }

    /// <inheritdoc />
    public async Task<ProdutorDto?> ObterPorCpfAsync(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return null;

        var produtor = await _produtorRepository.ObterPorCpfAsync(cpf);
        return produtor != null ? _mapper.Map<ProdutorDto>(produtor) : null;
    }

    /// <inheritdoc />
    public async Task<ProdutorDto?> ObterPorCnpjAsync(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return null;

        var produtor = await _produtorRepository.ObterPorCnpjAsync(cnpj);
        return produtor != null ? _mapper.Map<ProdutorDto>(produtor) : null;
    }

    /// <inheritdoc />
    public async Task<PagedResult<ProdutorDto>> ObterPaginadoAsync(FiltrosProdutorDto filtros)
    {
        var resultado = await _produtorRepository.ObterPaginadoAsync(
            filtros.Pagina,
            filtros.TamanhoPagina,
            filtros.Filtro,
            filtros.Status,
            filtros.CulturaId);

        var produtoresDto = _mapper.Map<IEnumerable<ProdutorDto>>(resultado.Items);

        return new PagedResult<ProdutorDto>(
            produtoresDto,
            resultado.PageNumber,
            resultado.PageSize,
            resultado.TotalCount);
    }

    /// <inheritdoc />
    public async Task<Result<ProdutorDto>> CriarAsync(CriarProdutorDto dto)
    {
        try
        {
            // Validações de negócio
            if (string.IsNullOrWhiteSpace(dto.Nome))
                return Result<ProdutorDto>.Failure("Nome do produtor é obrigatório");

            if (string.IsNullOrWhiteSpace(dto.Cpf) && string.IsNullOrWhiteSpace(dto.Cnpj))
                return Result<ProdutorDto>.Failure("CPF ou CNPJ deve ser informado");

            // Verifica se já existe produtor com o mesmo documento
            if (!string.IsNullOrWhiteSpace(dto.Cpf))
            {
                var existeCpf = await _produtorRepository.ExistePorCpfAsync(dto.Cpf);
                if (existeCpf)
                    return Result<ProdutorDto>.Failure("Já existe um produtor cadastrado com este CPF");
            }

            if (!string.IsNullOrWhiteSpace(dto.Cnpj))
            {
                var existeCnpj = await _produtorRepository.ExistePorCnpjAsync(dto.Cnpj);
                if (existeCnpj)
                    return Result<ProdutorDto>.Failure("Já existe um produtor cadastrado com este CNPJ");
            }

            // Cria o produtor
            var produtor = new Produtor(
                dto.Nome,
                !string.IsNullOrWhiteSpace(dto.Cpf) ? new Cpf(dto.Cpf) : null,
                !string.IsNullOrWhiteSpace(dto.Cnpj) ? new Cnpj(dto.Cnpj) : null,
                dto.InscricaoEstadual,
                dto.TipoAtividade,
                dto.Telefone1,
                dto.Telefone2,
                dto.Telefone3,
                dto.Email,
                new AreaPlantio(dto.AreaPlantio));

            // Adiciona culturas
            foreach (var culturaId in dto.Culturas)
            {
                produtor.AdicionarCultura(culturaId);
            }

            // Salva no repositório
            var produtorSalvo = await _produtorRepository.AdicionarAsync(produtor);

            // Tenta validação automática
            await _domainService.ValidarAutomaticamenteAsync(produtorSalvo);

            var produtorDto = _mapper.Map<ProdutorDto>(produtorSalvo);
            return Result<ProdutorDto>.Success(produtorDto);
        }
        catch (ArgumentException ex)
        {
            return Result<ProdutorDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<ProdutorDto>.Failure($"Erro interno: {ex.Message}");
        }
    }

    public async Task<Result<ProdutorDto>> CriarCompletoAsync(CriarProdutorCompletoRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            // 1. Validar documento
            Cpf? cpf = null;
            Cnpj? cnpj = null;
            
            if (request.TipoCliente.ToUpper() == "PF")
            {
                cpf = new Cpf(request.CpfCnpj);
                var existeCpf = await _produtorRepository.ExistePorCpfAsync(cpf.Valor);
                if (existeCpf)
                    return Result<ProdutorDto>.Failure("Já existe um produtor cadastrado com este CPF");
            }
            else
            {
                cnpj = new Cnpj(request.CpfCnpj);
                var existeCnpj = await _produtorRepository.ExistePorCnpjAsync(cnpj.Valor);
                if (existeCnpj)
                    return Result<ProdutorDto>.Failure("Já existe um produtor cadastrado com este CNPJ");
            }

            // 2. Criar produtor
            var produtor = new Produtor(
                request.Nome,
                cpf,
                cnpj,
                request.InscricaoEstadual,
                request.TipoAtividade,
                request.Telefone1,
                request.Telefone2,
                request.Telefone3,
                request.Email,
                new AreaPlantio(request.AreaPlantio));

            // Adicionar culturas
            foreach (var culturaId in request.Culturas)
            {
                produtor.AdicionarCultura(culturaId);
            }

            var produtorCriado = await _produtorRepository.AdicionarAsync(produtor);

            // 3. Criar usuário master
            var criarUsuarioDto = new CriarUsuarioDto
            {
                Nome = request.UsuarioMaster.Nome,
                Email = request.UsuarioMaster.Email,
                Celular = request.UsuarioMaster.Telefone,
                Cpf = request.UsuarioMaster.Cpf,
                Senha = request.UsuarioMaster.Senha,
                Roles = new List<Roles> { Roles.RoleComprador }
            };
            
            var usuarioCriado = await _usuarioService.CriarAsync(criarUsuarioDto, cancellationToken);

            // 4. Criar relacionamento UsuarioProdutor
            var usuarioProdutor = new UsuarioProdutor(
                usuarioCriado.Id,
                produtorCriado.Id,
                true); // É proprietário principal
            
            await _usuarioProdutorRepository.AdicionarAsync(usuarioProdutor, cancellationToken);

            // 5. Tentar validação automática
            await _domainService.ValidarAutomaticamenteAsync(produtorCriado);

            var produtorDto = _mapper.Map<ProdutorDto>(produtorCriado);
            return Result<ProdutorDto>.Success(produtorDto);
        }
        catch (ArgumentException ex)
        {
            return Result<ProdutorDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<ProdutorDto>.Failure($"Erro interno: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<ProdutorDto>> AtualizarAsync(int id, AtualizarProdutorDto dto)
    {
        try
        {
            var produtor = await _produtorRepository.ObterPorIdAsync(id);
            if (produtor == null)
                return Result<ProdutorDto>.Failure("Produtor não encontrado");

            if (!_domainService.PodeSerEditado(produtor))
                return Result<ProdutorDto>.Failure("Este produtor não pode ser editado");

            // Atualiza os dados básicos
            produtor.AtualizarDados(
                dto.Nome,
                dto.InscricaoEstadual,
                dto.TipoAtividade,
                dto.Telefone1,
                dto.Telefone2,
                dto.Telefone3,
                dto.Email);

            // Atualiza área de plantio
            produtor.AtualizarAreaPlantio(new AreaPlantio(dto.AreaPlantio));

            // Atualiza culturas
            var culturasProperty = typeof(Produtor).GetProperty("Culturas");
            culturasProperty?.SetValue(produtor, dto.Culturas);

            await _produtorRepository.AtualizarAsync(produtor);

            var produtorDto = _mapper.Map<ProdutorDto>(produtor);
            return Result<ProdutorDto>.Success(produtorDto);
        }
        catch (Exception ex)
        {
            return Result<ProdutorDto>.Failure($"Erro interno: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> RemoverAsync(int id)
    {
        try
        {
            var produtor = await _produtorRepository.ObterPorIdAsync(id);
            if (produtor == null)
                return Result<bool>.Failure("Produtor não encontrado");

            await _produtorRepository.RemoverAsync(id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Erro interno: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<ProdutorDto>> ValidarAutomaticamenteAsync(int id)
    {
        try
        {
            var produtor = await _produtorRepository.ObterPorIdAsync(id);
            if (produtor == null)
                return Result<ProdutorDto>.Failure("Produtor não encontrado");

            var resultado = await _domainService.ValidarAutomaticamenteAsync(produtor);
            await _produtorRepository.AtualizarAsync(produtor);

            var produtorDto = _mapper.Map<ProdutorDto>(produtor);
            
            if (resultado.Sucesso)
                return Result<ProdutorDto>.Success(produtorDto);
            else
                return Result<ProdutorDto>.Failure(resultado.Mensagem);
        }
        catch (Exception ex)
        {
            return Result<ProdutorDto>.Failure($"Erro interno: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<ProdutorDto>> AutorizarManualmenteAsync(int id, int usuarioAutorizacaoId)
    {
        try
        {
            var produtor = await _produtorRepository.ObterPorIdAsync(id);
            if (produtor == null)
                return Result<ProdutorDto>.Failure("Produtor não encontrado");

            produtor.AtualizarStatus(Agriis.Produtores.Dominio.Enums.StatusProdutor.AutorizadoManualmente, usuarioAutorizacaoId);
            await _produtorRepository.AtualizarAsync(produtor);

            var produtorDto = _mapper.Map<ProdutorDto>(produtor);
            return Result<ProdutorDto>.Success(produtorDto);
        }
        catch (Exception ex)
        {
            return Result<ProdutorDto>.Failure($"Erro interno: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<ProdutorDto>> NegarAsync(int id, int usuarioAutorizacaoId)
    {
        try
        {
            var produtor = await _produtorRepository.ObterPorIdAsync(id);
            if (produtor == null)
                return Result<ProdutorDto>.Failure("Produtor não encontrado");

            produtor.AtualizarStatus(Agriis.Produtores.Dominio.Enums.StatusProdutor.Negado, usuarioAutorizacaoId);
            await _produtorRepository.AtualizarAsync(produtor);

            var produtorDto = _mapper.Map<ProdutorDto>(produtor);
            return Result<ProdutorDto>.Success(produtorDto);
        }
        catch (Exception ex)
        {
            return Result<ProdutorDto>.Failure($"Erro interno: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ProdutorEstatisticasDto> ObterEstatisticasAsync()
    {
        var estatisticas = await _produtorRepository.ObterEstatisticasAsync();
        return _mapper.Map<ProdutorEstatisticasDto>(estatisticas);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProdutorDto>> ObterPorFornecedorAsync(int fornecedorId)
    {
        var produtores = await _produtorRepository.ObterPorFornecedorAsync(fornecedorId);
        return _mapper.Map<IEnumerable<ProdutorDto>>(produtores);
    }
}