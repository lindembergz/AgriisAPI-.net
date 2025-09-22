using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;
using Agriis.Tests.Shared.Authentication;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Pontos de Distribuição
/// Migrado de test_pontos_distribuicao.py
/// </summary>
public class TestPontosDistribuicao : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestPontosDistribuicao(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_List_Paged()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 10,
            descricao = "mirante",
            endereco = "mirante"
        };

        var response = await PostAsync("v1/pontos_distribuicao/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura da resposta
        obj.Properties().Should().HaveCount(4);
        _jsonMatchers.ShouldHaveProperty(obj, "items");
        _jsonMatchers.ShouldHaveProperty(obj, "max_per_page");
        _jsonMatchers.ShouldHaveProperty(obj, "page");
        _jsonMatchers.ShouldHaveProperty(obj, "total");

        var items = _jsonMatchers.ShouldBeArray(obj["items"]!);
        if (items.Count > 0)
        {
            var firstItem = items[0];
            var itemObj = _jsonMatchers.ShouldBeObject(firstItem);

            // Validar estrutura do item
            itemObj.Properties().Should().HaveCount(3);
            _jsonMatchers.ShouldHaveProperty(itemObj, "descricao");
            _jsonMatchers.ShouldHaveProperty(itemObj, "id");
            _jsonMatchers.ShouldHaveProperty(itemObj, "endereco");
        }
    }

    [Fact]
    public async Task Test_List_Paged_Invalid_Max_Per_Page()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 0, // Valor inválido
            descricao = "mirante",
            endereco = "mirante"
        };

        var response = await PostAsync("v1/pontos_distribuicao/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Test_List_Paged_Invalid_Page()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            page = -1, // Valor inválido
            max_per_page = 10,
            descricao = "mirante",
            endereco = "mirante"
        };

        var response = await PostAsync("v1/pontos_distribuicao/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Test_Find_By_Id()
    {
        await AuthenticateAsProducerAsync();

        var pontoDistribuicaoId = 5;
        var response = await GetAsync($"v1/pontos_distribuicao/{pontoDistribuicaoId}");
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
    public async Task Test_Create_Delete()
    {
        // Teste do cadastro de um ponto de distribuição
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            descricao = DataGenerator.GerarNome(),
            municipio_id = 1505031,
            location = new[] { 0.0000, 90.0000 }
        };

        var response = await PostAsync("v1/pontos_distribuicao/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        var pontoDistribuicaoId = GetIdFromLocationHeader(response);
        pontoDistribuicaoId.Should().BeGreaterThan(0);

        // Teste de exclusão
        var deleteResponse = await DeleteAsync($"v1/pontos_distribuicao/{pontoDistribuicaoId}/");
        _jsonMatchers.ShouldHaveStatusCode(deleteResponse, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Create_Without_Authentication()
    {
        ClearAuthentication();

        var requestData = new
        {
            descricao = DataGenerator.GerarNome(),
            municipio_id = 1505031,
            location = new[] { 0.0000, 90.0000 }
        };

        var response = await PostAsync("v1/pontos_distribuicao/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Create_With_Invalid_Municipio()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            descricao = DataGenerator.GerarNome(),
            municipio_id = 99999999, // Município inexistente
            location = new[] { 0.0000, 90.0000 }
        };

        var response = await PostAsync("v1/pontos_distribuicao/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Create_With_Invalid_Location()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            descricao = DataGenerator.GerarNome(),
            municipio_id = 1505031,
            location = new[] { 200.0000, 200.0000 } // Coordenadas inválidas
        };

        var response = await PostAsync("v1/pontos_distribuicao/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Find_By_Id_Not_Found()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("v1/pontos_distribuicao/99999");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Test_Delete_Not_Found()
    {
        await AuthenticateAsSupplierAsync();

        var response = await DeleteAsync("v1/pontos_distribuicao/99999/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Test_Create_With_Empty_Descricao()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            descricao = "", // Descrição vazia
            municipio_id = 1505031,
            location = new[] { 0.0000, 90.0000 }
        };

        var response = await PostAsync("v1/pontos_distribuicao/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_List_Paged_With_Filters()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 5,
            descricao = "test",
            endereco = "test"
        };

        var response = await PostAsync("v1/pontos_distribuicao/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Verificar se os filtros foram aplicados
        _jsonMatchers.ShouldHaveProperty(obj, "items");
        _jsonMatchers.ShouldHaveProperty(obj, "total");

        var maxPerPage = obj["max_per_page"]!.Value<int>();
        maxPerPage.Should().Be(5);

        var page = obj["page"]!.Value<int>();
        page.Should().Be(0);
    }
}