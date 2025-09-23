using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Produtores
/// Migrado de test_produtores.py
/// </summary>
public class TestProdutores : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestProdutores(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_Create_Pessoa_Fisica()
    {
        // Teste normal do cadastro de um produtor do tipo pessoa física
        await AuthenticateAsProducerAsync();

        var requestData = new
        {
            produtor = new
            {
                nome = DataGenerator.GerarNome(),
                cpf = DataGenerator.GerarCpfFormatado()
            },
            culturas = new[]
            {
                new { id = 1, area = 500 },
                new { id = 2, area = 300 },
                new { id = 3, area = 430 }
            }
        };

        // Primeira tentativa - deve criar com sucesso
        var response = await PostAsync("api/produtores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        // Segunda tentativa com os mesmos dados - deve falhar
        var duplicateResponse = await PostAsync("api/produtores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(duplicateResponse, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Create_Pessoa_Juridica()
    {
        // Teste normal do cadastro de um produtor do tipo pessoa jurídica
        await AuthenticateAsProducerAsync();

        var requestData = new
        {
            nome = DataGenerator.GerarNomeEmpresa(),
            cnpj = DataGenerator.GerarCnpjFormatado(),
            inscricao_estadual = DataGenerator.GerarInscricaoEstadual()
        };

        // Primeira tentativa - deve criar com sucesso
        var response = await PostAsync("api/produtores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        // Segunda tentativa com os mesmos dados - deve falhar
        var duplicateResponse = await PostAsync("api/produtores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(duplicateResponse, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Create_Pessoa_Juridica_Sem_Inscricao_Estadual()
    {
        // Teste do cadastro de um produtor do tipo pessoa jurídica sem informar a inscrição estadual
        await AuthenticateAsProducerAsync();

        var requestData = new
        {
            nome = DataGenerator.GerarNomeEmpresa(),
            cnpj = DataGenerator.GerarCnpjFormatado()
            // inscricao_estadual omitida intencionalmente
        };

        var response = await PostAsync("api/produtores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Find_Propriedade_By_Id()
    {
        await AuthenticateAsProducerAsync();
        var produtorId = 1;
        var propriedadeId = 3;

        var response = await GetAsync($"api/produtores/{produtorId}/propriedades/{propriedadeId}/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura da resposta (equivalente ao has_members_size e exists do Python)
        obj.Properties().Should().HaveCount(7);
        _jsonMatchers.ShouldHaveProperty(obj, "id");
        _jsonMatchers.ShouldHaveProperty(obj, "area");
        _jsonMatchers.ShouldHaveProperty(obj, "culturas");
        _jsonMatchers.ShouldHaveProperty(obj, "endereco_id");
        _jsonMatchers.ShouldHaveProperty(obj, "nirf");
        _jsonMatchers.ShouldHaveProperty(obj, "nome");
        _jsonMatchers.ShouldHaveProperty(obj, "produtor_id");
    }

    [Fact]
    public async Task Test_List_Fornecedores_By_Propriedade()
    {
        await AuthenticateAsProducerAsync();
        var produtorId = 1;
        var propriedadeId = 3;

        var response = await GetAsync($"api/produtores/{produtorId}/propriedades/{propriedadeId}/fornecedores/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);
        
        if (array.Count > 0)
        {
            var firstItem = array[0];
            var obj = _jsonMatchers.ShouldBeObject(firstItem);

            // Validar estrutura do primeiro item
            obj.Properties().Should().HaveCount(7);
            _jsonMatchers.ShouldHaveProperty(obj, "id");
            _jsonMatchers.ShouldHaveProperty(obj, "cnpj");
            _jsonMatchers.ShouldHaveProperty(obj, "estado_uf");
            _jsonMatchers.ShouldHaveProperty(obj, "municipio_nome");
            _jsonMatchers.ShouldHaveProperty(obj, "nome");
            _jsonMatchers.ShouldHaveProperty(obj, "url_logo_small");
            _jsonMatchers.ShouldHaveProperty(obj, "url_site");
        }
    }

    [Fact]
    public async Task Test_List_Catalogos_By_Fornecedor()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 10,
            estado_id = 11,
            nome = "s"
        };

        var response = await PostAsync("api/produtores/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura de paginação
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
            itemObj.Properties().Should().HaveCount(6);
            _jsonMatchers.ShouldHaveProperty(itemObj, "area_plantio");
            _jsonMatchers.ShouldHaveProperty(itemObj, "cnpj");
            _jsonMatchers.ShouldHaveProperty(itemObj, "id");
            _jsonMatchers.ShouldHaveProperty(itemObj, "nome");
            _jsonMatchers.ShouldHaveProperty(itemObj, "propriedades");
            _jsonMatchers.ShouldHaveProperty(itemObj, "segmentacao");
        }
    }

    [Fact]
    public async Task Test_List_Catalogos_By_Fornecedor_Invalid_Max_Per_Page()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 0, // Valor inválido
            estado_id = 11,
            nome = "s"
        };

        var response = await PostAsync("api/produtores/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Test_List_Catalogos_By_Fornecedor_Invalid_Page()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            page = -1, // Valor inválido
            max_per_page = 10,
            estado_id = 11,
            nome = "s"
        };

        var response = await PostAsync("api/produtores/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Test_Validate_Cpf_Format()
    {
        await AuthenticateAsProducerAsync();

        // Teste com CPF inválido
        var requestData = new
        {
            produtor = new
            {
                nome = DataGenerator.GerarNome(),
                cpf = "123.456.789-00" // CPF inválido
            },
            culturas = new[]
            {
                new { id = 1, area = 500 }
            }
        };

        var response = await PostAsync("api/produtores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Validate_Cnpj_Format()
    {
        await AuthenticateAsProducerAsync();

        // Teste com CNPJ inválido
        var requestData = new
        {
            nome = DataGenerator.GerarNomeEmpresa(),
            cnpj = "12.345.678/0001-00", // CNPJ inválido
            inscricao_estadual = DataGenerator.GerarInscricaoEstadual()
        };

        var response = await PostAsync("api/produtores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Create_Produtor_Without_Authentication()
    {
        // Teste sem autenticação
        ClearAuthentication();

        var requestData = new
        {
            produtor = new
            {
                nome = DataGenerator.GerarNome(),
                cpf = DataGenerator.GerarCpfFormatado()
            },
            culturas = new[]
            {
                new { id = 1, area = 500 }
            }
        };

        var response = await PostAsync("api/produtores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Create_Produtor_With_Invalid_Cultura()
    {
        await AuthenticateAsProducerAsync();

        var requestData = new
        {
            produtor = new
            {
                nome = DataGenerator.GerarNome(),
                cpf = DataGenerator.GerarCpfFormatado()
            },
            culturas = new[]
            {
                new { id = 99999, area = 500 } // ID de cultura inexistente
            }
        };

        var response = await PostAsync("api/produtores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Create_Produtor_With_Zero_Area()
    {
        await AuthenticateAsProducerAsync();

        var requestData = new
        {
            produtor = new
            {
                nome = DataGenerator.GerarNome(),
                cpf = DataGenerator.GerarCpfFormatado()
            },
            culturas = new[]
            {
                new { id = 1, area = 0 } // Área zero
            }
        };

        var response = await PostAsync("api/produtores/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }
}