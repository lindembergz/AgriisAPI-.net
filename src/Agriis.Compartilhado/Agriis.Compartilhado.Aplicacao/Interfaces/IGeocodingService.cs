using Agriis.Compartilhado.Aplicacao.DTOs;

namespace Agriis.Compartilhado.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviços de geocodificação
/// </summary>
public interface IGeocodingService
{
    /// <summary>
    /// Obtém informações de endereço através de coordenadas (geocodificação reversa)
    /// </summary>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    /// <returns>Informações do endereço encontrado</returns>
    Task<GeocodingResultDto?> ObterEnderecoPorCoordenadas(double latitude, double longitude);
    
    /// <summary>
    /// Obtém coordenadas através de um endereço (geocodificação direta)
    /// </summary>
    /// <param name="endereco">Endereço completo</param>
    /// <returns>Coordenadas encontradas</returns>
    Task<GeocodingResultDto?> ObterCoordenadasPorEndereco(string endereco);
    
    /// <summary>
    /// Valida se as coordenadas estão dentro do território brasileiro
    /// </summary>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    /// <returns>True se as coordenadas estão no Brasil</returns>
    bool ValidarCoordenadasBrasil(double latitude, double longitude);
}