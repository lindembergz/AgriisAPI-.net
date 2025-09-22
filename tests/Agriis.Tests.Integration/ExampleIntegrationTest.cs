using System.Net;
using Agriis.Tests.Shared.Base;
using FluentAssertions;
using Xunit;

namespace Agriis.Tests.Integration;

/// <summary>
/// Exemplo de teste de integração usando a infraestrutura base
/// </summary>
public class ExampleIntegrationTest : BaseTestCase
{
    public ExampleIntegrationTest(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Get_HealthCheck_ShouldReturnOk()
    {
        // Act
        var response = await GetAsync("/health");

        // Assert
        JsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_ProtectedEndpoint_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await GetAsync("/api/produtores");

        // Assert
        JsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_ProtectedEndpoint_WithAuth_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsProducerAsync();

        // Act
        var response = await GetAsync("/api/produtores");

        // Assert
        JsonMatchers.ShouldBeSuccessful(response);
        var json = await JsonMatchers.ShouldHaveValidJsonAsync(response);
        JsonMatchers.ShouldHavePaginationStructure(json);
    }

    [Fact]
    public async Task Post_CreateEntity_ShouldWork()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        
        var newEntity = new
        {
            nome = DataGenerator.GerarNome(),
            email = DataGenerator.GerarEmail(),
            cpf = DataGenerator.GerarCpf()
        };

        // Act
        var response = await PostAsync("/api/usuarios", newEntity);

        // Assert
        JsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);
        var json = await JsonMatchers.ShouldHaveValidJsonAsync(response);
        JsonMatchers.ShouldHaveAuditProperties(json);
        JsonMatchers.ShouldHavePropertyWithValue(json, "nome", newEntity.nome);
        JsonMatchers.ShouldHavePropertyWithValue(json, "email", newEntity.email);
    }

    [Fact]
    public async Task DatabaseOperations_ShouldWork()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        // Verifica que o banco está limpo
        var initialCount = await CountInDatabaseAsync<object>(); // Substitua por uma entidade real
        initialCount.Should().Be(0);

        // Act & Assert - operações com banco funcionam
        var exists = await ExistsInDatabaseAsync<object>(1); // Substitua por uma entidade real
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DataGenerator_ShouldGenerateValidData()
    {
        // Act
        var cpf = DataGenerator.GerarCpf();
        var cnpj = DataGenerator.GerarCnpj();
        var nome = DataGenerator.GerarNome();
        var email = DataGenerator.GerarEmail();
        var endereco = DataGenerator.GerarEndereco();

        // Assert
        cpf.Should().NotBeNullOrEmpty();
        cpf.Should().HaveLength(11);
        
        cnpj.Should().NotBeNullOrEmpty();
        cnpj.Should().HaveLength(14);
        
        nome.Should().NotBeNullOrEmpty();
        email.Should().Contain("@");
        
        endereco.Should().NotBeNull();
        endereco.Logradouro.Should().NotBeNullOrEmpty();
        endereco.Cep.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UserAuth_ShouldGenerateValidTokens()
    {
        // Act
        var produtorToken = await UserAuth.GetTokenAsync("PRODUTOR");
        var fornecedorToken = await UserAuth.GetTokenAsync("FORNECEDOR");
        var adminToken = await UserAuth.GetTokenAsync("ADMIN");

        // Assert
        produtorToken.Should().NotBeNullOrEmpty();
        fornecedorToken.Should().NotBeNullOrEmpty();
        adminToken.Should().NotBeNullOrEmpty();

        UserAuth.ValidateToken(produtorToken).Should().BeTrue();
        UserAuth.ValidateToken(fornecedorToken).Should().BeTrue();
        UserAuth.ValidateToken(adminToken).Should().BeTrue();
    }

    [Fact]
    public async Task JsonMatchers_ShouldValidateStructures()
    {
        // Arrange
        await AuthenticateAsProducerAsync();

        // Act
        var response = await GetAsync("/api/culturas");

        // Assert
        JsonMatchers.ShouldBeSuccessful(response);
        var json = await JsonMatchers.ShouldHaveValidJsonAsync(response);
        
        if (json.Type == Newtonsoft.Json.Linq.JTokenType.Array)
        {
            var array = JsonMatchers.ShouldBeArray(json);
            // Se houver itens, valida estrutura
            if (array.Count > 0)
            {
                JsonMatchers.AllItemsShouldHaveRequiredProperties(array, "id", "nome");
            }
        }
        else
        {
            // Se for objeto paginado
            JsonMatchers.ShouldHavePaginationStructure(json);
        }
    }
}