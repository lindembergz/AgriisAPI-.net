using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using NetTopologySuite.Geometries;

namespace Agriis.Propriedades.Dominio.Entidades;

public class Talhao : EntidadeBase
{
    public string Nome { get; private set; } = string.Empty;
    public AreaPlantio Area { get; private set; } = new(0);
    public string? Descricao { get; private set; }
    public Point? Localizacao { get; private set; }
    public Polygon? Geometria { get; private set; }
    public int PropriedadeId { get; private set; }

    // Navigation Properties
    public virtual Propriedade Propriedade { get; private set; } = null!;

    protected Talhao() { } // EF Constructor

    public Talhao(string nome, AreaPlantio area, int propriedadeId, string? descricao)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Area = area ?? throw new ArgumentNullException(nameof(area));
        PropriedadeId = propriedadeId;
        Descricao = descricao;
    }

    public void AtualizarDados(string nome, AreaPlantio area, string? descricao = null)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Area = area ?? throw new ArgumentNullException(nameof(area));
        Descricao = descricao;
        AtualizarDataModificacao();
    }

    public void DefinirLocalizacao(double latitude, double longitude)
    {
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        Localizacao = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
        AtualizarDataModificacao();
    }

    public void DefinirGeometria(Polygon geometria)
    {
        Geometria = geometria;
        AtualizarDataModificacao();
    }

    public double? CalcularDistancia(Point outroPonto)
    {
        if (Localizacao == null || outroPonto == null)
            return null;

        return Localizacao.Distance(outroPonto);
    }
}