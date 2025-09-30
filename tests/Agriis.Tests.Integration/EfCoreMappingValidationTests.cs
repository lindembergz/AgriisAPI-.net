using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Agriis.Tests.Shared.Base;
using Xunit;
using Xunit.Abstractions;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes para validar mapeamentos do Entity Framework Core
/// Detecta inconsistências entre DDL e configurações EF
/// </summary>
public class EfCoreMappingValidationTests : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    private readonly ITestOutputHelper _output;

    public EfCoreMappingValidationTests(TestWebApplicationFactory factory, ITestOutputHelper output) 
        : base(factory)
    {
        _output = output;
    }

    [Fact]
    public void DeveValidarTodosOsMapeamentosEfCore()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DbContext>();
        var errors = new List<string>();

        // Act - Tentar validar o modelo completo do EF Core
        try
        {
            var model = context.Model;
            
            // Validar se o modelo foi construído corretamente
            Assert.NotNull(model);
            
            // Listar todas as entidades para debug
            var entityTypes = model.GetEntityTypes().ToList();
            _output.WriteLine($"Total de entidades encontradas: {entityTypes.Count}");
            
            foreach (var entityType in entityTypes)
            {
                _output.WriteLine($"Entidade: {entityType.ClrType.Name} -> Tabela: {entityType.GetTableName()}");
                
                // Validar foreign keys
                var foreignKeys = entityType.GetForeignKeys();
                foreach (var fk in foreignKeys)
                {
                    var constraintName = fk.GetConstraintName();
                    var principalTable = fk.PrincipalEntityType.GetTableName();
                    
                    _output.WriteLine($"  FK: {constraintName} -> {principalTable}");
                    
                    // Verificar se constraint names seguem padrão esperado
                    if (constraintName != null && constraintName.Contains("MunicipiosReferencia"))
                    {
                        errors.Add($"FK incorreta detectada: {constraintName} na entidade {entityType.ClrType.Name}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Erro na validação do modelo EF Core: {ex.Message}");
        }

        // Assert
        if (errors.Any())
        {
            var errorMessage = "Erros de mapeamento detectados:\n" + string.Join("\n", errors);
            _output.WriteLine(errorMessage);
            Assert.Fail(errorMessage);
        }
    }

    [Fact]
    public void DeveValidarNavegacoesFornecedor()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DbContext>();

        // Act & Assert
        try
        {
            var fornecedorEntityType = context.Model.FindEntityType("Agriis.Fornecedores.Dominio.Entidades.Fornecedor");
            
            if (fornecedorEntityType != null)
            {
                // Verificar navegação Municipio
                var municipioNav = fornecedorEntityType.FindNavigation("Municipio");
                if (municipioNav != null)
                {
                    var targetType = municipioNav.TargetEntityType.ClrType;
                    _output.WriteLine($"Municipio navigation aponta para: {targetType.FullName}");
                    
                    // Deve apontar para Enderecos.Municipio, não Referencias.Municipio
                    Assert.Contains("Enderecos", targetType.FullName);
                    Assert.DoesNotContain("Referencias", targetType.FullName);
                }

                // Verificar navegação Estado
                var estadoNav = fornecedorEntityType.FindNavigation("Estado");
                if (estadoNav != null)
                {
                    var targetType = estadoNav.TargetEntityType.ClrType;
                    _output.WriteLine($"Estado navigation aponta para: {targetType.FullName}");
                    
                    // Deve apontar para Enderecos.Estado
                    Assert.Contains("Enderecos", targetType.FullName);
                }
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Erro na validação de navegações: {ex.Message}");
            throw;
        }
    }
}