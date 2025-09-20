using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Safras.Dominio.Entidades;

/// <summary>
/// Representa uma safra agrícola com períodos de plantio
/// </summary>
public class Safra : EntidadeBase
{
    /// <summary>
    /// Data inicial do período de plantio
    /// </summary>
    public DateTime PlantioInicial { get; private set; }
    
    /// <summary>
    /// Data final do período de plantio
    /// </summary>
    public DateTime PlantioFinal { get; private set; }
    
    /// <summary>
    /// Nome do período de plantio (ex: S1, S2)
    /// </summary>
    public string PlantioNome { get; private set; } = string.Empty;
    
    /// <summary>
    /// Descrição da safra
    /// </summary>
    public string Descricao { get; private set; } = string.Empty;
    
    /// <summary>
    /// Ano de colheita calculado
    /// </summary>
    public int AnoColheita { get; private set; }
    
    /// <summary>
    /// Construtor protegido para o Entity Framework
    /// </summary>
    protected Safra() { }
    
    /// <summary>
    /// Construtor para criar uma nova safra
    /// </summary>
    /// <param name="plantioInicial">Data inicial do plantio</param>
    /// <param name="plantioFinal">Data final do plantio</param>
    /// <param name="plantioNome">Nome do período de plantio</param>
    /// <param name="descricao">Descrição da safra</param>
    public Safra(DateTime plantioInicial, DateTime plantioFinal, string plantioNome, string descricao)
    {
        ValidarDatasPlantio(plantioInicial, plantioFinal);
        ValidarPlantioNome(plantioNome);
        ValidarDescricao(descricao);
        
        PlantioInicial = plantioInicial;
        PlantioFinal = plantioFinal;
        PlantioNome = plantioNome.Trim();
        Descricao = descricao.Trim();
        AnoColheita = CalcularAnoColheita(plantioInicial, plantioFinal);
    }
    
    /// <summary>
    /// Atualiza os dados da safra
    /// </summary>
    /// <param name="plantioInicial">Nova data inicial do plantio</param>
    /// <param name="plantioFinal">Nova data final do plantio</param>
    /// <param name="plantioNome">Novo nome do período de plantio</param>
    /// <param name="descricao">Nova descrição da safra</param>
    public void Atualizar(DateTime plantioInicial, DateTime plantioFinal, string plantioNome, string descricao)
    {
        ValidarDatasPlantio(plantioInicial, plantioFinal);
        ValidarPlantioNome(plantioNome);
        ValidarDescricao(descricao);
        
        PlantioInicial = plantioInicial;
        PlantioFinal = plantioFinal;
        PlantioNome = plantioNome.Trim();
        Descricao = descricao.Trim();
        AnoColheita = CalcularAnoColheita(plantioInicial, plantioFinal);
        
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Verifica se a safra está ativa (dentro do período de plantio)
    /// </summary>
    /// <returns>True se a safra está ativa</returns>
    public bool EstaAtiva()
    {
        var agora = DateTime.Now;
        return agora >= PlantioInicial && agora <= PlantioFinal && PlantioNome == "S1";
    }
    
    /// <summary>
    /// Obtém a representação textual da safra no formato "YYYY/YYYY Nome"
    /// </summary>
    /// <returns>String formatada da safra</returns>
    public string ObterSafraFormatada()
    {
        return $"{PlantioInicial.Year}/{PlantioFinal.Year} {PlantioNome}";
    }
    
    /// <summary>
    /// Obtém a representação textual da safra apenas com os anos
    /// </summary>
    /// <returns>String formatada apenas com os anos</returns>
    public string ObterSafraAnosFormatada()
    {
        return $"{PlantioInicial.Year}/{PlantioFinal.Year}";
    }
    
    /// <summary>
    /// Valida as datas de plantio
    /// </summary>
    /// <param name="plantioInicial">Data inicial</param>
    /// <param name="plantioFinal">Data final</param>
    /// <exception cref="ArgumentException">Quando as datas são inválidas</exception>
    private static void ValidarDatasPlantio(DateTime plantioInicial, DateTime plantioFinal)
    {
        if (plantioInicial >= plantioFinal)
            throw new ArgumentException("A data inicial do plantio deve ser anterior à data final");
            
        if (plantioInicial < new DateTime(1900, 1, 1))
            throw new ArgumentException("A data inicial do plantio não pode ser anterior a 1900");
            
        if (plantioFinal > DateTime.Now.AddYears(10))
            throw new ArgumentException("A data final do plantio não pode ser superior a 10 anos no futuro");
    }
    
    /// <summary>
    /// Valida o nome do plantio
    /// </summary>
    /// <param name="plantioNome">Nome do plantio</param>
    /// <exception cref="ArgumentException">Quando o nome é inválido</exception>
    private static void ValidarPlantioNome(string plantioNome)
    {
        if (string.IsNullOrWhiteSpace(plantioNome))
            throw new ArgumentException("O nome do plantio é obrigatório");
            
        if (plantioNome.Trim().Length > 256)
            throw new ArgumentException("O nome do plantio não pode ter mais de 256 caracteres");
    }
    
    /// <summary>
    /// Valida a descrição
    /// </summary>
    /// <param name="descricao">Descrição da safra</param>
    /// <exception cref="ArgumentException">Quando a descrição é inválida</exception>
    private static void ValidarDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição é obrigatória");
            
        if (descricao.Trim().Length > 64)
            throw new ArgumentException("A descrição não pode ter mais de 64 caracteres");
    }
    
    /// <summary>
    /// Calcula o ano de colheita baseado nas datas de plantio
    /// </summary>
    /// <param name="plantioInicial">Data inicial do plantio</param>
    /// <param name="plantioFinal">Data final do plantio</param>
    /// <returns>Ano de colheita</returns>
    private static int CalcularAnoColheita(DateTime plantioInicial, DateTime plantioFinal)
    {
        // Se o plantio vai até o ano seguinte, o ano de colheita é o ano final
        // Caso contrário, é o ano inicial
        return plantioFinal.Year > plantioInicial.Year ? plantioFinal.Year : plantioInicial.Year;
    }
}