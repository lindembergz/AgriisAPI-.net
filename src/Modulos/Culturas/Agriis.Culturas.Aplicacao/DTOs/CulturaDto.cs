namespace Agriis.Culturas.Aplicacao.DTOs;

public class CulturaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class CriarCulturaDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}

public class AtualizarCulturaDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}