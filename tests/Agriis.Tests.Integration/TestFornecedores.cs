using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Fornecedores
/// Migrado de test_fornecedores.py
/// </summary>
public class TestFornecedores : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestFornecedores(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_List_Fornecedores()
    {
        await AuthenticateAsSupplierAsync();

        var response = await GetAsync("api/fornecedores/?is_fabricante=true");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        if (array.Count > 0)
        {
            var firstItem = array[0];
            var obj = _jsonMatchers.ShouldBeObject(firstItem);

            // Validar estrutura do item
            obj.Properties().Should().HaveCount(2);
            _jsonMatchers.ShouldHaveProperty(obj, "id");
            _jsonMatchers.ShouldHaveProperty(obj, "nome");
        }
    }

    [Fact]
    public async Task Test_Find_By_Id()
    {
        await AuthenticateAsProducerAsync();
        var fornecedorId = 1;

        var response = await GetAsync($"api/fornecedores/{fornecedorId}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura da resposta
        obj.Properties().Should().HaveCount(4);
        _jsonMatchers.ShouldHaveProperty(obj, "id");
        _jsonMatchers.ShouldHaveProperty(obj, "descricao");
        _jsonMatchers.ShouldHaveProperty(obj, "estado");
        _jsonMatchers.ShouldHaveProperty(obj, "municipio");
    }

    [Fact]
    public async Task Test_List_Pontos_Distribuicao()
    {
        await AuthenticateAsSupplierAsync(fornecedorId: 1);
        var fornecedorId = 1;

        var response = await GetAsync($"api/fornecedores/{fornecedorId}/pontos_distribuicao/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        if (array.Count > 0)
        {
            var firstItem = array[0];
            var obj = _jsonMatchers.ShouldBeObject(firstItem);

            // Validar estrutura do item
            obj.Properties().Should().HaveCount(2);
            _jsonMatchers.ShouldHaveProperty(obj, "id");
            _jsonMatchers.ShouldHaveProperty(obj, "descricao");
        }
    }

    [Fact]
    public async Task Test_Create()
    {
        // Teste do cadastro de um novo fornecedor já com usuário
        await AuthenticateAsSupplierAsync();

        var fornecedorEmail = "forn_sementes@iagro.ag";

        var requestData = new
        {
            cpf = DataGenerator.GerarCpfFormatado(),
            cnpj = DataGenerator.GerarCnpjFormatado(),
            email = fornecedorEmail,
            nome = "ADICIONADO MANUALMENTE",
            senha = "Xpto1234",
            municipio_id = 1505031
        };

        var response = await PostAsync("api/fornecedores/", requestData);
        
        // Deve criar com sucesso ou retornar conflito se já existe
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Test_Create_Multiple_Fornecedores()
    {
        await AuthenticateAsSupplierAsync();

        var fornecedoresEmails = new[] { "forn_sementes@iagro.ag" };
        var emailsNaoAdicionados = new List<string>();

        foreach (var fornEmail in fornecedoresEmails)
        {
            var requestData = new
            {
                cpf = DataGenerator.GerarCpfFormatado(),
                cnpj = DataGenerator.GerarCnpjFormatado(),
                email = fornEmail,
                nome = "ADICIONADO MANUALMENTE",
                senha = "Xpto1234",
                municipio_id = 1505031
            };

            var response = await PostAsync("api/fornecedores/", requestData);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                emailsNaoAdicionados.Add(fornEmail);
            }
        }

        // Todos os emails devem ter sido processados com sucesso (criados ou já existentes)
        emailsNaoAdicionados.Should().BeEmpty();
    }

    [Fact]
    public async Task Test_Update()
    {
        // Teste de atualização dos dados do fornecedor
        await AuthenticateAsSupplierAsync(fornecedorId: 1);
        var fornecedorId = 1;

        // Simular upload de logo (em um teste real, seria um arquivo)
        var updateData = new
        {
            nome = "Fornecedor Novo Nome",
            municipio_id = 1505031,
            url_site = "www.teste.com.br"
        };

        var response = await PutAsync($"api/fornecedores/{fornecedorId}", updateData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Create_With_Invalid_Cpf()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            cpf = "123.456.789-00", // CPF inválido
            cnpj = DataGenerator.GerarCnpjFormatado(),
            email = DataGenerator.GerarEmail(),
            nome = "TESTE FORNECEDOR",
            senha = "Xpto1234",
            municipio_id = 1505031
        };

        var response = await PostAsync("api/fornecedores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Create_With_Invalid_Cnpj()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            cpf = DataGenerator.GerarCpfFormatado(),
            cnpj = "12.345.678/0001-00", // CNPJ inválido
            email = DataGenerator.GerarEmail(),
            nome = "TESTE FORNECEDOR",
            senha = "Xpto1234",
            municipio_id = 1505031
        };

        var response = await PostAsync("api/fornecedores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Create_With_Invalid_Email()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            cpf = DataGenerator.GerarCpfFormatado(),
            cnpj = DataGenerator.GerarCnpjFormatado(),
            email = "email-invalido", // Email inválido
            nome = "TESTE FORNECEDOR",
            senha = "Xpto1234",
            municipio_id = 1505031
        };

        var response = await PostAsync("api/fornecedores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Create_With_Weak_Password()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            cpf = DataGenerator.GerarCpfFormatado(),
            cnpj = DataGenerator.GerarCnpjFormatado(),
            email = DataGenerator.GerarEmail(),
            nome = "TESTE FORNECEDOR",
            senha = "123", // Senha fraca
            municipio_id = 1505031
        };

        var response = await PostAsync("api/fornecedores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Create_With_Invalid_Municipio()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            cpf = DataGenerator.GerarCpfFormatado(),
            cnpj = DataGenerator.GerarCnpjFormatado(),
            email = DataGenerator.GerarEmail(),
            nome = "TESTE FORNECEDOR",
            senha = "Xpto1234",
            municipio_id = 99999999 // Município inexistente
        };

        var response = await PostAsync("api/fornecedores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Update_Without_Permission()
    {
        // Tentar atualizar fornecedor sem ser o próprio fornecedor
        await AuthenticateAsSupplierAsync(fornecedorId: 2); // Fornecedor diferente
        var fornecedorId = 1;

        var updateData = new
        {
            nome = "Tentativa de Alteração Não Autorizada",
            municipio_id = 1505031
        };

        var response = await PutAsync($"api/fornecedores/{fornecedorId}", updateData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_Find_Nonexistent_Fornecedor()
    {
        await AuthenticateAsProducerAsync();
        var fornecedorId = 99999; // ID inexistente

        var response = await GetAsync($"api/fornecedores/{fornecedorId}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Test_List_Fornecedores_Without_Authentication()
    {
        ClearAuthentication();

        var response = await GetAsync("api/fornecedores/?is_fabricante=true");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Update_With_Invalid_Url_Site()
    {
        await AuthenticateAsSupplierAsync(fornecedorId: 1);
        var fornecedorId = 1;

        var updateData = new
        {
            nome = "Fornecedor Teste",
            municipio_id = 1505031,
            url_site = "url-invalida" // URL inválida
        };

        var response = await PutAsync($"api/fornecedores/{fornecedorId}", updateData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Create_Duplicate_Email()
    {
        await AuthenticateAsSupplierAsync();

        var email = DataGenerator.GerarEmail();

        var requestData = new
        {
            cpf = DataGenerator.GerarCpfFormatado(),
            cnpj = DataGenerator.GerarCnpjFormatado(),
            email = email,
            nome = "PRIMEIRO FORNECEDOR",
            senha = "Xpto1234",
            municipio_id = 1505031
        };

        // Primeira criação
        var firstResponse = await PostAsync("api/fornecedores/", requestData);
        
        // Segunda criação com mesmo email
        var duplicateData = new
        {
            cpf = DataGenerator.GerarCpfFormatado(),
            cnpj = DataGenerator.GerarCnpjFormatado(),
            email = email, // Mesmo email
            nome = "SEGUNDO FORNECEDOR",
            senha = "Xpto1234",
            municipio_id = 1505031
        };

        var duplicateResponse = await PostAsync("api/fornecedores/", duplicateData);
        
        // Deve falhar por email duplicado
        duplicateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Conflict);
    }
}