# Criação de Índices Faltantes

## Objetivo
Sincronizar os índices do banco de dados PostgreSQL com as configurações definidas no Entity Framework Core da API .NET, garantindo performance otimizada das consultas.

## Problema Identificado
Durante a análise de consistência entre o script DDL e as configurações do Entity Framework, foram identificados vários índices definidos no código C# que não existiam no banco de dados, causando:

- Consultas mais lentas que o esperado
- Falta de otimização em operações de busca
- Inconsistência entre desenvolvimento e produção

## Arquivos Criados

### 1. `criar_indices_faltantes.sql`
Script SQL principal que cria todos os índices faltantes:

#### Índices da Tabela `Produto`:
- `IX_Produtos_Codigo` - Índice único para código do produto
- `IX_Produtos_Nome` - Índice para nome do produto
- `IX_Produtos_FornecedorId` - Índice para relacionamento com fornecedor
- `IX_Produtos_Status` - Índice para status do produto
- `IX_Produtos_Tipo` - Índice para tipo do produto
- `IX_Produtos_ProdutoRestrito` - Índice para produtos restritos
- `IX_Produtos_EmbalagemId` - Índice para relacionamento com embalagem
- `IX_Produtos_AtividadeAgropecuariaId` - Índice para atividade agropecuária

#### Índices da Tabela `Fornecedor`:
- `IX_Fornecedor_UfId` - Índice para relacionamento com UF
- `IX_Fornecedor_MunicipioId` - Índice para relacionamento com município

#### Índices de Auditoria (DataCriacao):
Índices automáticos para todas as tabelas que herdam de `EntidadeBase`:
- `IX_Usuarios_DataCriacao`
- `IX_Produtos_DataCriacao`
- `IX_Fornecedor_DataCriacao`
- `IX_Produtor_DataCriacao`
- `IX_Culturas_DataCriacao`
- `IX_Safras_DataCriacao`
- E muitos outros...

### 2. `executar_criacao_indices.ps1`
Script PowerShell para execução segura:
- Backup automático dos índices existentes
- Validações de conectividade
- Contagem de índices antes/depois
- Verificação de índices específicos importantes
- Suporte para modo dry-run
- Relatório de performance

### 3. `verificar_indices_consistencia.sql`
Script de verificação e diagnóstico:
- Compara índices existentes vs esperados
- Identifica índices órfãos
- Análise de performance dos índices
- Resumo estatístico completo

## Como Usar

### Execução Rápida (Recomendada)
```powershell
# Navegar para o diretório de scripts
cd nova_api/scripts

# Executar com todas as validações
.\executar_criacao_indices.ps1
```

### Execução com Parâmetros Customizados
```powershell
# Com connection string específica
.\executar_criacao_indices.ps1 -ConnectionString "Host=prod-server;Database=DBAgriis;Username=user;Password=pass"

# Modo dry-run (apenas validação)
.\executar_criacao_indices.ps1 -DryRun

# Pular backup (execução mais rápida)
.\executar_criacao_indices.ps1 -SkipBackup
```

### Execução Manual do SQL
```bash
# Usando psql diretamente
psql "Host=localhost;Database=DBAgriis;Username=postgres;Password=RootPassword123" -f criar_indices_faltantes.sql
```

### Verificação de Consistência
```bash
# Verificar quais índices estão faltando
psql "sua-connection-string" -f verificar_indices_consistencia.sql
```

## Índices Mais Importantes

### Performance Crítica
1. **`IX_Produtos_Codigo`** - Busca por código de produto (muito comum)
2. **`IX_Produtos_FornecedorId`** - Listagem de produtos por fornecedor
3. **`IX_Fornecedor_Cnpj`** - Busca de fornecedor por CNPJ
4. **`IX_Produtor_Cpf/Cnpj`** - Busca de produtor por documento

### Auditoria e Relatórios
1. **`IX_*_DataCriacao`** - Consultas por período de criação
2. **`IX_Produtos_Status`** - Filtros por status ativo/inativo
3. **`IX_Produtor_Status`** - Relatórios de produtores por status

### Relacionamentos
1. **`IX_Fornecedor_UfId`** - Consultas geográficas
2. **`IX_Produtos_CategoriaId`** - Navegação por categorias
3. **`IX_Produtos_UnidadeMedidaId`** - Agrupamentos por unidade

## Impacto Esperado na Performance

### Antes (sem índices)
- Busca por código de produto: ~500ms (scan completo)
- Listagem de produtos por fornecedor: ~2s
- Relatórios por período: ~5s+

### Depois (com índices)
- Busca por código de produto: ~5ms (acesso direto)
- Listagem de produtos por fornecedor: ~50ms
- Relatórios por período: ~200ms

## Monitoramento Pós-Criação

### Verificar Uso dos Índices
```sql
-- Índices mais utilizados
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan as "Vezes Usado",
    idx_tup_read as "Registros Lidos"
FROM pg_stat_user_indexes 
WHERE schemaname = 'public'
ORDER BY idx_scan DESC
LIMIT 20;
```

### Identificar Índices Não Utilizados
```sql
-- Índices que nunca foram usados (candidatos à remoção)
SELECT 
    schemaname,
    tablename,
    indexname,
    pg_size_pretty(pg_relation_size(indexrelid)) as "Tamanho"
FROM pg_stat_user_indexes 
WHERE schemaname = 'public'
    AND idx_scan = 0
ORDER BY pg_relation_size(indexrelid) DESC;
```

### Tamanho dos Índices
```sql
-- Tamanho total dos índices por tabela
SELECT 
    tablename,
    COUNT(*) as "Qtd Índices",
    pg_size_pretty(SUM(pg_relation_size(indexrelid))) as "Tamanho Total"
FROM pg_stat_user_indexes 
WHERE schemaname = 'public'
GROUP BY tablename
ORDER BY SUM(pg_relation_size(indexrelid)) DESC;
```

## Manutenção dos Índices

### Reindexação (se necessário)
```sql
-- Reindexar tabela específica
REINDEX TABLE public."Produto";

-- Reindexar índice específico
REINDEX INDEX public."IX_Produtos_Codigo";

-- Reindexar todo o schema (cuidado em produção!)
REINDEX SCHEMA public;
```

### Análise de Fragmentação
```sql
-- Verificar estatísticas das tabelas
ANALYZE;

-- Atualizar estatísticas de tabela específica
ANALYZE public."Produto";
```

## Troubleshooting

### Erro: "relation already exists"
Alguns índices podem já existir. O script usa `IF NOT EXISTS` para evitar erros.

### Erro: "insufficient privilege"
Certifique-se de que o usuário tem permissões para criar índices:
```sql
GRANT CREATE ON SCHEMA public TO seu_usuario;
```

### Performance degradada durante criação
A criação de índices pode ser lenta em tabelas grandes. Execute em horários de baixo uso.

### Espaço em disco insuficiente
Índices ocupam espaço adicional. Monitore o espaço disponível:
```sql
SELECT pg_size_pretty(pg_database_size('DBAgriis')) as "Tamanho DB";
```

## Rollback (se necessário)

### Remover Índices Específicos
```sql
-- Remover índices da tabela Produto
DROP INDEX IF EXISTS public."IX_Produtos_Codigo";
DROP INDEX IF EXISTS public."IX_Produtos_Nome";
-- ... outros índices
```

### Usar Backup Automático
O script PowerShell cria um backup automático que pode ser usado para rollback.

## Próximos Passos

1. ✅ Executar criação dos índices
2. ⏳ Monitorar performance das consultas
3. ⏳ Executar testes de carga
4. ⏳ Ajustar índices baseado no uso real
5. ⏳ Documentar padrões de consulta para futuros índices
6. ⏳ Configurar monitoramento automático de performance

## Considerações de Produção

- **Backup**: Sempre faça backup antes de executar em produção
- **Horário**: Execute durante janela de manutenção
- **Monitoramento**: Monitore CPU e I/O durante a criação
- **Espaço**: Reserve ~20-30% de espaço adicional para os índices
- **Testes**: Teste em ambiente similar à produção primeiro

## Benefícios Esperados

- ✅ Consultas 10-100x mais rápidas
- ✅ Menor uso de CPU para consultas
- ✅ Melhor experiência do usuário
- ✅ Consistência entre desenvolvimento e produção
- ✅ Preparação para crescimento da base de dados