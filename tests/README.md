# Infraestrutura de Testes - Agriis

Este documento descreve a infraestrutura de testes implementada para o projeto Agriis, equivalente ao sistema de testes do Python original.

## Estrutura de Projetos

```
tests/
├── Agriis.Tests.Shared/           # Infraestrutura compartilhada de testes
├── Agriis.Tests.Unit/             # Testes unitários
├── Agriis.Tests.Integration/      # Testes de integração
└── Agriis.Pedidos.Tests.Unit/     # Testes unitários específicos do módulo Pedidos
```

## Componentes Principais

### 1. BaseTestCase

Classe base para todos os testes, equivalente ao `BaseTestCase` do Python. Fornece:

- Setup automático da aplicação de teste
- Cliente HTTP configurado
- Contexto de banco de dados em memória
- Sistema de autenticação para testes
- Helpers para operações HTTP (GET, POST, PUT, DELETE)
- Helpers para operações de banco de dados

**Uso:**

```csharp
public class MeuTeste : BaseTestCase
{
    public MeuTeste(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task MeuTeste_DevePassar()
    {
        // Arrange
        await AuthenticateAsProducerAsync();
        
        // Act
        var response = await GetAsync("/api/produtores");
        
        // Assert
        JsonMatchers.ShouldBeSuccessful(response);
    }
}
```

### 2. TestWebApplicationFactory

Factory para criar aplicação de teste com:

- Banco de dados em memória (InMemory)
- Configurações de teste
- Serviços de background desabilitados
- Logging configurado para testes

### 3. TestUserAuth

Sistema de autenticação para testes, equivalente ao `IUserAuth` do Python:

- Geração de tokens JWT para diferentes roles
- Usuários de teste pré-configurados
- Validação de tokens
- Extração de claims

**Roles disponíveis:**
- `PRODUTOR` - Produtor rural
- `FORNECEDOR` - Fornecedor de insumos
- `ADMIN` - Administrador do sistema
- `REPRESENTANTE` - Representante comercial

**Uso:**

```csharp
// Autenticar como produtor
await AuthenticateAsProducerAsync();

// Autenticar como fornecedor específico
await AuthenticateAsSupplierAsync(fornecedorId: 123);

// Gerar token customizado
var token = await UserAuth.GetTokenAsync("ADMIN", userId: 456);
```

### 4. TestDataGenerator

Gerador de dados de teste, equivalente ao `random_generators.py`:

- Documentos brasileiros válidos (CPF, CNPJ)
- Dados pessoais (nomes, emails, telefones)
- Endereços completos com geolocalização
- Dados agrícolas (culturas, áreas, produtos)
- Datas e períodos
- Helpers para listas e escolhas aleatórias

**Uso:**

```csharp
var generator = new TestDataGenerator();

// Documentos
var cpf = generator.GerarCpf();
var cnpj = generator.GerarCnpj();

// Dados pessoais
var nome = generator.GerarNome();
var email = generator.GerarEmail();

// Endereço completo
var endereco = generator.GerarEndereco();

// Dados agrícolas
var cultura = generator.GerarNomeCultura();
var area = generator.GerarAreaPlantio(min: 10, max: 1000);
```

### 5. JsonMatchers

Validadores para respostas JSON, equivalente aos `JsonMatchers` do Python:

- Validação de status codes
- Validação de estruturas JSON
- Validação de propriedades obrigatórias
- Validação de estruturas de paginação
- Validação de estruturas de erro
- Validação de tipos de dados

**Uso:**

```csharp
var response = await GetAsync("/api/produtores");

// Validações básicas
JsonMatchers.ShouldBeSuccessful(response);
var json = await JsonMatchers.ShouldHaveValidJsonAsync(response);

// Validações de estrutura
JsonMatchers.ShouldHavePaginationStructure(json);
JsonMatchers.ShouldHaveProperty(json, "items");

// Validações de array
var items = JsonMatchers.ShouldBeArray(json["items"]);
JsonMatchers.AllItemsShouldHaveRequiredProperties(items, "id", "nome");
```

## Configuração de Testes

### Configurações Automáticas

A infraestrutura configura automaticamente:

- Banco de dados em memória único por teste
- JWT com chaves de teste
- Logging reduzido (apenas warnings)
- Serviços externos desabilitados
- CORS configurado para desenvolvimento

### Configurações Customizadas

```csharp
// Configuração customizada
var customConfig = TestConfiguration.GetCustomConfiguration(new Dictionary<string, string?>
{
    ["MinhaConfiguracao"] = "MeuValor"
});
```

## Helpers de Banco de Dados

### DatabaseHelper

Fornece operações convenientes para testes:

```csharp
// Limpar todas as tabelas
await ClearDatabaseAsync();

// Verificar existência
var exists = await ExistsInDatabaseAsync<Produtor>(id: 1);

// Contar registros
var count = await CountInDatabaseAsync<Produto>();

// Executar em transação que será revertida
await ExecuteInTransactionAsync(async () =>
{
    // Operações que serão revertidas
});
```

## Padrões de Teste

### Testes de API

```csharp
[Fact]
public async Task Get_Produtores_DeveRetornarListaPaginada()
{
    // Arrange
    await AuthenticateAsProducerAsync();
    
    // Act
    var response = await GetAsync("/api/produtores");
    
    // Assert
    JsonMatchers.ShouldBeSuccessful(response);
    var json = await JsonMatchers.ShouldHaveValidJsonAsync(response);
    JsonMatchers.ShouldHavePaginationStructure(json);
}
```

### Testes com Dados

```csharp
[Fact]
public async Task Post_CriarProdutor_DeveRetornarProdutorCriado()
{
    // Arrange
    await AuthenticateAsAdminAsync();
    var novoProdutor = new
    {
        nome = DataGenerator.GerarNome(),
        cpf = DataGenerator.GerarCpf(),
        area_plantio = DataGenerator.GerarAreaPlantio()
    };
    
    // Act
    var response = await PostAsync("/api/produtores", novoProdutor);
    
    // Assert
    JsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.Created);
    var json = await JsonMatchers.ShouldHaveValidJsonAsync(response);
    JsonMatchers.ShouldHaveAuditProperties(json);
    JsonMatchers.ShouldHavePropertyWithValue(json, "nome", novoProdutor.nome);
}
```

### Testes de Validação

```csharp
[Fact]
public async Task Post_CriarProdutor_ComCpfInvalido_DeveRetornarErro()
{
    // Arrange
    await AuthenticateAsAdminAsync();
    var produtorInvalido = new { cpf = "123" }; // CPF inválido
    
    // Act
    var response = await PostAsync("/api/produtores", produtorInvalido);
    
    // Assert
    JsonMatchers.ShouldHaveStatusCode(response, HttpStatusCode.BadRequest);
    await JsonMatchers.ShouldHaveErrorStructureAsync(response, "VALIDATION_ERROR");
}
```

## Executando Testes

### Comandos Básicos

```bash
# Todos os testes
dotnet test

# Testes com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Testes específicos
dotnet test --filter "FullyQualifiedName~Produtores"

# Testes por categoria
dotnet test --filter "Category=Integration"
```

### Configuração de IDE

Para Visual Studio / VS Code, os testes aparecerão automaticamente no Test Explorer.

## Migração do Python

Esta infraestrutura replica as funcionalidades do sistema de testes Python:

| Python | C# | Descrição |
|--------|----|--------------|
| `BaseTestCase` | `BaseTestCase` | Classe base para testes |
| `IUserAuth` | `TestUserAuth` | Sistema de autenticação |
| `random_generators.py` | `TestDataGenerator` | Geração de dados de teste |
| `JsonMatchers` | `JsonMatchers` | Validação de JSON |
| `TestWebApplicationFactory` | `TestWebApplicationFactory` | Factory de aplicação |

## Próximos Passos

1. **Migrar testes existentes**: Usar esta infraestrutura para migrar os testes do Python
2. **Adicionar testes de performance**: Implementar testes de carga usando NBomber
3. **Configurar CI/CD**: Integrar com pipeline de build
4. **Relatórios de cobertura**: Configurar relatórios automáticos
5. **Testes de contrato**: Implementar testes de API contract

## Troubleshooting

### Problemas Comuns

1. **Banco não limpa entre testes**: Verificar se `ClearDatabaseAsync()` está sendo chamado
2. **Tokens inválidos**: Verificar configuração JWT nas configurações de teste
3. **Testes lentos**: Verificar se serviços de background estão desabilitados
4. **Falhas de autenticação**: Verificar se `AuthenticateAsync()` está sendo chamado antes das requisições

### Debug

Para debug detalhado, altere o nível de log:

```csharp
var config = TestConfiguration.GetCustomConfiguration(new Dictionary<string, string?>
{
    ["Logging:LogLevel:Default"] = "Debug"
});
```