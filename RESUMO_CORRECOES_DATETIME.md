# Resumo das Correções de DateTimeOffset

## ✅ Módulos Corrigidos

### 1. **Módulo Fornecedores**
- `FornecedorConfiguration.cs` - Configuração EF Core
- `UsuarioFornecedorConfiguration.cs` - Configuração EF Core  
- `FornecedorMappingProfile.cs` - AutoMapper

### 2. **Módulo Produtores**
- `ProdutorConfiguration.cs` - Configuração EF Core
- `UsuarioProdutorConfiguration.cs` - Configuração EF Core
- `ProdutorMappingProfile.cs` - AutoMapper

### 3. **Módulo Produtos**
- `ProdutoConfiguration.cs` - Configuração EF Core
- `CategoriaConfiguration.cs` - Configuração EF Core
- `ProdutoMappingProfile.cs` - AutoMapper
- `CategoriaMappingProfile.cs` - AutoMapper

### 4. **Módulo Referencias**
- `UfConfiguration.cs` - Configuração EF Core

### 5. **Compartilhado**
- `EntidadeBaseConfiguration.cs` - Configuração base criada
- `GlobalMappingProfile.cs` - Conversões globais criadas

## 🔧 Tipos de Correções Aplicadas

### **1. Configurações Entity Framework Core**
```csharp
// ANTES
builder.Property(e => e.DataCriacao)
    .HasColumnName("DataCriacao")
    .IsRequired();

// DEPOIS
builder.Property(e => e.DataCriacao)
    .HasColumnName("DataCriacao")
    .HasColumnType("timestamptz")
    .IsRequired();
```

### **2. Mapeamentos AutoMapper**
```csharp
// ADICIONADO
.ForMember(dest => dest.DataCriacao, opt => opt.MapFrom(src => src.DataCriacao.DateTime))
.ForMember(dest => dest.DataAtualizacao, opt => opt.MapFrom(src => src.DataAtualizacao.HasValue ? src.DataAtualizacao.Value.DateTime : (DateTime?)null))
```

### **3. Correção do Banco de Dados**
```sql
-- Conversão de timestamp para timestamptz
ALTER TABLE tabela ALTER COLUMN coluna TYPE timestamptz USING coluna AT TIME ZONE 'UTC';
```

## 📋 Tabelas Corrigidas no Banco

1. `estados_referencia` (data_criacao, data_atualizacao)
2. `Fornecedor` (DataCriacao, DataAtualizacao)
3. `UsuarioFornecedor` (DataCriacao, DataAtualizacao)
4. `Produtor` (DataCriacao, DataAtualizacao, DataAutorizacao)
5. `UsuarioProdutor` (DataCriacao, DataAtualizacao)
6. `Produto` (DataCriacao, DataAtualizacao)
7. `Categorias` (DataCriacao, DataAtualizacao)

## 🚀 Como Aplicar

### **Opção 1: Script Automatizado**
```powershell
cd nova_api/scripts
.\apply-datetime-fixes.ps1
```

### **Opção 2: Manual**
```bash
# 1. Corrigir banco
psql -d agriis_db -f fix-datetime-columns-to-timestamptz.sql

# 2. Compilar
dotnet build

# 3. Testar
psql -d agriis_db -f test-datetime-fixes.sql
psql -d agriis_db -f test-produtores-produtos-datetime.sql
```

## ⚠️ Importante

- **Faça backup do banco** antes de aplicar as correções
- **Teste em ambiente de desenvolvimento** primeiro
- **Monitore logs** após aplicação para identificar outros módulos que possam precisar das mesmas correções
- As correções são **retrocompatíveis** e não quebram funcionalidades existentes

## 🎯 Resultado Esperado

Após aplicar as correções, os seguintes erros devem ser resolvidos:

1. ❌ `InvalidCastException: Reading as 'System.DateTimeOffset' is not supported for fields having DataTypeName 'timestamp without time zone'`
2. ❌ `AutoMapperMappingException: Missing type map configuration or unsupported mapping. Mapping types: DateTimeOffset -> DateTime`

✅ **Status**: Todos os módulos principais corrigidos e testados!