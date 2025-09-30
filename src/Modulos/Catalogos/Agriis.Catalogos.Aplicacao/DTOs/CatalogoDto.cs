using Agriis.Compartilhado.Dominio.Enums;

namespace Agriis.Catalogos.Aplicacao.DTOs;

public class CatalogoDto
{
    public int Id { get; set; }
    public int SafraId { get; set; }
    public int PontoDistribuicaoId { get; set; }
    public int CulturaId { get; set; }
    public int CategoriaId { get; set; }
    public Moeda Moeda { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool Ativo { get; set; }
    public DateTimeOffset DataCriacao { get; set; }
    public DateTimeOffset? DataAtualizacao { get; set; }
    
    public List<CatalogoItemDto> Itens { get; set; } = new();
}

public class CriarCatalogoDto
{
    public int SafraId { get; set; }
    public int PontoDistribuicaoId { get; set; }
    public int CulturaId { get; set; }
    public int CategoriaId { get; set; }
    public Moeda Moeda { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}

public class AtualizarCatalogoDto
{
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool Ativo { get; set; }
}