using Microsoft.EntityFrameworkCore;
using Agriis.Api.Contexto;
using Agriis.Tests.Shared.Generators;

namespace Agriis.Tests.Shared.Helpers;

/// <summary>
/// Helper para operações com banco de dados em testes
/// </summary>
public class DatabaseHelper
{
    private readonly AgriisDbContext _context;
    private readonly TestDataGenerator _dataGenerator;

    public DatabaseHelper(AgriisDbContext context)
    {
        _context = context;
        _dataGenerator = new TestDataGenerator();
    }

    /// <summary>
    /// Limpa todas as tabelas do banco
    /// </summary>
    public async Task ClearAllTablesAsync()
    {
        // Ordem de limpeza respeitando foreign keys
        var tableNames = new[]
        {
            "ComboCategoriaDescontos", "ComboLocalRecebimentos", "ComboItens", "Combos",
            "Propostas", "PedidoItemTransportes", "PedidoItens", "Pedidos",
            "CatalogoItens", "Catalogos", "ProdutosCulturas", "Produtos",
            "UsuariosFornecedoresTerritorio", "UsuariosFornecedores", "PontosDistribuicao", "Fornecedores",
            "PropriedadesCulturas", "Talhoes", "Propriedades", "UsuariosProdutores", "Produtores",
            "GruposSegmentacao", "Segmentacoes", "CulturaFormasPagamento", "FormasPagamento",
            "Usuarios", "Culturas", "Safras", "Enderecos", "Municipios", "Estados",
            "AuditoriaTentativaAcessos", "TentativaAcessoSerpro", "InstitucionalIfarmer"
        };

        foreach (var tableName in tableNames)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync($"DELETE FROM \"{tableName}\"");
            }
            catch (Exception)
            {
                // Ignora erros de tabelas que não existem
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Reseta sequências de ID
    /// </summary>
    public async Task ResetSequencesAsync()
    {
        var sequences = new[]
        {
            "Estados_Id_seq", "Municipios_Id_seq", "Enderecos_Id_seq",
            "Usuarios_Id_seq", "Produtores_Id_seq", "Fornecedores_Id_seq",
            "Culturas_Id_seq", "Safras_Id_seq", "Produtos_Id_seq",
            "Catalogos_Id_seq", "Pedidos_Id_seq", "Propostas_Id_seq"
        };

        foreach (var sequence in sequences)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync($"ALTER SEQUENCE \"{sequence}\" RESTART WITH 1");
            }
            catch (Exception)
            {
                // Ignora erros de sequências que não existem
            }
        }
    }

    /// <summary>
    /// Executa uma ação em uma transação que será revertida
    /// </summary>
    public async Task ExecuteInRollbackTransactionAsync(Func<Task> action)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
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
    /// Verifica se uma entidade existe
    /// </summary>
    public async Task<bool> ExistsAsync<T>(int id) where T : class
    {
        return await _context.Set<T>().FindAsync(id) != null;
    }

    /// <summary>
    /// Conta registros de uma entidade
    /// </summary>
    public async Task<int> CountAsync<T>() where T : class
    {
        return await _context.Set<T>().CountAsync();
    }

    /// <summary>
    /// Obtém o primeiro registro de uma entidade
    /// </summary>
    public async Task<T?> GetFirstAsync<T>() where T : class
    {
        return await _context.Set<T>().FirstOrDefaultAsync();
    }

    /// <summary>
    /// Obtém um registro por ID
    /// </summary>
    public async Task<T?> GetByIdAsync<T>(int id) where T : class
    {
        return await _context.Set<T>().FindAsync(id);
    }

    /// <summary>
    /// Adiciona uma entidade ao contexto
    /// </summary>
    public async Task<T> AddAsync<T>(T entity) where T : class
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Adiciona múltiplas entidades ao contexto
    /// </summary>
    public async Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class
    {
        _context.Set<T>().AddRange(entities);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Remove uma entidade por ID
    /// </summary>
    public async Task<bool> RemoveAsync<T>(int id) where T : class
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity == null) return false;

        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Executa SQL raw
    /// </summary>
    public async Task ExecuteSqlAsync(string sql, params object[] parameters)
    {
        await _context.Database.ExecuteSqlRawAsync(sql, parameters);
    }

    /// <summary>
    /// Força a sincronização com o banco
    /// </summary>
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Recarrega uma entidade do banco
    /// </summary>
    public async Task ReloadAsync<T>(T entity) where T : class
    {
        await _context.Entry(entity).ReloadAsync();
    }

    /// <summary>
    /// Desanexa uma entidade do contexto
    /// </summary>
    public void Detach<T>(T entity) where T : class
    {
        _context.Entry(entity).State = EntityState.Detached;
    }
}