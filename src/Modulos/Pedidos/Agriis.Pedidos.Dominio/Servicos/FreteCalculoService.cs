using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.Enums;

namespace Agriis.Pedidos.Dominio.Servicos;

/// <summary>
/// Serviço de domínio para cálculos de frete
/// </summary>
public class FreteCalculoService
{
    /// <summary>
    /// Calcula o frete para um item de pedido baseado em peso e cubagem
    /// </summary>
    /// <param name="produto">Produto para calcular o frete</param>
    /// <param name="quantidade">Quantidade do produto</param>
    /// <param name="distanciaKm">Distância em quilômetros</param>
    /// <param name="valorPorKgKm">Valor por quilograma por quilômetro</param>
    /// <param name="valorMinimoFrete">Valor mínimo de frete</param>
    /// <returns>Informações do cálculo de frete</returns>
    public CalculoFreteResult CalcularFrete(
        Produto produto, 
        decimal quantidade, 
        decimal distanciaKm,
        decimal valorPorKgKm = 0.05m,
        decimal valorMinimoFrete = 50.00m)
    {
        if (produto == null)
            throw new ArgumentNullException(nameof(produto));
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantidade));
        if (distanciaKm <= 0)
            throw new ArgumentException("Distância deve ser maior que zero", nameof(distanciaKm));

        // Calcula peso total baseado no tipo de cálculo do produto
        var pesoUnitario = produto.CalcularPesoParaFrete();
        var pesoTotal = pesoUnitario * quantidade;

        // Calcula volume total
        var volumeUnitario = produto.Dimensoes.CalcularVolume();
        var volumeTotal = volumeUnitario * quantidade;

        // Calcula peso cúbico se houver densidade
        decimal? pesoCubadoTotal = null;
        if (produto.Dimensoes.Densidade.HasValue)
        {
            pesoCubadoTotal = volumeTotal * produto.Dimensoes.Densidade.Value;
        }

        // Determina o peso para cálculo de frete
        var pesoParaFrete = produto.TipoCalculoPeso switch
        {
            TipoCalculoPeso.PesoNominal => produto.Dimensoes.PesoNominal * quantidade,
            TipoCalculoPeso.PesoCubado => pesoCubadoTotal ?? (produto.Dimensoes.PesoNominal * quantidade),
            _ => produto.Dimensoes.PesoNominal * quantidade
        };

        // Calcula valor do frete
        var valorCalculado = pesoParaFrete * distanciaKm * valorPorKgKm;
        var valorFrete = Math.Max(valorCalculado, valorMinimoFrete);

        return new CalculoFreteResult(
            pesoTotal,
            volumeTotal,
            pesoCubadoTotal,
            pesoParaFrete,
            valorFrete,
            distanciaKm,
            produto.TipoCalculoPeso
        );
    }

    /// <summary>
    /// Calcula o frete consolidado para múltiplos itens
    /// </summary>
    /// <param name="itens">Lista de itens com produtos e quantidades</param>
    /// <param name="distanciaKm">Distância em quilômetros</param>
    /// <param name="valorPorKgKm">Valor por quilograma por quilômetro</param>
    /// <param name="valorMinimoFrete">Valor mínimo de frete</param>
    /// <returns>Informações consolidadas do cálculo de frete</returns>
    public CalculoFreteConsolidadoResult CalcularFreteConsolidado(
        IEnumerable<(Produto produto, decimal quantidade)> itens,
        decimal distanciaKm,
        decimal valorPorKgKm = 0.05m,
        decimal valorMinimoFrete = 50.00m)
    {
        if (itens == null || !itens.Any())
            throw new ArgumentException("Lista de itens não pode ser vazia", nameof(itens));

        var calculosIndividuais = new List<CalculoFreteResult>();
        decimal pesoTotalConsolidado = 0;
        decimal volumeTotalConsolidado = 0;
        decimal? pesoCubadoTotalConsolidado = 0;
        bool temDensidade = false;

        foreach (var (produto, quantidade) in itens)
        {
            var calculo = CalcularFrete(produto, quantidade, distanciaKm, valorPorKgKm, 0); // Sem mínimo individual
            calculosIndividuais.Add(calculo);

            pesoTotalConsolidado += calculo.PesoTotal;
            volumeTotalConsolidado += calculo.VolumeTotal;
            
            if (calculo.PesoCubadoTotal.HasValue)
            {
                pesoCubadoTotalConsolidado += calculo.PesoCubadoTotal.Value;
                temDensidade = true;
            }
        }

        // Se nenhum produto tem densidade, não calcular peso cúbico consolidado
        if (!temDensidade)
            pesoCubadoTotalConsolidado = null;

        // Calcula valor consolidado (soma dos valores individuais, mas respeitando mínimo global)
        var valorTotalCalculado = calculosIndividuais.Sum(c => c.ValorFrete);
        var valorFreteConsolidado = Math.Max(valorTotalCalculado, valorMinimoFrete);

        return new CalculoFreteConsolidadoResult(
            calculosIndividuais,
            pesoTotalConsolidado,
            volumeTotalConsolidado,
            pesoCubadoTotalConsolidado,
            valorFreteConsolidado,
            distanciaKm
        );
    }

    /// <summary>
    /// Valida se a quantidade está disponível para transporte
    /// </summary>
    /// <param name="pedidoItem">Item do pedido</param>
    /// <param name="quantidadeTransporte">Quantidade a ser transportada</param>
    /// <returns>True se a quantidade está disponível</returns>
    public bool ValidarDisponibilidadeQuantidade(PedidoItem pedidoItem, decimal quantidadeTransporte)
    {
        if (pedidoItem == null)
            throw new ArgumentNullException(nameof(pedidoItem));

        // Calcula quantidade já agendada para transporte
        var quantidadeJaAgendada = pedidoItem.Transportes.Sum(t => t.Quantidade);
        var quantidadeDisponivel = pedidoItem.Quantidade - quantidadeJaAgendada;

        return quantidadeTransporte <= quantidadeDisponivel;
    }

    /// <summary>
    /// Calcula a quantidade disponível para agendamento de transporte
    /// </summary>
    /// <param name="pedidoItem">Item do pedido</param>
    /// <returns>Quantidade disponível</returns>
    public decimal CalcularQuantidadeDisponivel(PedidoItem pedidoItem)
    {
        if (pedidoItem == null)
            throw new ArgumentNullException(nameof(pedidoItem));

        var quantidadeJaAgendada = pedidoItem.Transportes.Sum(t => t.Quantidade);
        return Math.Max(0, pedidoItem.Quantidade - quantidadeJaAgendada);
    }
}

/// <summary>
/// Resultado do cálculo de frete para um item
/// </summary>
public record CalculoFreteResult(
    decimal PesoTotal,
    decimal VolumeTotal,
    decimal? PesoCubadoTotal,
    decimal PesoParaFrete,
    decimal ValorFrete,
    decimal DistanciaKm,
    TipoCalculoPeso TipoCalculoUtilizado
);

/// <summary>
/// Resultado do cálculo de frete consolidado para múltiplos itens
/// </summary>
public record CalculoFreteConsolidadoResult(
    IEnumerable<CalculoFreteResult> CalculosIndividuais,
    decimal PesoTotalConsolidado,
    decimal VolumeTotalConsolidado,
    decimal? PesoCubadoTotalConsolidado,
    decimal ValorFreteConsolidado,
    decimal DistanciaKm
);