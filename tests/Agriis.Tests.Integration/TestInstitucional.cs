using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;
using Agriis.Tests.Shared.Authentication;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo Institucional
/// Migrado de test_institucional.py
/// </summary>
public class TestInstitucional : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestInstitucional(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_Ifarmer_Audit_Cadastrese()
    {
        // Teste do endpoint de auditoria de cadastro (sem autenticação necessária)
        var requestData = new
        {
            page = 0,
            max_per_page = 10,
            descricao = "mirante",
            endereco = "mirante"
        };

        var response = await PostAsync("api/institucional/ifarmer/audit_cadastrese", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura básica da resposta
        _jsonMatchers.ShouldHaveProperty(obj, "success");
        _jsonMatchers.ShouldHaveProperty(obj, "message");

        var success = obj["success"]!.Value<bool>();
        success.Should().BeTrue();
    }

    [Fact]
    public async Task Test_Get_Configuracao_Sistema()
    {
        // Teste para obter configurações do sistema
        var response = await GetAsync("api/institucional/configuracoes");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar se retorna configurações básicas
        _jsonMatchers.ShouldHaveProperty(obj, "versao_sistema");
        _jsonMatchers.ShouldHaveProperty(obj, "nome_aplicacao");
        _jsonMatchers.ShouldHaveProperty(obj, "configuracoes_gerais");
    }

    [Fact]
    public async Task Test_Get_Termos_Uso()
    {
        // Teste para obter termos de uso
        var response = await GetAsync("api/institucional/termos-uso");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura dos termos de uso
        _jsonMatchers.ShouldHaveProperty(obj, "conteudo");
        _jsonMatchers.ShouldHaveProperty(obj, "versao");
        _jsonMatchers.ShouldHaveProperty(obj, "data_atualizacao");

        var conteudo = obj["conteudo"]!.Value<string>();
        var versao = obj["versao"]!.Value<string>();

        conteudo.Should().NotBeNullOrEmpty();
        versao.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Test_Get_Politica_Privacidade()
    {
        // Teste para obter política de privacidade
        var response = await GetAsync("api/institucional/politica-privacidade");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura da política de privacidade
        _jsonMatchers.ShouldHaveProperty(obj, "conteudo");
        _jsonMatchers.ShouldHaveProperty(obj, "versao");
        _jsonMatchers.ShouldHaveProperty(obj, "data_atualizacao");

        var conteudo = obj["conteudo"]!.Value<string>();
        var versao = obj["versao"]!.Value<string>();

        conteudo.Should().NotBeNullOrEmpty();
        versao.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Test_Create_Configuracao()
    {
        await AuthenticateAsAdminAsync();

        var requestData = new
        {
            chave = "teste_config_" + Guid.NewGuid().ToString("N")[..8],
            valor = new
            {
                descricao = "Configuração de teste",
                ativo = true,
                parametros = new
                {
                    timeout = 30,
                    retry_count = 3
                }
            }
        };

        var response = await PostAsync("api/institucional/configuracoes", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        var configId = GetIdFromLocationHeader(response);
        configId.Should().BeGreaterThan(0);

        // Verificar se a configuração foi criada
        var getResponse = await GetAsync($"api/institucional/configuracoes/{configId}");
        _jsonMatchers.ShouldHaveStatusCode(getResponse, HttpStatusCode.OK);

        var getJson = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var getObj = _jsonMatchers.ShouldBeObject(getJson);

        _jsonMatchers.ShouldHaveProperty(getObj, "id");
        _jsonMatchers.ShouldHaveProperty(getObj, "chave");
        _jsonMatchers.ShouldHaveProperty(getObj, "valor");

        var chave = getObj["chave"]!.Value<string>();
        chave.Should().Be(requestData.chave);
    }

    [Fact]
    public async Task Test_Update_Configuracao()
    {
        await AuthenticateAsAdminAsync();

        // Primeiro criar uma configuração
        var createData = new
        {
            chave = "teste_update_" + Guid.NewGuid().ToString("N")[..8],
            valor = new
            {
                descricao = "Configuração original",
                ativo = true
            }
        };

        var createResponse = await PostAsync("api/institucional/configuracoes", createData);
        _jsonMatchers.ShouldHaveStatusCode(createResponse, HttpStatusCode.Created);

        var configId = GetIdFromLocationHeader(createResponse);

        // Atualizar a configuração
        var updateData = new
        {
            chave = createData.chave,
            valor = new
            {
                descricao = "Configuração atualizada",
                ativo = false
            }
        };

        var updateResponse = await PutAsync($"api/institucional/configuracoes/{configId}", updateData);
        _jsonMatchers.ShouldHaveStatusCode(updateResponse, HttpStatusCode.OK);

        // Verificar se foi atualizada
        var getResponse = await GetAsync($"api/institucional/configuracoes/{configId}");
        var getJson = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var getObj = _jsonMatchers.ShouldBeObject(getJson);

        var valor = getObj["valor"]!.Value<JObject>();
        var descricao = valor!["descricao"]!.Value<string>();
        var ativo = valor["ativo"]!.Value<bool>();

        descricao.Should().Be("Configuração atualizada");
        ativo.Should().BeFalse();
    }

    [Fact]
    public async Task Test_Delete_Configuracao()
    {
        await AuthenticateAsAdminAsync();

        // Primeiro criar uma configuração
        var createData = new
        {
            chave = "teste_delete_" + Guid.NewGuid().ToString("N")[..8],
            valor = new
            {
                descricao = "Configuração para deletar",
                ativo = true
            }
        };

        var createResponse = await PostAsync("api/institucional/configuracoes", createData);
        _jsonMatchers.ShouldHaveStatusCode(createResponse, HttpStatusCode.Created);

        var configId = GetIdFromLocationHeader(createResponse);

        // Deletar a configuração
        var deleteResponse = await DeleteAsync($"api/institucional/configuracoes/{configId}");
        _jsonMatchers.ShouldHaveStatusCode(deleteResponse, HttpStatusCode.OK);

        // Verificar se foi deletada
        var getResponse = await GetAsync($"api/institucional/configuracoes/{configId}");
        _jsonMatchers.ShouldHaveStatusCode(getResponse, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Test_List_Configuracoes_Paged()
    {
        await AuthenticateAsAdminAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 10,
            chave = "sistema"
        };

        var response = await PostAsync("api/institucional/configuracoes/all", requestData);
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

            _jsonMatchers.ShouldHaveProperty(itemObj, "id");
            _jsonMatchers.ShouldHaveProperty(itemObj, "chave");
            _jsonMatchers.ShouldHaveProperty(itemObj, "valor");
            _jsonMatchers.ShouldHaveProperty(itemObj, "data_criacao");
            _jsonMatchers.ShouldHaveProperty(itemObj, "data_atualizacao");
        }
    }

    [Fact]
    public async Task Test_Audit_Cadastrese_With_Invalid_Data()
    {
        var requestData = new
        {
            page = -1, // Página inválida
            max_per_page = 0, // Max per page inválido
            descricao = "",
            endereco = ""
        };

        var response = await PostAsync("api/institucional/ifarmer/audit_cadastrese", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Create_Configuracao_Without_Authentication()
    {
        ClearAuthentication();

        var requestData = new
        {
            chave = "teste_sem_auth",
            valor = new
            {
                descricao = "Teste sem autenticação"
            }
        };

        var response = await PostAsync("api/institucional/configuracoes", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Create_Configuracao_With_Non_Admin_User()
    {
        await AuthenticateAsProducerAsync();

        var requestData = new
        {
            chave = "teste_nao_admin",
            valor = new
            {
                descricao = "Teste usuário não admin"
            }
        };

        var response = await PostAsync("api/institucional/configuracoes", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_Create_Configuracao_With_Duplicate_Key()
    {
        await AuthenticateAsAdminAsync();

        var chaveUnica = "teste_duplicada_" + Guid.NewGuid().ToString("N")[..8];

        var requestData = new
        {
            chave = chaveUnica,
            valor = new
            {
                descricao = "Primeira configuração"
            }
        };

        // Primeira criação - deve funcionar
        var response1 = await PostAsync("api/institucional/configuracoes", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response1, HttpStatusCode.Created);

        // Segunda criação com a mesma chave - deve falhar
        var response2 = await PostAsync("api/institucional/configuracoes", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response2, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Get_Configuracao_By_Chave()
    {
        await AuthenticateAsAdminAsync();

        var chave = "versao_sistema";
        var response = await GetAsync($"api/institucional/configuracoes/chave/{chave}");
        
        // Pode retornar OK se existe ou NotFound se não existe
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
            var obj = _jsonMatchers.ShouldBeObject(json);

            _jsonMatchers.ShouldHaveProperty(obj, "chave");
            _jsonMatchers.ShouldHaveProperty(obj, "valor");

            var chaveRetornada = obj["chave"]!.Value<string>();
            chaveRetornada.Should().Be(chave);
        }
    }
}