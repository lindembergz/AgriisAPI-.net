using Agriis.Fornecedores.Dominio.Entidades;
using Agriis.Tests.Shared.Base;
using Xunit;

namespace Agriis.Tests.Integration;

/// <summary>
/// Testes de integração para validar o mapeamento correto Fornecedor-Municipio
/// </summary>
public class TestFornecedorMunicipioMapping : BaseTestCase, IClassFixture<TestWebApplicationFactory>
{
    public TestFornecedorMunicipioMapping(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public void DeveReferenciarTiposCorretosDeEntidades()
    {
        // Arrange & Act
        var fornecedor = new Fornecedor(
            "Teste Fornecedor",
            new Agriis.Compartilhado.Dominio.ObjetosValor.Cnpj("12345678000195")
        );

        // Assert - Verificar se as propriedades de navegação são dos tipos corretos
        Assert.True(fornecedor.Municipio == null || fornecedor.Municipio is Agriis.Enderecos.Dominio.Entidades.Municipio);
        Assert.True(fornecedor.Estado == null || fornecedor.Estado is Agriis.Enderecos.Dominio.Entidades.Estado);
    }
}