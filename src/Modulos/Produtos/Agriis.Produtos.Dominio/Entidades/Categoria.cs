using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Produtos.Dominio.Enums;

namespace Agriis.Produtos.Dominio.Entidades;

/// <summary>
/// Entidade que representa uma categoria de produtos
/// </summary>
public class Categoria : EntidadeBase
{
    /// <summary>
    /// Nome da categoria
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// Descrição da categoria
    /// </summary>
    public string? Descricao { get; private set; }
    
    /// <summary>
    /// Tipo da categoria
    /// </summary>
    public CategoriaProduto Tipo { get; private set; }
    
    /// <summary>
    /// Indica se a categoria está ativa
    /// </summary>
    public bool Ativo { get; private set; }
    
    /// <summary>
    /// Categoria pai (para hierarquia)
    /// </summary>
    public int? CategoriaPaiId { get; private set; }
    
    /// <summary>
    /// Ordem de exibição
    /// </summary>
    public int Ordem { get; private set; }

    // Navigation Properties
    public virtual Categoria? CategoriaPai { get; private set; }
    public virtual ICollection<Categoria> SubCategorias { get; private set; } = new List<Categoria>();
    public virtual ICollection<Produto> Produtos { get; private set; } = new List<Produto>();

    protected Categoria() { } // EF Constructor

    public Categoria(string nome, CategoriaProduto tipo, string? descricao = null, int? categoriaPaiId = null, int ordem = 0)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Tipo = tipo;
        Descricao = descricao;
        CategoriaPaiId = categoriaPaiId;
        Ordem = ordem;
        Ativo = true;
    }

    /// <summary>
    /// Atualiza o nome da categoria
    /// </summary>
    public void AtualizarNome(string nome)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza a descrição da categoria
    /// </summary>
    public void AtualizarDescricao(string? descricao)
    {
        Descricao = descricao;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza o tipo da categoria
    /// </summary>
    public void AtualizarTipo(CategoriaProduto tipo)
    {
        Tipo = tipo;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza a ordem de exibição
    /// </summary>
    public void AtualizarOrdem(int ordem)
    {
        Ordem = ordem;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Ativa a categoria
    /// </summary>
    public void Ativar()
    {
        if (!Ativo)
        {
            Ativo = true;
            AtualizarDataModificacao();
        }
    }

    /// <summary>
    /// Desativa a categoria
    /// </summary>
    public void Desativar()
    {
        if (Ativo)
        {
            Ativo = false;
            AtualizarDataModificacao();
        }
    }

    /// <summary>
    /// Define a categoria pai
    /// </summary>
    public void DefinirCategoriaPai(int? categoriaPaiId)
    {
        // Validar se não está criando referência circular
        if (categoriaPaiId.HasValue && categoriaPaiId.Value == Id)
            throw new InvalidOperationException("Uma categoria não pode ser pai de si mesma");

        CategoriaPaiId = categoriaPaiId;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Verifica se é uma categoria raiz (sem pai)
    /// </summary>
    public bool EhCategoriaRaiz() => !CategoriaPaiId.HasValue;

    /// <summary>
    /// Verifica se tem subcategorias
    /// </summary>
    public bool TemSubCategorias() => SubCategorias.Any();
}