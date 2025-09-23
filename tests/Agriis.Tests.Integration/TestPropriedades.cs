using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;
using Agriis.Tests.Shared.Authentication;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Propriedades
/// Migrado de test_propriedades.py
/// </summary>
public class TestPropriedades : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestPropriedades(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_Create()
    {
        // Teste do cadastro de uma propriedade do tipo pessoa jurídica sem informar a inscrição estadual
        await AuthenticateAsProducerAsync();

        var municipios = new[] { 1100015, 1100023, 1100031, 1100049, 1100056, 1100064, 1100072, 1100080, 1100098, 1100106, 1100114 };
        var municipioId = municipios[Random.Shared.Next(0, municipios.Length)];

        var requestData = new
        {
            nome = DataGenerator.GerarNome(),
            nirf = DataGenerator.GerarNirf(),
            area = 500,
            produtor = new { id = TestUserAuth.ProdutorMobileSandbox.ProdutorId },
            endereco = new
            {
                municipio = new { id = municipioId },
                location = new[] { -8.31894899, -55.09931758 }
            },
            culturas = new[]
            {
                new { id = 17, area = 5 },
                new { id = 18, area = 5 },
                new { id = 19, area = 5 },
                new { id = 20, area = 5 },
                new { id = 21, area = 5 },
                new { id = 22, area = 5 },
                new { id = 23, area = 5 },
                new { id = 24, area = 5 }
            }
        };

        var response = await PostAsync("api/propriedades/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);
    }

    [Fact]
    public async Task Test_Delete()
    {
        // Teste de exclusão de uma propriedade
        await AuthenticateAsProducerAsync();

        var propriedadeId = TestUserAuth.ProdutorMobileSandbox.PropriedadeId;
        var response = await DeleteAsync($"api/propriedades/{propriedadeId}/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Create_Without_Authentication()
    {
        // Teste sem autenticação
        ClearAuthentication();

        var requestData = new
        {
            nome = DataGenerator.GerarNome(),
            nirf = DataGenerator.GerarNirf(),
            area = 500,
            produtor = new { id = 1 },
            endereco = new
            {
                municipio = new { id = 1100015 },
                location = new[] { -8.31894899, -55.09931758 }
            },
            culturas = new[]
            {
                new { id = 17, area = 5 }
            }
        };

        var response = await PostAsync("api/propriedades/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Create_With_Invalid_Area()
    {
        await AuthenticateAsProducerAsync();

        var requestData = new
        {
            nome = DataGenerator.GerarNome(),
            nirf = DataGenerator.GerarNirf(),
            area = -100, // Área inválida
            produtor = new { id = TestUserAuth.ProdutorMobileSandbox.ProdutorId },
            endereco = new
            {
                municipio = new { id = 1100015 },
                location = new[] { -8.31894899, -55.09931758 }
            },
            culturas = new[]
            {
                new { id = 17, area = 5 }
            }
        };

        var response = await PostAsync("api/propriedades/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Create_With_Invalid_Nirf()
    {
        await AuthenticateAsProducerAsync();

        var requestData = new
        {
            nome = DataGenerator.GerarNome(),
            nirf = "123", // NIRF inválido
            area = 500,
            produtor = new { id = TestUserAuth.ProdutorMobileSandbox.ProdutorId },
            endereco = new
            {
                municipio = new { id = 1100015 },
                location = new[] { -8.31894899, -55.09931758 }
            },
            culturas = new[]
            {
                new { id = 17, area = 5 }
            }
        };

        var response = await PostAsync("api/propriedades/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Create_With_Invalid_Municipio()
    {
        await AuthenticateAsProducerAsync();

        var requestData = new
        {
            nome = DataGenerator.GerarNome(),
            nirf = DataGenerator.GerarNirf(),
            area = 500,
            produtor = new { id = TestUserAuth.ProdutorMobileSandbox.ProdutorId },
            endereco = new
            {
                municipio = new { id = 99999999 }, // Município inexistente
                location = new[] { -8.31894899, -55.09931758 }
            },
            culturas = new[]
            {
                new { id = 17, area = 5 }
            }
        };

        var response = await PostAsync("api/propriedades/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Delete_Nonexistent_Property()
    {
        await AuthenticateAsProducerAsync();

        var response = await DeleteAsync("api/propriedades/99999/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.NotFound);
    }
}