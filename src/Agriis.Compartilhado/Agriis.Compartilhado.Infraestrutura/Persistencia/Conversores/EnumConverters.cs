using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Agriis.Compartilhado.Dominio.Enums;

namespace Agriis.Compartilhado.Infraestrutura.Persistencia.Conversores;

/// <summary>
/// Conversores de enum para PostgreSQL
/// </summary>
public static class EnumConverters
{
    /// <summary>
    /// Conversor para StatusProdutor
    /// </summary>
    public static ValueConverter<StatusProdutor, int> StatusProdutorConverter =>
        new(
            v => (int)v,
            v => (StatusProdutor)v
        );

    /// <summary>
    /// Conversor para StatusPedido
    /// </summary>
    public static ValueConverter<StatusPedido, int> StatusPedidoConverter =>
        new(
            v => (int)v,
            v => (StatusPedido)v
        );

    /// <summary>
    /// Conversor para StatusCarrinho
    /// </summary>
    public static ValueConverter<StatusCarrinho, int> StatusCarrinhoConverter =>
        new(
            v => (int)v,
            v => (StatusCarrinho)v
        );

    /// <summary>
    /// Conversor para AcaoCompradorPedido
    /// </summary>
    public static ValueConverter<AcaoCompradorPedido, int> AcaoCompradorPedidoConverter =>
        new(
            v => (int)v,
            v => (AcaoCompradorPedido)v
        );

    /// <summary>
    /// Conversor para TipoVenda
    /// </summary>
    public static ValueConverter<TipoVenda, int> TipoVendaConverter =>
        new(
            v => (int)v,
            v => (TipoVenda)v
        );

    /// <summary>
    /// Conversor para Roles
    /// </summary>
    public static ValueConverter<Roles, int> RolesConverter =>
        new(
            v => (int)v,
            v => (Roles)v
        );

    /// <summary>
    /// Conversor para BarterTipoEntrega
    /// </summary>
    public static ValueConverter<BarterTipoEntrega, int> BarterTipoEntregaConverter =>
        new(
            v => (int)v,
            v => (BarterTipoEntrega)v
        );

    /// <summary>
    /// Conversor para ModalidadePagamento
    /// </summary>
    public static ValueConverter<ModalidadePagamento, int> ModalidadePagamentoConverter =>
        new(
            v => (int)v,
            v => (ModalidadePagamento)v
        );

    /// <summary>
    /// Conversor para CalculoCubagem
    /// </summary>
    public static ValueConverter<CalculoCubagem, int> CalculoCubagemConverter =>
        new(
            v => (int)v,
            v => (CalculoCubagem)v
        );

    /// <summary>
    /// Conversor para CalculoFrete
    /// </summary>
    public static ValueConverter<CalculoFrete, int> CalculoFreteConverter =>
        new(
            v => (int)v,
            v => (CalculoFrete)v
        );

    /// <summary>
    /// Conversor para ClassificacaoProduto
    /// </summary>
    public static ValueConverter<ClassificacaoProduto, int> ClassificacaoProdutoConverter =>
        new(
            v => (int)v,
            v => (ClassificacaoProduto)v
        );

    /// <summary>
    /// Conversor para TipoAcessoAuditoria
    /// </summary>
    public static ValueConverter<TipoAcessoAuditoria, int> TipoAcessoAuditoriaConverter =>
        new(
            v => (int)v,
            v => (TipoAcessoAuditoria)v
        );

    /// <summary>
    /// Conversor para StatusTentativaSerpro
    /// </summary>
    public static ValueConverter<StatusTentativaSerpro, int> StatusTentativaSerproConverter =>
        new(
            v => (int)v,
            v => (StatusTentativaSerpro)v
        );

    /// <summary>
    /// Conversor para TipoUnidade
    /// </summary>
    public static ValueConverter<TipoUnidade, int> TipoUnidadeConverter =>
        new(
            v => (int)v,
            v => (TipoUnidade)v
        );

    /// <summary>
    /// Conversor para Moeda
    /// </summary>
    public static ValueConverter<Moeda, int> MoedaConverter =>
        new(
            v => (int)v,
            v => (Moeda)v
        );

    /// <summary>
    /// Conversor para StatusGenerico
    /// </summary>
    public static ValueConverter<StatusGenerico, int> StatusGenericoConverter =>
        new(
            v => (int)v,
            v => (StatusGenerico)v
        );

    /// <summary>
    /// Conversor para TipoOperacao
    /// </summary>
    public static ValueConverter<TipoOperacao, int> TipoOperacaoConverter =>
        new(
            v => (int)v,
            v => (TipoOperacao)v
        );

    /// <summary>
    /// Conversor para NivelLog
    /// </summary>
    public static ValueConverter<NivelLog, int> NivelLogConverter =>
        new(
            v => (int)v,
            v => (NivelLog)v
        );

    /// <summary>
    /// Conversor para TipoDocumento
    /// </summary>
    public static ValueConverter<TipoDocumento, int> TipoDocumentoConverter =>
        new(
            v => (int)v,
            v => (TipoDocumento)v
        );

    /// <summary>
    /// Conversor para EstadoBrasil
    /// </summary>
    public static ValueConverter<EstadoBrasil, int> EstadoBrasilConverter =>
        new(
            v => (int)v,
            v => (EstadoBrasil)v
        );

    /// <summary>
    /// Conversor para TipoEndereco
    /// </summary>
    public static ValueConverter<TipoEndereco, int> TipoEnderecoConverter =>
        new(
            v => (int)v,
            v => (TipoEndereco)v
        );

    /// <summary>
    /// Conversor para TipoContato
    /// </summary>
    public static ValueConverter<TipoContato, int> TipoContatoConverter =>
        new(
            v => (int)v,
            v => (TipoContato)v
        );

    /// <summary>
    /// Conversor para TipoArquivo
    /// </summary>
    public static ValueConverter<TipoArquivo, int> TipoArquivoConverter =>
        new(
            v => (int)v,
            v => (TipoArquivo)v
        );

    /// <summary>
    /// Conversor para ExtensaoArquivo
    /// </summary>
    public static ValueConverter<ExtensaoArquivo, int> ExtensaoArquivoConverter =>
        new(
            v => (int)v,
            v => (ExtensaoArquivo)v
        );
}