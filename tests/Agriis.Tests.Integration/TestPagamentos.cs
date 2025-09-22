using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Pagamentos
/// Migrado de test_pagamentos.py
/// </summary>
public class TestPagamentos : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestPagamentos(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_List_Forma_Pagamentos()
    {
        await AuthenticateAsProducerAsync(1);

        var response = await GetAsync("v1/pagamentos/formas/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        if (array.Count > 0)
        {
            var firstItem = array[0];
            var obj = _jsonMatchers.ShouldBeObject(firstItem);

            // Validar estrutura da forma de pagamento (2 propriedades)
            obj.Properties().Should().HaveCount(2);
            _jsonMatchers.ShouldHaveProperty(obj, "id");
            _jsonMatchers.ShouldHaveProperty(obj, "descricao");
        }
    }

    [Fact]
    public async Task Test_List_Formas_Pagamento_By_Pedido()
    {
        await AuthenticateAsProducerAsync(1);
        var pedidoId = 368;

        var response = await GetAsync($"v1/pagamentos/pedido/{pedidoId}/formas/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        // Verificar se retorna array (pode estar vazio)
        array.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_Create_Forma_Pagamento()
    {
        await AuthenticateAsSupplierAsync(1);

        var requestData = new
        {
            descricao = "Cartão de Crédito Visa",
            tipo = "CARTAO_CREDITO",
            ativo = true,
            aceita_parcelamento = true,
            max_parcelas = 12,
            taxa_juros = 2.5m
        };

        var response = await PostAsync("v1/pagamentos/formas/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        // Verificar se foi criado corretamente
        var location = response.Headers.Location?.ToString();
        location.Should().NotBeNullOrEmpty();
        var formaId = ExtractIdFromLocation(location!);

        var getResponse = await GetAsync($"v1/pagamentos/formas/{formaId}/");
        _jsonMatchers.ShouldHaveStatusCode(getResponse, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHavePropertyWithValue(obj, "descricao", "Cartão de Crédito Visa");
        _jsonMatchers.ShouldHavePropertyWithValue(obj, "tipo", "CARTAO_CREDITO");
        _jsonMatchers.ShouldHavePropertyWithValue(obj, "ativo", true);
    }

    [Fact]
    public async Task Test_Update_Forma_Pagamento()
    {
        await AuthenticateAsSupplierAsync(1);

        // Primeiro criar uma forma de pagamento
        var createData = new
        {
            descricao = "PIX Original",
            tipo = "PIX",
            ativo = true,
            aceita_parcelamento = false,
            max_parcelas = 1,
            taxa_juros = 0m
        };

        var createResponse = await PostAsync("v1/pagamentos/formas/", createData);
        _jsonMatchers.ShouldHaveStatusCode(createResponse, HttpStatusCode.Created);

        var location = createResponse.Headers.Location?.ToString();
        var formaId = ExtractIdFromLocation(location!);

        // Atualizar a forma de pagamento
        var updateData = new
        {
            descricao = "PIX Atualizado",
            tipo = "PIX",
            ativo = false, // Desativar
            aceita_parcelamento = false,
            max_parcelas = 1,
            taxa_juros = 0m
        };

        var updateResponse = await PutAsync($"v1/pagamentos/formas/{formaId}/", updateData);
        _jsonMatchers.ShouldHaveStatusCode(updateResponse, HttpStatusCode.OK);

        // Verificar se foi atualizado
        var getResponse = await GetAsync($"v1/pagamentos/formas/{formaId}/");
        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHavePropertyWithValue(obj, "descricao", "PIX Atualizado");
        _jsonMatchers.ShouldHavePropertyWithValue(obj, "ativo", false);
    }

    [Fact]
    public async Task Test_Delete_Forma_Pagamento()
    {
        await AuthenticateAsSupplierAsync(1);

        // Criar uma forma de pagamento para deletar
        var createData = new
        {
            descricao = "Forma Temporária",
            tipo = "BOLETO",
            ativo = true,
            aceita_parcelamento = false,
            max_parcelas = 1,
            taxa_juros = 0m
        };

        var createResponse = await PostAsync("v1/pagamentos/formas/", createData);
        _jsonMatchers.ShouldHaveStatusCode(createResponse, HttpStatusCode.Created);

        var location = createResponse.Headers.Location?.ToString();
        var formaId = ExtractIdFromLocation(location!);

        // Deletar a forma de pagamento
        var deleteResponse = await DeleteAsync($"v1/pagamentos/formas/{formaId}/");
        _jsonMatchers.ShouldHaveStatusCode(deleteResponse, HttpStatusCode.OK);

        // Verificar se foi deletado
        var getResponse = await GetAsync($"v1/pagamentos/formas/{formaId}/");
        _jsonMatchers.ShouldHaveStatusCode(getResponse, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Test_Associate_Forma_Pagamento_To_Pedido()
    {
        await AuthenticateAsProducerAsync(1);
        var pedidoId = 1;
        var formaPagamentoId = 1;

        var requestData = new
        {
            forma_pagamento_id = formaPagamentoId,
            pedido_id = pedidoId,
            valor = 1500.00m,
            parcelas = 3,
            data_vencimento = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd")
        };

        var response = await PostAsync($"v1/pagamentos/pedido/{pedidoId}/formas/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        // Verificar se a associação foi criada
        var getResponse = await GetAsync($"v1/pagamentos/pedido/{pedidoId}/formas/");
        _jsonMatchers.ShouldHaveStatusCode(getResponse, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var array = _jsonMatchers.ShouldBeArray(json);

        array.Should().NotBeEmpty();
        _jsonMatchers.ShouldContainItemWithId(array, formaPagamentoId);
    }

    [Fact]
    public async Task Test_Calculate_Installments()
    {
        await AuthenticateAsProducerAsync(1);

        var requestData = new
        {
            valor_total = 5000.00m,
            forma_pagamento_id = 1,
            numero_parcelas = 6
        };

        var response = await PostAsync("v1/pagamentos/calcular-parcelas/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHaveProperty(obj, "valor_total");
        _jsonMatchers.ShouldHaveProperty(obj, "valor_parcela");
        _jsonMatchers.ShouldHaveProperty(obj, "numero_parcelas");
        _jsonMatchers.ShouldHaveProperty(obj, "taxa_juros");
        _jsonMatchers.ShouldHaveProperty(obj, "valor_juros");
        _jsonMatchers.ShouldHaveProperty(obj, "parcelas");

        var parcelas = _jsonMatchers.ShouldBeArray(obj["parcelas"]!);
        parcelas.Count.Should().Be(6);

        // Verificar estrutura de cada parcela
        foreach (var parcela in parcelas)
        {
            var parcelaObj = _jsonMatchers.ShouldBeObject(parcela);
            _jsonMatchers.ShouldHaveProperty(parcelaObj, "numero");
            _jsonMatchers.ShouldHaveProperty(parcelaObj, "valor");
            _jsonMatchers.ShouldHaveProperty(parcelaObj, "data_vencimento");
        }
    }

    [Fact]
    public async Task Test_Payment_Types_Validation()
    {
        await AuthenticateAsSupplierAsync(1);

        // Teste com tipo de pagamento inválido
        var invalidData = new
        {
            descricao = "Forma Inválida",
            tipo = "TIPO_INEXISTENTE", // Tipo inválido
            ativo = true,
            aceita_parcelamento = false,
            max_parcelas = 1,
            taxa_juros = 0m
        };

        var response = await PostAsync("v1/pagamentos/formas/", invalidData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Payment_Installment_Validation()
    {
        await AuthenticateAsSupplierAsync(1);

        // Teste com número de parcelas inválido
        var invalidData = new
        {
            descricao = "Cartão com Parcelas Inválidas",
            tipo = "CARTAO_CREDITO",
            ativo = true,
            aceita_parcelamento = true,
            max_parcelas = 0, // Número inválido
            taxa_juros = 2.5m
        };

        var response = await PostAsync("v1/pagamentos/formas/", invalidData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Payment_Interest_Rate_Validation()
    {
        await AuthenticateAsSupplierAsync(1);

        // Teste com taxa de juros negativa
        var invalidData = new
        {
            descricao = "Forma com Taxa Negativa",
            tipo = "CARTAO_CREDITO",
            ativo = true,
            aceita_parcelamento = true,
            max_parcelas = 12,
            taxa_juros = -1.5m // Taxa negativa
        };

        var response = await PostAsync("v1/pagamentos/formas/", invalidData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Payment_Without_Authentication()
    {
        ClearAuthentication();

        var response = await GetAsync("v1/pagamentos/formas/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Payment_Unauthorized_Creation()
    {
        // Produtor tentando criar forma de pagamento (apenas fornecedores podem)
        await AuthenticateAsProducerAsync(1);

        var requestData = new
        {
            descricao = "Forma Não Autorizada",
            tipo = "PIX",
            ativo = true,
            aceita_parcelamento = false,
            max_parcelas = 1,
            taxa_juros = 0m
        };

        var response = await PostAsync("v1/pagamentos/formas/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Test_Payment_PIX_Configuration()
    {
        await AuthenticateAsSupplierAsync(1);

        var pixData = new
        {
            descricao = "PIX Instantâneo",
            tipo = "PIX",
            ativo = true,
            aceita_parcelamento = false,
            max_parcelas = 1,
            taxa_juros = 0m,
            tempo_processamento = "INSTANTANEO",
            disponivel_24h = true
        };

        var response = await PostAsync("v1/pagamentos/formas/", pixData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        var location = response.Headers.Location?.ToString();
        var formaId = ExtractIdFromLocation(location!);

        var getResponse = await GetAsync($"v1/pagamentos/formas/{formaId}/");
        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHavePropertyWithValue(obj, "tipo", "PIX");
        _jsonMatchers.ShouldHavePropertyWithValue(obj, "aceita_parcelamento", false);
        _jsonMatchers.ShouldHavePropertyWithValue(obj, "max_parcelas", 1);
        _jsonMatchers.ShouldHavePropertyWithValue(obj, "taxa_juros", 0);
    }

    [Fact]
    public async Task Test_Payment_Credit_Card_Configuration()
    {
        await AuthenticateAsSupplierAsync(1);

        var creditCardData = new
        {
            descricao = "Cartão de Crédito Premium",
            tipo = "CARTAO_CREDITO",
            ativo = true,
            aceita_parcelamento = true,
            max_parcelas = 24,
            taxa_juros = 3.99m,
            bandeiras_aceitas = new[] { "VISA", "MASTERCARD", "ELO" },
            valor_minimo_parcela = 50.00m
        };

        var response = await PostAsync("v1/pagamentos/formas/", creditCardData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        var location = response.Headers.Location?.ToString();
        var formaId = ExtractIdFromLocation(location!);

        var getResponse = await GetAsync($"v1/pagamentos/formas/{formaId}/");
        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHavePropertyWithValue(obj, "tipo", "CARTAO_CREDITO");
        _jsonMatchers.ShouldHavePropertyWithValue(obj, "aceita_parcelamento", true);
        _jsonMatchers.ShouldHavePropertyWithValue(obj, "max_parcelas", 24);
        _jsonMatchers.ShouldBeNumber(obj, "taxa_juros");
    }

    [Fact]
    public async Task Test_Payment_Boleto_Configuration()
    {
        await AuthenticateAsSupplierAsync(1);

        var boletoData = new
        {
            descricao = "Boleto Bancário",
            tipo = "BOLETO",
            ativo = true,
            aceita_parcelamento = false,
            max_parcelas = 1,
            taxa_juros = 0m,
            dias_vencimento = 30,
            multa_atraso = 2.0m,
            juros_mora_dia = 0.033m
        };

        var response = await PostAsync("v1/pagamentos/formas/", boletoData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        var location = response.Headers.Location?.ToString();
        var formaId = ExtractIdFromLocation(location!);

        var getResponse = await GetAsync($"v1/pagamentos/formas/{formaId}/");
        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHavePropertyWithValue(obj, "tipo", "BOLETO");
        _jsonMatchers.ShouldHavePropertyWithValue(obj, "aceita_parcelamento", false);
    }

    [Fact]
    public async Task Test_Payment_Status_Transitions()
    {
        await AuthenticateAsProducerAsync(1);
        var pedidoId = 1;
        var formaPagamentoId = 1;

        // Criar pagamento
        var createData = new
        {
            forma_pagamento_id = formaPagamentoId,
            pedido_id = pedidoId,
            valor = 2000.00m,
            parcelas = 1,
            status = "PENDENTE"
        };

        var createResponse = await PostAsync($"v1/pagamentos/pedido/{pedidoId}/formas/", createData);
        _jsonMatchers.ShouldHaveStatusCode(createResponse, HttpStatusCode.Created);

        var location = createResponse.Headers.Location?.ToString();
        var pagamentoId = ExtractIdFromLocation(location!);

        // Atualizar status para PROCESSANDO
        var updateData = new
        {
            status = "PROCESSANDO"
        };

        var updateResponse = await PutAsync($"v1/pagamentos/{pagamentoId}/status/", updateData);
        _jsonMatchers.ShouldHaveStatusCode(updateResponse, HttpStatusCode.OK);

        // Verificar se o status foi atualizado
        var getResponse = await GetAsync($"v1/pagamentos/{pagamentoId}/");
        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(getResponse);
        var obj = _jsonMatchers.ShouldBeObject(json);

        _jsonMatchers.ShouldHavePropertyWithValue(obj, "status", "PROCESSANDO");
    }

    [Fact]
    public async Task Test_Payment_History_Tracking()
    {
        await AuthenticateAsProducerAsync(1);
        var pedidoId = 1;

        // Buscar histórico de pagamentos do pedido
        var response = await GetAsync($"v1/pagamentos/pedido/{pedidoId}/historico/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var array = _jsonMatchers.ShouldBeArray(json);

        // Verificar estrutura do histórico
        foreach (var item in array)
        {
            var obj = _jsonMatchers.ShouldBeObject(item);
            _jsonMatchers.ShouldHaveProperty(obj, "id");
            _jsonMatchers.ShouldHaveProperty(obj, "status");
            _jsonMatchers.ShouldHaveProperty(obj, "data_alteracao");
            _jsonMatchers.ShouldHaveProperty(obj, "valor");
            _jsonMatchers.ShouldHaveProperty(obj, "forma_pagamento");
        }
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