using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Pedidos
/// Migrado de test_pedidos.py
/// Inclui todos os cenários de carrinho e propostas
/// </summary>
public class TestPedidos : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestPedidos(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_List_All_Pedidos()
    {
        await AuthenticateAsProducerAsync(1); // ProdutorMobileSandbox equivalent

        var requestData = new
        {
            page = 0,
            max_per_page = 10,
            status_carrinho = "EM_ABERTO",
            produtor_id = 1
        };

        var response = await PostAsync("v1/pedidos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura de paginação
        _jsonMatchers.ShouldHaveProperty(obj, "items");
        var items = _jsonMatchers.ShouldBeArray(obj["items"]!);

        if (items.Count > 0)
        {
            var firstItem = items[0];
            var itemObj = _jsonMatchers.ShouldBeObject(firstItem);

            // Validar estrutura do pedido (13 propriedades como no Python)
            itemObj.Properties().Should().HaveCount(13);
            _jsonMatchers.ShouldHaveProperty(itemObj, "id");
            _jsonMatchers.ShouldHaveProperty(itemObj, "permite_contato");
            _jsonMatchers.ShouldHaveProperty(itemObj, "fornecedor_id");
            _jsonMatchers.ShouldHaveProperty(itemObj, "fornecedor_nome");
            _jsonMatchers.ShouldHaveProperty(itemObj, "produtor_id");
            _jsonMatchers.ShouldHaveProperty(itemObj, "quantidade_itens");
            _jsonMatchers.ShouldHaveProperty(itemObj, "quantidade_total_itens");
            _jsonMatchers.ShouldHaveProperty(itemObj, "ultima_interacao");
            _jsonMatchers.ShouldHaveProperty(itemObj, "url_logo_small");
            _jsonMatchers.ShouldHaveProperty(itemObj, "status");
            _jsonMatchers.ShouldHaveProperty(itemObj, "total");
        }
    }

    [Fact]
    public async Task Test_Find_Pedido_Em_Aberto()
    {
        await AuthenticateAsProducerAsync(1);
        var fornecedorId = 1;
        var produtorId = 1;

        var response = await GetAsync($"v1/pedidos/fornecedor/{fornecedorId}/produtor/{produtorId}/aberto/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Find_Representante_Json_By_Pedido_Id()
    {
        await AuthenticateAsProducerAsync(1);
        var pedidoId = 1;

        var response = await GetAsync($"v1/pedidos/{pedidoId}/representante/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Find_Pedido_By_Id()
    {
        await AuthenticateAsProducerAsync(1);
        var pedidoId = 1;

        var response = await GetAsync($"v1/pedidos/{pedidoId}/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura do pedido (13 propriedades)
        obj.Properties().Should().HaveCount(13);
    }

    [Fact]
    public async Task Test_Find_Item_By_Id()
    {
        await AuthenticateAsProducerAsync(1);
        var itemId = 264;

        var response = await GetAsync($"v1/pedidos/itens/{itemId}/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_List_Itens_Pedido()
    {
        await AuthenticateAsProducerAsync(1);
        var pedidoId = 1;

        var response = await GetAsync($"v1/pedidos/{pedidoId}/itens/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Calcular_Item_Pedido()
    {
        await AuthenticateAsProducerAsync(1);

        var requestData = new
        {
            propriedade_id = 1,
            data_pagamento_solicitada = "29/07/2020",
            catalogo_item_id = 1,
            produtor_id = 1,
            transportes = new[]
            {
                new { quantidade = 5, data = "30/07/2020" }
            }
        };

        var response = await PostAsync("v1/pedidos/itens/calcular", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Create_Item_Pedido()
    {
        await AuthenticateAsProducerAsync(1);

        var requestData = new
        {
            propriedade_id = 1,
            data_pagamento_solicitada = "30/04/2021",
            catalogo_item_id = 1,
            produtor_id = 1,
            transportes = new[]
            {
                new
                {
                    quantidade = 112,
                    data = "01/09/2020",
                    retirar_no_local = false,
                    valor_frete = (decimal?)null
                }
            }
        };

        var response = await PostAsync("v1/pedidos/itens/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);
    }

    [Fact]
    public async Task Test_Update_Item_Pedido()
    {
        await AuthenticateAsProducerAsync(1);
        var itemId = 497;

        var requestData = new
        {
            propriedade_id = 1,
            data_pagamento_solicitada = "03/10/2020",
            catalogo_item_id = 1,
            produtor_id = 1,
            transportes = new[]
            {
                new { quantidade = 5, data = "30/07/2020" }
            }
        };

        var response = await PutAsync($"v1/pedidos/itens/{itemId}/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        // Verificar se o header Location contém o ID do item atualizado
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_Update_Pedido()
    {
        await AuthenticateAsProducerAsync(1);
        var pedidoId = 1;

        var requestData = new
        {
            permite_contato = false
        };

        var response = await PutAsync($"v1/pedidos/{pedidoId}/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Delete_Item_Pedido()
    {
        await AuthenticateAsProducerAsync(1);
        var itemId = 498;

        var response = await DeleteAsync($"v1/pedidos/itens/{itemId}/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Create_Proposta_Complete_Flow()
    {
        var pedidoId = 27;
        var url = $"v1/pedidos/{pedidoId}/propostas/";

        // Inicia negociação como produtor
        await AuthenticateAsProducerAsync(1);
        var requestData = new { };

        var response = await PostAsync(url, requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        // Tentativa de criar proposta novamente sem enviar uma ação - deve falhar
        var duplicateResponse = await PostAsync(url, requestData);
        _jsonMatchers.ShouldHaveStatusCode(duplicateResponse, HttpStatusCode.UnprocessableEntity);

        // Aguardar um pouco para simular tempo entre interações
        await Task.Delay(1000);

        // Fornecedor interage com a negociação
        await AuthenticateAsSupplierAsync(1);
        var fornecedorData = new
        {
            observacao = "Neque porro quisquam est qui dolorem ipsum quia dolor sit amet, adipisci velit..."
        };

        var fornecedorResponse = await PostAsync(url, fornecedorData);
        _jsonMatchers.ShouldHaveStatusCode(fornecedorResponse, HttpStatusCode.Created);

        // Aguardar novamente
        await Task.Delay(1000);

        // Fornecedor interage novamente
        var fornecedorData2 = new
        {
            observacao = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor"
        };

        var fornecedorResponse2 = await PostAsync(url, fornecedorData2);
        _jsonMatchers.ShouldHaveStatusCode(fornecedorResponse2, HttpStatusCode.Created);
    }

    [Fact]
    public async Task Test_List_All_Propostas()
    {
        await AuthenticateAsProducerAsync(1);
        var pedidoId = 1;

        var requestData = new
        {
            page = 0,
            max_per_page = 10
        };

        var response = await PostAsync($"v1/pedidos/{pedidoId}/propostas/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Carrinho_Add_Multiple_Items()
    {
        await AuthenticateAsProducerAsync(1);

        // Adicionar primeiro item
        var item1 = new
        {
            propriedade_id = 1,
            data_pagamento_solicitada = "30/04/2021",
            catalogo_item_id = 1,
            produtor_id = 1,
            transportes = new[]
            {
                new { quantidade = 50, data = "01/09/2020", retirar_no_local = false }
            }
        };

        var response1 = await PostAsync("v1/pedidos/itens/", item1);
        _jsonMatchers.ShouldHaveStatusCode(response1, HttpStatusCode.Created);

        // Adicionar segundo item
        var item2 = new
        {
            propriedade_id = 1,
            data_pagamento_solicitada = "30/04/2021",
            catalogo_item_id = 2,
            produtor_id = 1,
            transportes = new[]
            {
                new { quantidade = 25, data = "01/09/2020", retirar_no_local = false }
            }
        };

        var response2 = await PostAsync("v1/pedidos/itens/", item2);
        _jsonMatchers.ShouldHaveStatusCode(response2, HttpStatusCode.Created);

        // Verificar se o carrinho foi atualizado
        var pedidoId = 1;
        var pedidoResponse = await GetAsync($"v1/pedidos/{pedidoId}/");
        _jsonMatchers.ShouldHaveStatusCode(pedidoResponse, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(pedidoResponse);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHaveProperty(obj, "quantidade_itens");
        _jsonMatchers.ShouldHaveProperty(obj, "total");
    }

    [Fact]
    public async Task Test_Carrinho_Update_Quantities()
    {
        await AuthenticateAsProducerAsync(1);
        var itemId = 1;

        // Atualizar quantidade do item
        var updateData = new
        {
            propriedade_id = 1,
            data_pagamento_solicitada = "03/10/2020",
            catalogo_item_id = 1,
            produtor_id = 1,
            transportes = new[]
            {
                new { quantidade = 100, data = "30/07/2020" } // Quantidade alterada
            }
        };

        var response = await PutAsync($"v1/pedidos/itens/{itemId}/", updateData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        // Verificar se os totais foram recalculados
        var pedidoId = 1;
        var pedidoResponse = await GetAsync($"v1/pedidos/{pedidoId}/");
        _jsonMatchers.ShouldHaveStatusCode(pedidoResponse, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Proposta_Negotiation_Flow()
    {
        var pedidoId = 1;

        // Produtor inicia negociação
        await AuthenticateAsProducerAsync(1);
        var iniciarNegociacao = new { };

        var response = await PostAsync($"v1/pedidos/{pedidoId}/propostas/", iniciarNegociacao);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        // Fornecedor responde com contraproposta
        await AuthenticateAsSupplierAsync(1);
        var contraproposta = new
        {
            observacao = "Podemos negociar um desconto de 5% para pagamento à vista",
            desconto_percentual = 5.0m
        };

        var contrapropostaResponse = await PostAsync($"v1/pedidos/{pedidoId}/propostas/", contraproposta);
        _jsonMatchers.ShouldHaveStatusCode(contrapropostaResponse, HttpStatusCode.Created);

        // Produtor aceita a proposta
        await AuthenticateAsProducerAsync(1);
        var aceitarProposta = new
        {
            acao_comprador = "ACEITOU"
        };

        var aceitarResponse = await PostAsync($"v1/pedidos/{pedidoId}/propostas/", aceitarProposta);
        _jsonMatchers.ShouldHaveStatusCode(aceitarResponse, HttpStatusCode.Created);
    }

    [Fact]
    public async Task Test_Pedido_Status_Transitions()
    {
        await AuthenticateAsProducerAsync(1);
        var pedidoId = 1;

        // Verificar status inicial
        var response = await GetAsync($"v1/pedidos/{pedidoId}/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHaveProperty(obj, "status");
        _jsonMatchers.ShouldHaveProperty(obj, "status_carrinho");

        // Atualizar status do pedido
        var updateStatus = new
        {
            status = "FECHADO"
        };

        var updateResponse = await PutAsync($"v1/pedidos/{pedidoId}/", updateStatus);
        _jsonMatchers.ShouldHaveStatusCode(updateResponse, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Pedido_Data_Limite_Interacao()
    {
        await AuthenticateAsProducerAsync(1);

        // Criar pedido com data limite específica
        var createData = new
        {
            fornecedor_id = 1,
            produtor_id = 1,
            permite_contato = true,
            negociar_pedido = true,
            data_limite_interacao = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd")
        };

        var response = await PostAsync("v1/pedidos/", createData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        // Verificar se a data limite foi definida corretamente
        var pedidoId = 1;
        var pedidoResponse = await GetAsync($"v1/pedidos/{pedidoId}/");
        _jsonMatchers.ShouldHaveStatusCode(pedidoResponse, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(pedidoResponse);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHaveProperty(obj, "data_limite_interacao");
        _jsonMatchers.ShouldHaveValidDateFormat(obj, "data_limite_interacao");
    }

    [Fact]
    public async Task Test_Pedido_Validation_Errors()
    {
        await AuthenticateAsProducerAsync(1);

        // Teste com dados inválidos
        var invalidData = new
        {
            propriedade_id = 0, // ID inválido
            data_pagamento_solicitada = "invalid-date", // Data inválida
            catalogo_item_id = -1, // ID negativo
            produtor_id = 1,
            transportes = new object[] { } // Array vazio
        };

        var response = await PostAsync("v1/pedidos/itens/", invalidData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Pedido_Without_Authentication()
    {
        ClearAuthentication();

        var requestData = new
        {
            page = 0,
            max_per_page = 10,
            status_carrinho = "EM_ABERTO",
            produtor_id = 1
        };

        var response = await PostAsync("v1/pedidos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Pedido_Unauthorized_Access()
    {
        // Tentar acessar pedido de outro produtor
        await AuthenticateAsProducerAsync(2);
        var pedidoId = 1; // Pedido do produtor 1

        var response = await GetAsync($"v1/pedidos/{pedidoId}/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Forbidden);
    }
}