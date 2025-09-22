using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;
using Agriis.Tests.Shared.Authentication;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Safras
/// Migrado de test_safras.py
/// </summary>
public class TestSafras : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestSafras(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_List_Safras()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("v1/safras/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        if (array.Count > 0)
        {
            var firstItem = array[0];
            var obj = _jsonMatchers.ShouldBeObject(firstItem);

            // Validar estrutura básica da safra
            _jsonMatchers.ShouldHaveProperty(obj, "id");
            _jsonMatchers.ShouldHaveProperty(obj, "descricao");
            _jsonMatchers.ShouldHaveProperty(obj, "data_plantio_inicial");
            _jsonMatchers.ShouldHaveProperty(obj, "data_plantio_final");
            _jsonMatchers.ShouldHaveProperty(obj, "ano_colheita");
            _jsonMatchers.ShouldHaveProperty(obj, "tipo");
        }
    }

    [Fact]
    public async Task Test_Find_Atual()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("v1/safras/atual");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura da safra atual
        _jsonMatchers.ShouldHaveProperty(obj, "id");
        _jsonMatchers.ShouldHaveProperty(obj, "descricao");
        _jsonMatchers.ShouldHaveProperty(obj, "data_plantio_inicial");
        _jsonMatchers.ShouldHaveProperty(obj, "data_plantio_final");
        _jsonMatchers.ShouldHaveProperty(obj, "ano_colheita");
        _jsonMatchers.ShouldHaveProperty(obj, "tipo");

        // Verificar se é uma safra do tipo S1 (safra atual)
        var tipo = obj["tipo"]!.Value<string>();
        tipo.Should().Be("S1");
    }

    [Fact]
    public async Task Test_List_Safras_Without_Authentication()
    {
        ClearAuthentication();

        var response = await GetAsync("v1/safras/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Find_Atual_Without_Authentication()
    {
        ClearAuthentication();

        var response = await GetAsync("v1/safras/atual");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Find_Safra_By_Id()
    {
        await AuthenticateAsProducerAsync();

        var safraId = 1;
        var response = await GetAsync($"v1/safras/{safraId}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura da safra específica
        _jsonMatchers.ShouldHaveProperty(obj, "id");
        _jsonMatchers.ShouldHaveProperty(obj, "descricao");
        _jsonMatchers.ShouldHaveProperty(obj, "data_plantio_inicial");
        _jsonMatchers.ShouldHaveProperty(obj, "data_plantio_final");
        _jsonMatchers.ShouldHaveProperty(obj, "ano_colheita");
        _jsonMatchers.ShouldHaveProperty(obj, "tipo");

        // Verificar se o ID retornado corresponde ao solicitado
        var id = obj["id"]!.Value<int>();
        id.Should().Be(safraId);
    }

    [Fact]
    public async Task Test_Find_Safra_By_Id_Not_Found()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("v1/safras/99999");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Test_List_Safras_By_Ano_Colheita()
    {
        await AuthenticateAsProducerAsync();

        var anoColheita = DateTime.Now.Year;
        var response = await GetAsync($"v1/safras?ano_colheita={anoColheita}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        // Verificar se todas as safras retornadas são do ano solicitado
        foreach (var item in array)
        {
            var obj = _jsonMatchers.ShouldBeObject(item);
            var ano = obj["ano_colheita"]!.Value<int>();
            ano.Should().Be(anoColheita);
        }
    }

    [Fact]
    public async Task Test_List_Safras_By_Tipo()
    {
        await AuthenticateAsProducerAsync();

        var tipo = "S1";
        var response = await GetAsync($"v1/safras?tipo={tipo}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        // Verificar se todas as safras retornadas são do tipo solicitado
        foreach (var item in array)
        {
            var obj = _jsonMatchers.ShouldBeObject(item);
            var tipoSafra = obj["tipo"]!.Value<string>();
            tipoSafra.Should().Be(tipo);
        }
    }

    [Fact]
    public async Task Test_Validate_Safra_Dates()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("v1/safras/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        // Verificar se as datas de plantio são válidas
        foreach (var item in array)
        {
            var obj = _jsonMatchers.ShouldBeObject(item);
            
            var dataInicialStr = obj["data_plantio_inicial"]!.Value<string>();
            var dataFinalStr = obj["data_plantio_final"]!.Value<string>();

            DateTime.TryParse(dataInicialStr, out var dataInicial).Should().BeTrue();
            DateTime.TryParse(dataFinalStr, out var dataFinal).Should().BeTrue();

            // Data inicial deve ser anterior à data final
            dataInicial.Should().BeBefore(dataFinal);
        }
    }

    [Fact]
    public async Task Test_Safra_Atual_Is_Current_Period()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("v1/safras/atual");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        var dataInicialStr = obj["data_plantio_inicial"]!.Value<string>();
        var dataFinalStr = obj["data_plantio_final"]!.Value<string>();

        DateTime.TryParse(dataInicialStr, out var dataInicial).Should().BeTrue();
        DateTime.TryParse(dataFinalStr, out var dataFinal).Should().BeTrue();

        var hoje = DateTime.Now.Date;

        // A safra atual deve estar dentro do período de plantio ou ser a mais próxima
        // Esta validação pode variar dependendo da lógica de negócio específica
        (hoje >= dataInicial.Date && hoje <= dataFinal.Date || 
         dataInicial.Date > hoje).Should().BeTrue();
    }

    [Fact]
    public async Task Test_List_Safras_Ordered_By_Date()
    {
        await AuthenticateAsProducerAsync();

        var response = await GetAsync("v1/safras/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        if (array.Count > 1)
        {
            // Verificar se as safras estão ordenadas por data de plantio inicial
            DateTime? dataAnterior = null;
            
            foreach (var item in array)
            {
                var obj = _jsonMatchers.ShouldBeObject(item);
                var dataInicialStr = obj["data_plantio_inicial"]!.Value<string>();
                DateTime.TryParse(dataInicialStr, out var dataInicial).Should().BeTrue();

                if (dataAnterior.HasValue)
                {
                    dataInicial.Should().BeOnOrAfter(dataAnterior.Value);
                }
                
                dataAnterior = dataInicial;
            }
        }
    }
}