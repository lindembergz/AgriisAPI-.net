# Task 28: Sistema de Logging - Implementação Completa

## Resumo da Implementação

Implementado sistema de logging estruturado completo com Serilog, incluindo múltiplos destinos, correlação de logs por request, configuração por ambiente e logging de performance.

## Componentes Implementados

### 1. Configuração Centralizada do Serilog

**Arquivo:** `Agriis.Compartilhado.Infraestrutura/Logging/SerilogConfiguration.cs`

- Configuração automática baseada no ambiente
- Múltiplos sinks (Console, File, PostgreSQL, Seq)
- Enriquecimento automático com informações de contexto
- Filtros para reduzir ruído nos logs
- Configuração de níveis de log por namespace

### 2. Contexto de Logging Estruturado

**Arquivos:**
- `ILoggingContext.cs` - Interface para contexto de logging
- `LoggingContext.cs` - Implementação thread-safe do contexto
- `CorrelationIdEnricher.cs` - Enriquecedor para correlação de requests
- `UserContextEnricher.cs` - Enriquecedor para informações do usuário

### 3. Extensões de Logging

**Arquivo:** `LoggerExtensions.cs`

Métodos de conveniência para logging estruturado:
- `BeginOperation()` - Scope automático para operações
- `LogStructuredError()` - Logs de erro com contexto
- `LogPerformance()` - Logs de performance
- `LogAudit()` - Logs de auditoria
- `LogSecurity()` - Logs de segurança
- `LogBusinessEvent()` - Eventos de negócio

### 4. Logger de Performance

**Arquivo:** `PerformanceLogger.cs`

- Monitoramento automático de operações
- Logs específicos para banco de dados
- Logs de chamadas para APIs externas
- Monitoramento de uso de memória
- Alertas automáticos para operações lentas

### 5. Middleware de Request Logging Aprimorado

**Arquivo:** `RequestLoggingMiddleware.cs`

Funcionalidades:
- Correlação automática de requests
- Logging configurável de request/response body
- Mascaramento de dados sensíveis
- Integração com contexto de logging
- Configuração por ambiente

### 6. Configuração por Ambiente

#### Desenvolvimento (`appsettings.Development.json`)
- Nível Debug habilitado
- Logs detalhados do Entity Framework
- Suporte opcional ao Seq
- Request/Response body logging habilitado

#### Produção (`appsettings.Production.json`)
- Formato JSON compacto no console
- Sink PostgreSQL habilitado
- Logs de erro em arquivo separado
- Configuração via variáveis de ambiente

#### Testes (`appsettings.Testing.json`)
- Logs mínimos para reduzir ruído
- Apenas erros críticos
- Request logging desabilitado

## Configurações Principais

### Sinks Configurados

1. **Console**
   - Desenvolvimento: Template legível
   - Produção: JSON compacto

2. **File**
   - Logs gerais: rotação diária, 30 dias
   - Logs de erro: arquivo separado, 90 dias
   - Flush automático a cada segundo

3. **PostgreSQL** (apenas produção)
   - Tabela `logs` com colunas estruturadas
   - Batch de 50 registros a cada 5 segundos
   - Criação automática da tabela

4. **Seq** (opcional para desenvolvimento)
   - Interface web para análise de logs
   - Configuração via `Serilog:Seq:ServerUrl`

### Enriquecimento Automático

- **CorrelationId**: ID único por request
- **UserId/UserEmail**: Informações do usuário autenticado
- **MachineName**: Nome da máquina
- **Environment**: Ambiente de execução
- **ProcessId/ThreadId**: Identificadores de processo/thread
- **RequestPath/Method**: Informações da requisição HTTP

### Filtros Implementados

- Health checks (`/health`, `/metrics`)
- Arquivos estáticos (CSS, JS, imagens)
- Swagger UI
- Favicon

## Exemplos de Uso

### 1. Logging Básico com Contexto

```csharp
public class ProdutorService
{
    private readonly ILogger<ProdutorService> _logger;
    
    public async Task<Produtor> CriarProdutorAsync(CriarProdutorDto dto)
    {
        using var operation = _logger.BeginOperation("CriarProdutor", new { dto.Nome, dto.Cpf });
        
        try
        {
            var produtor = new Produtor(dto.Nome, dto.Cpf);
            await _repository.AdicionarAsync(produtor);
            
            _logger.LogBusinessEvent("ProdutorCriado", new { produtor.Id, produtor.Nome });
            
            return produtor;
        }
        catch (Exception ex)
        {
            _logger.LogStructuredError(ex, "Erro ao criar produtor", new { dto.Nome });
            throw;
        }
    }
}
```

### 2. Performance Logging

```csharp
public class ProdutorRepository
{
    private readonly IPerformanceLogger _performanceLogger;
    
    public async Task<List<Produtor>> ObterTodosAsync()
    {
        using var operation = _performanceLogger.BeginOperation("ObterTodosProdutores");
        
        var stopwatch = Stopwatch.StartNew();
        var produtores = await _context.Produtores.ToListAsync();
        stopwatch.Stop();
        
        _performanceLogger.LogDatabaseOperation("SELECT Produtores", stopwatch.Elapsed, produtores.Count);
        
        return produtores;
    }
}
```

### 3. Logging de Auditoria

```csharp
public class AuthController
{
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _authService.AuthenticateAsync(dto.Email, dto.Password);
        
        if (user != null)
        {
            _logger.LogAudit("UserLogin", user.Id.ToString(), new { dto.Email, IpAddress = HttpContext.Connection.RemoteIpAddress });
        }
        else
        {
            _logger.LogSecurity("LoginFailed", null, HttpContext.Connection.RemoteIpAddress?.ToString(), new { dto.Email });
        }
        
        return Ok();
    }
}
```

## Configuração de Produção

### Variáveis de Ambiente

```bash
# Conexão para logs
LOG_DB_CONNECTION="Host=prod-db;Database=AgriisLogs;Username=logger;Password=***"

# Configurações de retenção
LOG_RETENTION_DAYS=90
LOG_BATCH_SIZE=100
```

### Docker Compose

```yaml
services:
  agriis-api:
    environment:
      - LOG_DB_CONNECTION=Host=postgres;Database=AgriisLogs;Username=logger;Password=logpass
    volumes:
      - ./logs:/app/logs
```

## Monitoramento e Alertas

### Métricas Importantes

1. **Logs de Erro por Minuto**
   - Threshold: > 10 erros/min
   - Ação: Alerta para equipe

2. **Operações Lentas**
   - Database: > 5 segundos
   - Business: > 3 segundos
   - External API: > 10 segundos

3. **Uso de Memória**
   - Threshold: > 100MB de diferença
   - Ação: Investigar vazamentos

### Queries Úteis (PostgreSQL)

```sql
-- Erros nas últimas 24 horas
SELECT message, exception, timestamp, properties
FROM logs 
WHERE level = 'Error' 
  AND timestamp > NOW() - INTERVAL '24 hours'
ORDER BY timestamp DESC;

-- Operações mais lentas
SELECT message, properties->>'ElapsedMilliseconds' as elapsed_ms
FROM logs 
WHERE message LIKE '%PERFORMANCE%'
  AND (properties->>'ElapsedMilliseconds')::numeric > 1000
ORDER BY (properties->>'ElapsedMilliseconds')::numeric DESC;

-- Usuários mais ativos
SELECT properties->>'UserId' as user_id, COUNT(*) as requests
FROM logs 
WHERE properties->>'UserId' IS NOT NULL
  AND timestamp > NOW() - INTERVAL '1 hour'
GROUP BY properties->>'UserId'
ORDER BY requests DESC;
```

## Benefícios Implementados

1. **Observabilidade Completa**
   - Correlação de logs por request
   - Contexto estruturado em todos os logs
   - Métricas de performance automáticas

2. **Configuração Flexível**
   - Diferentes níveis por ambiente
   - Múltiplos destinos configuráveis
   - Filtros para reduzir ruído

3. **Segurança**
   - Mascaramento automático de dados sensíveis
   - Logs de auditoria e segurança
   - Configuração de headers sensíveis

4. **Performance**
   - Logging assíncrono
   - Batching para PostgreSQL
   - Filtros para reduzir overhead

5. **Facilidade de Uso**
   - Extensões para logging estruturado
   - Scopes automáticos para operações
   - Integração transparente com DI

## Próximos Passos

1. **Integração com APM** (Application Performance Monitoring)
2. **Dashboards no Grafana** para visualização
3. **Alertas automáticos** baseados em métricas
4. **Análise de logs com IA** para detecção de padrões
5. **Compliance** com LGPD para logs de dados pessoais

## Requisitos Atendidos

✅ **14.3** - Configurar Serilog com múltiplos destinos
✅ **14.3** - Implementar structured logging  
✅ **14.3** - Configurar log levels por ambiente
✅ **14.3** - Implementar correlação de logs por request

O sistema de logging está completamente implementado e pronto para uso em todos os ambientes.