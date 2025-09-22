using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Agriis.Api;
using Agriis.Api.Contexto;
using Agriis.Tests.Shared.Authentication;
using Agriis.Tests.Shared.Generators;
using Agriis.Tests.Shared.Matchers;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Xunit;

namespace Agriis.Tests.Shared.Base;

/// <summary>
/// Classe base para todos os testes, equivalente ao BaseTestCase do Python
/// Fornece setup de aplicação, autenticação e helpers para testes
/// </summary>
public abstract class BaseTestCase : IDisposable
{
    protected readonly TestWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;
    protected readonly AgriisDbContext DbContext;
    protected readonly TestUserAuth UserAuth;
    protected readonly TestDataGenerator DataGenerator;
    protected readonly JsonMatchers JsonMatchers;

    protected BaseTestCase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<AgriisDbContext>();
        UserAuth = new TestUserAuth(factory);
        DataGenerator = new TestDataGenerator();
        JsonMatchers = new JsonMatchers();
        
        // Configurar cliente HTTP
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Autentica um usuário para os testes
    /// </summary>
    protected async Task AuthenticateAsync(string role = "PRODUTOR", int? userId = null)
    {
        var token = await UserAuth.GetTokenAsync(role, userId);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Autentica como produtor
    /// </summary>
    protected async Task AuthenticateAsProducerAsync(int? produtorId = null)
    {
        await AuthenticateAsync("PRODUTOR", produtorId);
    }

    /// <summary>
    /// Autentica como fornecedor
    /// </summary>
    protected async Task AuthenticateAsSupplierAsync(int? fornecedorId = null)
    {
        await AuthenticateAsync("FORNECEDOR", fornecedorId);
    }

    /// <summary>
    /// Autentica como administrador
    /// </summary>
    protected async Task AuthenticateAsAdminAsync()
    {
        await AuthenticateAsync("ADMIN");
    }

    /// <summary>
    /// Remove autenticação
    /// </summary>
    protected void ClearAuthentication()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Faz uma requisição GET
    /// </summary>
    protected async Task<HttpResponseMessage> GetAsync(string requestUri)
    {
        return await Client.GetAsync(requestUri);
    }

    /// <summary>
    /// Faz uma requisição POST com JSON
    /// </summary>
    protected async Task<HttpResponseMessage> PostAsync(string requestUri, object content)
    {
        var json = JsonConvert.SerializeObject(content);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        return await Client.PostAsync(requestUri, stringContent);
    }

    /// <summary>
    /// Faz uma requisição PUT com JSON
    /// </summary>
    protected async Task<HttpResponseMessage> PutAsync(string requestUri, object content)
    {
        var json = JsonConvert.SerializeObject(content);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        return await Client.PutAsync(requestUri, stringContent);
    }

    /// <summary>
    /// Faz uma requisição DELETE
    /// </summary>
    protected async Task<HttpResponseMessage> DeleteAsync(string requestUri)
    {
        return await Client.DeleteAsync(requestUri);
    }

    /// <summary>
    /// Lê o conteúdo da resposta como string
    /// </summary>
    protected async Task<string> ReadResponseAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Lê o conteúdo da resposta como objeto JSON
    /// </summary>
    protected async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(content)!;
    }

    /// <summary>
    /// Limpa o banco de dados para testes isolados
    /// </summary>
    protected async Task ClearDatabaseAsync()
    {
        // Remove todos os dados das tabelas principais
        var tableNames = new[]
        {
            "Propostas", "PedidoItens", "Pedidos", "CatalogoItens", "Catalogos",
            "ProdutosCulturas", "Produtos", "UsuariosProdutores", "Produtores",
            "UsuariosFornecedores", "Fornecedores", "Propriedades", "Usuarios",
            "Culturas", "Safras", "Estados", "Municipios", "Enderecos"
        };

        foreach (var tableName in tableNames)
        {
            await DbContext.Database.ExecuteSqlRawAsync($"DELETE FROM \"{tableName}\"");
        }

        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Reseta a sequência de IDs para uma tabela
    /// </summary>
    protected async Task ResetSequenceAsync(string tableName, string? sequenceName = null)
    {
        sequenceName ??= $"{tableName}_Id_seq";
        await DbContext.Database.ExecuteSqlRawAsync($"ALTER SEQUENCE \"{sequenceName}\" RESTART WITH 1");
    }

    /// <summary>
    /// Executa uma ação dentro de uma transação que será revertida
    /// </summary>
    protected async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            await action();
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }

    /// <summary>
    /// Verifica se uma entidade existe no banco
    /// </summary>
    protected async Task<bool> ExistsInDatabaseAsync<T>(int id) where T : class
    {
        return await DbContext.Set<T>().FindAsync(id) != null;
    }

    /// <summary>
    /// Conta registros de uma entidade
    /// </summary>
    protected async Task<int> CountInDatabaseAsync<T>() where T : class
    {
        return await DbContext.Set<T>().CountAsync();
    }

    /// <summary>
    /// Extrai o ID do header Location da resposta (equivalente ao get_id_from_header do Python)
    /// </summary>
    protected int GetIdFromLocationHeader(HttpResponseMessage response)
    {
        if (response.Headers.Location == null)
        {
            throw new InvalidOperationException("Response does not contain Location header");
        }

        var location = response.Headers.Location.ToString();
        var segments = location.Split('/');
        var idSegment = segments.LastOrDefault(s => int.TryParse(s, out _));
        
        if (idSegment == null || !int.TryParse(idSegment, out var id))
        {
            throw new InvalidOperationException($"Could not extract ID from Location header: {location}");
        }

        return id;
    }

    public virtual void Dispose()
    {
        Client?.Dispose();
        Scope?.Dispose();
        GC.SuppressFinalize(this);
    }
}