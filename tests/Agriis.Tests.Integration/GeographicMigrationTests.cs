using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Agriis.Enderecos.Dominio.Entidades;
using Agriis.Fornecedores.Dominio.Entidades;
using Agriis.Api.Contexto;
using Xunit;
using FluentAssertions;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para validar a migração das tabelas geográficas
/// </summary>
[Collection("Database")]
public class GeographicMigrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly AgriisDbContext _context;

    public GeographicMigrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _context = _fixture.ServiceProvider.GetRequiredService<AgriisDbContext>();
    }

    [Fact]
    public async Task Estados_ShouldHaveAllBrazilianStates()
    {
        // Arrange & Act
        var estados = await _context.Set<Estado>()
            .OrderBy(e => e.Uf)
            .ToListAsync();

        // Assert
        estados.Should().HaveCountGreaterOrEqualTo(27); // 26 estados + DF
        
        // Verificar alguns estados essenciais
        var estadosEssenciais = new[] { "SP", "RJ", "MG", "RS", "PR", "SC", "BA", "GO", "DF" };
        foreach (var uf in estadosEssenciais)
        {
            estados.Should().Contain(e => e.Uf == uf, $"Estado {uf} deve existir");
        }
    }

    [Fact]
    public async Task Estados_ShouldHaveUniqueUfCodes()
    {
        // Arrange & Act
        var ufs = await _context.Set<Estado>()
            .Select(e => e.Uf)
            .ToListAsync();

        // Assert
        var duplicateUfs = ufs.GroupBy(uf => uf)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        duplicateUfs.Should().BeEmpty("Não deve haver UFs duplicadas");
    }

    [Fact]
    public async Task Estados_ShouldHaveUniqueCodigoIbge()
    {
        // Arrange & Act
        var codigosIbge = await _context.Set<Estado>()
            .Select(e => e.CodigoIbge)
            .ToListAsync();

        // Assert
        var duplicateCodes = codigosIbge.GroupBy(code => code)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        duplicateCodes.Should().BeEmpty("Não deve haver códigos IBGE duplicados");
    }

    [Fact]
    public async Task Estados_ShouldHaveValidRegions()
    {
        // Arrange
        var validRegions = new[] { "Norte", "Nordeste", "Centro-Oeste", "Sudeste", "Sul" };

        // Act
        var estados = await _context.Set<Estado>()
            .ToListAsync();

        // Assert
        foreach (var estado in estados)
        {
            validRegions.Should().Contain(estado.Regiao, 
                $"Estado {estado.Uf} deve ter uma região válida");
        }
    }

    [Fact]
    public async Task Estados_ShouldHaveValidPaisId()
    {
        // Arrange & Act
        var estados = await _context.Set<Estado>()
            .ToListAsync();

        // Assert
        foreach (var estado in estados)
        {
            estado.PaisId.Should().BeGreaterThan(0, 
                $"Estado {estado.Uf} deve ter um PaisId válido");
            estado.PaisId.Should().Be(1, 
                $"Estado {estado.Uf} deve pertencer ao Brasil (PaisId = 1)");
        }
    }

    [Fact]
    public async Task Municipios_ShouldHaveUniqueCodigoIbge()
    {
        // Arrange & Act
        var codigosIbge = await _context.Set<Municipio>()
            .Select(m => m.CodigoIbge)
            .ToListAsync();

        // Assert
        var duplicateCodes = codigosIbge.GroupBy(code => code)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        duplicateCodes.Should().BeEmpty("Não deve haver códigos IBGE de municípios duplicados");
    }

    [Fact]
    public async Task Municipios_ShouldBelongToValidEstados()
    {
        // Arrange & Act
        var municipiosOrfaos = await _context.Set<Municipio>()
            .Where(m => m.Estado == null)
            .CountAsync();

        // Assert
        municipiosOrfaos.Should().Be(0, "Todos os municípios devem pertencer a um estado válido");
    }

    [Fact]
    public async Task Municipios_ShouldHaveValidCoordinates()
    {
        // Arrange & Act
        var municipiosComCoordenadas = await _context.Set<Municipio>()
            .Where(m => m.Latitude.HasValue && m.Longitude.HasValue)
            .ToListAsync();

        // Assert
        foreach (var municipio in municipiosComCoordenadas)
        {
            municipio.Latitude.Should().BeInRange(-90, 90, 
                $"Latitude do município {municipio.Nome} deve estar entre -90 e 90");
            municipio.Longitude.Should().BeInRange(-180, 180, 
                $"Longitude do município {municipio.Nome} deve estar entre -180 e 180");
        }
    }

    [Fact]
    public async Task Fornecedores_ShouldHaveValidGeographicReferences()
    {
        // Arrange & Act
        var fornecedores = await _context.Set<Fornecedor>()
            .Include(f => f.Estado)
            .Include(f => f.Municipio)
            .Where(f => f.UfId.HasValue || f.MunicipioId.HasValue)
            .ToListAsync();

        // Assert
        foreach (var fornecedor in fornecedores)
        {
            if (fornecedor.UfId.HasValue)
            {
                fornecedor.Estado.Should().NotBeNull(
                    $"Fornecedor {fornecedor.Nome} com UfId deve ter Estado válido");
            }

            if (fornecedor.MunicipioId.HasValue)
            {
                fornecedor.Municipio.Should().NotBeNull(
                    $"Fornecedor {fornecedor.Nome} com MunicipioId deve ter Município válido");
            }
        }
    }

    [Fact]
    public async Task Enderecos_ShouldHaveValidGeographicReferences()
    {
        // Arrange & Act
        var enderecos = await _context.Set<Endereco>()
            .Include(e => e.Estado)
            .Include(e => e.Municipio)
            .Take(100) // Limitar para performance
            .ToListAsync();

        // Assert
        foreach (var endereco in enderecos)
        {
            endereco.Estado.Should().NotBeNull(
                $"Endereço {endereco.Id} deve ter Estado válido");
            endereco.Municipio.Should().NotBeNull(
                $"Endereço {endereco.Id} deve ter Município válido");
            endereco.Municipio.EstadoId.Should().Be(endereco.EstadoId,
                $"Município do endereço {endereco.Id} deve pertencer ao mesmo estado");
        }
    }

    [Fact]
    public async Task GeographicTables_ShouldNotHaveReferenceTablesAnymore()
    {
        // Arrange & Act
        var tableNames = await _context.Database
            .SqlQueryRaw<string>("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name LIKE '%_referencia'")
            .ToListAsync();

        // Assert
        tableNames.Should().NotContain("estados_referencia", 
            "Tabela estados_referencia deve ter sido removida");
        tableNames.Should().NotContain("municipios_referencia", 
            "Tabela municipios_referencia deve ter sido removida");
    }

    [Fact]
    public async Task Estados_ShouldHaveExpectedCount()
    {
        // Arrange & Act
        var estadosCount = await _context.Set<Estado>().CountAsync();

        // Assert
        estadosCount.Should().Be(27, "Brasil deve ter exatamente 27 unidades federativas (26 estados + DF)");
    }

    [Fact]
    public async Task Municipios_ShouldHaveMinimumExpectedCount()
    {
        // Arrange & Act
        var municipiosCount = await _context.Set<Municipio>().CountAsync();

        // Assert
        municipiosCount.Should().BeGreaterThan(5000, 
            "Brasil deve ter mais de 5000 municípios");
    }

    [Fact]
    public async Task GeographicData_ShouldHaveConsistentAuditFields()
    {
        // Arrange & Act
        var estadosSemDataCriacao = await _context.Set<Estado>()
            .Where(e => e.DataCriacao == default)
            .CountAsync();

        var municipiosSemDataCriacao = await _context.Set<Municipio>()
            .Where(m => m.DataCriacao == default)
            .CountAsync();

        // Assert
        estadosSemDataCriacao.Should().Be(0, 
            "Todos os estados devem ter data de criação");
        municipiosSemDataCriacao.Should().Be(0, 
            "Todos os municípios devem ter data de criação");
    }

    [Fact]
    public async Task Estados_ShouldHaveCorrectRegionMapping()
    {
        // Arrange
        var expectedRegions = new Dictionary<string, string>
        {
            { "SP", "Sudeste" },
            { "RJ", "Sudeste" },
            { "MG", "Sudeste" },
            { "ES", "Sudeste" },
            { "RS", "Sul" },
            { "SC", "Sul" },
            { "PR", "Sul" },
            { "BA", "Nordeste" },
            { "PE", "Nordeste" },
            { "CE", "Nordeste" },
            { "GO", "Centro-Oeste" },
            { "MT", "Centro-Oeste" },
            { "MS", "Centro-Oeste" },
            { "DF", "Centro-Oeste" },
            { "AM", "Norte" },
            { "PA", "Norte" },
            { "AC", "Norte" }
        };

        // Act
        var estados = await _context.Set<Estado>()
            .Where(e => expectedRegions.Keys.Contains(e.Uf))
            .ToListAsync();

        // Assert
        foreach (var estado in estados)
        {
            estado.Regiao.Should().Be(expectedRegions[estado.Uf], 
                $"Estado {estado.Uf} deve pertencer à região {expectedRegions[estado.Uf]}");
        }
    }

    [Fact]
    public async Task ForeignKeyConstraints_ShouldBeProperlyConfigured()
    {
        // Arrange & Act
        var constraintInfo = await _context.Database
            .SqlQueryRaw<string>(@"
                SELECT constraint_name 
                FROM information_schema.table_constraints 
                WHERE table_name = 'Fornecedor' 
                AND constraint_type = 'FOREIGN KEY'
                AND (constraint_name LIKE '%Estados%' OR constraint_name LIKE '%Municipios%')")
            .ToListAsync();

        // Assert
        constraintInfo.Should().Contain(c => c.Contains("Estados"), 
            "Deve existir constraint de FK para Estados");
        constraintInfo.Should().Contain(c => c.Contains("Municipios"), 
            "Deve existir constraint de FK para Municipios");
    }
}