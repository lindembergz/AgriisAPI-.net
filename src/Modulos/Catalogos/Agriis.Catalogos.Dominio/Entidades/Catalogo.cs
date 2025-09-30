using System.Text.Json;
using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.Enums;

namespace Agriis.Catalogos.Dominio.Entidades;

public class Catalogo : EntidadeRaizAgregada
{
    public int SafraId { get; private set; }
    public int PontoDistribuicaoId { get; private set; }
    public int CulturaId { get; private set; }
    public int CategoriaId { get; private set; }
    public Moeda Moeda { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime? DataFim { get; private set; }
    public bool Ativo { get; private set; }
    
    // Navigation Properties
    public virtual ICollection<CatalogoItem> Itens { get; private set; }
    
    protected Catalogo() 
    {
        Itens = new List<CatalogoItem>();
    }
    
    public Catalogo(int safraId, int pontoDistribuicaoId, int culturaId, int categoriaId, 
                   Moeda moeda, DateTime dataInicio, DateTime? dataFim = null)
    {
        SafraId = safraId;
        PontoDistribuicaoId = pontoDistribuicaoId;
        CulturaId = culturaId;
        CategoriaId = categoriaId;
        Moeda = moeda;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Ativo = true;
        Itens = new List<CatalogoItem>();
    }
    
    public void AdicionarItem(CatalogoItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        if (Itens.Any(i => i.ProdutoId == item.ProdutoId))
            throw new InvalidOperationException($"Produto {item.ProdutoId} já existe no catálogo");
            
        Itens.Add(item);
        AtualizarDataModificacao();
    }
    
    public void RemoverItem(int produtoId)
    {
        var item = Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (item != null)
        {
            Itens.Remove(item);
            AtualizarDataModificacao();
        }
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
    
    public bool EstaVigente(DateTimeOffset data)
    {
        return Ativo && 
               data >= DataInicio && 
               (DataFim == null || data <= DataFim);
    }
}