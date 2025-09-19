using Agriis.Compartilhado.Dominio.Entidades;
using NetTopologySuite.Geometries;

namespace Agriis.Enderecos.Dominio.Entidades;

/// <summary>
/// Entidade que representa um município brasileiro
/// </summary>
public class Municipio : EntidadeBase
{
    /// <summary>
    /// Nome do município
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// Código IBGE do município
    /// </summary>
    public int CodigoIbge { get; private set; }
    
    /// <summary>
    /// CEP principal do município
    /// </summary>
    public string? CepPrincipal { get; private set; }
    
    /// <summary>
    /// Latitude do centro do município
    /// </summary>
    public double? Latitude { get; private set; }
    
    /// <summary>
    /// Longitude do centro do município
    /// </summary>
    public double? Longitude { get; private set; }
    
    /// <summary>
    /// Ponto geográfico do centro do município (PostGIS)
    /// </summary>
    public Point? Localizacao { get; private set; }
    
    /// <summary>
    /// ID do estado ao qual o município pertence
    /// </summary>
    public int EstadoId { get; private set; }
    
    /// <summary>
    /// Estado ao qual o município pertence
    /// </summary>
    public virtual Estado Estado { get; private set; } = null!;
    
    /// <summary>
    /// Endereços do município
    /// </summary>
    public virtual ICollection<Endereco> Enderecos { get; private set; } = new List<Endereco>();
    
    /// <summary>
    /// Construtor protegido para o Entity Framework
    /// </summary>
    protected Municipio() { }
    
    /// <summary>
    /// Construtor para criar um novo município
    /// </summary>
    /// <param name="nome">Nome do município</param>
    /// <param name="codigoIbge">Código IBGE do município</param>
    /// <param name="estadoId">ID do estado</param>
    /// <param name="cepPrincipal">CEP principal do município</param>
    /// <param name="latitude">Latitude do centro do município</param>
    /// <param name="longitude">Longitude do centro do município</param>
    public Municipio(string nome, int codigoIbge, int estadoId, string? cepPrincipal = null, 
                    double? latitude = null, double? longitude = null)
    {
        ValidarParametros(nome, codigoIbge, estadoId);
        
        Nome = nome;
        CodigoIbge = codigoIbge;
        EstadoId = estadoId;
        CepPrincipal = cepPrincipal;
        
        DefinirLocalizacao(latitude, longitude);
    }
    
    /// <summary>
    /// Atualiza os dados do município
    /// </summary>
    /// <param name="nome">Nome do município</param>
    /// <param name="codigoIbge">Código IBGE do município</param>
    /// <param name="cepPrincipal">CEP principal do município</param>
    /// <param name="latitude">Latitude do centro do município</param>
    /// <param name="longitude">Longitude do centro do município</param>
    public void Atualizar(string nome, int codigoIbge, string? cepPrincipal = null, 
                         double? latitude = null, double? longitude = null)
    {
        ValidarParametros(nome, codigoIbge, EstadoId);
        
        Nome = nome;
        CodigoIbge = codigoIbge;
        CepPrincipal = cepPrincipal;
        
        DefinirLocalizacao(latitude, longitude);
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Define a localização geográfica do município
    /// </summary>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    public void DefinirLocalizacao(double? latitude, double? longitude)
    {
        if (latitude.HasValue && longitude.HasValue)
        {
            ValidarCoordenadas(latitude.Value, longitude.Value);
            
            Latitude = latitude.Value;
            Longitude = longitude.Value;
            
            // Criar ponto geográfico para PostGIS (SRID 4326 = WGS84)
            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            Localizacao = geometryFactory.CreatePoint(new Coordinate(longitude.Value, latitude.Value));
        }
        else
        {
            Latitude = null;
            Longitude = null;
            Localizacao = null;
        }
        
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Calcula a distância em quilômetros para outro município
    /// </summary>
    /// <param name="outroMunicipio">Município de destino</param>
    /// <returns>Distância em quilômetros ou null se algum município não tiver localização</returns>
    public double? CalcularDistanciaKm(Municipio outroMunicipio)
    {
        if (Localizacao == null || outroMunicipio.Localizacao == null)
            return null;
            
        // Usar a função de distância do PostGIS (em metros, converter para km)
        var distanciaMetros = Localizacao.Distance(outroMunicipio.Localizacao);
        return distanciaMetros / 1000.0;
    }
    
    /// <summary>
    /// Verifica se o município possui localização definida
    /// </summary>
    /// <returns>True se possui latitude e longitude</returns>
    public bool PossuiLocalizacao()
    {
        return Latitude.HasValue && Longitude.HasValue && Localizacao != null;
    }
    
    private static void ValidarParametros(string nome, int codigoIbge, int estadoId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do município é obrigatório", nameof(nome));
            
        if (codigoIbge <= 0)
            throw new ArgumentException("Código IBGE deve ser maior que zero", nameof(codigoIbge));
            
        if (estadoId <= 0)
            throw new ArgumentException("ID do estado deve ser maior que zero", nameof(estadoId));
    }
    
    private static void ValidarCoordenadas(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude deve estar entre -90 e 90 graus", nameof(latitude));
            
        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude deve estar entre -180 e 180 graus", nameof(longitude));
    }
}