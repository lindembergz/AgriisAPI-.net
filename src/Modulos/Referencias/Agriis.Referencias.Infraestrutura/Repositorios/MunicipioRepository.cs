using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;
// Usar a entidade do módulo Enderecos para evitar conflitos de tabela
using MunicipioEnderecos = Agriis.Enderecos.Dominio.Entidades.Municipio;
using MunicipioReferencias = Agriis.Referencias.Dominio.Entidades.Municipio;

namespace Agriis.Referencias.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de municípios (usa entidade do módulo Enderecos)
/// </summary>
public class MunicipioRepository : ReferenciaRepositoryBase<MunicipioReferencias, DbContext>, IMunicipioRepository
{
    public MunicipioRepository(DbContext context) : base(context)
    {
    }

    /// <summary>
    /// Converte entidade do módulo Enderecos para módulo Referencias
    /// </summary>
    private MunicipioReferencias ConvertToReferenciasMunicipio(MunicipioEnderecos municipioEnderecos)
    {
        // Usar reflexão para criar a instância já que o construtor é protegido
        var municipio = (MunicipioReferencias)Activator.CreateInstance(typeof(MunicipioReferencias), true)!;
        
        // Definir propriedades usando reflexão
        var idProperty = typeof(MunicipioReferencias).GetProperty("Id");
        var nomeField = typeof(MunicipioReferencias).GetField("Nome", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var codigoIbgeField = typeof(MunicipioReferencias).GetField("CodigoIbge", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var ufIdField = typeof(MunicipioReferencias).GetField("UfId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var ativoField = typeof(MunicipioReferencias).GetField("Ativo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        idProperty?.SetValue(municipio, municipioEnderecos.Id);
        nomeField?.SetValue(municipio, municipioEnderecos.Nome);
        codigoIbgeField?.SetValue(municipio, municipioEnderecos.CodigoIbge);
        ufIdField?.SetValue(municipio, municipioEnderecos.EstadoId); // Mapear EstadoId para UfId
        ativoField?.SetValue(municipio, true); // Assumir ativo por padrão
        
        return municipio;
    }

    /// <summary>
    /// Verifica se existe um município com o nome especificado
    /// </summary>
    public new async Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<MunicipioEnderecos>().Where(m => m.Nome == nome);
        
        if (idExcluir.HasValue)
            query = query.Where(m => m.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se existe um município com o código IBGE especificado
    /// </summary>
    public async Task<bool> ExisteCodigoIbgeAsync(int codigoIbge, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<MunicipioEnderecos>().Where(m => m.CodigoIbge == codigoIbge);
        
        if (idExcluir.HasValue)
            query = query.Where(m => m.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se existe um município com o código IBGE especificado (string - compatibilidade)
    /// </summary>
    public async Task<bool> ExisteCodigoIbgeAsync(string codigoIbge, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        if (int.TryParse(codigoIbge, out int codigo))
        {
            return await ExisteCodigoIbgeAsync(codigo, idExcluir, cancellationToken);
        }
        return false;
    }

    /// <summary>
    /// Verifica se existe um município com o nome especificado na UF
    /// </summary>
    public async Task<bool> ExisteNomeNaUfAsync(string nome, int ufId, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        // Nota: UfId do módulo Referencias corresponde ao EstadoId do módulo Enderecos
        var query = Context.Set<MunicipioEnderecos>().Where(m => m.Nome == nome && m.EstadoId == ufId);
        
        if (idExcluir.HasValue)
            query = query.Where(m => m.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém municípios por UF
    /// </summary>
    public async Task<IEnumerable<MunicipioReferencias>> ObterPorUfAsync(int ufId, CancellationToken cancellationToken = default)
    {
        var municipiosEnderecos = await Context.Set<MunicipioEnderecos>()
            .Include(m => m.Estado)
            .Where(m => m.EstadoId == ufId)
            .OrderBy(m => m.Nome)
            .ToListAsync(cancellationToken);

        return municipiosEnderecos.Select(ConvertToReferenciasMunicipio);
    }

    /// <summary>
    /// Obtém municípios ativos por UF
    /// </summary>
    public async Task<IEnumerable<MunicipioReferencias>> ObterAtivosPorUfAsync(int ufId, CancellationToken cancellationToken = default)
    {
        // Como a entidade Enderecos não tem campo Ativo, retornar todos
        return await ObterPorUfAsync(ufId, cancellationToken);
    }

    /// <summary>
    /// Busca municípios por nome (busca parcial)
    /// </summary>
    public async Task<IEnumerable<MunicipioReferencias>> BuscarPorNomeAsync(string nome, int? ufId = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<MunicipioEnderecos>()
            .Include(m => m.Estado)
            .Where(m => m.Nome.Contains(nome));

        if (ufId.HasValue)
            query = query.Where(m => m.EstadoId == ufId.Value);

        var municipiosEnderecos = await query
            .OrderBy(m => m.Nome)
            .ToListAsync(cancellationToken);

        return municipiosEnderecos.Select(ConvertToReferenciasMunicipio);
    }

    /// <summary>
    /// Busca municípios por código IBGE (busca parcial)
    /// </summary>
    public async Task<IEnumerable<MunicipioReferencias>> BuscarPorCodigoIbgeAsync(string codigoIbgeParcial, int? ufId = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<MunicipioEnderecos>()
            .Where(m => m.CodigoIbge.ToString().Contains(codigoIbgeParcial));

        if (ufId.HasValue)
            query = query.Where(m => m.EstadoId == ufId.Value);

        var municipiosEnderecos = await query
            .OrderBy(m => m.CodigoIbge)
            .ToListAsync(cancellationToken);

        return municipiosEnderecos.Select(ConvertToReferenciasMunicipio);
    }

    /// <summary>
    /// Obtém municípios por região (baseado no código IBGE)
    /// </summary>
    public async Task<IEnumerable<MunicipioReferencias>> ObterPorRegiaoAsync(string prefixoCodigoIbge, CancellationToken cancellationToken = default)
    {
        var municipiosEnderecos = await Context.Set<MunicipioEnderecos>()
            .Include(m => m.Estado)
            .Where(m => m.CodigoIbge.ToString().StartsWith(prefixoCodigoIbge))
            .OrderBy(m => m.Nome)
            .ToListAsync(cancellationToken);

        return municipiosEnderecos.Select(ConvertToReferenciasMunicipio);
    }

    /// <summary>
    /// Obtém um município por código IBGE
    /// </summary>
    public async Task<MunicipioReferencias?> ObterPorCodigoIbgeAsync(int codigoIbge, CancellationToken cancellationToken = default)
    {
        var municipioEnderecos = await Context.Set<MunicipioEnderecos>()
            .Include(m => m.Estado)
            .FirstOrDefaultAsync(m => m.CodigoIbge == codigoIbge, cancellationToken);

        return municipioEnderecos != null ? ConvertToReferenciasMunicipio(municipioEnderecos) : null;
    }

    /// <summary>
    /// Obtém um município por código IBGE (string - compatibilidade)
    /// </summary>
    public async Task<MunicipioReferencias?> ObterPorCodigoIbgeAsync(string codigoIbge, CancellationToken cancellationToken = default)
    {
        if (int.TryParse(codigoIbge, out int codigo))
        {
            return await ObterPorCodigoIbgeAsync(codigo, cancellationToken);
        }
        return null;
    }

    /// <summary>
    /// Obtém um município por ID
    /// </summary>
    public override async Task<MunicipioReferencias?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var municipioEnderecos = await Context.Set<MunicipioEnderecos>()
            .Include(m => m.Estado)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        return municipioEnderecos != null ? ConvertToReferenciasMunicipio(municipioEnderecos) : null;
    }

    /// <summary>
    /// Obtém todos os municípios
    /// </summary>
    public override async Task<IEnumerable<MunicipioReferencias>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var municipiosEnderecos = await Context.Set<MunicipioEnderecos>()
            .OrderBy(m => m.Nome)
            .ToListAsync(cancellationToken);

        return municipiosEnderecos.Select(ConvertToReferenciasMunicipio);
    }

    /// <summary>
    /// Obtém municípios ativos
    /// </summary>
    public override async Task<IEnumerable<MunicipioReferencias>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        // Como a entidade Enderecos não tem campo Ativo, retornar todos
        return await ObterTodosAsync(cancellationToken);
    }

    // Métodos não implementados para operações de escrita (não suportadas neste adaptador)
    public override Task<MunicipioReferencias> AdicionarAsync(MunicipioReferencias entidade, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Operações de escrita devem ser feitas através do módulo Enderecos");
    }

    public override Task AtualizarAsync(MunicipioReferencias entidade, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Operações de escrita devem ser feitas através do módulo Enderecos");
    }

    public override Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Operações de escrita devem ser feitas através do módulo Enderecos");
    }

    protected override System.Linq.Expressions.Expression<Func<MunicipioReferencias, bool>> GetAtivoExpression()
    {
        return m => true; // Sempre ativo já que não temos o campo na entidade Enderecos
    }

    protected override System.Linq.Expressions.Expression<Func<MunicipioReferencias, bool>> GetCodigoExpression(string codigo)
    {
        if (int.TryParse(codigo, out int codigoInt))
            return m => m.CodigoIbge == codigoInt;
        else
            return m => false;
    }

    protected override System.Linq.Expressions.Expression<Func<MunicipioReferencias, bool>> GetNomeExpression(string nome)
    {
        return m => m.Nome == nome;
    }

    protected override void SetAtivo(MunicipioReferencias entidade, bool ativo)
    {
        // Não suportado - entidade Enderecos não tem campo Ativo
    }

    public override async Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        return false; // Não permitir remoção através deste adaptador
    }
}