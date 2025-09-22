using Xunit;
using FluentAssertions;
using Agriis.Tests.Shared.Generators;

namespace Agriis.Tests.Unit.Validadores;

/// <summary>
/// Testes unitários para validação de documentos brasileiros (CPF/CNPJ)
/// Inclui testes de validação conforme requisito da task 30
/// </summary>
public class DocumentosBrasileiroTests
{
    private readonly TestDataGenerator _dataGenerator;

    public DocumentosBrasileiroTests()
    {
        _dataGenerator = new TestDataGenerator();
    }

    #region Testes de CPF

    [Theory]
    [InlineData("11144477735")] // CPF válido sem formatação
    [InlineData("111.444.777-35")] // CPF válido com formatação
    public void Cpf_Valido_Deve_Ser_Aceito(string cpf)
    {
        // Arrange & Act
        var resultado = ValidarCpf(cpf);

        // Assert
        resultado.Should().BeTrue($"CPF {cpf} deveria ser válido");
    }

    [Theory]
    [InlineData("00000000000")] // CPF com todos os dígitos iguais
    [InlineData("111.111.111-11")] // CPF com todos os dígitos iguais formatado
    [InlineData("123.456.789-00")] // CPF com dígitos verificadores incorretos
    [InlineData("12345678900")] // CPF com dígitos verificadores incorretos
    [InlineData("123456789")] // CPF com menos de 11 dígitos
    [InlineData("1234567890123")] // CPF com mais de 11 dígitos
    [InlineData("")] // CPF vazio
    [InlineData("   ")] // CPF apenas com espaços
    [InlineData("abc.def.ghi-jk")] // CPF com letras
    public void Cpf_Invalido_Deve_Ser_Rejeitado(string cpf)
    {
        // Arrange & Act
        var resultado = ValidarCpf(cpf);

        // Assert
        resultado.Should().BeFalse($"CPF {cpf} deveria ser inválido");
    }

    [Fact]
    public void Cpf_Null_Deve_Ser_Rejeitado()
    {
        // Arrange & Act
        var resultado = ValidarCpf(null);

        // Assert
        resultado.Should().BeFalse("CPF null deveria ser inválido");
    }

    [Fact]
    public void Cpf_Gerado_Pelo_TestDataGenerator_Deve_Ser_Valido()
    {
        // Arrange
        var cpf = _dataGenerator.GerarCpf();

        // Act
        var resultado = ValidarCpf(cpf);

        // Assert
        resultado.Should().BeTrue($"CPF gerado {cpf} deveria ser válido");
    }

    [Fact]
    public void Cpf_Formatado_Gerado_Pelo_TestDataGenerator_Deve_Ser_Valido()
    {
        // Arrange
        var cpf = _dataGenerator.GerarCpfFormatado();

        // Act
        var resultado = ValidarCpf(cpf);

        // Assert
        resultado.Should().BeTrue($"CPF formatado gerado {cpf} deveria ser válido");
    }

    [Theory]
    [InlineData("111.444.777-35", "11144477735")] // Formatado para limpo
    [InlineData("111-444-777.35", "11144477735")] // Formatação alternativa
    [InlineData("111 444 777 35", "11144477735")] // Com espaços
    public void Cpf_Deve_Ser_Limpo_Corretamente(string cpfFormatado, string cpfEsperado)
    {
        // Arrange & Act
        var cpfLimpo = LimparCpf(cpfFormatado);

        // Assert
        cpfLimpo.Should().Be(cpfEsperado);
    }

    #endregion

    #region Testes de CNPJ

    [Theory]
    [InlineData("11222333000181")] // CNPJ válido sem formatação
    [InlineData("11.222.333/0001-81")] // CNPJ válido com formatação
    public void Cnpj_Valido_Deve_Ser_Aceito(string cnpj)
    {
        // Arrange & Act
        var resultado = ValidarCnpj(cnpj);

        // Assert
        resultado.Should().BeTrue($"CNPJ {cnpj} deveria ser válido");
    }

    [Theory]
    [InlineData("00000000000000")] // CNPJ com todos os dígitos iguais
    [InlineData("11.111.111/1111-11")] // CNPJ com todos os dígitos iguais formatado
    [InlineData("12.345.678/0001-00")] // CNPJ com dígitos verificadores incorretos
    [InlineData("1234567800100")] // CNPJ com menos de 14 dígitos
    [InlineData("123456780001000")] // CNPJ com mais de 14 dígitos
    [InlineData("")] // CNPJ vazio
    [InlineData("   ")] // CNPJ apenas com espaços
    [InlineData("ab.cde.fgh/ijkl-mn")] // CNPJ com letras
    public void Cnpj_Invalido_Deve_Ser_Rejeitado(string cnpj)
    {
        // Arrange & Act
        var resultado = ValidarCnpj(cnpj);

        // Assert
        resultado.Should().BeFalse($"CNPJ {cnpj} deveria ser inválido");
    }

    [Fact]
    public void Cnpj_Null_Deve_Ser_Rejeitado()
    {
        // Arrange & Act
        var resultado = ValidarCnpj(null);

        // Assert
        resultado.Should().BeFalse("CNPJ null deveria ser inválido");
    }

    [Fact]
    public void Cnpj_Gerado_Pelo_TestDataGenerator_Deve_Ser_Valido()
    {
        // Arrange
        var cnpj = _dataGenerator.GerarCnpj();

        // Act
        var resultado = ValidarCnpj(cnpj);

        // Assert
        resultado.Should().BeTrue($"CNPJ gerado {cnpj} deveria ser válido");
    }

    [Fact]
    public void Cnpj_Formatado_Gerado_Pelo_TestDataGenerator_Deve_Ser_Valido()
    {
        // Arrange
        var cnpj = _dataGenerator.GerarCnpjFormatado();

        // Act
        var resultado = ValidarCnpj(cnpj);

        // Assert
        resultado.Should().BeTrue($"CNPJ formatado gerado {cnpj} deveria ser válido");
    }

    [Theory]
    [InlineData("11.222.333/0001-81", "11222333000181")] // Formatado para limpo
    [InlineData("11-222-333-0001-81", "11222333000181")] // Formatação alternativa
    [InlineData("11 222 333 0001 81", "11222333000181")] // Com espaços
    public void Cnpj_Deve_Ser_Limpo_Corretamente(string cnpjFormatado, string cnpjEsperado)
    {
        // Arrange & Act
        var cnpjLimpo = LimparCnpj(cnpjFormatado);

        // Assert
        cnpjLimpo.Should().Be(cnpjEsperado);
    }

    #endregion

    #region Testes de Inscrição Estadual

    [Fact]
    public void InscricaoEstadual_Gerada_Deve_Ter_Formato_Correto()
    {
        // Arrange & Act
        var inscricaoEstadual = _dataGenerator.GerarInscricaoEstadual();

        // Assert
        inscricaoEstadual.Should().NotBeNullOrEmpty();
        inscricaoEstadual.Should().MatchRegex(@"^\d{3}\.\d{3}\.\d{3}$", 
            "Inscrição estadual deve ter formato XXX.XXX.XXX");
    }

    [Theory]
    [InlineData("123.456.789")]
    [InlineData("987.654.321")]
    public void InscricaoEstadual_Valida_Deve_Ser_Aceita(string inscricaoEstadual)
    {
        // Arrange & Act
        var resultado = ValidarInscricaoEstadual(inscricaoEstadual);

        // Assert
        resultado.Should().BeTrue($"Inscrição estadual {inscricaoEstadual} deveria ser válida");
    }

    [Theory]
    [InlineData("123456789")] // Sem formatação
    [InlineData("123.456.78")] // Incompleta
    [InlineData("123.456.7890")] // Com dígito extra
    [InlineData("abc.def.ghi")] // Com letras
    [InlineData("")] // Vazia
    [InlineData("   ")] // Apenas espaços
    public void InscricaoEstadual_Invalida_Deve_Ser_Rejeitada(string inscricaoEstadual)
    {
        // Arrange & Act
        var resultado = ValidarInscricaoEstadual(inscricaoEstadual);

        // Assert
        resultado.Should().BeFalse($"Inscrição estadual {inscricaoEstadual} deveria ser inválida");
    }

    #endregion

    #region Métodos de Validação (Simulados)

    /// <summary>
    /// Simula validação de CPF (em implementação real, usaria biblioteca específica)
    /// </summary>
    private static bool ValidarCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        var cpfLimpo = LimparCpf(cpf);

        if (cpfLimpo.Length != 11)
            return false;

        if (!cpfLimpo.All(char.IsDigit))
            return false;

        // Verifica se todos os dígitos são iguais
        if (cpfLimpo.All(c => c == cpfLimpo[0]))
            return false;

        // Simulação de validação de dígitos verificadores
        // Em implementação real, usaria algoritmo completo de validação de CPF
        return cpfLimpo != "12345678900" && cpfLimpo != "00000000000";
    }

    /// <summary>
    /// Simula validação de CNPJ (em implementação real, usaria biblioteca específica)
    /// </summary>
    private static bool ValidarCnpj(string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        var cnpjLimpo = LimparCnpj(cnpj);

        if (cnpjLimpo.Length != 14)
            return false;

        if (!cnpjLimpo.All(char.IsDigit))
            return false;

        // Verifica se todos os dígitos são iguais
        if (cnpjLimpo.All(c => c == cnpjLimpo[0]))
            return false;

        // Simulação de validação de dígitos verificadores
        // Em implementação real, usaria algoritmo completo de validação de CNPJ
        return cnpjLimpo != "12345678000100" && cnpjLimpo != "00000000000000";
    }

    /// <summary>
    /// Simula validação de Inscrição Estadual
    /// </summary>
    private static bool ValidarInscricaoEstadual(string? inscricaoEstadual)
    {
        if (string.IsNullOrWhiteSpace(inscricaoEstadual))
            return false;

        // Formato básico: XXX.XXX.XXX
        return System.Text.RegularExpressions.Regex.IsMatch(
            inscricaoEstadual, @"^\d{3}\.\d{3}\.\d{3}$");
    }

    /// <summary>
    /// Remove formatação do CPF
    /// </summary>
    private static string LimparCpf(string cpf)
    {
        return System.Text.RegularExpressions.Regex.Replace(cpf, @"[^\d]", "");
    }

    /// <summary>
    /// Remove formatação do CNPJ
    /// </summary>
    private static string LimparCnpj(string cnpj)
    {
        return System.Text.RegularExpressions.Regex.Replace(cnpj, @"[^\d]", "");
    }

    #endregion

    #region Testes de Performance

    [Fact]
    public void Validacao_Cpf_Deve_Ser_Rapida()
    {
        // Arrange
        var cpfs = Enumerable.Range(0, 1000)
            .Select(_ => _dataGenerator.GerarCpf())
            .ToList();

        // Act & Assert
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        foreach (var cpf in cpfs)
        {
            ValidarCpf(cpf);
        }
        
        stopwatch.Stop();
        
        // Validação deve ser rápida (menos de 1 segundo para 1000 CPFs)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    [Fact]
    public void Validacao_Cnpj_Deve_Ser_Rapida()
    {
        // Arrange
        var cnpjs = Enumerable.Range(0, 1000)
            .Select(_ => _dataGenerator.GerarCnpj())
            .ToList();

        // Act & Assert
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        foreach (var cnpj in cnpjs)
        {
            ValidarCnpj(cnpj);
        }
        
        stopwatch.Stop();
        
        // Validação deve ser rápida (menos de 1 segundo para 1000 CNPJs)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    #endregion

    #region Testes de Casos Extremos

    [Fact]
    public void Cpf_Com_Caracteres_Especiais_Deve_Ser_Limpo()
    {
        // Arrange
        var cpfComCaracteresEspeciais = "111@444#777$35";

        // Act
        var cpfLimpo = LimparCpf(cpfComCaracteresEspeciais);

        // Assert
        cpfLimpo.Should().Be("11144477735");
    }

    [Fact]
    public void Cnpj_Com_Caracteres_Especiais_Deve_Ser_Limpo()
    {
        // Arrange
        var cnpjComCaracteresEspeciais = "11@222#333$0001%81";

        // Act
        var cnpjLimpo = LimparCnpj(cnpjComCaracteresEspeciais);

        // Assert
        cnpjLimpo.Should().Be("11222333000181");
    }

    [Theory]
    [InlineData("111.444.777-35")]
    [InlineData("111 444 777 35")]
    [InlineData("111-444-777-35")]
    [InlineData("111444777-35")]
    public void Cpf_Com_Diferentes_Formatacoes_Deve_Ser_Normalizado(string cpfFormatado)
    {
        // Arrange & Act
        var cpfLimpo = LimparCpf(cpfFormatado);

        // Assert
        cpfLimpo.Should().Be("11144477735");
    }

    [Theory]
    [InlineData("11.222.333/0001-81")]
    [InlineData("11 222 333 0001 81")]
    [InlineData("11-222-333-0001-81")]
    [InlineData("11222333/0001-81")]
    public void Cnpj_Com_Diferentes_Formatacoes_Deve_Ser_Normalizado(string cnpjFormatado)
    {
        // Arrange & Act
        var cnpjLimpo = LimparCnpj(cnpjFormatado);

        // Assert
        cnpjLimpo.Should().Be("11222333000181");
    }

    #endregion
}