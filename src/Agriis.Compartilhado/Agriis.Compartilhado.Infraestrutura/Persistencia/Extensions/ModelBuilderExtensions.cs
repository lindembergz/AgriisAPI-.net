using Microsoft.EntityFrameworkCore;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Compartilhado.Infraestrutura.Persistencia.Conversores;

namespace Agriis.Compartilhado.Infraestrutura.Persistencia.Extensions;

/// <summary>
/// Extensões para configuração do ModelBuilder
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configura conversores de enum para PostgreSQL
    /// Este método deve ser chamado no OnModelCreating do DbContext específico
    /// para cada entidade que usa os enums
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder do Entity Framework</param>
    /// <returns>ModelBuilder configurado</returns>
    public static ModelBuilder ConfigurarConversoresEnum(this ModelBuilder modelBuilder)
    {
        // Os conversores serão aplicados automaticamente quando as entidades forem configuradas
        // Este método serve como documentação dos conversores disponíveis
        return modelBuilder;
    }

    /// <summary>
    /// Configura conversores de objetos de valor
    /// Este método deve ser chamado no OnModelCreating do DbContext específico
    /// para cada entidade que usa os objetos de valor
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder do Entity Framework</param>
    /// <returns>ModelBuilder configurado</returns>
    public static ModelBuilder ConfigurarConversoresObjetosValor(this ModelBuilder modelBuilder)
    {
        // Os conversores serão aplicados automaticamente quando as entidades forem configuradas
        // Este método serve como documentação dos conversores disponíveis
        return modelBuilder;
    }

    /// <summary>
    /// Configura todos os conversores (enums e objetos de valor)
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder do Entity Framework</param>
    /// <returns>ModelBuilder configurado</returns>
    public static ModelBuilder ConfigurarTodosConversores(this ModelBuilder modelBuilder)
    {
        return modelBuilder
            .ConfigurarConversoresEnum()
            .ConfigurarConversoresObjetosValor();
    }
}