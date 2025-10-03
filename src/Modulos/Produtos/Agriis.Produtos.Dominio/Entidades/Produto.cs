using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Produtos.Dominio.Enums;
using Agriis.Produtos.Dominio.ObjetosValor;
using Agriis.Referencias.Dominio.Entidades;
using System.Text.Json;

namespace Agriis.Produtos.Dominio.Entidades;

/// <summary>
/// Entidade que representa um produto agrícola
/// </summary>
public class Produto : EntidadeRaizAgregada
{
    /// <summary>
    /// Nome do produto
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// Descrição detalhada do produto
    /// </summary>
    public string? Descricao { get; private set; }
    
    /// <summary>
    /// Código/SKU do produto
    /// </summary>
    public string Codigo { get; private set; } = string.Empty;
    
    /// <summary>
    /// Marca do produto
    /// </summary>
    public string? Marca { get; private set; }
    
    /// <summary>
    /// Tipo do produto (Fabricante/Revendedor)
    /// </summary>
    public TipoProduto Tipo { get; private set; }
    
    /// <summary>
    /// Status do produto
    /// </summary>
    public StatusProduto Status { get; private set; }
    
    /// <summary>
    /// ID da unidade de medida
    /// </summary>
    public int UnidadeMedidaId { get; private set; }
    
    /// <summary>
    /// Dimensões físicas do produto
    /// </summary>
    public DimensoesProduto Dimensoes { get; private set; } = null!;
    
    /// <summary>
    /// Tipo de cálculo de peso para frete
    /// </summary>
    public TipoCalculoPeso TipoCalculoPeso { get; private set; }
    
    /// <summary>
    /// Indica se é um produto restrito (requer validações especiais)
    /// </summary>
    public bool ProdutoRestrito { get; private set; }
    
    /// <summary>
    /// Observações sobre restrições
    /// </summary>
    public string? ObservacoesRestricao { get; private set; }
    
    /// <summary>
    /// ID da categoria do produto
    /// </summary>
    public int CategoriaId { get; private set; }
    
    /// <summary>
    /// ID do fornecedor do produto
    /// </summary>
    public int FornecedorId { get; private set; }
    
    /// <summary>
    /// ID da embalagem do produto
    /// </summary>
    public int? EmbalagemId { get; private set; }
    
    /// <summary>
    /// Quantidade de produto por embalagem
    /// </summary>
    public decimal QuantidadeEmbalagem { get; private set; }
    
    /// <summary>
    /// ID da atividade agropecuária relacionada ao produto
    /// </summary>
    public int? AtividadeAgropecuariaId { get; private set; }
    
    /// <summary>
    /// ID do produto pai (para produtos fabricantes vs revendedores)
    /// </summary>
    public int? ProdutoPaiId { get; private set; }
    
    /// <summary>
    /// Dados adicionais em formato JSON
    /// </summary>
    public JsonDocument? DadosAdicionais { get; private set; }

    // Navigation Properties
    public virtual Categoria Categoria { get; private set; } = null!;
    public virtual UnidadeMedida UnidadeMedida { get; private set; } = null!;
    public virtual Embalagem? Embalagem { get; private set; }
    public virtual AtividadeAgropecuaria? AtividadeAgropecuaria { get; private set; }
    public virtual Produto? ProdutoPai { get; private set; }
    public virtual ICollection<Produto> ProdutosFilhos { get; private set; } = new List<Produto>();
    public virtual ICollection<ProdutoCultura> ProdutosCulturas { get; private set; } = new List<ProdutoCultura>();

    protected Produto() { } // EF Constructor

    public Produto(
        string nome,
        string codigo,
        TipoProduto tipo,
        int unidadeMedidaId,
        DimensoesProduto dimensoes,
        int categoriaId,
        int fornecedorId,
        string? descricao = null,
        string? marca = null,
        TipoCalculoPeso tipoCalculoPeso = TipoCalculoPeso.PesoNominal,
        bool produtoRestrito = false,
        string? observacoesRestricao = null,
        int? produtoPaiId = null,
        int? embalagemId = null,
        decimal quantidadeEmbalagem = 1.0m,
        int? atividadeAgropecuariaId = null)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
        Tipo = tipo;
        UnidadeMedidaId = unidadeMedidaId;
        Dimensoes = dimensoes ?? throw new ArgumentNullException(nameof(dimensoes));
        CategoriaId = categoriaId;
        FornecedorId = fornecedorId;
        EmbalagemId = embalagemId;
        QuantidadeEmbalagem = quantidadeEmbalagem > 0 ? quantidadeEmbalagem : throw new ArgumentException("Quantidade de embalagem deve ser maior que zero", nameof(quantidadeEmbalagem));
        AtividadeAgropecuariaId = atividadeAgropecuariaId;
        Descricao = descricao;
        Marca = marca;
        TipoCalculoPeso = tipoCalculoPeso;
        ProdutoRestrito = produtoRestrito;
        ObservacoesRestricao = observacoesRestricao;
        ProdutoPaiId = produtoPaiId;
        Status = StatusProduto.Ativo;

        ValidarRegrasNegocio();
    }

    /// <summary>
    /// Atualiza informações básicas do produto
    /// </summary>
    public void AtualizarInformacoes(string nome, string? descricao, string? marca)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Descricao = descricao;
        Marca = marca;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza o código do produto
    /// </summary>
    public void AtualizarCodigo(string codigo)
    {
        Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza as dimensões do produto
    /// </summary>
    public void AtualizarDimensoes(DimensoesProduto dimensoes)
    {
        Dimensoes = dimensoes ?? throw new ArgumentNullException(nameof(dimensoes));
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza o tipo de cálculo de peso
    /// </summary>
    public void AtualizarTipoCalculoPeso(TipoCalculoPeso tipoCalculoPeso)
    {
        TipoCalculoPeso = tipoCalculoPeso;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Define se o produto é restrito
    /// </summary>
    public void DefinirRestricao(bool produtoRestrito, string? observacoesRestricao = null)
    {
        ProdutoRestrito = produtoRestrito;
        ObservacoesRestricao = produtoRestrito ? observacoesRestricao : null;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza a categoria do produto
    /// </summary>
    public void AtualizarCategoria(int categoriaId)
    {
        CategoriaId = categoriaId;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza a unidade de medida do produto
    /// </summary>
    public void AtualizarUnidadeMedida(int unidadeMedidaId)
    {
        UnidadeMedidaId = unidadeMedidaId;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza a embalagem do produto
    /// </summary>
    public void AtualizarEmbalagem(int? embalagemId)
    {
        EmbalagemId = embalagemId;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza a embalagem e quantidade do produto
    /// </summary>
    public void AtualizarEmbalagem(int? embalagemId, decimal quantidadeEmbalagem)
    {
        if (quantidadeEmbalagem <= 0)
            throw new ArgumentException("Quantidade de embalagem deve ser maior que zero", nameof(quantidadeEmbalagem));

        EmbalagemId = embalagemId;
        QuantidadeEmbalagem = quantidadeEmbalagem;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza apenas a quantidade de embalagem
    /// </summary>
    public void AtualizarQuantidadeEmbalagem(decimal quantidadeEmbalagem)
    {
        if (quantidadeEmbalagem <= 0)
            throw new ArgumentException("Quantidade de embalagem deve ser maior que zero", nameof(quantidadeEmbalagem));

        QuantidadeEmbalagem = quantidadeEmbalagem;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza a atividade agropecuária do produto
    /// </summary>
    public void AtualizarAtividadeAgropecuaria(int? atividadeAgropecuariaId)
    {
        AtividadeAgropecuariaId = atividadeAgropecuariaId;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Define o produto pai
    /// </summary>
    public void DefinirProdutoPai(int? produtoPaiId)
    {
        if (produtoPaiId.HasValue && produtoPaiId.Value == Id)
            throw new InvalidOperationException("Um produto não pode ser pai de si mesmo");

        ProdutoPaiId = produtoPaiId;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Ativa o produto
    /// </summary>
    public void Ativar()
    {
        if (Status != StatusProduto.Ativo)
        {
            Status = StatusProduto.Ativo;
            AtualizarDataModificacao();
        }
    }

    /// <summary>
    /// Inativa o produto
    /// </summary>
    public void Inativar()
    {
        if (Status != StatusProduto.Inativo)
        {
            Status = StatusProduto.Inativo;
            AtualizarDataModificacao();
        }
    }

    /// <summary>
    /// Descontinua o produto
    /// </summary>
    public void Descontinuar()
    {
        if (Status != StatusProduto.Descontinuado)
        {
            Status = StatusProduto.Descontinuado;
            AtualizarDataModificacao();
        }
    }

    /// <summary>
    /// Adiciona uma cultura compatível com o produto
    /// </summary>
    public void AdicionarCultura(int culturaId)
    {
        if (ProdutosCulturas.Any(pc => pc.CulturaId == culturaId))
            return; // Já existe

        var produtoCultura = new ProdutoCultura(Id, culturaId);
        ProdutosCulturas.Add(produtoCultura);
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Remove uma cultura do produto
    /// </summary>
    public void RemoverCultura(int culturaId)
    {
        var produtoCultura = ProdutosCulturas.FirstOrDefault(pc => pc.CulturaId == culturaId);
        if (produtoCultura != null)
        {
            ProdutosCulturas.Remove(produtoCultura);
            AtualizarDataModificacao();
        }
    }

    /// <summary>
    /// Atualiza dados adicionais em formato JSON
    /// </summary>
    public void AtualizarDadosAdicionais(JsonDocument? dadosAdicionais)
    {
        DadosAdicionais = dadosAdicionais;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Calcula o peso do produto replicando exatamente a lógica Python
    /// </summary>
    /// <returns>Peso do produto em quilogramas</returns>
    public decimal CalcularPesoProduto()
    {
        // Replicar lógica Python exata:
        // if produto.categoria.nome == 'Sementes' and produto.unidade == TipoUnidade.SEMENTES:
        //     peso_produto = ((produto.pms / 1000) / 1000) * produto.quantidade_minima
        // else:
        //     peso_produto = produto.peso_embalagem
        
        // Para manter compatibilidade, vamos usar o símbolo da unidade de medida
        var unidadeSimbolo = UnidadeMedida?.Simbolo ?? string.Empty;
        var tipoUnidade = MapearSimboloParaTipoUnidade(unidadeSimbolo);
        
        return Dimensoes.CalcularPesoProduto(Categoria?.Nome ?? string.Empty, tipoUnidade);
    }

    /// <summary>
    /// Calcula o peso para frete baseado no tipo de cálculo configurado (replicando lógica Python)
    /// </summary>
    /// <param name="quantidade">Quantidade de produtos para calcular o peso total</param>
    /// <returns>Peso total para cálculo de frete</returns>
    public decimal CalcularPesoParaFrete(decimal quantidade = 1)
    {
        decimal pesoUnitario;
        
        if (TipoCalculoPeso == TipoCalculoPeso.PesoCubado)
        {
            // Lógica Python para PESOCUBADO:
            // dimensoes = produto.comprimento * produto.largura * produto.profundidade
            // peso_produto = produto.peso_embalagem
            // fator_cubagem = produto.faixa_densidade_inicial
            // pre_peso_cubado = (dimensoes / 1000000) * fator_cubagem
            // peso_cubado = pre_peso_cubado if pre_peso_cubado > peso_produto else peso_produto
            // frete_peso = peso_cubado * quantidade
            
            var pesoCubado = Dimensoes.CalcularPesoCubado();
            pesoUnitario = pesoCubado ?? Dimensoes.PesoEmbalagem;
        }
        else
        {
            // Lógica Python para PESONOMINAL:
            // if produto.categoria.nome == 'Sementes' and produto.unidade == TipoUnidade.SEMENTES:
            //     peso_produto = ((produto.pms / 1000) / 1000) * produto.quantidade_minima
            // else:
            //     peso_produto = produto.peso_embalagem
            // frete_peso = peso_produto * quantidade
            
            pesoUnitario = CalcularPesoProduto();
        }
        
        return pesoUnitario * quantidade;
    }

    /// <summary>
    /// Verifica se o produto é fabricante
    /// </summary>
    public bool EhFabricante() => Tipo == TipoProduto.Fabricante;

    /// <summary>
    /// Verifica se o produto é revendedor
    /// </summary>
    public bool EhRevendedor() => Tipo == TipoProduto.Revendedor;

    /// <summary>
    /// Verifica se o produto está ativo
    /// </summary>
    public bool EstaAtivo() => Status == StatusProduto.Ativo;

    /// <summary>
    /// Verifica se o produto tem produto pai
    /// </summary>
    public bool TemProdutoPai() => ProdutoPaiId.HasValue;

    /// <summary>
    /// Verifica se o produto tem produtos filhos
    /// </summary>
    public bool TemProdutosFilhos() => ProdutosFilhos.Any();

    /// <summary>
    /// Obtém as culturas compatíveis
    /// </summary>
    public IEnumerable<int> ObterCulturasCompativeis()
    {
        return ProdutosCulturas.Select(pc => pc.CulturaId);
    }

    /// <summary>
    /// Verifica se o produto é compatível com uma cultura
    /// </summary>
    public bool EhCompativelComCultura(int culturaId)
    {
        return ProdutosCulturas.Any(pc => pc.CulturaId == culturaId);
    }

    private void ValidarRegrasNegocio()
    {
        // Produtos revendedores devem ter produto pai
        if (Tipo == TipoProduto.Revendedor && !ProdutoPaiId.HasValue)
            throw new InvalidOperationException("Produtos revendedores devem ter um produto pai (fabricante)");

        // Produtos fabricantes não podem ter produto pai
        if (Tipo == TipoProduto.Fabricante && ProdutoPaiId.HasValue)
            throw new InvalidOperationException("Produtos fabricantes não podem ter produto pai");
    }

    /// <summary>
    /// Mapeia o símbolo da unidade de medida para o tipo de unidade (compatibilidade com sistema Python)
    /// </summary>
    private TipoUnidade MapearSimboloParaTipoUnidade(string simbolo)
    {
        return simbolo?.ToUpper() switch
        {
            "SEMENTES" => TipoUnidade.Sementes,
            "KG" => TipoUnidade.Quilo,
            "T" => TipoUnidade.Tonelada,
            "L" => TipoUnidade.Litro,
            "HA" => TipoUnidade.Hectare,
            "DOSE" => TipoUnidade.Dose,
            "FRASCO" => TipoUnidade.Frasco,
            "OVOS" => TipoUnidade.Ovos,
            "PARASITOIDE" => TipoUnidade.Parasitoide,
            _ => TipoUnidade.Quilo
        };
    }
}