using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Produtores.Dominio.Entidades;
using Agriis.Produtores.Dominio.Enums;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace Agriis.Tests.Unit.Entidades;

/// <summary>
/// Testes unitários para a entidade Produtor
/// </summary>
public class ProdutorTests
{
    [Fact]
    public void Produtor_DeveCriarComCpf()
    {
        // Arrange
        var nome = "João Silva";
        var cpf = new Cpf("11144477735");
        var inscricaoEstadual = "123456789";
        var tipoAtividade = "Agricultura";
        var areaPlantio = new AreaPlantio(100.5m);

        // Act
        var produtor = new Produtor(nome, cpf, null, inscricaoEstadual, tipoAtividade, areaPlantio);

        // Assert
        produtor.Nome.Should().Be(nome);
        produtor.Cpf.Should().Be(cpf);
        produtor.Cnpj.Should().BeNull();
        produtor.InscricaoEstadual.Should().Be(inscricaoEstadual);
        produtor.TipoAtividade.Should().Be(tipoAtividade);
        produtor.AreaPlantio.Should().Be(areaPlantio);
        produtor.Status.Should().Be(StatusProdutor.PendenteValidacaoAutomatica);
        produtor.DataAutorizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        produtor.Culturas.Should().BeEmpty();
        produtor.EhPessoaFisica().Should().BeTrue();
        produtor.EhPessoaJuridica().Should().BeFalse();
    }

    [Fact]
    public void Produtor_DeveCriarComCnpj()
    {
        // Arrange
        var nome = "Fazenda ABC Ltda";
        var cnpj = new Cnpj("11222333000181");
        var inscricaoEstadual = "123456789";
        var tipoAtividade = "Agricultura";
        var areaPlantio = new AreaPlantio(500m);

        // Act
        var produtor = new Produtor(nome, null, cnpj, inscricaoEstadual, tipoAtividade, areaPlantio);

        // Assert
        produtor.Nome.Should().Be(nome);
        produtor.Cpf.Should().BeNull();
        produtor.Cnpj.Should().Be(cnpj);
        produtor.EhPessoaFisica().Should().BeFalse();
        produtor.EhPessoaJuridica().Should().BeTrue();
    }

    [Fact]
    public void Produtor_DeveCriarComParametrosMinimos()
    {
        // Arrange
        var nome = "João Silva";
        var cpf = new Cpf("11144477735");

        // Act
        var produtor = new Produtor(nome, cpf);

        // Assert
        produtor.Nome.Should().Be(nome);
        produtor.Cpf.Should().Be(cpf);
        produtor.InscricaoEstadual.Should().BeNull();
        produtor.TipoAtividade.Should().BeNull();
        produtor.AreaPlantio.Valor.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Produtor_DeveLancarExcecaoParaNomeInvalido(string nomeInvalido)
    {
        // Arrange
        var cpf = new Cpf("11144477735");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Produtor(nomeInvalido, cpf));
        exception.Message.Should().Contain("Nome do produtor é obrigatório");
    }

    [Fact]
    public void Produtor_DeveLancarExcecaoParaNomeNulo()
    {
        // Arrange
        var cpf = new Cpf("11144477735");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Produtor(null!, cpf));
        exception.Message.Should().Contain("Nome do produtor é obrigatório");
    }

    [Fact]
    public void Produtor_DeveLancarExcecaoQuandoNemCpfNemCnpj()
    {
        // Arrange
        var nome = "João Silva";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Produtor(nome));
        exception.Message.Should().Contain("CPF ou CNPJ deve ser informado");
    }

    [Fact]
    public void Produtor_DeveAtualizarStatus()
    {
        // Arrange
        var produtor = CriarProdutorValido();
        var novoStatus = StatusProdutor.AutorizadoAutomaticamente;
        var usuarioAutorizacaoId = 123;

        // Act
        produtor.AtualizarStatus(novoStatus, usuarioAutorizacaoId);

        // Assert
        produtor.Status.Should().Be(novoStatus);
        produtor.UsuarioAutorizacaoId.Should().Be(usuarioAutorizacaoId);
        produtor.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produtor_DeveAdicionarCultura()
    {
        // Arrange
        var produtor = CriarProdutorValido();
        var culturaId = 1;

        // Act
        produtor.AdicionarCultura(culturaId);

        // Assert
        produtor.Culturas.Should().Contain(culturaId);
        produtor.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produtor_NaoDeveAdicionarCulturaDuplicada()
    {
        // Arrange
        var produtor = CriarProdutorValido();
        var culturaId = 1;
        produtor.AdicionarCultura(culturaId);
        var quantidadeAntes = produtor.Culturas.Count;

        // Act
        produtor.AdicionarCultura(culturaId);

        // Assert
        produtor.Culturas.Count.Should().Be(quantidadeAntes);
        produtor.Culturas.Should().Contain(culturaId);
    }

    [Fact]
    public void Produtor_DeveLancarExcecaoParaCulturaIdInvalido()
    {
        // Arrange
        var produtor = CriarProdutorValido();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => produtor.AdicionarCultura(0));
        exception.Message.Should().Contain("ID da cultura deve ser maior que zero");
    }

    [Fact]
    public void Produtor_DeveRemoverCultura()
    {
        // Arrange
        var produtor = CriarProdutorValido();
        var culturaId = 1;
        produtor.AdicionarCultura(culturaId);

        // Act
        produtor.RemoverCultura(culturaId);

        // Assert
        produtor.Culturas.Should().NotContain(culturaId);
        produtor.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produtor_DeveAtualizarAreaPlantio()
    {
        // Arrange
        var produtor = CriarProdutorValido();
        var novaArea = new AreaPlantio(200m);

        // Act
        produtor.AtualizarAreaPlantio(novaArea);

        // Assert
        produtor.AreaPlantio.Should().Be(novaArea);
        produtor.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Produtor_DeveLancarExcecaoParaAreaNula()
    {
        // Arrange
        var produtor = CriarProdutorValido();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => produtor.AtualizarAreaPlantio(null!));
    }

    [Fact]
    public void Produtor_DeveArmazenarRetornosApiCheck()
    {
        // Arrange
        var produtor = CriarProdutorValido();
        var retornos = JsonDocument.Parse("""{"status": "success", "data": {"valid": true}}""");

        // Act
        produtor.ArmazenarRetornosApiCheck(retornos);

        // Assert
        produtor.RetornosApiCheckProdutor.Should().NotBeNull();
        produtor.DataAtualizacao.Should().NotBeNull();
    }

    [Theory]
    [InlineData(StatusProdutor.AutorizadoAutomaticamente, true)]
    [InlineData(StatusProdutor.AutorizadoManualmente, true)]
    [InlineData(StatusProdutor.PendenteValidacaoAutomatica, false)]
    [InlineData(StatusProdutor.PendenteValidacaoManual, false)]
    [InlineData(StatusProdutor.Negado, false)]
    public void Produtor_DeveVerificarSeEstaAutorizado(StatusProdutor status, bool esperado)
    {
        // Arrange
        var produtor = CriarProdutorValido();
        produtor.AtualizarStatus(status);

        // Act & Assert
        produtor.EstaAutorizado().Should().Be(esperado);
    }

    [Fact]
    public void Produtor_DeveObterDocumentoPrincipalCpf()
    {
        // Arrange
        var cpf = new Cpf("11144477735");
        var produtor = new Produtor("João Silva", cpf);

        // Act
        var documento = produtor.ObterDocumentoPrincipal();

        // Assert
        documento.Should().Be(cpf.ValorFormatado);
    }

    [Fact]
    public void Produtor_DeveObterDocumentoPrincipalCnpj()
    {
        // Arrange
        var cnpj = new Cnpj("11222333000181");
        var produtor = new Produtor("Fazenda ABC", null, cnpj);

        // Act
        var documento = produtor.ObterDocumentoPrincipal();

        // Assert
        documento.Should().Be(cnpj.ValorFormatado);
    }

    [Fact]
    public void Produtor_DeveRemoverCulturaInexistente()
    {
        // Arrange
        var produtor = CriarProdutorValido();
        var culturaIdInexistente = 999;

        // Act
        produtor.RemoverCultura(culturaIdInexistente);

        // Assert
        produtor.Culturas.Should().NotContain(culturaIdInexistente);
        // Não deve atualizar data de modificação se não removeu nada
    }

    [Fact]
    public void Produtor_DeveRemoverEspacosDoNome()
    {
        // Arrange
        var nomeComEspacos = "  João Silva  ";
        var cpf = new Cpf("11144477735");

        // Act
        var produtor = new Produtor(nomeComEspacos, cpf);

        // Assert
        produtor.Nome.Should().Be("João Silva");
    }

    [Fact]
    public void Produtor_DeveRemoverEspacosDaInscricaoEstadual()
    {
        // Arrange
        var nome = "João Silva";
        var cpf = new Cpf("11144477735");
        var inscricaoComEspacos = "  123456789  ";

        // Act
        var produtor = new Produtor(nome, cpf, null, inscricaoComEspacos);

        // Assert
        produtor.InscricaoEstadual.Should().Be("123456789");
    }

    [Fact]
    public void Produtor_DeveRemoverEspacosDoTipoAtividade()
    {
        // Arrange
        var nome = "João Silva";
        var cpf = new Cpf("11144477735");
        var tipoAtividadeComEspacos = "  Agricultura  ";

        // Act
        var produtor = new Produtor(nome, cpf, null, null, tipoAtividadeComEspacos);

        // Assert
        produtor.TipoAtividade.Should().Be("Agricultura");
    }

    private static Produtor CriarProdutorValido()
    {
        return new Produtor(
            "João Silva",
            new Cpf("11144477735"),
            null,
            "123456789",
            "Agricultura",
            new AreaPlantio(100m));
    }
}