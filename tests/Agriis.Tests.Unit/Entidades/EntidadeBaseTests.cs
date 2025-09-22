using Agriis.Compartilhado.Dominio.Entidades;
using FluentAssertions;
using Xunit;

namespace Agriis.Tests.Unit.Entidades;

/// <summary>
/// Testes unitários para a classe base EntidadeBase
/// </summary>
public class EntidadeBaseTests
{
    // Classe concreta para testar EntidadeBase
    private class EntidadeTeste : EntidadeBase
    {
        public string Nome { get; set; } = string.Empty;
        
        public EntidadeTeste(string nome)
        {
            Nome = nome;
        }
        
        protected EntidadeTeste() { } // EF Constructor
    }

    [Fact]
    public void EntidadeBase_DeveDefinirDataCriacaoAoCriar()
    {
        // Arrange
        var dataAntes = DateTime.UtcNow;

        // Act
        var entidade = new EntidadeTeste("Teste");
        var dataDepois = DateTime.UtcNow;

        // Assert
        entidade.DataCriacao.Should().BeAfter(dataAntes.AddMilliseconds(-1));
        entidade.DataCriacao.Should().BeBefore(dataDepois.AddMilliseconds(1));
        entidade.DataAtualizacao.Should().BeNull();
    }

    [Fact]
    public void EntidadeBase_DeveAtualizarDataModificacao()
    {
        // Arrange
        var entidade = new EntidadeTeste("Teste");
        var dataOriginal = entidade.DataCriacao;
        Thread.Sleep(1); // Garantir diferença de tempo

        // Act
        entidade.AtualizarDataModificacao();

        // Assert
        entidade.DataAtualizacao.Should().NotBeNull();
        entidade.DataAtualizacao.Should().BeAfter(dataOriginal);
    }

    [Fact]
    public void EntidadeBase_DeveDefinirDataCriacaoCustomizada()
    {
        // Arrange
        var entidade = new EntidadeTeste("Teste");
        var dataCustomizada = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        // Act
        entidade.SetDataCriacao(dataCustomizada);

        // Assert
        entidade.DataCriacao.Should().Be(dataCustomizada);
    }

    [Fact]
    public void EntidadeBase_DeveSerTransitoriaQuandoIdZero()
    {
        // Arrange
        var entidade = new EntidadeTeste("Teste");

        // Act & Assert
        entidade.EhTransitoria().Should().BeTrue();
        entidade.Id.Should().Be(0);
    }

    [Fact]
    public void EntidadeBase_DeveSerIgualQuandoMesmoId()
    {
        // Arrange
        var entidade1 = new EntidadeTeste("Teste1");
        var entidade2 = new EntidadeTeste("Teste2");
        
        // Simular que foram persistidas com mesmo ID
        var propriedadeId = typeof(EntidadeBase).GetProperty("Id");
        propriedadeId!.SetValue(entidade1, 1);
        propriedadeId!.SetValue(entidade2, 1);

        // Act & Assert
        entidade1.Should().Be(entidade2);
        entidade1.GetHashCode().Should().Be(entidade2.GetHashCode());
        (entidade1 == entidade2).Should().BeTrue();
        (entidade1 != entidade2).Should().BeFalse();
    }

    [Fact]
    public void EntidadeBase_DeveSerDiferenteQuandoIdsDiferentes()
    {
        // Arrange
        var entidade1 = new EntidadeTeste("Teste1");
        var entidade2 = new EntidadeTeste("Teste2");
        
        // Simular que foram persistidas com IDs diferentes
        var propriedadeId = typeof(EntidadeBase).GetProperty("Id");
        propriedadeId!.SetValue(entidade1, 1);
        propriedadeId!.SetValue(entidade2, 2);

        // Act & Assert
        entidade1.Should().NotBe(entidade2);
        entidade1.GetHashCode().Should().NotBe(entidade2.GetHashCode());
        (entidade1 == entidade2).Should().BeFalse();
        (entidade1 != entidade2).Should().BeTrue();
    }

    [Fact]
    public void EntidadeBase_DeveSerDiferenteQuandoTiposDiferentes()
    {
        // Arrange
        var entidade1 = new EntidadeTeste("Teste");
        var entidade2 = new OutraEntidadeTeste("Teste");
        
        // Simular que foram persistidas com mesmo ID
        var propriedadeId = typeof(EntidadeBase).GetProperty("Id");
        propriedadeId!.SetValue(entidade1, 1);
        propriedadeId!.SetValue(entidade2, 1);

        // Act & Assert
        entidade1.Should().NotBe(entidade2);
    }

    [Fact]
    public void EntidadeBase_DeveSerDiferenteQuandoUmaEhTransitoria()
    {
        // Arrange
        var entidade1 = new EntidadeTeste("Teste1"); // Transitória (Id = 0)
        var entidade2 = new EntidadeTeste("Teste2");
        
        // Simular que entidade2 foi persistida
        var propriedadeId = typeof(EntidadeBase).GetProperty("Id");
        propriedadeId!.SetValue(entidade2, 1);

        // Act & Assert
        entidade1.Should().NotBe(entidade2);
        entidade1.EhTransitoria().Should().BeTrue();
        entidade2.EhTransitoria().Should().BeFalse();
    }

    [Fact]
    public void EntidadeBase_DeveSerDiferenteQuandoAmbasTransitorias()
    {
        // Arrange
        var entidade1 = new EntidadeTeste("Teste1");
        var entidade2 = new EntidadeTeste("Teste2");

        // Act & Assert
        entidade1.Should().NotBe(entidade2);
        entidade1.EhTransitoria().Should().BeTrue();
        entidade2.EhTransitoria().Should().BeTrue();
    }

    [Fact]
    public void EntidadeBase_DeveSerIgualAMesmaReferencia()
    {
        // Arrange
        var entidade = new EntidadeTeste("Teste");

        // Act & Assert
        entidade.Should().Be(entidade);
        entidade.Equals(entidade).Should().BeTrue();
    }

    [Fact]
    public void EntidadeBase_DeveSerDiferenteDeNull()
    {
        // Arrange
        var entidade = new EntidadeTeste("Teste");

        // Act & Assert
        entidade.Should().NotBe(null);
        entidade.Equals(null).Should().BeFalse();
        (entidade == null).Should().BeFalse();
        (entidade != null).Should().BeTrue();
    }

    [Fact]
    public void EntidadeBase_DeveSerDiferenteDeObjetoOutroTipo()
    {
        // Arrange
        var entidade = new EntidadeTeste("Teste");
        var outroObjeto = "string";

        // Act & Assert
        entidade.Equals(outroObjeto).Should().BeFalse();
    }

    [Fact]
    public void EntidadeBase_HashCode_DeveUsarIdQuandoNaoTransitoria()
    {
        // Arrange
        var entidade = new EntidadeTeste("Teste");
        var propriedadeId = typeof(EntidadeBase).GetProperty("Id");
        propriedadeId!.SetValue(entidade, 123);

        // Act
        var hashCode = entidade.GetHashCode();

        // Assert
        hashCode.Should().Be(123.GetHashCode());
    }

    [Fact]
    public void EntidadeBase_HashCode_DeveUsarBaseQuandoTransitoria()
    {
        // Arrange
        var entidade = new EntidadeTeste("Teste");

        // Act
        var hashCode1 = entidade.GetHashCode();
        var hashCode2 = entidade.GetHashCode();

        // Assert
        entidade.EhTransitoria().Should().BeTrue();
        hashCode1.Should().Be(hashCode2); // Deve ser consistente
    }

    // Classe auxiliar para testar tipos diferentes
    private class OutraEntidadeTeste : EntidadeBase
    {
        public string Nome { get; set; } = string.Empty;
        
        public OutraEntidadeTeste(string nome)
        {
            Nome = nome;
        }
    }
}