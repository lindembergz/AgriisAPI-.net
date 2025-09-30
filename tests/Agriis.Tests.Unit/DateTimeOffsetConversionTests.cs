using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Usuarios.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.Enums;
using Xunit;

namespace Agriis.Tests.Unit;

/// <summary>
/// Testes para validar a conversão de DateTime para DateTimeOffset
/// </summary>
public class DateTimeOffsetConversionTests
{
    [Fact]
    public void EntidadeBase_DeveUsarDateTimeOffsetParaAuditoria()
    {
        // Arrange & Act
        var entidade = new TestEntidade();
        
        // Assert
        Assert.IsType<DateTimeOffset>(entidade.DataCriacao);
        Assert.Equal(DateTimeKind.Utc, entidade.DataCriacao.DateTime.Kind);
        Assert.Equal(TimeSpan.Zero, entidade.DataCriacao.Offset);
    }
    
    [Fact]
    public void EntidadeBase_AtualizarDataModificacao_DeveUsarDateTimeOffsetUtc()
    {
        // Arrange
        var entidade = new TestEntidade();
        var dataOriginal = entidade.DataCriacao;
        
        // Act
        Thread.Sleep(10); // Garantir diferença de tempo
        entidade.AtualizarDataModificacao();
        
        // Assert
        Assert.NotNull(entidade.DataAtualizacao);
        Assert.IsType<DateTimeOffset>(entidade.DataAtualizacao.Value);
        Assert.Equal(DateTimeKind.Utc, entidade.DataAtualizacao.Value.DateTime.Kind);
        Assert.Equal(TimeSpan.Zero, entidade.DataAtualizacao.Value.Offset);
        Assert.True(entidade.DataAtualizacao > dataOriginal);
    }
    
    [Fact]
    public void Usuario_UltimoLogin_DeveUsarDateTimeOffset()
    {
        // Arrange
        var usuario = new Usuario("Test User", "test@example.com", "hashedpassword");
        
        // Act
        usuario.RegistrarLogin();
        
        // Assert
        Assert.NotNull(usuario.UltimoLogin);
        Assert.IsType<DateTimeOffset>(usuario.UltimoLogin.Value);
        Assert.Equal(DateTimeKind.Utc, usuario.UltimoLogin.Value.DateTime.Kind);
        Assert.Equal(TimeSpan.Zero, usuario.UltimoLogin.Value.Offset);
    }
    
    [Fact]
    public void UsuarioRole_DataAtribuicao_DeveUsarDateTimeOffset()
    {
        // Arrange & Act
        var usuarioRole = new UsuarioRole(1, Roles.Administrador);
        
        // Assert
        Assert.IsType<DateTimeOffset>(usuarioRole.DataAtribuicao);
        Assert.Equal(DateTimeKind.Utc, usuarioRole.DataAtribuicao.DateTime.Kind);
        Assert.Equal(TimeSpan.Zero, usuarioRole.DataAtribuicao.Offset);
    }
    
    [Fact]
    public void DateTimeOffset_DevePreservarTimezone()
    {
        // Arrange
        var utcNow = DateTimeOffset.UtcNow;
        var localNow = DateTimeOffset.Now;
        
        // Assert
        Assert.Equal(TimeSpan.Zero, utcNow.Offset);
        // Note: localNow.Offset pode ser zero se estivermos em UTC
        
        // Conversão para UTC deve manter o mesmo instante
        var utcTicks = utcNow.UtcDateTime.Ticks / 10000000;
        var localTicks = localNow.UtcDateTime.Ticks / 10000000;
        Assert.True(Math.Abs(utcTicks - localTicks) <= 1); // Diferença máxima de 1 segundo
    }
    
    [Fact]
    public void DateTimeOffset_ConversaoParaDateTime_DeveManterPrecisao()
    {
        // Arrange
        var dateTimeOffset = DateTimeOffset.UtcNow;
        
        // Act
        var dateTime = dateTimeOffset.DateTime;
        var utcDateTime = dateTimeOffset.UtcDateTime;
        
        // Assert
        Assert.Equal(DateTimeKind.Utc, dateTime.Kind);
        Assert.Equal(DateTimeKind.Utc, utcDateTime.Kind);
        Assert.Equal(dateTime, utcDateTime);
    }
    
    [Fact]
    public void DateTimeOffset_SerializacaoJson_DeveIncluirTimezone()
    {
        // Arrange
        var dateTimeOffset = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        
        // Act
        var isoString = dateTimeOffset.ToString("O"); // ISO 8601
        
        // Assert
        Assert.Contains("Z", isoString); // UTC timezone indicator
        Assert.Equal("2024-01-15T10:30:00.0000000Z", isoString);
    }
    
    [Fact]
    public void DateTimeOffset_ComparacaoComDiferentesTimezones_DeveFuncionar()
    {
        // Arrange
        var utc = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var plus3 = new DateTimeOffset(2024, 1, 15, 13, 0, 0, TimeSpan.FromHours(3));
        
        // Assert
        Assert.Equal(utc, plus3); // Mesmo instante, timezones diferentes
        Assert.Equal(utc.UtcDateTime, plus3.UtcDateTime);
    }
}

/// <summary>
/// Entidade de teste para validar comportamento da EntidadeBase
/// </summary>
public class TestEntidade : EntidadeBase
{
    public string Nome { get; private set; } = "Test";
    
    public TestEntidade() : base()
    {
    }
}