using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;
using Agriis.Tests.Shared.Authentication;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Culturas
/// Migrado de test_culturas.py
/// </summary>
public class TestCulturas : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestCulturas(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_List_Culturas()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("api/culturas/");
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

            // Verificar tipos dos campos
            var id = obj["id"]!.Value<int>();
            var nome = obj["nome"]!.Value<string>();

            id.Should().BeGreaterThan(0);
            nome.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Test_Find_By_Id()
    {
        await AuthenticateAsProducerAsync();

        var culturaId = 1;
        var response = await GetAsync($"api/culturas/{culturaId}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura da resposta
        obj.Properties().Should().HaveCount(4);
        _jsonMatchers.ShouldHaveProperty(obj, "id");
        _jsonMatchers.ShouldHaveProperty(obj, "descricao");
        _jsonMatchers.ShouldHaveProperty(obj, "estado");
        _jsonMatchers.ShouldHaveProperty(obj, "municipio");

        // Verificar se o ID retornado corresponde ao solicitado
        var id = obj["id"]!.Value<int>();
        id.Should().Be(culturaId);
    }

    [Fact]
    public async Task Test_List_Culturas_Without_Authentication()
    {
        ClearAuthentication();

        var response = await GetAsync("api/culturas/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Find_By_Id_Without_Authentication()
    {
        ClearAuthentication();

        var response = await GetAsync("api/culturas/1");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Find_By_Id_Not_Found()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("api/culturas/99999");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Test_List_Culturas_Ordered_By_Name()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("api/culturas/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        if (array.Count > 1)
        {
            // Verificar se as culturas estão ordenadas por nome
            string? nomeAnterior = null;
            
            foreach (var item in array)
            {
                var obj = _jsonMatchers.ShouldBeObject(item);
                var nome = obj["nome"]!.Value<string>()!;

                if (nomeAnterior != null)
                {
                    string.Compare(nome, nomeAnterior, StringComparison.OrdinalIgnoreCase)
                        .Should().BeGreaterOrEqualTo(0);
                }
                
                nomeAnterior = nome;
            }
        }
    }

    [Fact]
    public async Task Test_List_Culturas_With_Search()
    {
        await AuthenticateAsProducerAsync();

        var searchTerm = "soja";
        var response = await GetAsync($"api/culturas/?search={searchTerm}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        // Verificar se os resultados contêm o termo de busca
        foreach (var item in array)
        {
            var obj = _jsonMatchers.ShouldBeObject(item);
            var nome = obj["nome"]!.Value<string>()!;
            
            nome.Should().Contain(searchTerm, "resultado deve conter o termo de busca");
        }
    }

    [Fact]
    public async Task Test_Cultura_Has_Valid_Structure()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("api/culturas/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        foreach (var item in array)
        {
            var obj = _jsonMatchers.ShouldBeObject(item);
            
            // Verificar se todos os campos obrigatórios estão presentes
            _jsonMatchers.ShouldHaveProperty(obj, "id");
            _jsonMatchers.ShouldHaveProperty(obj, "nome");

            // Verificar tipos e valores válidos
            var id = obj["id"]!.Value<int>();
            var nome = obj["nome"]!.Value<string>();

            id.Should().BeGreaterThan(0);
            nome.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task Test_Find_Cultura_With_Details()
    {
        await AuthenticateAsProducerAsync();

        var culturaId = 1;
        var response = await GetAsync($"api/culturas/{culturaId}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Verificar se os campos detalhados estão presentes
        _jsonMatchers.ShouldHaveProperty(obj, "id");
        _jsonMatchers.ShouldHaveProperty(obj, "descricao");
        _jsonMatchers.ShouldHaveProperty(obj, "estado");
        _jsonMatchers.ShouldHaveProperty(obj, "municipio");

        // Verificar tipos dos campos
        var id = obj["id"]!.Value<int>();
        var descricao = obj["descricao"]!.Value<string>();

        id.Should().BeGreaterThan(0);
        descricao.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Test_List_Culturas_Performance()
    {
        await AuthenticateAsProducerAsync();

        var startTime = DateTime.UtcNow;
        var response = await GetAsync("api/culturas/");
        var endTime = DateTime.UtcNow;

        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        // Verificar se a resposta foi rápida (menos de 2 segundos)
        var duration = endTime - startTime;
        duration.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Test_Cultura_Ids_Are_Unique()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("api/culturas/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        var ids = new HashSet<int>();
        
        foreach (var item in array)
        {
            var obj = _jsonMatchers.ShouldBeObject(item);
            var id = obj["id"]!.Value<int>();
            
            ids.Add(id).Should().BeTrue("IDs devem ser únicos");
        }
    }
}