using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Agriis.Compartilhado.Dominio.ObjetosValor;

namespace Agriis.Compartilhado.Infraestrutura.Persistencia.Conversores;

/// <summary>
/// Conversores de objetos de valor para Entity Framework
/// </summary>
public static class ValueObjectConverters
{
    /// <summary>
    /// Conversor para CPF
    /// </summary>
    public static ValueConverter<Cpf, string> CpfConverter =>
        new(
            v => v.Valor,
            v => new Cpf(v)
        );

    /// <summary>
    /// Conversor para CNPJ
    /// </summary>
    public static ValueConverter<Cnpj, string> CnpjConverter =>
        new(
            v => v.Valor,
            v => new Cnpj(v)
        );

    /// <summary>
    /// Conversor para AreaPlantio
    /// </summary>
    public static ValueConverter<AreaPlantio, decimal> AreaPlantioConverter =>
        new(
            v => v.Valor,
            v => new AreaPlantio(v)
        );
}