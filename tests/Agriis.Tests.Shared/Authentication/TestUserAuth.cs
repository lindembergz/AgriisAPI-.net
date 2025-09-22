using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Agriis.Tests.Shared.Base;

namespace Agriis.Tests.Shared.Authentication;

/// <summary>
/// Sistema de autenticação para testes, equivalente ao IUserAuth do Python
/// </summary>
public class TestUserAuth
{
    private readonly TestWebApplicationFactory _factory;
    private readonly Dictionary<string, TestUser> _testUsers;

    // Usuários de teste estáticos para compatibilidade com os testes migrados
    public static readonly TestUserData ProdutorMobileSandbox = new()
    {
        ProdutorId = 1,
        PropriedadeId = 3,
        UserId = 1
    };

    public static readonly TestUserData FornecedorWebSandbox = new()
    {
        FornecedorId = 5,
        ComboId = 1,
        CategoriaId = 3,
        UserId = 2
    };

    public static readonly TestUserData AdminWebSandbox = new()
    {
        UserId = 3
    };

    public TestUserAuth(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _testUsers = new Dictionary<string, TestUser>();
        InitializeTestUsers();
    }

    /// <summary>
    /// Inicializa usuários de teste padrão
    /// </summary>
    private void InitializeTestUsers()
    {
        _testUsers["PRODUTOR"] = new TestUser
        {
            Id = 1,
            Nome = "João Produtor",
            Email = "joao.produtor@test.com",
            Role = "PRODUTOR",
            ProdutorId = 1,
            Cpf = "12345678901"
        };

        _testUsers["FORNECEDOR"] = new TestUser
        {
            Id = 2,
            Nome = "Maria Fornecedora",
            Email = "maria.fornecedora@test.com",
            Role = "FORNECEDOR",
            FornecedorId = 1,
            Cnpj = "12345678000195"
        };

        _testUsers["ADMIN"] = new TestUser
        {
            Id = 3,
            Nome = "Admin Sistema",
            Email = "admin@test.com",
            Role = "ADMIN"
        };

        _testUsers["REPRESENTANTE"] = new TestUser
        {
            Id = 4,
            Nome = "Carlos Representante",
            Email = "carlos.representante@test.com",
            Role = "REPRESENTANTE",
            FornecedorId = 1
        };
    }

    /// <summary>
    /// Gera um token JWT para testes
    /// </summary>
    public Task<string> GetTokenAsync(string role = "PRODUTOR", int? userId = null)
    {
        var user = GetTestUser(role, userId);
        return Task.FromResult(GenerateJwtToken(user));
    }

    /// <summary>
    /// Obtém um usuário de teste
    /// </summary>
    public TestUser GetTestUser(string role = "PRODUTOR", int? userId = null)
    {
        if (!_testUsers.ContainsKey(role))
        {
            throw new ArgumentException($"Role de teste não encontrada: {role}");
        }

        var user = _testUsers[role];
        
        if (userId.HasValue)
        {
            user = user with { Id = userId.Value };
        }

        return user;
    }

    /// <summary>
    /// Cria um usuário de teste customizado
    /// </summary>
    public TestUser CreateCustomUser(string nome, string email, string role, int? produtorId = null, int? fornecedorId = null)
    {
        return new TestUser
        {
            Id = Random.Shared.Next(1000, 9999),
            Nome = nome,
            Email = email,
            Role = role,
            ProdutorId = produtorId,
            FornecedorId = fornecedorId
        };
    }

    /// <summary>
    /// Gera token JWT para um usuário específico
    /// </summary>
    public string GenerateJwtToken(TestUser user)
    {
        var key = "test-key-with-at-least-32-characters-for-security";
        var issuer = "test-issuer";
        var audience = "test-audience";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Nome),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("user_id", user.Id.ToString())
        };

        // Adiciona claims específicas por role
        if (user.ProdutorId.HasValue)
        {
            claims.Add(new Claim("produtor_id", user.ProdutorId.Value.ToString()));
        }

        if (user.FornecedorId.HasValue)
        {
            claims.Add(new Claim("fornecedor_id", user.FornecedorId.Value.ToString()));
        }

        if (!string.IsNullOrEmpty(user.Cpf))
        {
            claims.Add(new Claim("cpf", user.Cpf));
        }

        if (!string.IsNullOrEmpty(user.Cnpj))
        {
            claims.Add(new Claim("cnpj", user.Cnpj));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Valida se um token é válido
    /// </summary>
    public bool ValidateToken(string token)
    {
        try
        {
            var key = "test-key-with-at-least-32-characters-for-security";
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = true,
                ValidIssuer = "test-issuer",
                ValidateAudience = true,
                ValidAudience = "test-audience",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extrai claims de um token
    /// </summary>
    public ClaimsPrincipal GetClaimsFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(token);
        
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        return new ClaimsPrincipal(identity);
    }
}

/// <summary>
/// Representa um usuário de teste
/// </summary>
public record TestUser
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public int? ProdutorId { get; init; }
    public int? FornecedorId { get; init; }
    public string? Cpf { get; init; }
    public string? Cnpj { get; init; }
}

/// <summary>
/// Dados de usuário de teste para compatibilidade com testes migrados
/// </summary>
public class TestUserData
{
    public int? ProdutorId { get; init; }
    public int? FornecedorId { get; init; }
    public int? PropriedadeId { get; init; }
    public int? ComboId { get; init; }
    public int? CategoriaId { get; init; }
    public int UserId { get; init; }
}