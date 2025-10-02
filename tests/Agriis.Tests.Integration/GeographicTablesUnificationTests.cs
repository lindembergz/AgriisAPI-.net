using Agriis.Enderecos.Dominio.Entidades;
using Agriis.Enderecos.Dominio.Interfaces;
using Agriis.Fornecedores.Dominio.Entidades;
using Agriis.Fornecedores.Dominio.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para validar a unificação das tabelas geográficas
/// </summary>
[Collection("Database")]
public class GeographicTablesUnificationTests : IntegrationTestBase
{
    private readonly IEstadoRepository _estadoRepository;
    private readonly IMunicipioRepository _municipioRepository;
    private readonly IFornecedorRepository _fornecedorRepository;

    public GeographicTablesUnificationTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _estadoRepository = ServiceProvider.GetRequiredService<IEstadoRepository>();
        _municipioRepository = ServiceProvider.GetRequiredService<IMunicipioRepository>();
        _fornecedorRepository = ServiceProvider.GetRequiredService<IFornecedorRepository>();
    }

    [Fact]
    public async Task Estados_ShouldBeAccessibleFromUnifiedTable()
    {
        // Arrange & Act
        var estados = await _estadoRepository.ObterTodosAsync();

        // Assert
        estados.Should().NotBeEmpty("Estados should be available from unified table");
        estados.Should().OnlyHaveUniqueItems(e => e.Uf, "Each UF should be unique");
        estados.Should().OnlyHaveUniqueItems(e => e.CodigoIbge, "Each IBGE code should be unique");
        
        foreach (var estado in estados)
        {
            estado.Nome.Should().NotBeNullOrWhiteSpace("Estado name should not be empty");
            estado.Uf.Should().NotBeNullOrWhiteSpace("Estado UF should not be empty");
            estado.Uf.Should().HaveLength(2, "UF should have exactly 2 characters");
            estado.CodigoIbge.Should().BeGreaterThan(0, "IBGE code should be positive");
            estado.Regiao.Should().NotBeNullOrWhiteSpace("Region should not be empty");
        }
    }

    [Fact]
    public async Task Municipios_ShouldBeAccessibleFromUnifiedTable()
    {
        // Arrange & Act
        var municipios = await _municipioRepository.ObterTodosAsync();

        // Assert
        municipios.Should().NotBeEmpty("Municipios should be available from unified table");
        municipios.Should().OnlyHaveUniqueItems(m => m.CodigoIbge, "Each IBGE code should be unique");
        
        foreach (var municipio in municipios)
        {
            municipio.Nome.Should().NotBeNullOrWhiteSpace("Municipio name should not be empty");
            municipio.CodigoIbge.Should().BeGreaterThan(0, "IBGE code should be positive");
            municipio.EstadoId.Should().BeGreaterThan(0, "Estado ID should be positive");
            municipio.Estado.Should().NotBeNull("Estado navigation property should be loaded");
        }
    }

    [Fact]
    public async Task Estados_ShouldHaveValidRelationshipWithMunicipios()
    {
        // Arrange
        var estadoSP = await _estadoRepository.ObterPorUfAsync("SP");
        
        // Act
        var municipiosSP = await _municipioRepository.ObterPorEstadoAsync(estadoSP!.Id);

        // Assert
        estadoSP.Should().NotBeNull("São Paulo state should exist");
        municipiosSP.Should().NotBeEmpty("São Paulo should have municipalities");
        
        foreach (var municipio in municipiosSP)
        {
            municipio.EstadoId.Should().Be(estadoSP.Id, "Municipality should belong to SP state");
            municipio.Estado.Uf.Should().Be("SP", "Navigation property should point to SP");
        }
    }

    [Fact]
    public async Task Fornecedores_ShouldHaveValidGeographicReferences()
    {
        // Arrange
        var fornecedores = await _fornecedorRepository.ObterTodosAsync();

        // Act & Assert
        foreach (var fornecedor in fornecedores.Where(f => f.UfId.HasValue))
        {
            var estado = await _estadoRepository.ObterPorIdAsync(fornecedor.UfId.Value);
            estado.Should().NotBeNull($"Fornecedor {fornecedor.Id} should have valid Estado reference");
        }

        foreach (var fornecedor in fornecedores.Where(f => f.MunicipioId.HasValue))
        {
            var municipio = await _municipioRepository.ObterPorIdAsync(fornecedor.MunicipioId.Value);
            municipio.Should().NotBeNull($"Fornecedor {fornecedor.Id} should have valid Municipio reference");
        }
    }

    [Fact]
    public async Task GeographicQueries_ShouldReturnConsistentResults()
    {
        // Arrange
        var ufSP = "SP";
        
        // Act
        var estadoSP = await _estadoRepository.ObterPorUfAsync(ufSP);
        var municipiosSP1 = await _municipioRepository.ObterPorEstadoAsync(estadoSP!.Id);
        var municipiosSP2 = await _municipioRepository.ObterPorUfAsync(ufSP);

        // Assert
        estadoSP.Should().NotBeNull("SP state should exist");
        municipiosSP1.Should().HaveCountGreaterThan(0, "SP should have municipalities via EstadoId");
        municipiosSP2.Should().HaveCountGreaterThan(0, "SP should have municipalities via UF");
        
        municipiosSP1.Count().Should().Be(municipiosSP2.Count(), 
            "Both query methods should return the same number of municipalities");
        
        var ids1 = municipiosSP1.Select(m => m.Id).OrderBy(id => id);
        var ids2 = municipiosSP2.Select(m => m.Id).OrderBy(id => id);
        ids1.Should().BeEquivalentTo(ids2, "Both queries should return the same municipalities");
    }

    [Fact]
    public async Task Estados_ShouldNotHaveDuplicateUFs()
    {
        // Arrange & Act
        var estados = await _estadoRepository.ObterTodosAsync();
        var ufs = estados.Select(e => e.Uf).ToList();

        // Assert
        ufs.Should().OnlyHaveUniqueItems("UFs should be unique across all states");
        ufs.Should().HaveCount(27, "Brazil should have 26 states + 1 federal district");
    }

    [Fact]
    public async Task Municipios_ShouldNotHaveDuplicateIBGECodes()
    {
        // Arrange & Act
        var municipios = await _municipioRepository.ObterTodosAsync();
        var codigosIbge = municipios.Select(m => m.CodigoIbge).ToList();

        // Assert
        codigosIbge.Should().OnlyHaveUniqueItems("IBGE codes should be unique across all municipalities");
    }

    [Fact]
    public async Task GeographicData_ShouldHaveProperDataTypes()
    {
        // Arrange
        var estado = await _estadoRepository.ObterPorUfAsync("SP");
        var municipio = await _municipioRepository.ObterPorCodigoIbgeAsync(3550308); // São Paulo city

        // Assert
        estado.Should().NotBeNull();
        estado!.CodigoIbge.Should().BeOfType<int>("Estado IBGE code should be integer");
        
        if (municipio != null)
        {
            municipio.CodigoIbge.Should().BeOfType<int>("Municipio IBGE code should be integer");
            
            if (municipio.Latitude.HasValue && municipio.Longitude.HasValue)
            {
                municipio.Latitude.Should().BeInRange(-90, 90, "Latitude should be valid");
                municipio.Longitude.Should().BeInRange(-180, 180, "Longitude should be valid");
            }
        }
    }

    [Fact]
    public async Task ForeignKeyConstraints_ShouldBeEnforced()
    {
        // Arrange
        var invalidEstadoId = 99999;
        var invalidMunicipioId = 99999;

        // Act & Assert - Estado FK constraint
        var estadoExists = await _estadoRepository.ObterPorIdAsync(invalidEstadoId);
        estadoExists.Should().BeNull("Invalid Estado ID should not exist");

        // Act & Assert - Municipio FK constraint  
        var municipioExists = await _municipioRepository.ObterPorIdAsync(invalidMunicipioId);
        municipioExists.Should().BeNull("Invalid Municipio ID should not exist");
    }

    [Fact]
    public async Task GeographicSearch_ShouldWorkCorrectly()
    {
        // Arrange
        var searchTerm = "São";

        // Act
        var municipiosFound = await _municipioRepository.BuscarPorNomeAsync(searchTerm);

        // Assert
        municipiosFound.Should().NotBeEmpty("Should find municipalities with 'São' in name");
        
        foreach (var municipio in municipiosFound)
        {
            municipio.Nome.Should().ContainEquivalentOf(searchTerm, 
                "Found municipalities should contain the search term");
            municipio.Estado.Should().NotBeNull("Estado navigation should be loaded");
        }
    }

    [Fact]
    public async Task GeographicCoordinates_ShouldBeHandledCorrectly()
    {
        // Arrange & Act
        var municipiosComLocalizacao = await _municipioRepository.ObterComLocalizacaoAsync();

        // Assert
        foreach (var municipio in municipiosComLocalizacao)
        {
            if (municipio.PossuiLocalizacao())
            {
                municipio.Latitude.Should().NotBeNull("Latitude should not be null when location exists");
                municipio.Longitude.Should().NotBeNull("Longitude should not be null when location exists");
                municipio.Latitude.Should().BeInRange(-90, 90, "Latitude should be valid");
                municipio.Longitude.Should().BeInRange(-180, 180, "Longitude should be valid");
            }
        }
    }

    [Fact]
    public async Task PerformanceTest_GeographicQueries_ShouldBeEfficient()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - Test multiple geographic queries
        var estados = await _estadoRepository.ObterTodosAsync();
        var municipiosSP = await _municipioRepository.ObterPorUfAsync("SP");
        var municipiosRJ = await _municipioRepository.ObterPorUfAsync("RJ");
        var estadosSudeste = await _estadoRepository.ObterPorRegiaoAsync("Sudeste");

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, 
            "Geographic queries should complete within 5 seconds");
        
        estados.Should().NotBeEmpty();
        municipiosSP.Should().NotBeEmpty();
        municipiosRJ.Should().NotBeEmpty();
        estadosSudeste.Should().NotBeEmpty();
    }
}