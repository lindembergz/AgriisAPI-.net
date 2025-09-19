using Agriis.Compartilhado.Dominio.Entidades;
using NetTopologySuite.Geometries;

namespace Agriis.Enderecos.Dominio.Entidades;

/// <summary>
/// Entidade que representa um endereço completo
/// </summary>
public class Endereco : EntidadeBase
{
    /// <summary>
    /// CEP do endereço
    /// </summary>
    public string Cep { get; private set; } = string.Empty;
    
    /// <summary>
    /// Logradouro (rua, avenida, etc.)
    /// </summary>
    public string Logradouro { get; private set; } = string.Empty;
    
    /// <summary>
    /// Número do endereço
    /// </summary>
    public string? Numero { get; private set; }
    
    /// <summary>
    /// Complemento do endereço
    /// </summary>
    public string? Complemento { get; private set; }
    
    /// <summary>
    /// Bairro
    /// </summary>
    public string Bairro { get; private set; } = string.Empty;
    
    /// <summary>
    /// Latitude específica do endereço
    /// </summary>
    public double? Latitude { get; private set; }
    
    /// <summary>
    /// Longitude específica do endereço
    /// </summary>
    public double? Longitude { get; private set; }
    
    /// <summary>
    /// Ponto geográfico específico do endereço (PostGIS)
    /// </summary>
    public Point? Localizacao { get; private set; }
    
    /// <summary>
    /// ID do município
    /// </summary>
    public int MunicipioId { get; private set; }
    
    /// <summary>
    /// Município do endereço
    /// </summary>
    public virtual Municipio Municipio { get; private set; } = null!;
    
    /// <summary>
    /// ID do estado
    /// </summary>
    public int EstadoId { get; private set; }
    
    /// <summary>
    /// Estado do endereço
    /// </summary>
    public virtual Estado Estado { get; private set; } = null!;
    
    /// <summary>
    /// Construtor protegido para o Entity Framework
    /// </summary>
    protected Endereco() { }
    
    /// <summary>
    /// Construtor para criar um novo endereço
    /// </summary>
    /// <param name="cep">CEP do endereço</param>
    /// <param name="logradouro">Logradouro</param>
    /// <param name="bairro">Bairro</param>
    /// <param name="municipioId">ID do município</param>
    /// <param name="estadoId">ID do estado</param>
    /// <param name="numero">Número do endereço</param>
    /// <param name="complemento">Complemento</param>
    /// <param name="latitude">Latitude específica</param>
    /// <param name="longitude">Longitude específica</param>
    public Endereco(string cep, string logradouro, string bairro, int municipioId, int estadoId,
                   string? numero = null, string? complemento = null, 
                   double? latitude = null, double? longitude = null)
    {
        ValidarParametros(cep, logradouro, bairro, municipioId, estadoId);
        
        Cep = LimparCep(cep);
        Logradouro = logradouro;
        Bairro = bairro;
        MunicipioId = municipioId;
        EstadoId = estadoId;
        Numero = numero;
        Complemento = complemento;
        
        DefinirLocalizacao(latitude, longitude);
    }
    
    /// <summary>
    /// Atualiza os dados do endereço
    /// </summary>
    /// <param name="cep">CEP do endereço</param>
    /// <param name="logradouro">Logradouro</param>
    /// <param name="bairro">Bairro</param>
    /// <param name="numero">Número do endereço</param>
    /// <param name="complemento">Complemento</param>
    /// <param name="latitude">Latitude específica</param>
    /// <param name="longitude">Longitude específica</param>
    public void Atualizar(string cep, string logradouro, string bairro,
                         string? numero = null, string? complemento = null,
                         double? latitude = null, double? longitude = null)
    {
        ValidarParametros(cep, logradouro, bairro, MunicipioId, EstadoId);
        
        Cep = LimparCep(cep);
        Logradouro = logradouro;
        Bairro = bairro;
        Numero = numero;
        Complemento = complemento;
        
        DefinirLocalizacao(latitude, longitude);
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Define a localização geográfica específica do endereço
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
    /// Calcula a distância em quilômetros para outro endereço
    /// </summary>
    /// <param name="outroEndereco">Endereço de destino</param>
    /// <returns>Distância em quilômetros ou null se algum endereço não tiver localização</returns>
    public double? CalcularDistanciaKm(Endereco outroEndereco)
    {
        if (Localizacao == null || outroEndereco.Localizacao == null)
            return null;
            
        // Usar a função de distância do PostGIS (em metros, converter para km)
        var distanciaMetros = Localizacao.Distance(outroEndereco.Localizacao);
        return distanciaMetros / 1000.0;
    }
    
    /// <summary>
    /// Calcula a distância em quilômetros para um município
    /// </summary>
    /// <param name="municipio">Município de destino</param>
    /// <returns>Distância em quilômetros ou null se não houver localização</returns>
    public double? CalcularDistanciaKm(Municipio municipio)
    {
        if (Localizacao == null || municipio.Localizacao == null)
            return null;
            
        var distanciaMetros = Localizacao.Distance(municipio.Localizacao);
        return distanciaMetros / 1000.0;
    }
    
    /// <summary>
    /// Verifica se o endereço possui localização específica definida
    /// </summary>
    /// <returns>True se possui latitude e longitude específicas</returns>
    public bool PossuiLocalizacao()
    {
        return Latitude.HasValue && Longitude.HasValue && Localizacao != null;
    }
    
    /// <summary>
    /// Retorna o endereço formatado como string
    /// </summary>
    /// <returns>Endereço formatado</returns>
    public string ObterEnderecoFormatado()
    {
        var endereco = Logradouro;
        
        if (!string.IsNullOrWhiteSpace(Numero))
            endereco += $", {Numero}";
            
        if (!string.IsNullOrWhiteSpace(Complemento))
            endereco += $", {Complemento}";
            
        endereco += $", {Bairro}";
        
        return endereco;
    }
    
    /// <summary>
    /// Retorna o CEP formatado (00000-000)
    /// </summary>
    /// <returns>CEP formatado</returns>
    public string ObterCepFormatado()
    {
        if (Cep.Length == 8)
            return $"{Cep[..5]}-{Cep[5..]}";
            
        return Cep;
    }
    
    private static void ValidarParametros(string cep, string logradouro, string bairro, int municipioId, int estadoId)
    {
        if (string.IsNullOrWhiteSpace(cep))
            throw new ArgumentException("CEP é obrigatório", nameof(cep));
            
        if (string.IsNullOrWhiteSpace(logradouro))
            throw new ArgumentException("Logradouro é obrigatório", nameof(logradouro));
            
        if (string.IsNullOrWhiteSpace(bairro))
            throw new ArgumentException("Bairro é obrigatório", nameof(bairro));
            
        if (municipioId <= 0)
            throw new ArgumentException("ID do município deve ser maior que zero", nameof(municipioId));
            
        if (estadoId <= 0)
            throw new ArgumentException("ID do estado deve ser maior que zero", nameof(estadoId));
            
        var cepLimpo = LimparCep(cep);
        if (cepLimpo.Length != 8 || !cepLimpo.All(char.IsDigit))
            throw new ArgumentException("CEP deve conter exatamente 8 dígitos", nameof(cep));
    }
    
    private static void ValidarCoordenadas(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude deve estar entre -90 e 90 graus", nameof(latitude));
            
        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude deve estar entre -180 e 180 graus", nameof(longitude));
    }
    
    private static string LimparCep(string cep)
    {
        return cep.Replace("-", "").Replace(".", "").Replace(" ", "");
    }
}