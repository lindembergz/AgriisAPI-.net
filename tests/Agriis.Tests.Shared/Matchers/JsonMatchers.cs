using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using System.Net;

namespace Agriis.Tests.Shared.Matchers;

/// <summary>
/// Matchers para validação de respostas JSON, equivalente aos JsonMatchers do Python
/// </summary>
public class JsonMatchers
{
    /// <summary>
    /// Valida se a resposta tem o status code esperado
    /// </summary>
    public void ShouldHaveStatusCode(HttpResponseMessage response, HttpStatusCode expectedStatusCode)
    {
        response.StatusCode.Should().Be(expectedStatusCode, 
            $"Expected status code {expectedStatusCode} but got {response.StatusCode}");
    }

    /// <summary>
    /// Valida se a resposta é um sucesso (2xx)
    /// </summary>
    public void ShouldBeSuccessful(HttpResponseMessage response)
    {
        response.IsSuccessStatusCode.Should().BeTrue(
            $"Expected successful response but got {response.StatusCode}");
    }

    /// <summary>
    /// Valida se a resposta contém JSON válido
    /// </summary>
    public async Task<JToken> ShouldHaveValidJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty("Response should contain content");

        try
        {
            return JToken.Parse(content);
        }
        catch (JsonReaderException ex)
        {
            throw new InvalidOperationException($"Response does not contain valid JSON: {ex.Message}\nContent: {content}");
        }
    }

    /// <summary>
    /// Valida se o JSON contém uma propriedade específica
    /// </summary>
    public void ShouldHaveProperty(JToken json, string propertyName)
    {
        if (json is JObject obj)
        {
            obj.Should().ContainKey(propertyName, $"JSON should contain property '{propertyName}'");
        }
        else
        {
            throw new InvalidOperationException($"Expected JObject but got {json.Type}");
        }
    }

    /// <summary>
    /// Valida se o JSON contém uma propriedade com valor específico
    /// </summary>
    public void ShouldHavePropertyWithValue(JToken json, string propertyName, object expectedValue)
    {
        ShouldHaveProperty(json, propertyName);
        
        var actualValue = json[propertyName];
        if (expectedValue == null)
        {
            actualValue.Should().BeNull($"Property '{propertyName}' should be null");
        }
        else
        {
            actualValue?.ToString().Should().Be(expectedValue.ToString(), 
                $"Property '{propertyName}' should have value '{expectedValue}'");
        }
    }

    /// <summary>
    /// Valida se o JSON não contém uma propriedade específica
    /// </summary>
    public void ShouldNotHaveProperty(JToken json, string propertyName)
    {
        if (json is JObject obj)
        {
            obj.Should().NotContainKey(propertyName, $"JSON should not contain property '{propertyName}'");
        }
    }

    /// <summary>
    /// Valida se o JSON é um array
    /// </summary>
    public JArray ShouldBeArray(JToken json)
    {
        json.Should().BeOfType<JArray>("JSON should be an array");
        return (JArray)json;
    }

    /// <summary>
    /// Valida se o JSON é um objeto
    /// </summary>
    public JObject ShouldBeObject(JToken json)
    {
        json.Should().BeOfType<JObject>("JSON should be an object");
        return (JObject)json;
    }

    /// <summary>
    /// Valida se o array tem o tamanho esperado
    /// </summary>
    public void ShouldHaveCount(JArray array, int expectedCount)
    {
        array.Count.Should().Be(expectedCount, $"Array should have {expectedCount} items");
    }

    /// <summary>
    /// Valida se o array não está vazio
    /// </summary>
    public void ShouldNotBeEmpty(JArray array)
    {
        array.Should().NotBeEmpty("Array should not be empty");
    }

    /// <summary>
    /// Valida se o array está vazio
    /// </summary>
    public void ShouldBeEmpty(JArray array)
    {
        array.Should().BeEmpty("Array should be empty");
    }

    /// <summary>
    /// Valida estrutura de erro padrão da API
    /// </summary>
    public async Task ShouldHaveErrorStructureAsync(HttpResponseMessage response, string? expectedErrorCode = null)
    {
        var json = await ShouldHaveValidJsonAsync(response);
        var errorObj = ShouldBeObject(json);

        ShouldHaveProperty(errorObj, "error_code");
        ShouldHaveProperty(errorObj, "error_description");

        if (!string.IsNullOrEmpty(expectedErrorCode))
        {
            ShouldHavePropertyWithValue(errorObj, "error_code", expectedErrorCode);
        }
    }

    /// <summary>
    /// Valida estrutura de sucesso padrão da API
    /// </summary>
    public async Task<JToken> ShouldHaveSuccessStructureAsync(HttpResponseMessage response)
    {
        ShouldBeSuccessful(response);
        return await ShouldHaveValidJsonAsync(response);
    }

    /// <summary>
    /// Valida estrutura de paginação
    /// </summary>
    public void ShouldHavePaginationStructure(JToken json)
    {
        var obj = ShouldBeObject(json);
        
        ShouldHaveProperty(obj, "items");
        ShouldHaveProperty(obj, "total_count");
        ShouldHaveProperty(obj, "page");
        ShouldHaveProperty(obj, "page_size");
        ShouldHaveProperty(obj, "total_pages");

        var items = obj["items"];
        items.Should().BeOfType<JArray>("items should be an array");
    }

    /// <summary>
    /// Valida se todos os itens do array têm uma propriedade específica
    /// </summary>
    public void AllItemsShouldHaveProperty(JArray array, string propertyName)
    {
        foreach (var item in array)
        {
            ShouldHaveProperty(item, propertyName);
        }
    }

    /// <summary>
    /// Valida se todos os itens do array têm as propriedades obrigatórias
    /// </summary>
    public void AllItemsShouldHaveRequiredProperties(JArray array, params string[] requiredProperties)
    {
        foreach (var item in array)
        {
            foreach (var property in requiredProperties)
            {
                ShouldHaveProperty(item, property);
            }
        }
    }

    /// <summary>
    /// Valida se o JSON contém propriedades de auditoria
    /// </summary>
    public void ShouldHaveAuditProperties(JToken json)
    {
        ShouldHaveProperty(json, "id");
        ShouldHaveProperty(json, "data_criacao");
        
        // data_atualizacao pode ser null
        if (json["data_atualizacao"] != null)
        {
            json["data_atualizacao"].Type.Should().BeOneOf(JTokenType.Date, JTokenType.String);
        }
    }

    /// <summary>
    /// Valida formato de data ISO
    /// </summary>
    public void ShouldHaveValidDateFormat(JToken json, string propertyName)
    {
        ShouldHaveProperty(json, propertyName);
        
        var dateValue = json[propertyName]?.ToString();
        if (!string.IsNullOrEmpty(dateValue))
        {
            DateTime.TryParse(dateValue, out _).Should().BeTrue(
                $"Property '{propertyName}' should have valid date format");
        }
    }

    /// <summary>
    /// Valida se o valor é um número
    /// </summary>
    public void ShouldBeNumber(JToken json, string propertyName)
    {
        ShouldHaveProperty(json, propertyName);
        
        var token = json[propertyName];
        token?.Type.Should().BeOneOf(new[] { JTokenType.Integer, JTokenType.Float }, 
            $"Property '{propertyName}' should be a number");
    }

    /// <summary>
    /// Valida se o valor é uma string não vazia
    /// </summary>
    public void ShouldBeNonEmptyString(JToken json, string propertyName)
    {
        ShouldHaveProperty(json, propertyName);
        
        var value = json[propertyName]?.ToString();
        value.Should().NotBeNullOrWhiteSpace($"Property '{propertyName}' should be a non-empty string");
    }

    /// <summary>
    /// Valida se o valor é um booleano
    /// </summary>
    public void ShouldBeBoolean(JToken json, string propertyName)
    {
        ShouldHaveProperty(json, propertyName);
        
        var token = json[propertyName];
        token?.Type.Should().Be(JTokenType.Boolean, $"Property '{propertyName}' should be a boolean");
    }

    /// <summary>
    /// Valida estrutura de resposta de lista paginada
    /// </summary>
    public async Task<JArray> ShouldHavePagedListStructureAsync(HttpResponseMessage response)
    {
        var json = await ShouldHaveSuccessStructureAsync(response);
        ShouldHavePaginationStructure(json);
        
        var items = (JArray)json["items"]!;
        return items;
    }

    /// <summary>
    /// Valida se a resposta contém um item específico por ID
    /// </summary>
    public void ShouldContainItemWithId(JArray array, int expectedId)
    {
        var item = array.FirstOrDefault(i => i["id"]?.Value<int>() == expectedId);
        item.Should().NotBeNull($"Array should contain item with id {expectedId}");
    }

    /// <summary>
    /// Valida se a resposta não contém um item específico por ID
    /// </summary>
    public void ShouldNotContainItemWithId(JArray array, int expectedId)
    {
        var item = array.FirstOrDefault(i => i["id"]?.Value<int>() == expectedId);
        item.Should().BeNull($"Array should not contain item with id {expectedId}");
    }
}