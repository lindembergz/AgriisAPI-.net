using System.Text.Json;

namespace Agriis.Catalogos.Aplicacao.DTOs;

public class CatalogoItemDto
{
    public int Id { get; set; }
    public int CatalogoId { get; set; }
    public int ProdutoId { get; set; }
    public JsonDocument EstruturaPrecosJson { get; set; } = null!;
    public decimal? PrecoBase { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class CriarCatalogoItemDto
{
    public int ProdutoId { get; set; }
    public JsonDocument EstruturaPrecosJson { get; set; } = null!;
    public decimal? PrecoBase { get; set; }
}

public class AtualizarCatalogoItemDto
{
    public JsonDocument EstruturaPrecosJson { get; set; } = null!;
    public decimal? PrecoBase { get; set; }
    public bool Ativo { get; set; }
}

public class ConsultarPrecoDto
{
    public string Uf { get; set; } = string.Empty;
    public DateTime Data { get; set; }
}