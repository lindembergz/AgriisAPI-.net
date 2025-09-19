using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using System.Text.Json;

namespace Agriis.Propriedades.Dominio.Entidades;

public class Propriedade : EntidadeRaizAgregada
{
    public string Nome { get; private set; } = string.Empty;
    public string? Nirf { get; private set; }
    public string? InscricaoEstadual { get; private set; }
    public AreaPlantio AreaTotal { get; private set; } = new(0);
    public int ProdutorId { get; private set; }
    public int? EnderecoId { get; private set; }
    public JsonDocument? DadosAdicionais { get; private set; }

    // Navigation Properties
    public virtual ICollection<Talhao> Talhoes { get; private set; } = new List<Talhao>();
    public virtual ICollection<PropriedadeCultura> PropriedadeCulturas { get; private set; } = new List<PropriedadeCultura>();

    protected Propriedade() { } // EF Constructor

    public Propriedade(string nome, int produtorId, AreaPlantio areaTotal, string? nirf, 
                      string? inscricaoEstadual, int? enderecoId)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        ProdutorId = produtorId;
        AreaTotal = areaTotal ?? throw new ArgumentNullException(nameof(areaTotal));
        Nirf = nirf;
        InscricaoEstadual = inscricaoEstadual;
        EnderecoId = enderecoId;
    }

    public void AtualizarDados(string nome, AreaPlantio areaTotal, string? nirf = null, 
                              string? inscricaoEstadual = null, int? enderecoId = null)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        AreaTotal = areaTotal ?? throw new ArgumentNullException(nameof(areaTotal));
        Nirf = nirf;
        InscricaoEstadual = inscricaoEstadual;
        EnderecoId = enderecoId;
        AtualizarDataModificacao();
    }

    public void AdicionarTalhao(Talhao talhao)
    {
        if (talhao == null) throw new ArgumentNullException(nameof(talhao));
        
        Talhoes.Add(talhao);
        AtualizarDataModificacao();
    }

    public void AdicionarCultura(int culturaId, AreaPlantio area)
    {
        var propriedadeCultura = new PropriedadeCultura(Id, culturaId, area, null);
        PropriedadeCulturas.Add(propriedadeCultura);
        AtualizarDataModificacao();
    }

    public void RemoverCultura(int culturaId)
    {
        var propriedadeCultura = PropriedadeCulturas.FirstOrDefault(pc => pc.CulturaId == culturaId);
        if (propriedadeCultura != null)
        {
            PropriedadeCulturas.Remove(propriedadeCultura);
            AtualizarDataModificacao();
        }
    }

    public AreaPlantio CalcularAreaTotalCulturas()
    {
        var areaTotal = PropriedadeCulturas.Sum(pc => pc.Area.Valor);
        return new AreaPlantio(areaTotal);
    }

    public void DefinirDadosAdicionais(object dados)
    {
        if (dados != null)
        {
            DadosAdicionais = JsonDocument.Parse(JsonSerializer.Serialize(dados));
            AtualizarDataModificacao();
        }
    }
}