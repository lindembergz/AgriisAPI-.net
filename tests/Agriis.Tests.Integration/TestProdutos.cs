using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;
using System.Text;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Produtos
/// Migrado de test_produtos.py
/// </summary>
public class TestProdutos : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestProdutos(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_List_Categorias()
    {
        await AuthenticateAsProducerAsync(1);

        var response = await GetAsync("api/produtos/categorias");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        if (array.Count > 0)
        {
            var firstItem = array[0];
            var obj = _jsonMatchers.ShouldBeObject(firstItem);

            // Validar estrutura da categoria (2 propriedades)
            obj.Properties().Should().HaveCount(2);
            _jsonMatchers.ShouldHaveProperty(obj, "id");
            _jsonMatchers.ShouldHaveProperty(obj, "nome");
        }
    }

    [Fact]
    public async Task Test_Find_Produto_By_Id()
    {
        await AuthenticateAsProducerAsync(1);
        var produtoId = 179;

        var response = await GetAsync($"api/produtos/{produtoId}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura do produto (14 propriedades)
        obj.Properties().Should().HaveCount(14);
        _jsonMatchers.ShouldHaveProperty(obj, "id");
        _jsonMatchers.ShouldHaveProperty(obj, "categoria");
        _jsonMatchers.ShouldHaveProperty(obj, "classificacao");
        _jsonMatchers.ShouldHaveProperty(obj, "codigo_cda");
        _jsonMatchers.ShouldHaveProperty(obj, "codigo_erp");
        _jsonMatchers.ShouldHaveProperty(obj, "codigo_mapa");
        _jsonMatchers.ShouldHaveProperty(obj, "culturas");
        _jsonMatchers.ShouldHaveProperty(obj, "is_fabricante");
        _jsonMatchers.ShouldHaveProperty(obj, "nome");
        _jsonMatchers.ShouldHaveProperty(obj, "pms");
        _jsonMatchers.ShouldHaveProperty(obj, "produto_fabricante_id");
        _jsonMatchers.ShouldHaveProperty(obj, "produto_restrito");
        _jsonMatchers.ShouldHaveProperty(obj, "quantidade_minima");
        _jsonMatchers.ShouldHaveProperty(obj, "unidade");
    }

    [Fact]
    public async Task Test_Create_Produto_Complete_Flow()
    {
        await AuthenticateAsSupplierAsync(1);

        // Simular upload de arquivos usando multipart/form-data
        var multipartContent = new MultipartFormDataContent();

        // Adicionar dados do produto como JSON
        var produtoData = new
        {
            produto = new
            {
                nome = DataGenerator.GerarNomeProduto(),
                fornecedor_id = 1,
                categoria_id = 7,
                quantidade_minima = 10.5m,
                unidade = "QUILO"
            },
            culturas = new[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        var jsonContent = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(produtoData),
            Encoding.UTF8,
            "application/json"
        );
        multipartContent.Add(jsonContent, "jsonData");

        // Simular arquivos PDF (bula e rótulo)
        var pdfContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF header
        pdfContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        multipartContent.Add(pdfContent, "bula", "bula.pdf");

        var rotuloContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF header
        rotuloContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        multipartContent.Add(rotuloContent, "rotulo", "rotulo.pdf");

        // Criar produto
        var createResponse = await Client.PostAsync("api/produtos/", multipartContent);
        _jsonMatchers.ShouldHaveStatusCode(createResponse, HttpStatusCode.Created);

        // Extrair ID do produto do header Location
        var location = createResponse.Headers.Location?.ToString();
        location.Should().NotBeNullOrEmpty();
        var produtoId = ExtractIdFromLocation(location!);

        // Atualizar produto
        var updateData = produtoData;
        updateData.produto.GetType().GetProperty("categoria_id")?.SetValue(updateData.produto, 5);

        var updateContent = new MultipartFormDataContent();
        var updateJsonContent = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(updateData),
            Encoding.UTF8,
            "application/json"
        );
        updateContent.Add(updateJsonContent, "jsonData");

        var updateResponse = await Client.PutAsync($"api/produtos/{produtoId}/", updateContent);
        _jsonMatchers.ShouldHaveStatusCode(updateResponse, HttpStatusCode.OK);

        // Deletar documentos
        var deleteBulaResponse = await DeleteAsync($"api/produtos/{produtoId}/documentos/bula");
        _jsonMatchers.ShouldHaveStatusCode(deleteBulaResponse, HttpStatusCode.OK);

        var deleteRotuloResponse = await DeleteAsync($"api/produtos/{produtoId}/documentos/rotulo");
        _jsonMatchers.ShouldHaveStatusCode(deleteRotuloResponse, HttpStatusCode.OK);

        // Deletar produto
        var deleteResponse = await DeleteAsync($"api/produtos/{produtoId}/");
        _jsonMatchers.ShouldHaveStatusCode(deleteResponse, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_List_Produtos_By_Fornecedor()
    {
        await AuthenticateAsSupplierAsync(1);

        var requestData = new
        {
            page = 0,
            max_per_page = 10
        };

        var response = await PostAsync("api/produtos/all/", requestData);
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

            // Validar estrutura do item (3 propriedades)
            itemObj.Properties().Should().HaveCount(3);
            _jsonMatchers.ShouldHaveProperty(itemObj, "categoria_nome");
            _jsonMatchers.ShouldHaveProperty(itemObj, "id");
            _jsonMatchers.ShouldHaveProperty(itemObj, "nome");
        }
    }

    [Fact]
    public async Task Test_List_Produtos_By_Fornecedor_Invalid_Max_Per_Page()
    {
        await AuthenticateAsSupplierAsync(1);

        var requestData = new
        {
            page = 0,
            max_per_page = 0 // Valor inválido
        };

        var response = await PostAsync("api/produtos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Test_List_Produtos_By_Fornecedor_Invalid_Page()
    {
        await AuthenticateAsSupplierAsync(1);

        var requestData = new
        {
            page = -1, // Valor inválido
            max_per_page = 10
        };

        var response = await PostAsync("api/produtos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Test_List_Produtos_By_Propriedade()
    {
        await AuthenticateAsProducerAsync(1);
        var propriedadeId = 1;

        var queryParams = new Dictionary<string, string>
        {
            { "cultura_id", "135" },
            { "moeda", "REAL" },
            { "safra_id", "13" },
            { "fornecedor_id", "" } // null equivalent
        };

        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        var response = await GetAsync($"api/produtos/propriedade/{propriedadeId}/?{queryString}");

        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Produto_Validation_Errors()
    {
        await AuthenticateAsSupplierAsync(1);

        // Teste com dados inválidos
        var invalidData = new
        {
            produto = new
            {
                nome = "", // Nome vazio
                fornecedor_id = 0, // ID inválido
                categoria_id = -1, // ID negativo
                quantidade_minima = -10.5m, // Quantidade negativa
                unidade = "INVALID_UNIT" // Unidade inválida
            },
            culturas = new int[] { } // Array vazio
        };

        var multipartContent = new MultipartFormDataContent();
        var jsonContent = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(invalidData),
            Encoding.UTF8,
            "application/json"
        );
        multipartContent.Add(jsonContent, "jsonData");

        var response = await Client.PostAsync("api/produtos/", multipartContent);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Produto_Duplicate_Name()
    {
        await AuthenticateAsSupplierAsync(1);

        var produtoData = new
        {
            produto = new
            {
                nome = "Produto Teste Duplicado",
                fornecedor_id = 1,
                categoria_id = 7,
                quantidade_minima = 10.5m,
                unidade = "QUILO"
            },
            culturas = new[] { 1, 2, 3 }
        };

        var multipartContent1 = new MultipartFormDataContent();
        var jsonContent1 = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(produtoData),
            Encoding.UTF8,
            "application/json"
        );
        multipartContent1.Add(jsonContent1, "jsonData");

        // Primeira criação - deve funcionar
        var response1 = await Client.PostAsync("api/produtos/", multipartContent1);
        _jsonMatchers.ShouldHaveStatusCode(response1, HttpStatusCode.Created);

        // Segunda criação com mesmo nome - deve falhar
        var multipartContent2 = new MultipartFormDataContent();
        var jsonContent2 = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(produtoData),
            Encoding.UTF8,
            "application/json"
        );
        multipartContent2.Add(jsonContent2, "jsonData");

        var response2 = await Client.PostAsync("api/produtos/", multipartContent2);
        _jsonMatchers.ShouldHaveStatusCode(response2, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Produto_Without_Authentication()
    {
        ClearAuthentication();

        var response = await GetAsync("api/produtos/categorias");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Produto_Unauthorized_Access()
    {
        // Produtor tentando criar produto (apenas fornecedores podem)
        await AuthenticateAsProducerAsync(1);

        var produtoData = new
        {
            produto = new
            {
                nome = DataGenerator.GerarNomeProduto(),
                fornecedor_id = 1,
                categoria_id = 7,
                quantidade_minima = 10.5m,
                unidade = "QUILO"
            },
            culturas = new[] { 1, 2, 3 }
        };

        var multipartContent = new MultipartFormDataContent();
        var jsonContent = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(produtoData),
            Encoding.UTF8,
            "application/json"
        );
        multipartContent.Add(jsonContent, "jsonData");

        var response = await Client.PostAsync("api/produtos/", multipartContent);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_Produto_Culturas_Association()
    {
        await AuthenticateAsSupplierAsync(1);

        var produtoData = new
        {
            produto = new
            {
                nome = DataGenerator.GerarNomeProduto(),
                fornecedor_id = 1,
                categoria_id = 7,
                quantidade_minima = 10.5m,
                unidade = "QUILO"
            },
            culturas = new[] { 1, 2, 3, 4, 5 } // Múltiplas culturas
        };

        var multipartContent = new MultipartFormDataContent();
        var jsonContent = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(produtoData),
            Encoding.UTF8,
            "application/json"
        );
        multipartContent.Add(jsonContent, "jsonData");

        var response = await Client.PostAsync("api/produtos/", multipartContent);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        // Verificar se as culturas foram associadas corretamente
        var location = response.Headers.Location?.ToString();
        var produtoId = ExtractIdFromLocation(location!);

        var getResponse = await GetAsync($"api/produtos/{produtoId}");
        _jsonMatchers.ShouldHaveStatusCode(getResponse, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHaveProperty(obj, "culturas");
        var culturas = _jsonMatchers.ShouldBeArray(obj["culturas"]!);
        culturas.Count.Should().Be(5);
    }

    [Fact]
    public async Task Test_Produto_Dimensions_And_Weight()
    {
        await AuthenticateAsSupplierAsync(1);

        var produtoData = new
        {
            produto = new
            {
                nome = DataGenerator.GerarNomeProduto(),
                fornecedor_id = 1,
                categoria_id = 7,
                quantidade_minima = 10.5m,
                unidade = "QUILO",
                peso_nominal = 25.0m,
                altura = 30.0m,
                largura = 20.0m,
                comprimento = 40.0m,
                densidade = 0.8m
            },
            culturas = new[] { 1, 2, 3 }
        };

        var multipartContent = new MultipartFormDataContent();
        var jsonContent = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(produtoData),
            Encoding.UTF8,
            "application/json"
        );
        multipartContent.Add(jsonContent, "jsonData");

        var response = await Client.PostAsync("api/produtos/", multipartContent);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        // Verificar se as dimensões foram salvas corretamente
        var location = response.Headers.Location?.ToString();
        var produtoId = ExtractIdFromLocation(location!);

        var getResponse = await GetAsync($"api/produtos/{produtoId}");
        _jsonMatchers.ShouldHaveStatusCode(getResponse, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHaveProperty(obj, "peso_nominal");
        _jsonMatchers.ShouldHaveProperty(obj, "altura");
        _jsonMatchers.ShouldHaveProperty(obj, "largura");
        _jsonMatchers.ShouldHaveProperty(obj, "comprimento");
        _jsonMatchers.ShouldHaveProperty(obj, "densidade");
    }

    [Fact]
    public async Task Test_Produto_Fabricante_Hierarchy()
    {
        await AuthenticateAsSupplierAsync(1);

        // Criar produto fabricante
        var fabricanteData = new
        {
            produto = new
            {
                nome = "Produto Fabricante Original",
                fornecedor_id = 1,
                categoria_id = 7,
                quantidade_minima = 10.5m,
                unidade = "QUILO",
                is_fabricante = true
            },
            culturas = new[] { 1, 2, 3 }
        };

        var fabricanteContent = new MultipartFormDataContent();
        var fabricanteJsonContent = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(fabricanteData),
            Encoding.UTF8,
            "application/json"
        );
        fabricanteContent.Add(fabricanteJsonContent, "jsonData");

        var fabricanteResponse = await Client.PostAsync("api/produtos/", fabricanteContent);
        _jsonMatchers.ShouldHaveStatusCode(fabricanteResponse, HttpStatusCode.Created);

        var fabricanteLocation = fabricanteResponse.Headers.Location?.ToString();
        var fabricanteId = ExtractIdFromLocation(fabricanteLocation!);

        // Criar produto revendedor baseado no fabricante
        var revendedorData = new
        {
            produto = new
            {
                nome = "Produto Revendedor",
                fornecedor_id = 1,
                categoria_id = 7,
                quantidade_minima = 5.0m,
                unidade = "QUILO",
                is_fabricante = false,
                produto_fabricante_id = fabricanteId
            },
            culturas = new[] { 1, 2, 3 }
        };

        var revendedorContent = new MultipartFormDataContent();
        var revendedorJsonContent = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(revendedorData),
            Encoding.UTF8,
            "application/json"
        );
        revendedorContent.Add(revendedorJsonContent, "jsonData");

        var revendedorResponse = await Client.PostAsync("api/produtos/", revendedorContent);
        _jsonMatchers.ShouldHaveStatusCode(revendedorResponse, HttpStatusCode.Created);

        // Verificar hierarquia
        var revendedorLocation = revendedorResponse.Headers.Location?.ToString();
        var revendedorId = ExtractIdFromLocation(revendedorLocation!);

        var getResponse = await GetAsync($"api/produtos/{revendedorId}");
        _jsonMatchers.ShouldHaveStatusCode(getResponse, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHaveProperty(obj, "produto_fabricante_id");
        _jsonMatchers.ShouldHavePropertyWithValue(obj, "produto_fabricante_id", fabricanteId);
        _jsonMatchers.ShouldHavePropertyWithValue(obj, "is_fabricante", false);
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