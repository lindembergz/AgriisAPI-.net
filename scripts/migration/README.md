# Geographic Tables Unification Migration

Este diretório contém todos os scripts necessários para unificar as tabelas geográficas duplicadas no banco de dados Agriis, consolidando `estados_referencia` e `municipios_referencia` nas tabelas principais `estados` e `municipios`.

## Visão Geral

A migração elimina a duplicação de dados geográficos, simplifica o schema do banco de dados e melhora a manutenção, mantendo a integridade dos dados e atualizando todas as referências de chaves estrangeiras.

## Arquivos da Migração

### Scripts SQL

1. **00_migration_setup.sql** - Configuração inicial do ambiente de migração
2. **01_backup_and_analysis.sql** - Criação de backups e análise de conflitos
3. **01.5_pais_id_migration.sql** - Migração do campo pais_id da tabela estados_referencia
4. **02_estado_unification.sql** - Unificação da tabela estados
4. **03_municipio_unification.sql** - Unificação da tabela municipios
5. **04_foreign_key_updates.sql** - Atualização de chaves estrangeiras
6. **05_migration_transaction.sql** - Transação principal da migração
7. **06_schema_cleanup.sql** - Limpeza do schema e remoção de tabelas duplicadas
8. **07_deployment_procedures.sql** - Procedimentos de deployment e rollback

### Scripts de Execução

- **execute_complete_migration.sql** - Script principal que executa toda a migração
- **Execute-GeographicMigration.ps1** - Script PowerShell para execução automatizada

## Pré-requisitos

### Banco de Dados
- PostgreSQL 13+
- Permissões de administrador no banco
- Espaço suficiente para tabelas de backup
- Backup completo do banco antes da execução

### Aplicação
- .NET 9
- Entity Framework Core 9.0.0
- Testes de integração configurados

## Execução da Migração

### Opção 1: Execução Manual (SQL)

```sql
-- Conectar ao banco de dados
psql -h localhost -U username -d agriis

-- Executar migração completa
\i execute_complete_migration.sql
```

### Opção 2: Execução Automatizada (PowerShell)

```powershell
# Execução normal
.\Execute-GeographicMigration.ps1 -ConnectionString "Host=localhost;Database=agriis;Username=user;Password=pass"

# Execução com backup personalizado
.\Execute-GeographicMigration.ps1 -ConnectionString "..." -BackupPath "C:\Backups"

# Dry run (simulação)
.\Execute-GeographicMigration.ps1 -ConnectionString "..." -DryRun

# Pular backup (não recomendado)
.\Execute-GeographicMigration.ps1 -ConnectionString "..." -SkipBackup

# Execução verbosa
.\Execute-GeographicMigration.ps1 -ConnectionString "..." -Verbose
```

## Validação Pós-Migração

### 1. Verificação de Dados

```sql
-- Verificar contagem de estados (deve ser 27)
SELECT COUNT(*) FROM estados;

-- Verificar contagem de municípios (deve ser > 5000)
SELECT COUNT(*) FROM municipios;

-- Verificar se tabelas de referência foram removidas
SELECT table_name FROM information_schema.tables 
WHERE table_name LIKE '%_referencia';
```

### 2. Testes de Integridade

```sql
-- Verificar integridade de chaves estrangeiras
SELECT COUNT(*) FROM "Fornecedor" f
LEFT JOIN estados e ON f."UfId" = e.id
WHERE f."UfId" IS NOT NULL AND e.id IS NULL;

-- Verificar relacionamentos de endereços
SELECT COUNT(*) FROM enderecos en
LEFT JOIN municipios m ON en.municipio_id = m.id
WHERE en.municipio_id IS NOT NULL AND m.id IS NULL;
```

### 3. Testes de Performance

Execute os testes de integração:

```bash
cd nova_api
dotnet test tests/Agriis.Tests.Integration/GeographicMigrationTests.cs
dotnet test tests/Agriis.Tests.Integration/GeographicPerformanceTests.cs
```

## Rollback

Em caso de problemas, use o procedimento de rollback:

```sql
-- Executar rollback
SELECT * FROM rollback_geographic_migration('DEPLOYMENT_ID');

-- Verificar status do rollback
SELECT * FROM check_deployment_status('DEPLOYMENT_ID');
```

Ou via PowerShell (se a migração falhou):

```powershell
# O script automaticamente tenta rollback em caso de erro
# Verifique os logs para detalhes
```

## Monitoramento

### Logs de Migração

```sql
-- Verificar logs da migração
SELECT * FROM migration_log 
WHERE created_at >= CURRENT_DATE 
ORDER BY created_at;

-- Verificar status de deployment
SELECT * FROM deployment_status 
WHERE deployment_id = 'SEU_DEPLOYMENT_ID'
ORDER BY start_time;
```

### Limpeza de Logs

```sql
-- Limpar logs antigos (manter últimos 30 dias)
SELECT cleanup_deployment_logs(30);
```

## Estrutura Final

Após a migração bem-sucedida:

### Tabelas Mantidas
- `estados` - Tabela unificada de estados (agora inclui campo `pais_id`)
- `municipios` - Tabela unificada de municípios
- `enderecos` - Mantém referências para tabelas unificadas

### Tabelas Removidas
- `estados_referencia` - Removida após migração
- `municipios_referencia` - Removida após migração

### Tabelas de Backup (Preservadas)
- `estados_backup` - Backup da tabela estados original
- `municipios_backup` - Backup da tabela municipios original
- `estados_referencia_backup` - Backup da tabela estados_referencia
- `municipios_referencia_backup` - Backup da tabela municipios_referencia
- `Fornecedor_backup` - Backup da tabela Fornecedor

## Próximos Passos

### 1. Atualização do Entity Framework

```bash
# Gerar nova migração EF
dotnet ef migrations add UnifyGeographicTables --project src/Agriis.Api

# Aplicar migração EF
dotnet ef database update --project src/Agriis.Api
```

### 2. Validação da Aplicação

- Execute todos os testes de integração
- Teste funcionalidades relacionadas a dados geográficos
- Valide performance das consultas geográficas
- Monitore logs da aplicação

### 3. Limpeza (Opcional)

Após validação completa (recomendado aguardar 30 dias):

```sql
-- Remover tabelas de backup (CUIDADO!)
DROP TABLE IF EXISTS estados_backup;
DROP TABLE IF EXISTS municipios_backup;
DROP TABLE IF EXISTS estados_referencia_backup;
DROP TABLE IF EXISTS municipios_referencia_backup;
DROP TABLE IF EXISTS "Fornecedor_backup";
```

## Troubleshooting

### Problemas Comuns

1. **Erro de permissões**
   - Verifique se o usuário tem permissões de administrador
   - Confirme acesso de escrita no diretório de backup

2. **Timeout de conexão**
   - Aumente o timeout na string de conexão
   - Execute durante horário de baixo tráfego

3. **Espaço insuficiente**
   - Verifique espaço em disco antes da execução
   - Considere executar limpeza de dados antigos

4. **Conflitos de dados**
   - Revise o relatório de análise de conflitos
   - Execute scripts de correção se necessário

### Contatos de Suporte

- Equipe de Desenvolvimento: [email]
- DBA: [email]
- Infraestrutura: [email]

## Changelog

- **v1.0** - Versão inicial da migração
- Criação de todos os scripts de migração
- Implementação de procedimentos de rollback
- Testes de integração e performance