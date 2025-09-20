using Microsoft.EntityFrameworkCore;
using Agriis.Combos.Dominio.Entidades;
using Agriis.Combos.Dominio.Enums;
using Agriis.Combos.Dominio.Interfaces;
using Agriis.Compartilhado.Infraestrutura.Persistencia;

namespace Agriis.Combos.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de combos
/// </summary>
public class ComboRepository : RepositoryBase<Combo, DbContext>, IComboRepository
{
    public ComboRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Combo>> ObterPorFornecedorAsync(int fornecedorId)
    {
        return await DbSet
            .Where(c => c.FornecedorId == fornecedorId)
            .Include(c => c.Itens)
            .Include(c => c.LocaisRecebimento)
            .Include(c => c.CategoriasDesconto)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Combo>> ObterCombosVigentesAsync()
    {
        var agora = DateTime.UtcNow;
        
        return await DbSet
            .Where(c => c.Status == StatusCombo.Ativo && 
                       c.DataInicio <= agora && 
                       c.DataFim >= agora)
            .Include(c => c.Itens)
            .Include(c => c.LocaisRecebimento)
            .Include(c => c.CategoriasDesconto)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Combo>> ObterPorSafraAsync(int safraId)
    {
        return await DbSet
            .Where(c => c.SafraId == safraId)
            .Include(c => c.Itens)
            .Include(c => c.LocaisRecebimento)
            .Include(c => c.CategoriasDesconto)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Combo>> ObterCombosValidosParaProdutorAsync(
        int produtorId, 
        decimal hectareProdutor, 
        int municipioId)
    {
        var agora = DateTime.UtcNow;
        
        return await DbSet
            .Where(c => c.Status == StatusCombo.Ativo && 
                       c.DataInicio <= agora && 
                       c.DataFim >= agora &&
                       c.HectareMinimo <= hectareProdutor &&
                       c.HectareMaximo >= hectareProdutor)
            .Include(c => c.Itens)
            .Include(c => c.LocaisRecebimento)
            .Include(c => c.CategoriasDesconto)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<Combo?> ObterCompletoAsync(int id)
    {
        return await DbSet
            .Where(c => c.Id == id)
            .Include(c => c.Itens)
            .Include(c => c.LocaisRecebimento)
            .Include(c => c.CategoriasDesconto)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExisteComboAtivoAsync(int fornecedorId, int safraId, string nome)
    {
        return await DbSet
            .AnyAsync(c => c.FornecedorId == fornecedorId && 
                          c.SafraId == safraId && 
                          c.Nome == nome && 
                          c.Status == StatusCombo.Ativo);
    }

    public async Task<IEnumerable<Combo>> ObterCombosExpirandoAsync(DateTime dataLimite)
    {
        return await DbSet
            .Where(c => c.Status == StatusCombo.Ativo && 
                       c.DataFim <= dataLimite)
            .Include(c => c.Itens)
            .Include(c => c.LocaisRecebimento)
            .Include(c => c.CategoriasDesconto)
            .ToListAsync();
    }
}