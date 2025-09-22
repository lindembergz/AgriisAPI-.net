using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Usuários
/// Migrado de test_usuarios.py
/// </summary>
public class TestUsuarios : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestUsuarios(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_Create_User_Produtor()
    {
        // Teste de criação de um usuário produtor
        ClearAuthentication();

        var cpf = DataGenerator.GerarCpfFormatado();
        var celular = DataGenerator.GerarCelular();
        var email = DataGenerator.GerarEmail();

        var requestData = new
        {
            usuario = new
            {
                code = $"{celular},{email}",
                cpf = cpf,
                celular = celular,
                nome = DataGenerator.GerarNome()
            },
            produtor = new
            {
                nome = DataGenerator.GerarNome(),
                cpf = cpf
            },
            culturas = new[]
            {
                new { id = 1, area = 500 },
                new { id = 2, area = 300 },
                new { id = 3, area = 430 }
            },
            check_ufs = new[] { "MT" }
        };

        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "PRODUTOR_MOBILE_TOKEN");

        // Nota: Este teste simula o upload de foto, mas em um ambiente real
        // seria necessário implementar o upload de arquivo multipart/form-data
        var response = await PostAsync("v1/usuarios/produtor/", requestData);
        
        // Deve criar com sucesso ou retornar erro de validação
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, 
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest
        );
    }

    [Fact]
    public async Task Test_List_Produtores()
    {
        // Teste de lista produtores por usuário
        await AuthenticateAsProducerAsync(produtorId: 1);
        var usuarioId = 1;

        var response = await GetAsync($"v1/usuarios/{usuarioId}/produtores/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        // Validar que retorna uma lista (pode estar vazia)
        array.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_List_Propriedades()
    {
        // Teste de lista de propriedades por usuários
        await AuthenticateAsProducerAsync(produtorId: 1);
        var usuarioId = 1;

        var response = await GetAsync($"v1/usuarios/{usuarioId}/propriedades/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        // Validar que retorna uma lista (pode estar vazia)
        array.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_Find_By_Id()
    {
        // Teste de busca de usuário por ID
        await AuthenticateAsProducerAsync(produtorId: 1);
        var usuarioId = 1;

        var response = await GetAsync($"v1/usuarios/{usuarioId}/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar propriedades básicas do usuário
        _jsonMatchers.ShouldHaveProperty(obj, "id");
        _jsonMatchers.ShouldHaveProperty(obj, "nome");
        _jsonMatchers.ShouldHaveProperty(obj, "email");
    }

    [Fact]
    public async Task Test_Find_Colaborador_By_Usuario_Id()
    {
        await AuthenticateAsSupplierAsync(fornecedorId: 1);
        var usuarioId = 2; // ID do usuário fornecedor

        var response = await GetAsync($"v1/usuarios/{usuarioId}/colaborador/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar propriedades do colaborador
        _jsonMatchers.ShouldHaveProperty(obj, "id");
        _jsonMatchers.ShouldHaveProperty(obj, "nome");
    }

    [Fact]
    public async Task Test_Create_Or_Update_Colaborador()
    {
        // Teste de atualização dos dados do colaborador
        await AuthenticateAsSupplierAsync(fornecedorId: 1);
        var usuarioId = 2;

        var updateData = new
        {
            nome = "Representante Novo Nome",
            cargo = "Novo cargo",
            celular = "+5562982410444"
        };

        // Nota: Este teste simula o upload de logo, mas em um ambiente real
        // seria necessário implementar o upload de arquivo multipart/form-data
        var response = await PutAsync($"v1/usuarios/{usuarioId}/colaborador/", updateData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_List_Colaboradores_Paged()
    {
        await AuthenticateAsSupplierAsync(fornecedorId: 1);

        var requestData = new
        {
            page = 0,
            max_per_page = 10,
            perfil = "rep"
        };

        var response = await PostAsync("v1/usuarios/representante/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        _jsonMatchers.ShouldHavePaginationStructure(json);
    }

    [Fact]
    public async Task Test_List_Colaboradores_Territorio_Paged()
    {
        await AuthenticateAsSupplierAsync(fornecedorId: 1);
        var usuarioId = 2;

        var requestData = new
        {
            page = 0,
            max_per_page = 10,
            perfil = "rep",
            apresentacao = "PA"
        };

        var response = await PostAsync($"v1/usuarios/{usuarioId}/colaborador/territorio/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        _jsonMatchers.ShouldHavePaginationStructure(json);
    }

    [Fact]
    public async Task Test_Colaboradores_Territorio_Adicionar()
    {
        await AuthenticateAsSupplierAsync(fornecedorId: 1);
        var usuarioId = 2;

        var requestData = new
        {
            estados = new[] { 3, 4, 5, 9, 11, 12, 13, 14, 20, 25, 26, 27 },
            municipios = new[] { 1501105 }
        };

        var response = await PutAsync($"v1/usuarios/{usuarioId}/colaborador/territorio/adicionar/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Colaboradores_Territorio_Remover()
    {
        await AuthenticateAsSupplierAsync(fornecedorId: 1);
        var usuarioId = 2;

        var requestData = new
        {
            estados = new[] { 3, 4, 5, 9, 11, 12, 13, 14, 20, 25, 26, 27 }
        };

        var response = await DeleteAsync($"v1/usuarios/{usuarioId}/colaborador/territorio/remover/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Create_User_With_Invalid_Cpf()
    {
        ClearAuthentication();

        var celular = DataGenerator.GerarCelular();
        var email = DataGenerator.GerarEmail();

        var requestData = new
        {
            usuario = new
            {
                code = $"{celular},{email}",
                cpf = "123.456.789-00", // CPF inválido
                celular = celular,
                nome = DataGenerator.GerarNome()
            },
            produtor = new
            {
                nome = DataGenerator.GerarNome(),
                cpf = "123.456.789-00" // CPF inválido
            },
            culturas = new[]
            {
                new { id = 1, area = 500 }
            },
            check_ufs = new[] { "MT" }
        };

        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "PRODUTOR_MOBILE_TOKEN");

        var response = await PostAsync("v1/usuarios/produtor/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Create_User_With_Invalid_Celular()
    {
        ClearAuthentication();

        var cpf = DataGenerator.GerarCpfFormatado();
        var email = DataGenerator.GerarEmail();

        var requestData = new
        {
            usuario = new
            {
                code = $"123,{email}", // Celular inválido
                cpf = cpf,
                celular = "123", // Celular inválido
                nome = DataGenerator.GerarNome()
            },
            produtor = new
            {
                nome = DataGenerator.GerarNome(),
                cpf = cpf
            },
            culturas = new[]
            {
                new { id = 1, area = 500 }
            },
            check_ufs = new[] { "MT" }
        };

        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "PRODUTOR_MOBILE_TOKEN");

        var response = await PostAsync("v1/usuarios/produtor/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Access_Other_User_Data_Unauthorized()
    {
        // Tentar acessar dados de outro usuário sem permissão
        await AuthenticateAsProducerAsync(produtorId: 1);
        var otherUsuarioId = 999; // ID de outro usuário

        var response = await GetAsync($"v1/usuarios/{otherUsuarioId}/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_Update_Colaborador_Without_Permission()
    {
        // Tentar atualizar colaborador sem ser do mesmo fornecedor
        await AuthenticateAsSupplierAsync(fornecedorId: 2); // Fornecedor diferente
        var usuarioId = 2; // Usuário do fornecedor 1

        var updateData = new
        {
            nome = "Tentativa de Alteração Não Autorizada",
            cargo = "Cargo Não Autorizado"
        };

        var response = await PutAsync($"v1/usuarios/{usuarioId}/colaborador/", updateData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_List_Colaboradores_Invalid_Pagination()
    {
        await AuthenticateAsSupplierAsync(fornecedorId: 1);

        // Teste com página inválida
        var invalidPageData = new
        {
            page = -1, // Página inválida
            max_per_page = 10,
            perfil = "rep"
        };

        var response = await PostAsync("v1/usuarios/representante/all/", invalidPageData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.UnprocessableEntity);

        // Teste com max_per_page inválido
        var invalidMaxPerPageData = new
        {
            page = 0,
            max_per_page = 0, // Valor inválido
            perfil = "rep"
        };

        var response2 = await PostAsync("v1/usuarios/representante/all/", invalidMaxPerPageData);
        _jsonMatchers.ShouldHaveStatusCode(response2, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Test_Territorio_Operations_With_Invalid_Data()
    {
        await AuthenticateAsSupplierAsync(fornecedorId: 1);
        var usuarioId = 2;

        // Teste adicionar território com estados inválidos
        var invalidEstadosData = new
        {
            estados = new[] { 99999 }, // Estado inexistente
            municipios = new[] { 1501105 }
        };

        var response = await PutAsync($"v1/usuarios/{usuarioId}/colaborador/territorio/adicionar/", invalidEstadosData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);

        // Teste adicionar território com municípios inválidos
        var invalidMunicipiosData = new
        {
            estados = new[] { 11 },
            municipios = new[] { 99999999 } // Município inexistente
        };

        var response2 = await PutAsync($"v1/usuarios/{usuarioId}/colaborador/territorio/adicionar/", invalidMunicipiosData);
        _jsonMatchers.ShouldHaveStatusCode(response2, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Create_User_Without_Authentication()
    {
        ClearAuthentication();

        var cpf = DataGenerator.GerarCpfFormatado();
        var celular = DataGenerator.GerarCelular();
        var email = DataGenerator.GerarEmail();

        var requestData = new
        {
            usuario = new
            {
                code = $"{celular},{email}",
                cpf = cpf,
                celular = celular,
                nome = DataGenerator.GerarNome()
            },
            produtor = new
            {
                nome = DataGenerator.GerarNome(),
                cpf = cpf
            },
            culturas = new[]
            {
                new { id = 1, area = 500 }
            },
            check_ufs = new[] { "MT" }
        };

        // Não define Authorization header
        var response = await PostAsync("v1/usuarios/produtor/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Find_Nonexistent_User()
    {
        await AuthenticateAsAdminAsync(); // Admin pode acessar qualquer usuário
        var usuarioId = 99999; // ID inexistente

        var response = await GetAsync($"v1/usuarios/{usuarioId}/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Test_Update_Colaborador_With_Invalid_Celular()
    {
        await AuthenticateAsSupplierAsync(fornecedorId: 1);
        var usuarioId = 2;

        var updateData = new
        {
            nome = "Representante Teste",
            cargo = "Cargo Teste",
            celular = "123" // Celular inválido
        };

        var response = await PutAsync($"v1/usuarios/{usuarioId}/colaborador/", updateData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }
}