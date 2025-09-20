namespace Agriis.Safras.Aplicacao.DTOs;

/// <summary>
/// DTO para retorno de dados de Safra
/// </summary>
public class SafraDto
{
    public int Id { get; set; }
    public DateTime PlantioInicial { get; set; }
    public DateTime PlantioFinal { get; set; }
    public string PlantioNome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int AnoColheita { get; set; }
    public string SafraFormatada { get; set; } = string.Empty;
    public bool Atual { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

/// <summary>
/// DTO para criação de nova Safra
/// </summary>
public class CriarSafraDto
{
    public DateTime PlantioInicial { get; set; }
    public DateTime PlantioFinal { get; set; }
    public string PlantioNome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}

/// <summary>
/// DTO para atualização de Safra
/// </summary>
public class AtualizarSafraDto
{
    public DateTime PlantioInicial { get; set; }
    public DateTime PlantioFinal { get; set; }
    public string PlantioNome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}

/// <summary>
/// DTO simplificado para retorno da safra atual
/// </summary>
public class SafraAtualDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Safra { get; set; } = string.Empty;
}