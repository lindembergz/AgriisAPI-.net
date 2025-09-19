namespace Agriis.Propriedades.Aplicacao.DTOs;

public class TalhaoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Area { get; set; }
    public string? Descricao { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int PropriedadeId { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class TalhaoCreateDto
{
    public string Nome { get; set; } = string.Empty;
    public decimal Area { get; set; }
    public string? Descricao { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int PropriedadeId { get; set; }
}

public class TalhaoUpdateDto
{
    public string Nome { get; set; } = string.Empty;
    public decimal Area { get; set; }
    public string? Descricao { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}