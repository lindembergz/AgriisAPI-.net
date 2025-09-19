using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.PontosDistribuicao.Dominio.Entidades;
using Agriis.Enderecos.Dominio.Entidades;

namespace Agriis.PontosDistribuicao.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de pontos de distribuição
/// </summary>
public interface IPontoDistribuicaoRepository : IRepository<PontoDistribuicao>
{
    /// <summary>
    /// Obtém pontos de distribuição por fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="apenasAtivos">Se deve retornar apenas pontos ativos</param>
    /// <returns>Lista de pontos de distribuição</returns>
    Task<IEnumerable<PontoDistribuicao>> ObterPorFornecedorAsync(int fornecedorId, bool apenasAtivos = true);
    
    /// <summary>
    /// Obtém pontos de distribuição que atendem um estado específico
    /// </summary>
    /// <param name="estadoId">ID do estado</param>
    /// <param name="apenasAtivos">Se deve retornar apenas pontos ativos</param>
    /// <returns>Lista de pontos de distribuição</returns>
    Task<IEnumerable<PontoDistribuicao>> ObterPorEstadoAsync(int estadoId, bool apenasAtivos = true);
    
    /// <summary>
    /// Obtém pontos de distribuição que atendem um município específico
    /// </summary>
    /// <param name="municipioId">ID do município</param>
    /// <param name="apenasAtivos">Se deve retornar apenas pontos ativos</param>
    /// <returns>Lista de pontos de distribuição</returns>
    Task<IEnumerable<PontoDistribuicao>> ObterPorMunicipioAsync(int municipioId, bool apenasAtivos = true);
    
    /// <summary>
    /// Obtém pontos de distribuição próximos a uma localização
    /// </summary>
    /// <param name="latitude">Latitude da localização</param>
    /// <param name="longitude">Longitude da localização</param>
    /// <param name="raioKm">Raio de busca em quilômetros</param>
    /// <param name="apenasAtivos">Se deve retornar apenas pontos ativos</param>
    /// <returns>Lista de pontos de distribuição ordenados por distância</returns>
    Task<IEnumerable<PontoDistribuicao>> ObterProximosAsync(double latitude, double longitude, 
                                                           double raioKm, bool apenasAtivos = true);
    
    /// <summary>
    /// Obtém pontos de distribuição próximos a um endereço
    /// </summary>
    /// <param name="endereco">Endereço de referência</param>
    /// <param name="raioKm">Raio de busca em quilômetros</param>
    /// <param name="apenasAtivos">Se deve retornar apenas pontos ativos</param>
    /// <returns>Lista de pontos de distribuição ordenados por distância</returns>
    Task<IEnumerable<PontoDistribuicao>> ObterProximosAsync(Endereco endereco, double raioKm, 
                                                           bool apenasAtivos = true);
    
    /// <summary>
    /// Obtém pontos de distribuição próximos a um município
    /// </summary>
    /// <param name="municipio">Município de referência</param>
    /// <param name="raioKm">Raio de busca em quilômetros</param>
    /// <param name="apenasAtivos">Se deve retornar apenas pontos ativos</param>
    /// <returns>Lista de pontos de distribuição ordenados por distância</returns>
    Task<IEnumerable<PontoDistribuicao>> ObterProximosAsync(Municipio municipio, double raioKm, 
                                                           bool apenasAtivos = true);
    
    /// <summary>
    /// Obtém pontos de distribuição que atendem uma localização específica
    /// Considera tanto cobertura territorial quanto raio de cobertura
    /// </summary>
    /// <param name="estadoId">ID do estado</param>
    /// <param name="municipioId">ID do município</param>
    /// <param name="endereco">Endereço específico (opcional)</param>
    /// <param name="apenasAtivos">Se deve retornar apenas pontos ativos</param>
    /// <returns>Lista de pontos de distribuição que atendem a localização</returns>
    Task<IEnumerable<PontoDistribuicao>> ObterQueAtendemLocalizacaoAsync(int estadoId, int municipioId, 
                                                                        Endereco? endereco = null, 
                                                                        bool apenasAtivos = true);
    
    /// <summary>
    /// Calcula distâncias de pontos de distribuição para uma localização
    /// </summary>
    /// <param name="pontosIds">IDs dos pontos de distribuição</param>
    /// <param name="latitude">Latitude da localização</param>
    /// <param name="longitude">Longitude da localização</param>
    /// <returns>Dicionário com ID do ponto e distância em km</returns>
    Task<Dictionary<int, double>> CalcularDistanciasAsync(IEnumerable<int> pontosIds, 
                                                         double latitude, double longitude);
    
    /// <summary>
    /// Verifica se existe ponto de distribuição com o mesmo nome para o fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="nome">Nome do ponto</param>
    /// <param name="pontoId">ID do ponto a excluir da verificação (para edição)</param>
    /// <returns>True se existe ponto com o mesmo nome</returns>
    Task<bool> ExisteComMesmoNomeAsync(int fornecedorId, string nome, int? pontoId = null);
    
    /// <summary>
    /// Obtém estatísticas de pontos de distribuição por fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Estatísticas dos pontos</returns>
    Task<(int Total, int Ativos, int Inativos)> ObterEstatisticasPorFornecedorAsync(int fornecedorId);
}