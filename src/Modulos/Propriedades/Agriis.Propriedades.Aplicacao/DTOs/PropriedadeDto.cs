namespace Agriis.Propriedades.Aplicacao.DTOs;

public class PropriedadeDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Nirf { get; set; }
    public string? InscricaoEstadual { get; set; }
    public decimal AreaTotal { get; set; }
    public int ProdutorId { get; set; }
    public int? EnderecoId { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public object? DadosAdicionais { get; set; }

    public List<TalhaoDto> Talhoes { get; set; } = new();
    public List<PropriedadeCulturaDto> Culturas { get; set; } = new();
}

public class PropriedadeCreateDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Nirf { get; set; }
    public string? InscricaoEstadual { get; set; }
    public decimal AreaTotal { get; set; }
    public int ProdutorId { get; set; }
    public int? EnderecoId { get; set; }
    public object? DadosAdicionais { get; set; }
}

public class PropriedadeUpdateDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Nirf { get; set; }
    public string? InscricaoEstadual { get; set; }
    public decimal AreaTotal { get; set; }
    public int? EnderecoId { get; set; }
    public object? DadosAdicionais { get; set; }
}