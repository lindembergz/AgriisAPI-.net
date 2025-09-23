using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;
using Agriis.Tests.Shared.Authentication;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Endereços
/// Migrado de test_enderecos.py
/// </summary>
public class TestEnderecos : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestEnderecos(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_List_Estados()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("api/enderecos/estados");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        if (array.Count > 0)
        {
            var firstItem = array[0];
            var obj = _jsonMatchers.ShouldBeObject(firstItem);

            // Validar estrutura do item
            obj.Properties().Should().HaveCount(3);
            _jsonMatchers.ShouldHaveProperty(obj, "id");
            _jsonMatchers.ShouldHaveProperty(obj, "nome");
            _jsonMatchers.ShouldHaveProperty(obj, "uf");

            // Verificar tipos dos campos
            var id = obj["id"]!.Value<int>();
            var nome = obj["nome"]!.Value<string>();
            var uf = obj["uf"]!.Value<string>();

            id.Should().BeGreaterThan(0);
            nome.Should().NotBeNullOrEmpty();
            uf.Should().NotBeNullOrEmpty();
            uf.Should().HaveLength(2); // UF deve ter 2 caracteres
        }
    }

    [Fact]
    public async Task Test_List_Municipios()
    {
        await AuthenticateAsProducerAsync();

        var estadoId = 11;
        var response = await GetAsync($"api/enderecos/municipios?estado_id={estadoId}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        if (array.Count > 0)
        {
            var firstItem = array[0];
            var obj = _jsonMatchers.ShouldBeObject(firstItem);

            // Validar estrutura do item
            obj.Properties().Should().HaveCount(6);
            _jsonMatchers.ShouldHaveProperty(obj, "id");
            _jsonMatchers.ShouldHaveProperty(obj, "nome");
            _jsonMatchers.ShouldHaveProperty(obj, "latitude");
            _jsonMatchers.ShouldHaveProperty(obj, "longitude");
            _jsonMatchers.ShouldHaveProperty(obj, "estado_id");
            _jsonMatchers.ShouldHaveProperty(obj, "capital");

            // Verificar tipos dos campos
            var id = obj["id"]!.Value<int>();
            var nome = obj["nome"]!.Value<string>();
            var latitude = obj["latitude"]!.Value<decimal>();
            var longitude = obj["longitude"]!.Value<decimal>();
            var estadoIdRetornado = obj["estado_id"]!.Value<int>();
            var capital = obj["capital"]!.Value<bool>();

            id.Should().BeGreaterThan(0);
            nome.Should().NotBeNullOrEmpty();
            estadoIdRetornado.Should().Be(estadoId);
            latitude.Should().BeInRange(-90, 90);
            longitude.Should().BeInRange(-180, 180);
        }
    }

    [Fact]
    public async Task Test_List_Urev()
    {
        await AuthenticateAsProducerAsync();

        var estadoId = 11;
        var response = await GetAsync($"api/enderecos/urevs?estado_id={estadoId}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        if (array.Count > 0)
        {
            var firstItem = array[0];
            var obj = _jsonMatchers.ShouldBeObject(firstItem);

            // Validar que tem 11 propriedades conforme especificado no teste Python
            obj.Properties().Should().HaveCount(11);
        }
    }

    [Fact]
    public async Task Test_Locais_Recebimento_By_Query()
    {
        await AuthenticateAsProducerAsync();

        var query = "Car";
        var response = await GetAsync($"api/enderecos/locais/recebimentos/full?query={query}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        // Verificar se os resultados contêm o termo de busca
        foreach (var item in array)
        {
            var obj = _jsonMatchers.ShouldBeObject(item);
            
            // Pelo menos um dos campos deve conter o termo de busca
            var hasQueryInAnyField = false;
            foreach (var property in obj.Properties())
            {
                var value = property.Value.Value<string>();
                if (value != null && value.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    hasQueryInAnyField = true;
                    break;
                }
            }
            
            hasQueryInAnyField.Should().BeTrue($"Item deve conter o termo '{query}' em algum campo");
        }
    }

    [Fact]
    public async Task Test_List_Estados_Without_Authentication()
    {
        ClearAuthentication();

        var response = await GetAsync("api/enderecos/estados");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_List_Municipios_Without_Authentication()
    {
        ClearAuthentication();

        var response = await GetAsync("api/enderecos/municipios?estado_id=11");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_List_Municipios_Without_Estado_Id()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("api/enderecos/municipios");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_List_Municipios_With_Invalid_Estado_Id()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("api/enderecos/municipios?estado_id=99999");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Test_Estados_Are_Ordered()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("api/enderecos/estados");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        if (array.Count > 1)
        {
            // Verificar se os estados estão ordenados por nome
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
    public async Task Test_Municipios_Are_Ordered()
    {
        await AuthenticateAsProducerAsync();

        var estadoId = 11;
        var response = await GetAsync($"api/enderecos/municipios?estado_id={estadoId}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        if (array.Count > 1)
        {
            // Verificar se os municípios estão ordenados por nome
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
    public async Task Test_Validate_Geographic_Coordinates()
    {
        await AuthenticateAsProducerAsync();

        var estadoId = 11;
        var response = await GetAsync($"api/enderecos/municipios?estado_id={estadoId}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        foreach (var item in array)
        {
            var obj = _jsonMatchers.ShouldBeObject(item);
            var latitude = obj["latitude"]!.Value<decimal>();
            var longitude = obj["longitude"]!.Value<decimal>();

            // Validar se as coordenadas estão dentro dos limites válidos
            latitude.Should().BeInRange(-90, 90, "Latitude deve estar entre -90 e 90 graus");
            longitude.Should().BeInRange(-180, 180, "Longitude deve estar entre -180 e 180 graus");

            // Para o Brasil, validar se as coordenadas estão aproximadamente corretas
            latitude.Should().BeInRange(-35, 5, "Latitude deve estar dentro dos limites do Brasil");
            longitude.Should().BeInRange(-75, -30, "Longitude deve estar dentro dos limites do Brasil");
        }
    }

    [Fact]
    public async Task Test_Estado_Ids_Are_Unique()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("api/enderecos/estados");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        var ids = new HashSet<int>();
        var ufs = new HashSet<string>();
        
        foreach (var item in array)
        {
            var obj = _jsonMatchers.ShouldBeObject(item);
            var id = obj["id"]!.Value<int>();
            var uf = obj["uf"]!.Value<string>()!;
            
            ids.Add(id).Should().BeTrue("IDs de estados devem ser únicos");
            ufs.Add(uf).Should().BeTrue("UFs devem ser únicas");
        }
    }

    [Fact]
    public async Task Test_Locais_Recebimento_Empty_Query()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("api/enderecos/locais/recebimentos/full?query=");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }
}