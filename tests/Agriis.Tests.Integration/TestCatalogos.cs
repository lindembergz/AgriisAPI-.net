using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Catálogos
/// Migrado de test_catalogos.py
/// </summary>
public class TestCatalogos : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestCatalogos(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_Find_Catalogo_By_Id()
    {
        await AuthenticateAsSupplierAsync(1);
        var catalogoId = 1;

        var response = await GetAsync($"api/Catalogos/{catalogoId}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura do catálogo (8 propriedades)
        obj.Properties().Should().HaveCount(8);
        _jsonMatchers.ShouldHaveProperty(obj, "id");
        _jsonMatchers.ShouldHaveProperty(obj, "categoria");
        _jsonMatchers.ShouldHaveProperty(obj, "cultura");
        _jsonMatchers.ShouldHaveProperty(obj, "data_criacao");
        _jsonMatchers.ShouldHaveProperty(obj, "descricao");
        _jsonMatchers.ShouldHaveProperty(obj, "ponto_distribuicao");
        _jsonMatchers.ShouldHaveProperty(obj, "moeda");
        _jsonMatchers.ShouldHaveProperty(obj, "safra");
    }

    [Fact]
    public async Task Test_Find_Item_Ou_Catalogo_Item()
    {
        await AuthenticateAsProducerAsync(1);
        var catalogoItemId = 1;
        var propriedadeId = 1;

        var queryParams = new Dictionary<string, string>
        {
            { "propriedade_id", propriedadeId.ToString() }
        };

        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        var response = await GetAsync($"api/Catalogos/item/{catalogoItemId}/?{queryString}");

        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_List_Catalogos_By_Fornecedor()
    {
        await AuthenticateAsSupplierAsync(1);

        var requestData = new
        {
            page = 0,
            max_per_page = 10,
            safra = "2019/2020 s3",
            cultura = "algo",
            moeda = "rea",
            descricao = "cat",
            ponto_distribuicao = "mir"
        };

        var response = await PostAsync("api/Catalogos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura de paginação (4 propriedades)
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

            // Validar estrutura do item (8 propriedades)
            itemObj.Properties().Should().HaveCount(8);
            _jsonMatchers.ShouldHaveProperty(itemObj, "categoria_nome");
            _jsonMatchers.ShouldHaveProperty(itemObj, "cultura_nome");
            _jsonMatchers.ShouldHaveProperty(itemObj, "descricao");
            _jsonMatchers.ShouldHaveProperty(itemObj, "id");
            _jsonMatchers.ShouldHaveProperty(itemObj, "moeda");
            _jsonMatchers.ShouldHaveProperty(itemObj, "pd_descricao");
            _jsonMatchers.ShouldHaveProperty(itemObj, "pd_id");
            _jsonMatchers.ShouldHaveProperty(itemObj, "safra");
        }
    }

    [Fact]
    public async Task Test_List_Catalogos_By_Fornecedor_Invalid_Max_Per_Page()
    {
        await AuthenticateAsSupplierAsync(1);

        var requestData = new
        {
            page = 0,
            max_per_page = 0, // Valor inválido
            safra = "2019/2020 s3",
            cultura = "algo",
            moeda = "rea",
            descricao = "cat",
            ponto_distribuicao = "mir"
        };

        var response = await PostAsync("api/Catalogos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Test_List_Catalogos_By_Fornecedor_Invalid_Page()
    {
        await AuthenticateAsSupplierAsync(1);

        var requestData = new
        {
            page = -1, // Valor inválido
            max_per_page = 10,
            safra = "2019/2020 s3",
            cultura = "algo",
            moeda = "rea",
            descricao = "cat",
            ponto_distribuicao = "mir"
        };

        var response = await PostAsync("api/Catalogos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Test_Create_Catalogo()
    {
        await AuthenticateAsSupplierAsync(1);

        var requestData = new
        {
            descricao = DataGenerator.GerarNome(),
            safra_id = 13,
            ponto_distribuicao_id = 15,
            cultura_id = 135,
            categoria_id = 16,
            moeda = "DOLAR"
        };

        var response = await PostAsync("api/Catalogos/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        // Extrair ID do catálogo do header Location
        var location = response.Headers.Location?.ToString();
        location.Should().NotBeNullOrEmpty();
        var catalogoId = ExtractIdFromLocation(location!);

        // Verificar se o catálogo foi criado corretamente
        var getResponse = await GetAsync($"api/Catalogos/{catalogoId}");
        _jsonMatchers.ShouldHaveStatusCode(getResponse, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHavePropertyWithValue(obj, "moeda", "DOLAR");
        _jsonMatchers.ShouldHaveProperty(obj, "descricao");
    }

    [Fact]
    public async Task Test_List_Items_By_Catalogo()
    {
        await AuthenticateAsSupplierAsync(1);
        var catalogoId = 1;

        var requestData = new
        {
            page = 0,
            max_per_page = 10,
            nome_produto = "fag",
            categoria_nome = "herb"
        };

        var response = await PostAsync($"api/Catalogos/{catalogoId}/items/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura de paginação (4 propriedades)
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

            // Validar estrutura do item (5 propriedades)
            itemObj.Properties().Should().HaveCount(5);
            _jsonMatchers.ShouldHaveProperty(itemObj, "categoria_nome");
            _jsonMatchers.ShouldHaveProperty(itemObj, "body");
            _jsonMatchers.ShouldHaveProperty(itemObj, "produto_id");
            _jsonMatchers.ShouldHaveProperty(itemObj, "produto_nome");
            _jsonMatchers.ShouldHaveProperty(itemObj, "id");
        }
    }

    [Fact]
    public async Task Test_List_Items_By_Catalogo_Invalid_Max_Per_Page()
    {
        await AuthenticateAsSupplierAsync(1);
        var catalogoId = 1;

        var requestData = new
        {
            page = 0,
            max_per_page = 0, // Valor inválido
            nome_produto = "fag",
            categoria_nome = "herb"
        };

        var response = await PostAsync($"api/Catalogos/{catalogoId}/items/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Test_Create_Catalogo_Item_Complete_Flow()
    {
        await AuthenticateAsSupplierAsync(1);
        var catalogoId = 120;
        var catalogoItemId = 622;

        var requestData = new
        {
            produto_id = 173,
            catalogo_id = catalogoId,
            data_pagamento = "26/01/2021",
            precos = new[]
            {
                new
                {
                    preco = 638.09m,
                    estado_id = 52,
                    data_entrega_final = "20/12/2020",
                    data_pagamento_max = "30/09/2021",
                    data_pagamento_min = "30/10/2020",
                    data_exibicao_inicial = "01/01/2017",
                    data_exibicao_final = "12/12/2021",
                    data_pagamento_base = "30/08/2021",
                    data_entrega_inicial = "01/09/2020",
                    frete_gratis = true
                }
            },
            tx_juros = 1m,
            tx_antecipacao = 0.9m,
            catalogo_item_id = catalogoItemId
        };

        // Atualizar item do catálogo
        var updateResponse = await PutAsync($"api/Catalogos/{catalogoId}/items/{catalogoItemId}/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(updateResponse, HttpStatusCode.OK);

        // Deletar item do catálogo
        var deleteResponse = await DeleteAsync($"api/Catalogos/{catalogoId}/items/{catalogoItemId}/");
        _jsonMatchers.ShouldHaveStatusCode(deleteResponse, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Create_Item_By_Catalogo()
    {
        await AuthenticateAsSupplierAsync(1);
        var catalogoId = 92;

        var requestData = new
        {
            produto_id = 265,
            catalogo_id = catalogoId,
            n_max = 0,
            n_min = 0,
            tx_juros = 0m,
            tx_antecipacao = 0m,
            precos = new[]
            {
                new
                {
                    preco = 10m,
                    estado_id = 12,
                    data_pagamento = "08/07/2020",
                    data_entrega_inicial = "21/07/2020",
                    data_entrega_final = "27/07/2020"
                }
            }
        };

        var response = await PostAsync($"api/Catalogos/{catalogoId}/items/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        // Verificar se o item foi criado
        var location = response.Headers.Location?.ToString();
        location.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Test_Catalogo_Validation_Errors()
    {
        await AuthenticateAsSupplierAsync(1);

        // Teste com dados inválidos
        var invalidData = new
        {
            descricao = "", // Descrição vazia
            safra_id = 0, // ID inválido
            ponto_distribuicao_id = -1, // ID negativo
            cultura_id = 0, // ID inválido
            categoria_id = -1, // ID negativo
            moeda = "INVALID_CURRENCY" // Moeda inválida
        };

        var response = await PostAsync("api/Catalogos/", invalidData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Catalogo_Item_Validation_Errors()
    {
        await AuthenticateAsSupplierAsync(1);
        var catalogoId = 1;

        // Teste com dados inválidos
        var invalidData = new
        {
            produto_id = 0, // ID inválido
            catalogo_id = catalogoId,
            n_max = -1, // Valor negativo
            n_min = -1, // Valor negativo
            tx_juros = -0.5m, // Taxa negativa
            tx_antecipacao = 1.5m, // Taxa maior que 100%
            precos = new[]
            {
                new
                {
                    preco = -10m, // Preço negativo
                    estado_id = 0, // ID inválido
                    data_pagamento = "invalid-date", // Data inválida
                    data_entrega_inicial = "invalid-date", // Data inválida
                    data_entrega_final = "invalid-date" // Data inválida
                }
            }
        };

        var response = await PostAsync($"api/Catalogos/{catalogoId}/items/", invalidData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Catalogo_Without_Authentication()
    {
        ClearAuthentication();

        var requestData = new
        {
            page = 0,
            max_per_page = 10
        };

        var response = await PostAsync("api/Catalogos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Catalogo_Unauthorized_Access()
    {
        // Produtor tentando criar catálogo (apenas fornecedores podem)
        await AuthenticateAsProducerAsync(1);

        var requestData = new
        {
            descricao = DataGenerator.GerarNome(),
            safra_id = 13,
            ponto_distribuicao_id = 15,
            cultura_id = 135,
            categoria_id = 16,
            moeda = "REAL"
        };

        var response = await PostAsync("api/Catalogos/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_Catalogo_Multiple_Currencies()
    {
        await AuthenticateAsSupplierAsync(1);

        // Criar catálogo em REAL
        var catalogoReal = new
        {
            descricao = "Catálogo em Real",
            safra_id = 13,
            ponto_distribuicao_id = 15,
            cultura_id = 135,
            categoria_id = 16,
            moeda = "REAL"
        };

        var responseReal = await PostAsync("api/Catalogos/", catalogoReal);
        _jsonMatchers.ShouldHaveStatusCode(responseReal, HttpStatusCode.Created);

        // Criar catálogo em DÓLAR
        var catalogoDolar = new
        {
            descricao = "Catálogo em Dólar",
            safra_id = 13,
            ponto_distribuicao_id = 15,
            cultura_id = 135,
            categoria_id = 16,
            moeda = "DOLAR"
        };

        var responseDolar = await PostAsync("api/Catalogos/", catalogoDolar);
        _jsonMatchers.ShouldHaveStatusCode(responseDolar, HttpStatusCode.Created);

        // Verificar se ambos foram criados com moedas corretas
        var locationReal = responseReal.Headers.Location?.ToString();
        var catalogoRealId = ExtractIdFromLocation(locationReal!);

        var locationDolar = responseDolar.Headers.Location?.ToString();
        var catalogoDolarId = ExtractIdFromLocation(locationDolar!);

        var getRealResponse = await GetAsync($"api/Catalogos/{catalogoRealId}");
        var jsonReal = await _jsonMatchers.ShouldHaveValidJsonAsync(getRealResponse);
        _jsonMatchers.ShouldHavePropertyWithValue(jsonReal, "moeda", "REAL");

        var getDolarResponse = await GetAsync($"api/Catalogos/{catalogoDolarId}");
        var jsonDolar = await _jsonMatchers.ShouldHaveValidJsonAsync(getDolarResponse);
        _jsonMatchers.ShouldHavePropertyWithValue(jsonDolar, "moeda", "DOLAR");
    }

    [Fact]
    public async Task Test_Catalogo_Item_Price_Ranges()
    {
        await AuthenticateAsSupplierAsync(1);
        var catalogoId = 1;

        var requestData = new
        {
            produto_id = 1,
            catalogo_id = catalogoId,
            n_max = 1000,
            n_min = 10,
            tx_juros = 0.05m,
            tx_antecipacao = 0.02m,
            precos = new[]
            {
                new
                {
                    preco = 100m,
                    estado_id = 12,
                    data_pagamento = "08/07/2020",
                    data_entrega_inicial = "21/07/2020",
                    data_entrega_final = "27/07/2020"
                },
                new
                {
                    preco = 95m, // Preço diferente para outro estado
                    estado_id = 13,
                    data_pagamento = "08/07/2020",
                    data_entrega_inicial = "21/07/2020",
                    data_entrega_final = "27/07/2020"
                }
            }
        };

        var response = await PostAsync($"api/Ctalogos/{catalogoId}/items/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        // Verificar se os preços foram salvos corretamente
        var location = response.Headers.Location?.ToString();
        var itemId = ExtractIdFromLocation(location!);

        var getResponse = await GetAsync($"api/Catalogos/item/{itemId}/");
        _jsonMatchers.ShouldHaveStatusCode(getResponse, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHaveProperty(obj, "precos");
        var precos = _jsonMatchers.ShouldBeArray(obj["precos"]!);
        precos.Count.Should().Be(2);
    }

    [Fact]
    public async Task Test_Catalogo_Search_Filters()
    {
        await AuthenticateAsSupplierAsync(1);

        // Teste com filtros específicos
        var requestData = new
        {
            page = 0,
            max_per_page = 10,
            safra = "2020/2021",
            cultura = "soja",
            moeda = "REAL",
            descricao = "herbicida",
            ponto_distribuicao = "matriz"
        };

        var response = await PostAsync("api/Catalogos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHavePaginationStructure(obj);

        // Teste sem filtros
        var requestDataEmpty = new
        {
            page = 0,
            max_per_page = 10
        };

        var responseEmpty = await PostAsync("api/Catalogos/all/", requestDataEmpty);
        _jsonMatchers.ShouldHaveStatusCode(responseEmpty, HttpStatusCode.OK);
    }

    /// <summary>
    /// Extrai o ID da entidade do header Location
    /// </summary>
    private static int ExtractIdFromLocation(string location)
    {
        var segments = location.Split('/');
        var idSegment = segments[^2]; // Penúltimo segmento (último é vazio devido ao '/')
        return int.Parse(idSegment);
    }
}