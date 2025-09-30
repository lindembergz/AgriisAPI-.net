namespace Agriis.Propriedades.Aplicacao.DTOs;

public class PropriedadeCulturaDto
{
    public int Id { get; set; }
    public int PropriedadeId { get; set; }
    public int CulturaId { get; set; }
    public decimal Area { get; set; }
    public int? SafraId { get; set; }
    public DateTime? DataPlantio { get; set; }
    public DateTime? DataColheitaPrevista { get; set; }
    public string? Observacoes { get; set; }
    public DateTimeOffset DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public bool EstaEmPeriodoPlantio { get; set; }
}

public class PropriedadeCulturaCreateDto
{
    public int PropriedadeId { get; set; }
    public int CulturaId { get; set; }
    public decimal Area { get; set; }
    public int? SafraId { get; set; }
    public DateTime? DataPlantio { get; set; }
    public DateTime? DataColheitaPrevista { get; set; }
    public string? Observacoes { get; set; }
}

public class PropriedadeCulturaUpdateDto
{
    public decimal Area { get; set; }
    public int? SafraId { get; set; }
    public DateTime? DataPlantio { get; set; }
    public DateTime? DataColheitaPrevista { get; set; }
    public string? Observacoes { get; set; }
}