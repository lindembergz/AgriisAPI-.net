using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Enderecos.Dominio.Entidades;

namespace Agriis.Enderecos.Dominio.Interfaces;

/// <summary>
/// Interface do repositório de Endereços
/// </summary>
public interface IEnderecoRepository : IRepository<Endereco>
{
    /// <summary>
    /// Obtém endereços por CEP
    /// </summary>
    /// <param name="cep">CEP (com ou sem formatação)</param>
    /// <returns>Lista de endereços com o CEP especificado</returns>
    Task<IEnumerable<Endereco>> ObterPorCepAsync(string cep);
    
    /// <summary>
    /// Obtém endereços por município
    /// </summary>
    /// <param name="municipioId">ID do município</param>
    /// <returns>Lista de endereços do município</returns>
    Task<IEnumerable<Endereco>> ObterPorMunicipioAsync(int municipioId);
    
    /// <summary>
    /// Obtém endereços por estado
    /// </summary>
    /// <param name="estadoId">ID do estado</param>
    /// <returns>Lista de endereços do estado</returns>
    Task<IEnumerable<Endereco>> ObterPorEstadoAsync(int estadoId);
    
    /// <summary>
    /// Busca endereços por logradouro
    /// </summary>
    /// <param name="logradouro">Logradouro ou parte do logradouro</param>
    /// <param name="municipioId">ID do município (opcional)</param>
    /// <returns>Lista de endereços encontrados</returns>
    Task<IEnumerable<Endereco>> BuscarPorLogradouroAsync(string logradouro, int? municipioId = null);
    
    /// <summary>
    /// Busca endereços por bairro
    /// </summary>
    /// <param name="bairro">Bairro ou parte do bairro</param>
    /// <param name="municipioId">ID do município (opcional)</param>
    /// <returns>Lista de endereços encontrados</returns>
    Task<IEnumerable<Endereco>> BuscarPorBairroAsync(string bairro, int? municipioId = null);
    
    /// <summary>
    /// Obtém endereços próximos a uma localização
    /// </summary>
    /// <param name="latitude">Latitude de referência</param>
    /// <param name="longitude">Longitude de referência</param>
    /// <param name="raioKm">Raio de busca em quilômetros</param>
    /// <param name="limite">Número máximo de resultados</param>
    /// <returns>Lista de endereços próximos ordenados por distância</returns>
    Task<IEnumerable<Endereco>> ObterProximosAsync(double latitude, double longitude, double raioKm, int limite = 10);
    
    /// <summary>
    /// Obtém endereços que possuem localização específica definida
    /// </summary>
    /// <param name="municipioId">ID do município (opcional)</param>
    /// <returns>Lista de endereços com localização específica</returns>
    Task<IEnumerable<Endereco>> ObterComLocalizacaoAsync(int? municipioId = null);
    
    /// <summary>
    /// Calcula a distância entre dois endereços
    /// </summary>
    /// <param name="enderecoOrigemId">ID do endereço de origem</param>
    /// <param name="enderecoDestinoId">ID do endereço de destino</param>
    /// <returns>Distância em quilômetros ou null se algum endereço não tiver localização</returns>
    Task<double?> CalcularDistanciaAsync(int enderecoOrigemId, int enderecoDestinoId);
    
    /// <summary>
    /// Verifica se existe um endereço com os dados especificados
    /// </summary>
    /// <param name="cep">CEP</param>
    /// <param name="logradouro">Logradouro</param>
    /// <param name="numero">Número</param>
    /// <param name="municipioId">ID do município</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteEnderecoAsync(string cep, string logradouro, string? numero, int municipioId);
}