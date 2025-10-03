# Migração do Campo Bairro - Fornecedor

Este diretório contém os scripts necessários para adicionar o campo **Bairro** na tabela **Fornecedor**.

## Arquivos Incluídos

1. **`add_bairro_migration.ps1`** - Script PowerShell para Windows
2. **`add_bairro_migration.sh`** - Script Bash para Linux/Mac
3. **`add_bairro_fornecedor_migration.sql`** - Script SQL direto (opcional)

## Como Executar

### Windows (PowerShell)

```powershell
# Navegar para o diretório do projeto
cd nova_api

# Executar o script
.\scripts\add_bairro_migration.ps1
```

### Linux/Mac (Bash)

```bash
# Navegar para o diretório do projeto
cd nova_api

# Tornar o script executável
chmod +x scripts/add_bairro_migration.sh

# Executar o script
./scripts/add_bairro_migration.sh
```

### Execução Manual (Entity Framework)

Se preferir executar manualmente:

```bash
# Navegar para o diretório da API
cd nova_api

# Criar a migração
dotnet ef migrations add AddBairroToFornecedor --project src/Agriis.Api

# Aplicar a migração
dotnet ef database update --project src/Agriis.Api
```

### Execução Direta SQL (Opcional)

Se preferir executar o SQL diretamente no banco:

```bash
# Conectar ao PostgreSQL e executar
psql -d sua_database -f scripts/add_bairro_fornecedor_migration.sql
```

## O que a Migração Faz

1. **Adiciona a coluna `Bairro`** na tabela `Fornecedor`
   - Tipo: `VARCHAR(100)`
   - Permite valores nulos
   
2. **Cria um índice** para melhorar performance
   - Nome: `IX_Fornecedor_Bairro`
   - Apenas para registros com bairro não nulo

3. **Adiciona comentário** na coluna para documentação

4. **Executa validações** para verificar se a migração foi aplicada corretamente

## Validação Pós-Migração

Após executar a migração, você pode validar se tudo funcionou:

```sql
-- Verificar se a coluna foi criada
SELECT column_name, data_type, character_maximum_length, is_nullable
FROM information_schema.columns 
WHERE table_name = 'Fornecedor' AND column_name = 'Bairro';

-- Verificar se o índice foi criado
SELECT indexname, tablename, indexdef
FROM pg_indexes 
WHERE tablename = 'Fornecedor' AND indexname = 'IX_Fornecedor_Bairro';
```

## Rollback (Reverter Migração)

Se precisar reverter a migração:

### Entity Framework
```bash
# Reverter para a migração anterior
dotnet ef database update NomeDaMigracaoAnterior --project src/Agriis.Api

# Ou remover a migração
dotnet ef migrations remove --project src/Agriis.Api
```

### SQL Direto
```sql
-- Remover índice
DROP INDEX IF EXISTS public."IX_Fornecedor_Bairro";

-- Remover coluna
ALTER TABLE public."Fornecedor" DROP COLUMN IF EXISTS "Bairro";
```

## Alterações no Código

A migração do banco deve ser acompanhada das seguintes alterações no código (já implementadas):

### Backend (C#)
- ✅ Entidade `Fornecedor` - Campo `Bairro` adicionado
- ✅ `FornecedorDto` - Propriedade `Bairro` adicionada
- ✅ `CriarFornecedorRequest` - Campo `Bairro` adicionado
- ✅ `AtualizarFornecedorRequest` - Campo `Bairro` adicionado
- ✅ Entity Framework Configuration - Mapeamento do campo
- ✅ Serviços - Lógica para criar/atualizar com bairro

### Frontend (Angular)
- ✅ Modelo `Fornecedor` - Propriedade `bairro` adicionada
- ✅ Componente de listagem - Coluna Bairro adicionada
- ✅ Métodos para exibir bairro na interface

## Próximos Passos

1. **Executar a migração** usando um dos scripts acima
2. **Compilar o projeto** para verificar se não há erros
3. **Testar a API** para verificar se o campo está funcionando
4. **Testar o frontend** para verificar se a coluna aparece na listagem
5. **Atualizar formulários** de criação/edição se necessário

## Troubleshooting

### Erro: "dotnet ef command not found"
```bash
# Instalar Entity Framework Tools
dotnet tool install --global dotnet-ef
```

### Erro: "Connection string not found"
- Verificar se o `appsettings.json` está configurado corretamente
- Verificar se o banco de dados está rodando

### Erro: "Migration already exists"
- Verificar se a migração já foi criada anteriormente
- Usar `dotnet ef migrations list` para ver migrações existentes

## Suporte

Se encontrar problemas:
1. Verificar os logs de erro
2. Verificar se todas as dependências estão instaladas
3. Verificar se o banco de dados está acessível
4. Consultar a documentação do Entity Framework Core