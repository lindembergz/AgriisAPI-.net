using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Enderecos.Dominio.Entidades;

namespace Agriis.Enderecos.Dominio.Interfaces;

/// <summary>
/// Interface do repositório de Municípios
/// </summary>
public interface IMunicipioRepository : IRepository<Municipio>
{
    /// <summary>
    /// Obtém um município pelo código IBGE
    /// </summary>
    /// <param name="codigoIbge">Código IBGE do município</param>
    /// <returns>Município encontrado ou null</returns>
    Task<Municipio?> ObterPorCodigoIbgeAsync(int codigoIbge);
    
    /// <summary>
    /// Obtém municípios por estado
    /// </summary>
    /// <param name="estadoId">ID do estado</param>
    /// <returns>Lista de municípios do estado</returns>
    Task<IEnumerable<Municipio>> ObterPorEstadoAsync(int estadoId);
    
    /// <summary>
    /// Obtém municípios por UF
    /// </summary>
    /// <param name="uf">Sigla do estado</param>
    /// <returns>Lista de municípios do estado</returns>
    Task<IEnumerable<Municipio>> ObterPorUfAsync(string uf);
    
    /// <summary>
    /// Busca municípios por nome (busca parcial)
    /// </summary>
    /// <param name="nome">Nome ou parte do nome do município</param>
    /// <param name="estadoId">ID do estado (opcional)</param>
    /// <returns>Lista de municípios encontrados</returns>
    Task<IEnumerable<Municipio>> BuscarPorNomeAsync(string nome, int? estadoId = null);
    
    /// <summary>
    /// Obtém municípios próximos a uma localização
    /// </summary>
    /// <param name="latitude">Latitude de referência</param>
    /// <param name="longitude">Longitude de referência</param>
    /// <param name="raioKm">Raio de busca em quilômetros</param>
    /// <param name="limite">Número máximo de resultados</param>
    /// <returns>Lista de municípios próximos ordenados por distância</returns>
    Task<IEnumerable<Municipio>> ObterProximosAsync(double latitude, double longitude, double raioKm, int limite = 10);
    
    /// <summary>
    /// Obtém municípios que possuem localização definida
    /// </summary>
    /// <param name="estadoId">ID do estado (opcional)</param>
    /// <returns>Lista de municípios com localização</returns>
    Task<IEnumerable<Municipio>> ObterComLocalizacaoAsync(int? estadoId = null);
    
    /// <summary>
    /// Calcula a distância entre dois municípios
    /// </summary>
    /// <param name="municipioOrigemId">ID do município de origem</param>
    /// <param name="municipioDestinoId">ID do município de destino</param>
    /// <returns>Distância em quilômetros ou null se algum município não tiver localização</returns>
    Task<double?> CalcularDistanciaAsync(int municipioOrigemId, int municipioDestinoId);
    
    /// <summary>
    /// Verifica se existe um município com o código IBGE especificado
    /// </summary>
    /// <param name="codigoIbge">Código IBGE</param>
    /// <returns>True se existe</returns>
    Task<bool> ExistePorCodigoIbgeAsync(int codigoIbge);
}