# Resumo da Correção: FK Fornecedor-Municipio

## Problema Identificado

A entidade `Fornecedor` estava mapeada incorretamente para o tipo `Municipio` do módulo `Referencias` em vez do módulo `Enderecos`, causando inconsistência entre o DDL do banco de dados e o mapeamento do Entity Framework Core.

### Detalhes do Problema

1. **DDL do Banco**: FK `FK_Fornecedor_Municipios_MunicipioId` aponta para tabela `municipios`
2. **EF Core**: Constraint configurada como `FK_Fornecedor_MunicipiosReferencia_MunicipioId` 
3. **Entidade**: Referenciava `Agriis.Referencias.Dominio.Entidades.Municipio` (tabela `municipios_referencia`)
4. **Impacto**: Qualquer operação `Include(f => f.Municipio)` resultaria em erro de runtime

## Correções Realizadas

### 1. Entidade Fornecedor (`Fornecedor.cs`)

**Antes:**
```csharp
using Agriis.Referencias.Dominio.Entidades;

public virtual Uf? Uf { get; private set; }
public virtual Municipio? Municipio { get; private set; }
```

**Depois:**
```csharp
using Agriis.Enderecos.Dominio.Entidades;

public virtual Estado? Estado { get; private set; }
public virtual Municipio? Municipio { get; private set; }
```

### 2. Configuração EF Core (`FornecedorConfiguration.cs`)

**Antes:**
```csharp
// Relacionamento com UF (tabela estados)
builder.HasOne(f => f.Uf)
    .WithMany()
    .HasForeignKey(f => f.UfId)
    .OnDelete(DeleteBehavior.Restrict)
    .HasConstraintName("FK_Fornecedor_Estados_UfId");

// Relacionamento com Municipio (tabela municipios_referencia)
builder.HasOne(f => f.Municipio)
    .WithMany()
    .HasForeignKey(f => f.MunicipioId)
    .OnDelete(DeleteBehavior.Restrict)
    .HasConstraintName("FK_Fornecedor_MunicipiosReferencia_MunicipioId");
```

**Depois:**
```csharp
// Relacionamento com Estado (tabela estados)
builder.HasOne(f => f.Estado)
    .WithMany()
    .HasForeignKey(f => f.UfId)
    .OnDelete(DeleteBehavior.Restrict)
    .HasConstraintName("FK_Fornecedor_Estados_UfId");

// Relacionamento com Municipio (tabela municipios)
builder.HasOne(f => f.Municipio)
    .WithMany()
    .HasForeignKey(f => f.MunicipioId)
    .OnDelete(DeleteBehavior.Restrict)
    .HasConstraintName("FK_Fornecedor_Municipios_MunicipioId");
```

### 3. Repositório (`FornecedorRepository.cs`)

**Antes:**
```csharp
.Include(f => f.Uf)
.Include(f => f.Municipio)
```

**Depois:**
```csharp
.Include(f => f.Estado)
.Include(f => f.Municipio)
```

### 4. Mapeamento AutoMapper (`FornecedorMappingProfile.cs`)

**Antes:**
```csharp
.ForMember(dest => dest.UfNome, opt => opt.MapFrom(src => src.Uf != null ? src.Uf.Nome : null))
.ForMember(dest => dest.UfCodigo, opt => opt.MapFrom(src => src.Uf != null ? src.Uf.Codigo : null))
```

**Depois:**
```csharp
.ForMember(dest => dest.UfNome, opt => opt.MapFrom(src => src.Estado != null ? src.Estado.Nome : null))
.ForMember(dest => dest.UfCodigo, opt => opt.MapFrom(src => src.Estado != null ? src.Estado.Uf : null))
```

### 5. Dependências do Projeto (`Agriis.Fornecedores.Dominio.csproj`)

**Adicionado:**
```xml
<ProjectReference Include="..\..\Enderecos\Agriis.Enderecos.Dominio\Agriis.Enderecos.Dominio.csproj" />
```

## Validação

### 1. Compilação
- ✅ Todos os projetos compilam sem erros
- ✅ Apenas warnings menores não relacionados à correção

### 2. Teste de Integração
- ✅ Criado teste `TestFornecedorMunicipioMapping`
- ✅ Valida que as propriedades de navegação usam os tipos corretos
- ✅ Teste passou com sucesso

### 3. Scripts de Validação
- ✅ Criado script SQL `validacao_fornecedor_municipio_fix.sql`
- ✅ Valida constraints FK no banco de dados
- ✅ Verifica estrutura das tabelas

## Impacto

### Positivo
- ✅ Navegação `Include(f => f.Municipio)` agora funciona corretamente
- ✅ FK constraints apontam para as tabelas corretas
- ✅ Consistência entre DDL e mapeamento EF Core
- ✅ Eliminação de erros de runtime

### Considerações
- ⚠️ Validação UF-Município ainda usa serviço do módulo Referencias
- ⚠️ Pode ser necessário revisar essa validação posteriormente

## Próximos Passos

1. **Executar migração EF Core** para aplicar as mudanças no banco
2. **Testar em ambiente de desenvolvimento** com dados reais
3. **Revisar validação UF-Município** no `FornecedorService`
4. **Continuar com outras inconsistências** identificadas no relatório

## Arquivos Modificados

- `nova_api/src/Modulos/Fornecedores/Agriis.Fornecedores.Dominio/Entidades/Fornecedor.cs`
- `nova_api/src/Modulos/Fornecedores/Agriis.Fornecedores.Infraestrutura/Configuracoes/FornecedorConfiguration.cs`
- `nova_api/src/Modulos/Fornecedores/Agriis.Fornecedores.Infraestrutura/Repositorios/FornecedorRepository.cs`
- `nova_api/src/Modulos/Fornecedores/Agriis.Fornecedores.Aplicacao/Mapeamentos/FornecedorMappingProfile.cs`
- `nova_api/src/Modulos/Fornecedores/Agriis.Fornecedores.Dominio/Agriis.Fornecedores.Dominio.csproj`

## Arquivos Criados

- `nova_api/scripts/validacao_fornecedor_municipio_fix.sql`
- `nova_api/tests/Agriis.Tests.Integration/TestFornecedorMunicipioMapping.cs`
- `nova_api/docs/task-1-fornecedor-municipio-fix-summary.md`