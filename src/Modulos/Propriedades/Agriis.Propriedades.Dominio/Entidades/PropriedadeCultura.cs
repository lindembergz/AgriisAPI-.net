using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.ObjetosValor;

namespace Agriis.Propriedades.Dominio.Entidades;

public class PropriedadeCultura : EntidadeBase
{
    public int PropriedadeId { get; private set; }
    public int CulturaId { get; private set; }
    public AreaPlantio Area { get; private set; } = new(0);
    public int? SafraId { get; private set; }
    public DateTime? DataPlantio { get; private set; }
    public DateTime? DataColheitaPrevista { get; private set; }
    public string? Observacoes { get; private set; }

    // Navigation Properties
    public virtual Propriedade Propriedade { get; private set; } = null!;

    protected PropriedadeCultura() { } // EF Constructor

    public PropriedadeCultura(int propriedadeId, int culturaId, AreaPlantio area, int? safraId)
    {
        PropriedadeId = propriedadeId;
        CulturaId = culturaId;
        Area = area ?? throw new ArgumentNullException(nameof(area));
        SafraId = safraId;
    }

    public void AtualizarArea(AreaPlantio novaArea)
    {
        Area = novaArea ?? throw new ArgumentNullException(nameof(novaArea));
        AtualizarDataModificacao();
    }

    public void DefinirSafra(int safraId)
    {
        SafraId = safraId;
        AtualizarDataModificacao();
    }

    public void DefinirDatasPlantioColheita(DateTime? dataPlantio, DateTime? dataColheitaPrevista)
    {
        DataPlantio = dataPlantio;
        DataColheitaPrevista = dataColheitaPrevista;
        AtualizarDataModificacao();
    }

    public void AdicionarObservacoes(string observacoes)
    {
        Observacoes = observacoes;
        AtualizarDataModificacao();
    }

    public bool EstaEmPeriodoPlantio()
    {
        if (DataPlantio == null) return false;
        
        var agora = DateTime.UtcNow;
        var inicioPlantio = DataPlantio.Value;
        var fimPlantio = DataColheitaPrevista ?? inicioPlantio.AddMonths(6);
        
        return agora >= inicioPlantio && agora <= fimPlantio;
    }
}