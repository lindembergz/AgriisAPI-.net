# Comandos para Gerar Nova Migração após Fix do DateTimeOffset

Após executar os scripts SQL de correção no banco de dados, você precisa gerar uma nova migração do Entity Framework para sincronizar o modelo com o banco.

## 1. Executar Scripts SQL (na ordem)

```bash
# 1. Execute primeiro o script principal
psql -h localhost -U postgres -d DBAgriis -f nova_api/scripts/fix-datetimeoffset-mapping.sql

# 2. Execute o script completo (se quiser corrigir todas as tabelas)
psql -h localhost -U postgres -d DBAgriis -f nova_api/scripts/fix-all-datetimeoffset-tables.sql

# 3. Valide as correções
psql -h localhost -U postgres -d DBAgriis -f nova_api/scripts/validate-datetimeoffset-fix.sql
```

## 2. Gerar Nova Migração EF Core

```bash
# Navegar para o diretório da API
cd nova_api/src/Agriis.Api

# Gerar nova migração
dotnet ef migrations add FixDateTimeOffsetMapping --verbose

# Verificar se a migração foi gerada corretamente
# (deve mostrar as alterações de timestamp without time zone para timestamp with time zone)

# Aplicar a migração (se necessário)
dotnet ef database update
```

## 3. Verificar se a Aplicação Funciona

```bash
# Compilar a aplicação
dotnet build

# Executar a aplicação
dotnet run --project src/Agriis.Api

# Verificar se não há mais erros de DateTimeOffset
```

## 4. Comandos de Rollback (se necessário)

Se algo der errado, você pode reverter usando:

```sql
-- Reverter apenas Catalogo e CatalogoItem
BEGIN;
ALTER TABLE "Catalogo" 
    ALTER COLUMN "DataCriacao" TYPE timestamp without time zone,
    ALTER COLUMN "DataAtualizacao" TYPE timestamp without time zone,
    ALTER COLUMN "DataInicio" TYPE timestamp without time zone,
    ALTER COLUMN "DataFim" TYPE timestamp without time zone;

ALTER TABLE "CatalogoItem" 
    ALTER COLUMN "DataCriacao" TYPE timestamp without time zone,
    ALTER COLUMN "DataAtualizacao" TYPE timestamp without time zone;
COMMIT;
```

## 5. Notas Importantes

- **Backup**: Sempre faça backup antes de executar em produção
- **Ambiente**: Teste primeiro em desenvolvimento
- **Dados**: Os dados existentes serão convertidos automaticamente usando `AT TIME ZONE 'UTC'`
- **Performance**: A conversão pode demorar em tabelas grandes
- **Timezone**: Todos os dados serão tratados como UTC durante a conversão

## 6. Verificação Final

Após tudo, execute este query para confirmar que não há mais problemas:

```sql
-- Verificar se ainda existem colunas problemáticas
SELECT 
    table_name,
    column_name,
    data_type
FROM information_schema.columns 
WHERE table_schema = 'public' 
    AND data_type = 'timestamp without time zone'
    AND column_name IN ('DataCriacao', 'DataAtualizacao')
ORDER BY table_name;

-- Resultado esperado: nenhuma linha retornada
```