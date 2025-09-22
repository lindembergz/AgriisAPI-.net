using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;
using Agriis.Tests.Shared.Authentication;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Segmentações
/// Migrado de test_segmentacoes.py
/// </summary>
public class TestSegmentacoes : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestSegmentacoes(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_List_Grupo_Segmentacao_By_Fornecedor()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 10
        };

        var response = await PostAsync("v1/segmentacoes/grupos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura da resposta
        obj.Properties().Should().HaveCount(5);
        _jsonMatchers.ShouldHaveProperty(obj, "items");
        _jsonMatchers.ShouldHaveProperty(obj, "max_per_page");
        _jsonMatchers.ShouldHaveProperty(obj, "page");
        _jsonMatchers.ShouldHaveProperty(obj, "col_segmentacoes");
        _jsonMatchers.ShouldHaveProperty(obj, "total");

        var colSegmentacoes = _jsonMatchers.ShouldBeArray(obj["col_segmentacoes"]!);
        var items = _jsonMatchers.ShouldBeArray(obj["items"]!);

        if (items.Count > 0)
        {
            var firstItem = items[0];
            var itemObj = _jsonMatchers.ShouldBeObject(firstItem);

            // Validar estrutura do item (col_segmentacoes + 3 campos fixos)
            itemObj.Properties().Should().HaveCount(colSegmentacoes.Count + 3);
            _jsonMatchers.ShouldHaveProperty(itemObj, "id");
            _jsonMatchers.ShouldHaveProperty(itemObj, "categoria_nome");
            _jsonMatchers.ShouldHaveProperty(itemObj, "grupo_descricao");
        }
    }

    [Fact]
    public async Task Test_List_Grupo_Segmentacao_Invalid_Max_Per_Page()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 0 // Valor inválido
        };

        var response = await PostAsync("v1/segmentacoes/grupos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Test_List_Grupo_Segmentacao_Invalid_Page()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            page = -1, // Valor inválido
            max_per_page = 10
        };

        var response = await PostAsync("v1/segmentacoes/grupos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Test_Create_Grupo()
    {
        // Teste do cadastro de um grupo aleatoriamente
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            grupo = new
            {
                area_minima = 101,
                area_maxima = 200,
                categoria_id = 3
            },
            grupo_segmentacoes = new[]
            {
                new { segmentacao_id = 3, percentual_desconto = 15.4 },
                new { segmentacao_id = 4, percentual_desconto = 10.1 }
            }
        };

        var response = await PostAsync("v1/segmentacoes/grupos/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        var grupoId = GetIdFromLocationHeader(response);
        grupoId.Should().BeGreaterThan(0);

        // Teste de atualização
        var updateData = new
        {
            grupo = new
            {
                area_minima = 101,
                area_maxima = 200,
                categoria_id = 4
            },
            categoria_id = 4,
            grupo_segmentacoes = new[]
            {
                new { segmentacao_id = 3, percentual_desconto = 15 }
            }
        };

        var updateResponse = await PutAsync($"v1/segmentacoes/grupos/{grupoId}/", updateData);
        _jsonMatchers.ShouldHaveStatusCode(updateResponse, HttpStatusCode.OK);

        // Teste de consulta por ID
        var getResponse = await GetAsync($"v1/segmentacoes/grupos/{grupoId}/");
        _jsonMatchers.ShouldHaveStatusCode(getResponse, HttpStatusCode.OK);

        var getJson = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var getObj = _jsonMatchers.ShouldBeObject(getJson);

        // Validar estrutura da resposta
        getObj.Properties().Should().HaveCount(6);
        _jsonMatchers.ShouldHaveProperty(getObj, "id");
        _jsonMatchers.ShouldHaveProperty(getObj, "area_maxima");
        _jsonMatchers.ShouldHaveProperty(getObj, "area_minima");
        _jsonMatchers.ShouldHaveProperty(getObj, "categoria");
        _jsonMatchers.ShouldHaveProperty(getObj, "fornecedor");
        _jsonMatchers.ShouldHaveProperty(getObj, "grupo_segmentacoes");

        // Teste de exclusão
        var deleteResponse = await DeleteAsync($"v1/segmentacoes/grupos/{grupoId}/");
        _jsonMatchers.ShouldHaveStatusCode(deleteResponse, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Create_Or_Update_Produtor_Fornecedor()
    {
        // Teste de atualização da segmentação do produtor
        await AuthenticateAsSupplierAsync();

        var segmentacaoId = 3;
        var produtorId = 27;

        var response = await PostAsync($"v1/segmentacoes/{segmentacaoId}/produtor/{produtorId}/", new { });
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Find_All()
    {
        // Teste de listagem de todas as segmentações
        await AuthenticateAsSupplierAsync();

        var response = await GetAsync("v1/segmentacoes/");
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
    public async Task Test_Create_Grupo_Without_Authentication()
    {
        ClearAuthentication();

        var requestData = new
        {
            grupo = new
            {
                area_minima = 101,
                area_maxima = 200,
                categoria_id = 3
            },
            grupo_segmentacoes = new[]
            {
                new { segmentacao_id = 3, percentual_desconto = 15.4 }
            }
        };

        var response = await PostAsync("v1/segmentacoes/grupos/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Create_Grupo_With_Invalid_Area()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            grupo = new
            {
                area_minima = 200, // Área mínima maior que máxima
                area_maxima = 100,
                categoria_id = 3
            },
            grupo_segmentacoes = new[]
            {
                new { segmentacao_id = 3, percentual_desconto = 15.4 }
            }
        };

        var response = await PostAsync("v1/segmentacoes/grupos/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Create_Grupo_With_Invalid_Percentual()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            grupo = new
            {
                area_minima = 101,
                area_maxima = 200,
                categoria_id = 3
            },
            grupo_segmentacoes = new[]
            {
                new { segmentacao_id = 3, percentual_desconto = 150.0 } // Percentual inválido
            }
        };

        var response = await PostAsync("v1/segmentacoes/grupos/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }
}