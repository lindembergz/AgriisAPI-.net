using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;
using Agriis.Tests.Shared.Authentication;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Acessos
/// Migrado de test_acessos.py
/// </summary>
public class TestAcessos : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestAcessos(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_Find_By_Id()
    {
        await AuthenticateAsAdminAsync();

        var idParam = 1;
        var response = await GetAsync($"api/acessos/OUTRO/{idParam}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura básica do acesso
        _jsonMatchers.ShouldHaveProperty(obj, "id");
        _jsonMatchers.ShouldHaveProperty(obj, "tipo");
        _jsonMatchers.ShouldHaveProperty(obj, "data_tentativa");
        _jsonMatchers.ShouldHaveProperty(obj, "dados");

        // Verificar se o ID retornado corresponde ao solicitado
        var id = obj["id"]!.Value<int>();
        id.Should().Be(idParam);

        // Verificar se o tipo é OUTRO
        var tipo = obj["tipo"]!.Value<string>();
        tipo.Should().Be("OUTRO");
    }

    [Fact]
    public async Task Test_List_Tentativas()
    {
        await AuthenticateAsAdminAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 10,
            cpf = "542",
            cultura = "algo",
            moeda = "rea",
            descricao = "cat",
            ponto_distribuicao = "mir"
        };

        var response = await PostAsync("api/acessos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura de paginação
        _jsonMatchers.ShouldHaveProperty(obj, "items");
        _jsonMatchers.ShouldHaveProperty(obj, "max_per_page");
        _jsonMatchers.ShouldHaveProperty(obj, "page");
        _jsonMatchers.ShouldHaveProperty(obj, "total");

        var items = _jsonMatchers.ShouldBeArray(obj["items"]!);
        var maxPerPage = obj["max_per_page"]!.Value<int>();
        var page = obj["page"]!.Value<int>();

        maxPerPage.Should().Be(10);
        page.Should().Be(0);

        if (items.Count > 0)
        {
            var firstItem = items[0];
            var itemObj = _jsonMatchers.ShouldBeObject(firstItem);

            // Validar estrutura do item de acesso
            _jsonMatchers.ShouldHaveProperty(itemObj, "id");
            _jsonMatchers.ShouldHaveProperty(itemObj, "tipo");
            _jsonMatchers.ShouldHaveProperty(itemObj, "data_tentativa");
            _jsonMatchers.ShouldHaveProperty(itemObj, "dados");
        }
    }

    [Fact]
    public async Task Test_Find_By_Id_Without_Authentication()
    {
        ClearAuthentication();

        var response = await GetAsync("api/acessos/OUTRO/1");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_List_Tentativas_Without_Authentication()
    {
        ClearAuthentication();

        var requestData = new
        {
            page = 0,
            max_per_page = 10
        };

        var response = await PostAsync("api/acessos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Find_By_Id_Not_Found()
    {
        await AuthenticateAsAdminAsync();

        var response = await GetAsync("api/acessos/OUTRO/99999");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Test_List_Tentativas_Invalid_Max_Per_Page()
    {
        await AuthenticateAsAdminAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 0 // Valor inválido
        };

        var response = await PostAsync("api/acessos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Test_List_Tentativas_Invalid_Page()
    {
        await AuthenticateAsAdminAsync();

        var requestData = new
        {
            page = -1, // Valor inválido
            max_per_page = 10
        };

        var response = await PostAsync("api/acessos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Test_Find_By_Different_Tipos()
    {
        await AuthenticateAsAdminAsync();

        var tipos = new[] { "SERPRO", "LOGIN", "CADASTRO", "OUTRO" };

        foreach (var tipo in tipos)
        {
            var response = await GetAsync($"api/acessos/{tipo}/1");
            
            // Pode retornar OK se existe ou NotFound se não existe, mas não deve dar erro de autorização
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task Test_List_Tentativas_With_Filters()
    {
        await AuthenticateAsAdminAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 5,
            cpf = "123",
            tipo = "SERPRO"
        };

        var response = await PostAsync("api/acessos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        var maxPerPage = obj["max_per_page"]!.Value<int>();
        maxPerPage.Should().Be(5);

        var items = _jsonMatchers.ShouldBeArray(obj["items"]!);
        
        // Verificar se os filtros foram aplicados (se houver resultados)
        foreach (var item in items)
        {
            var itemObj = _jsonMatchers.ShouldBeObject(item);
            var dados = itemObj["dados"]?.Value<string>();
            
            // Se há dados, verificar se contém o CPF filtrado
            if (!string.IsNullOrEmpty(dados))
            {
                dados.Should().Contain("123", "Dados devem conter o CPF filtrado");
            }
        }
    }

    [Fact]
    public async Task Test_Acesso_Data_Structure()
    {
        await AuthenticateAsAdminAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 10
        };

        var response = await PostAsync("api/acessos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        var items = _jsonMatchers.ShouldBeArray(obj["items"]!);

        foreach (var item in items)
        {
            var itemObj = _jsonMatchers.ShouldBeObject(item);
            
            // Verificar tipos dos campos obrigatórios
            var id = itemObj["id"]!.Value<int>();
            var tipo = itemObj["tipo"]!.Value<string>();
            var dataTentativa = itemObj["data_tentativa"]!.Value<string>();

            id.Should().BeGreaterThan(0);
            tipo.Should().NotBeNullOrEmpty();
            DateTime.TryParse(dataTentativa, out _).Should().BeTrue("Data deve ser válida");

            // Verificar se o tipo é um dos valores válidos
            tipo.Should().BeOneOf("SERPRO", "LOGIN", "CADASTRO", "OUTRO");
        }
    }

    [Fact]
    public async Task Test_List_Tentativas_Ordered_By_Date()
    {
        await AuthenticateAsAdminAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 10
        };

        var response = await PostAsync("api/acessos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        var items = _jsonMatchers.ShouldBeArray(obj["items"]!);

        if (items.Count > 1)
        {
            // Verificar se os acessos estão ordenados por data (mais recentes primeiro)
            DateTime? dataAnterior = null;
            
            foreach (var item in items)
            {
                var itemObj = _jsonMatchers.ShouldBeObject(item);
                var dataTentativaStr = itemObj["data_tentativa"]!.Value<string>()!;
                DateTime.TryParse(dataTentativaStr, out var dataTentativa).Should().BeTrue();

                if (dataAnterior.HasValue)
                {
                    dataTentativa.Should().BeOnOrBefore(dataAnterior.Value, 
                        "Acessos devem estar ordenados por data decrescente");
                }
                
                dataAnterior = dataTentativa;
            }
        }
    }

    [Fact]
    public async Task Test_Access_With_Non_Admin_User()
    {
        await AuthenticateAsSupplierAsync();

        var response = await GetAsync("api/acessos/OUTRO/1");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_List_Tentativas_With_Producer_User()
    {
        await AuthenticateAsProducerAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 10
        };

        var response = await PostAsync("api/acessos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Forbidden);
    }
}