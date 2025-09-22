using System.Net;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Agriis.Tests.Shared.Base;
using Agriis.Tests.Shared.Matchers;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para o módulo de Autenticação
/// Migrado de test_auth.py
/// </summary>
public class TestAuth : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly JsonMatchers _jsonMatchers;

    public TestAuth(TestWebApplicationFactory factory) : base(factory)
    {
        _jsonMatchers = new JsonMatchers();
    }

    [Fact]
    public async Task Test_Authentication_Produtor_Mobile()
    {
        ClearAuthentication();

        var requestData = new
        {
            code = "+5562986442638,wilsontads@gmail.com",
            grant_type = "access_token"
        };

        // Teste normal - deve autenticar com sucesso
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "PRODUTOR_MOBILE_TOKEN");

        var response = await PostAsync("oauth/token", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura da resposta de autenticação
        _jsonMatchers.ShouldHaveProperty(obj, "access_token");
        _jsonMatchers.ShouldHaveProperty(obj, "refresh_token");
        _jsonMatchers.ShouldHaveProperty(obj, "token_type");
        _jsonMatchers.ShouldHaveProperty(obj, "expires_in");

        // Teste com código inválido
        var invalidRequestData = new
        {
            code = "invalid_code",
            grant_type = "access_token"
        };

        var invalidResponse = await PostAsync("oauth/token", invalidRequestData);
        _jsonMatchers.ShouldHaveStatusCode(invalidResponse, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Refresh_Token()
    {
        ClearAuthentication();

        var requestData = new
        {
            code = "+5562986442638,wilsontads@gmail.com",
            grant_type = "access_token"
        };

        // Autentica para pegar refresh token
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "PRODUTOR_MOBILE_TOKEN");

        var authResponse = await PostAsync("oauth/token", requestData);
        _jsonMatchers.ShouldHaveStatusCode(authResponse, HttpStatusCode.OK);

        var authJson = await _jsonMatchers.ShouldHaveValidJsonAsync(authResponse);
        var refreshToken = authJson["refresh_token"]?.ToString();
        refreshToken.Should().NotBeNullOrEmpty();

        // Faz o refresh token corretamente
        var refreshRequestData = new
        {
            refresh_token = refreshToken,
            grant_type = "refresh_token"
        };

        var refreshResponse = await PostAsync("oauth/token", refreshRequestData);
        _jsonMatchers.ShouldHaveStatusCode(refreshResponse, HttpStatusCode.OK);

        var refreshJson = await _jsonMatchers.ShouldHaveValidJsonAsync(refreshResponse);
        var newAccessToken = refreshJson["access_token"]?.ToString();
        newAccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Test_Refresh_Token_Expired()
    {
        ClearAuthentication();

        // Cria um refresh token expirado
        var expiredToken = CreateExpiredToken();

        var refreshRequestData = new
        {
            refresh_token = expiredToken,
            grant_type = "refresh_token"
        };

        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "PRODUTOR_MOBILE_TOKEN");

        var response = await PostAsync("oauth/token", refreshRequestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Check_User()
    {
        ClearAuthentication();

        var queryParams = "code=+5562986442638,wilsontads@gmail.com";

        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "PRODUTOR_MOBILE_TOKEN");

        // Teste normal
        var response = await GetAsync($"oauth/check_user?{queryParams}");
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        // Teste com código inválido
        var invalidQueryParams = "code=invalid_code";
        var invalidResponse = await GetAsync($"oauth/check_user?{invalidQueryParams}");
        _jsonMatchers.ShouldHaveStatusCode(invalidResponse, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Authentication_Fornecedor_Web()
    {
        ClearAuthentication();

        var requestData = new
        {
            email = "email@exemplo.com",
            grant_type = "access_token"
        };

        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "FORNECEDOR_WEB_TOKEN");

        var response = await PostAsync("oauth/fornecedor_web/token", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.OK);

        var json = await _jsonMatchers.ShouldHaveValidJsonAsync(response);
        var obj = _jsonMatchers.ShouldBeObject(json);

        // Validar estrutura da resposta
        _jsonMatchers.ShouldHaveProperty(obj, "access_token");
        _jsonMatchers.ShouldHaveProperty(obj, "refresh_token");
        _jsonMatchers.ShouldHaveProperty(obj, "token_type");
        _jsonMatchers.ShouldHaveProperty(obj, "expires_in");
    }

    [Fact]
    public async Task Test_Check_Serpro()
    {
        ClearAuthentication();

        var cpf = "178.021.131-72"; // CPF de teste
        var celular = DataGenerator.GerarCelular();
        var email = DataGenerator.GerarEmail();

        // Simular dados de upload de foto (em um teste real seria um arquivo)
        var requestData = new
        {
            code = $"{celular},{email}",
            cpf = cpf,
            celular = celular,
            nome = DataGenerator.GerarNome()
        };

        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "PRODUTOR_MOBILE_TOKEN");

        // Nota: Este teste simula o upload de foto, mas em um ambiente real
        // seria necessário implementar o upload de arquivo multipart/form-data
        var response = await PostAsync("oauth/check_serpro/", requestData);
        
        // O status pode variar dependendo da implementação do SERPRO
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, 
            HttpStatusCode.BadRequest, 
            HttpStatusCode.UnprocessableEntity
        );
    }

    [Fact]
    public async Task Test_Invalid_Grant_Type()
    {
        ClearAuthentication();

        var requestData = new
        {
            code = "+5562986442638,wilsontads@gmail.com",
            grant_type = "invalid_grant_type"
        };

        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "PRODUTOR_MOBILE_TOKEN");

        var response = await PostAsync("oauth/token", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    [Fact]
    public async Task Test_Missing_Authorization_Header()
    {
        ClearAuthentication();

        var requestData = new
        {
            code = "+5562986442638,wilsontads@gmail.com",
            grant_type = "access_token"
        };

        // Não define Authorization header
        var response = await PostAsync("oauth/token", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Invalid_Basic_Token()
    {
        ClearAuthentication();

        var requestData = new
        {
            code = "+5562986442638,wilsontads@gmail.com",
            grant_type = "access_token"
        };

        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "INVALID_TOKEN");

        var response = await PostAsync("oauth/token", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Test_Token_Validation()
    {
        // Testa se o token gerado é válido
        var token = await UserAuth.GetTokenAsync("PRODUTOR");
        token.Should().NotBeNullOrEmpty();

        // Valida estrutura do JWT
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        jsonToken.Should().NotBeNull();
        jsonToken.Claims.Should().NotBeEmpty();
        
        // Verifica claims obrigatórias
        jsonToken.Claims.Should().Contain(c => c.Type == "user_id");
        jsonToken.Claims.Should().Contain(c => c.Type == "role");
    }

    [Fact]
    public async Task Test_Token_Expiration()
    {
        var token = await UserAuth.GetTokenAsync("PRODUTOR");
        
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        // Verifica se o token tem data de expiração
        jsonToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
        
        // Verifica se expira em aproximadamente 1 hora (configuração de teste)
        var expectedExpiration = DateTime.UtcNow.AddHours(1);
        jsonToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task Test_Different_User_Roles()
    {
        // Testa tokens para diferentes roles
        var produtorToken = await UserAuth.GetTokenAsync("PRODUTOR");
        var fornecedorToken = await UserAuth.GetTokenAsync("FORNECEDOR");
        var adminToken = await UserAuth.GetTokenAsync("ADMIN");

        var handler = new JwtSecurityTokenHandler();
        
        var produtorJwt = handler.ReadJwtToken(produtorToken);
        var fornecedorJwt = handler.ReadJwtToken(fornecedorToken);
        var adminJwt = handler.ReadJwtToken(adminToken);

        // Verifica roles específicas
        produtorJwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "PRODUTOR");
        fornecedorJwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "FORNECEDOR");
        adminJwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "ADMIN");
    }

    [Fact]
    public async Task Test_Authentication_With_Invalid_Credentials()
    {
        ClearAuthentication();

        var requestData = new
        {
            email = "usuario@inexistente.com",
            password = "senha_incorreta",
            grant_type = "password"
        };

        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "FORNECEDOR_WEB_TOKEN");

        var response = await PostAsync("oauth/fornecedor_web/token", requestData);
        _jsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Unauthorized);
        await _jsonMatchers.ShouldHaveErrorStructureAsync(response);
    }

    /// <summary>
    /// Cria um token JWT expirado para testes
    /// </summary>
    private string CreateExpiredToken()
    {
        var key = "test-key-with-at-least-32-characters-for-security";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "test-issuer",
            audience: "test-audience",
            expires: DateTime.UtcNow.AddSeconds(-1), // Expirado há 1 segundo
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}