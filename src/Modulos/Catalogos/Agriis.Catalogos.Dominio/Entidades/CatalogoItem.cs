using System.Text.Json;
using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Catalogos.Dominio.Entidades;

public class CatalogoItem : EntidadeBase
{
    public int CatalogoId { get; private set; }
    public int ProdutoId { get; private set; }
    public JsonDocument EstruturaPrecosJson { get; private set; }
    public decimal? PrecoBase { get; private set; }
    public bool Ativo { get; private set; }
    
    // Navigation Properties
    public virtual Catalogo Catalogo { get; private set; } = null!;
    
    protected CatalogoItem() 
    {
        EstruturaPrecosJson = null!;
    }
    
    public CatalogoItem(int catalogoId, int produtoId, JsonDocument estruturaPrecosJson, decimal? precoBase = null)
    {
        CatalogoId = catalogoId;
        ProdutoId = produtoId;
        EstruturaPrecosJson = estruturaPrecosJson ?? throw new ArgumentNullException(nameof(estruturaPrecosJson));
        PrecoBase = precoBase;
        Ativo = true;
    }
    
    public void AtualizarPrecos(JsonDocument novaEstruturaPrecosJson, decimal? novoPrecoBase = null)
    {
        EstruturaPrecosJson = novaEstruturaPrecosJson ?? throw new ArgumentNullException(nameof(novaEstruturaPrecosJson));
        PrecoBase = novoPrecoBase;
        AtualizarDataModificacao();
    }
    
    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }
    
    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }
    
    public decimal? ObterPrecoPorEstadoEData(string uf, DateTime data)
    {
        if (EstruturaPrecosJson == null)
            return PrecoBase;
            
        try
        {
            var root = EstruturaPrecosJson.RootElement;
            
            // Procurar por estado específico
            if (root.TryGetProperty("estados", out var estados) && 
                estados.TryGetProperty(uf, out var estadoElement))
            {
                return ObterPrecoParaData(estadoElement, data);
            }
            
            // Fallback para preço padrão
            if (root.TryGetProperty("padrao", out var padraoElement))
            {
                return ObterPrecoParaData(padraoElement, data);
            }
            
            return PrecoBase;
        }
        catch
        {
            return PrecoBase;
        }
    }
    
    private decimal? ObterPrecoParaData(JsonElement elemento, DateTime data)
    {
        if (elemento.ValueKind == JsonValueKind.Number)
        {
            return elemento.GetDecimal();
        }
        
        if (elemento.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in elemento.EnumerateArray())
            {
                if (item.TryGetProperty("data_inicio", out var dataInicioElement) &&
                    item.TryGetProperty("valor", out var valorElement))
                {
                    if (DateTime.TryParse(dataInicioElement.GetString(), out var dataInicio) &&
                        data >= dataInicio)
                    {
                        var dataFim = DateTime.MaxValue;
                        if (item.TryGetProperty("data_fim", out var dataFimElement) &&
                            DateTime.TryParse(dataFimElement.GetString(), out var dataFimParsed))
                        {
                            dataFim = dataFimParsed;
                        }
                        
                        if (data <= dataFim)
                        {
                            return valorElement.GetDecimal();
                        }
                    }
                }
            }
        }
        
        return null;
    }
}