using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;
using Agriis.Tests.Shared.Authentication;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Combos
/// Migrado de test_combos.py
/// </summary>
public class TestCombos : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestCombos(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_Create_Combo()
    {
        // Teste do cadastro do combo
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            safra_id = 13,
            cultura_id = 135,
            moeda = "DOLAR",
            estado_id = 52,
            modalidade_pagamento = "BARTER",
            fornecedor_id = 5,
            data_min_exibicao = "04/11/1989",
            data_max_exibicao = "04/11/2021",
            data_pagamento = "04/11/2022"
        };

        var response = await PostAsync("api/combos/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);

        var comboId = GetIdFromLocationHeader(response);
        comboId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Test_List_Municipios_Restritos_Paged()
    {
        await AuthenticateAsSupplierAsync();

        var comboId = TestUserAuth.FornecedorWebSandbox.ComboId;
        var requestData = new
        {
            page = 0,
            max_per_page = 10
        };

        var response = await PostAsync($"api/combos/{comboId}/municipios/restritos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Municipio_Restrito_Add()
    {
        // Teste municipio_restrito_add
        await AuthenticateAsSupplierAsync();

        var comboId = TestUserAuth.FornecedorWebSandbox.ComboId;
        var municipioId = 5203906;

        var response = await PutAsync($"api/combos/{comboId}/restringir/municipio/{municipioId}/", new { });
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Municipio_Restrito_Remove()
    {
        // Teste municipio_restrito_remove
        await AuthenticateAsSupplierAsync();

        var comboId = TestUserAuth.FornecedorWebSandbox.ComboId;
        var municipioId = 5203906;

        var response = await DeleteAsync($"api/combos/{comboId}/restringir/municipio/{municipioId}/");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Add_Local_Recebimento()
    {
        // Teste local_recebimento_add
        await AuthenticateAsSupplierAsync();

        var comboId = TestUserAuth.FornecedorWebSandbox.ComboId;
        var requestData = new
        {
            local_recebimento_id = 3,
            preco = 40.60
        };

        var response = await PostAsync($"api/combos/{comboId}/local/recebimento/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Remove_Local_Recebimento()
    {
        // Teste remove_local_recebimento
        await AuthenticateAsSupplierAsync();

        var comboId = TestUserAuth.FornecedorWebSandbox.ComboId;
        var localRecebimentoId = 2;

        var response = await DeleteAsync($"api/combos/{comboId}/local/recebimento/{localRecebimentoId}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_List_Combo_Local_Recebimento_Paged()
    {
        await AuthenticateAsSupplierAsync();

        var comboId = TestUserAuth.FornecedorWebSandbox.ComboId;
        var requestData = new
        {
            page = 0,
            max_per_page = 10
        };

        var response = await PostAsync($"api/combos/{comboId}/local/recebimento/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_List_Combo_Categoria_Desconto_Paged()
    {
        await AuthenticateAsSupplierAsync();

        var comboId = TestUserAuth.FornecedorWebSandbox.ComboId;
        var requestData = new
        {
            page = 0,
            max_per_page = 10
        };

        var response = await PostAsync($"api/combos/{comboId}/categoria/desconto/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Add_Categoria_Desconto()
    {
        // Teste add_categoria_desconto
        await AuthenticateAsSupplierAsync();

        var comboId = TestUserAuth.FornecedorWebSandbox.ComboId;
        var categoriaId = TestUserAuth.FornecedorWebSandbox.CategoriaId;
        var requestData = new
        {
            pecentual_desconto = 40.60
        };

        var response = await PostAsync($"api/combos/{comboId}/categoria/{categoriaId}/desconto", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Remove_Categoria_Desconto()
    {
        // Teste remove_categoria_desconto
        await AuthenticateAsSupplierAsync();

        var comboId = TestUserAuth.FornecedorWebSandbox.ComboId;
        var categoriaId = TestUserAuth.FornecedorWebSandbox.CategoriaId;

        var response = await DeleteAsync($"api/combos/{comboId}/categoria/{categoriaId}/desconto");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_List_Combos_By_Fornecedor()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            page = 0,
            max_per_page = 10
        };

        var response = await PostAsync("api/combos/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Find_Combo_By_Id()
    {
        await AuthenticateAsSupplierAsync();

        var comboId = 1;
        var response = await GetAsync($"api/combos/{comboId}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_List_Items_By_Combo()
    {
        await AuthenticateAsSupplierAsync();

        var comboId = 1;
        var requestData = new
        {
            page = 0,
            max_per_page = 10
        };

        var response = await PostAsync($"api/combos/{comboId}/items/all/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Create_Combo_Without_Authentication()
    {
        ClearAuthentication();

        var requestData = new
        {
            safra_id = 13,
            cultura_id = 135,
            moeda = "DOLAR",
            estado_id = 52,
            modalidade_pagamento = "BARTER",
            fornecedor_id = 5,
            data_min_exibicao = "04/11/1989",
            data_max_exibicao = "04/11/2021",
            data_pagamento = "04/11/2022"
        };

        var response = await PostAsync("api/combos/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Create_Combo_With_Invalid_Dates()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            safra_id = 13,
            cultura_id = 135,
            moeda = "DOLAR",
            estado_id = 52,
            modalidade_pagamento = "BARTER",
            fornecedor_id = 5,
            data_min_exibicao = "04/11/2022", // Data mínima posterior à máxima
            data_max_exibicao = "04/11/2021",
            data_pagamento = "04/11/2022"
        };

        var response = await PostAsync("api/combos/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Create_Combo_With_Invalid_Moeda()
    {
        await AuthenticateAsSupplierAsync();

        var requestData = new
        {
            safra_id = 13,
            cultura_id = 135,
            moeda = "EURO", // Moeda inválida
            estado_id = 52,
            modalidade_pagamento = "BARTER",
            fornecedor_id = 5,
            data_min_exibicao = "04/11/1989",
            data_max_exibicao = "04/11/2021",
            data_pagamento = "04/11/2022"
        };

        var response = await PostAsync("api/combos/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Add_Local_Recebimento_With_Invalid_Price()
    {
        await AuthenticateAsSupplierAsync();

        var comboId = TestUserAuth.FornecedorWebSandbox.ComboId;
        var requestData = new
        {
            local_recebimento_id = 3,
            preco = -10.50 // Preço negativo
        };

        var response = await PostAsync($"api/combos/{comboId}/local/recebimento/", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Test_Add_Categoria_Desconto_With_Invalid_Percentage()
    {
        await AuthenticateAsSupplierAsync();

        var comboId = TestUserAuth.FornecedorWebSandbox.ComboId;
        var categoriaId = TestUserAuth.FornecedorWebSandbox.CategoriaId;
        var requestData = new
        {
            pecentual_desconto = 150.0 // Percentual inválido
        };

        var response = await PostAsync($"api/combos/{comboId}/categoria/{categoriaId}/desconto", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    }
}