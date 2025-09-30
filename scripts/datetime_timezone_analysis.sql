-- =====================================================
-- ANÁLISE DE INCONSISTÊNCIAS DATETIME VS DATETIMEOFFSET
-- Identifica campos DateTime mapeados para timestamptz
-- =====================================================

-- Cabeçalho do relatório
SELECT 'ANÁLISE DATETIME VS DATETIMEOFFSET' as titulo, NOW() as data_execucao;

-- =====================================================
-- 1. IDENTIFICAR COLUNAS TIMESTAMPTZ NO BANCO
-- =====================================================
SELECT '1. COLUNAS TIMESTAMPTZ NO BANCO DE DADOS' as secao;

SELECT 
    table_name as tabela,
    column_name as coluna,
    data_type as tipo_dados,
    is_nullable as permite_null,
    column_default as valor_padrao,
    CASE 
        WHEN column_name ILIKE '%data%' OR column_name ILIKE '%date%' THEN '📅 Campo de Data'
        WHEN column_name ILIKE '%criacao%' OR column_name ILIKE '%created%' THEN '🆕 Auditoria - Criação'
        WHEN column_name ILIKE '%atualizacao%' OR column_name ILIKE '%updated%' OR column_name ILIKE '%modified%' THEN '✏️  Auditoria - Atualização'
        WHEN column_name ILIKE '%login%' OR column_name ILIKE '%acesso%' THEN '🔐 Controle de Acesso'
        WHEN column_name ILIKE '%expir%' OR column_name ILIKE '%valid%' THEN '⏰ Expiração/Validade'
        WHEN column_name ILIKE '%inicio%' OR column_name ILIKE '%fim%' OR column_name ILIKE '%start%' OR column_name ILIKE '%end%' THEN '📊 Período/Vigência'
        ELSE '📋 Outro Campo Temporal'
    END as categoria_campo,
    CASE 
        WHEN column_name ILIKE '%criacao%' OR column_name ILIKE '%atualizacao%' OR column_name ILIKE '%created%' OR column_name ILIKE '%updated%' THEN 'CRÍTICO - Campo de auditoria'
        WHEN column_name ILIKE '%login%' OR column_name ILIKE '%acesso%' OR column_name ILIKE '%expir%' THEN 'ALTO - Campo de controle'
        WHEN column_name ILIKE '%inicio%' OR column_name ILIKE '%fim%' OR column_name ILIKE '%vigencia%' THEN 'MÉDIO - Campo de negócio'
        ELSE 'BAIXO - Verificar necessidade'
    END as prioridade_conversao
FROM information_schema.columns 
WHERE table_schema = 'public' 
    AND data_type = 'timestamp with time zone'
    AND table_name NOT LIKE 'backup_%'
    AND table_name NOT LIKE 'migration_%'
ORDER BY 
    CASE 
        WHEN column_name ILIKE '%criacao%' OR column_name ILIKE '%atualizacao%' THEN 1
        WHEN column_name ILIKE '%login%' OR column_name ILIKE '%acesso%' THEN 2
        ELSE 3
    END,
    table_name, column_name;

-- =====================================================
-- 2. ANÁLISE POR TABELA PRINCIPAL
-- =====================================================
SELECT '2. ANÁLISE POR TABELA PRINCIPAL' as secao;

WITH tabelas_principais AS (
    SELECT unnest(ARRAY[
        'Fornecedor', 'Produto', 'Catalogo', 'Combo', 'Cultura', 
        'Pedido', 'Proposta', 'Produtor', 'Propriedade', 'Safra',
        'usuarios', 'refresh_tokens'
    ]) as table_name
),
colunas_timestamptz AS (
    SELECT 
        tp.table_name,
        COUNT(c.column_name) as total_colunas_timestamptz,
        STRING_AGG(c.column_name, ', ' ORDER BY c.column_name) as colunas_encontradas,
        COUNT(CASE WHEN c.column_name ILIKE '%criacao%' OR c.column_name ILIKE '%created%' THEN 1 END) as colunas_criacao,
        COUNT(CASE WHEN c.column_name ILIKE '%atualizacao%' OR c.column_name ILIKE '%updated%' THEN 1 END) as colunas_atualizacao
    FROM tabelas_principais tp
    LEFT JOIN information_schema.columns c 
        ON c.table_name = tp.table_name 
        AND c.table_schema = 'public' 
        AND c.data_type = 'timestamp with time zone'
    GROUP BY tp.table_name
)
SELECT 
    table_name as tabela,
    total_colunas_timestamptz as total_timestamptz,
    colunas_criacao as campos_criacao,
    colunas_atualizacao as campos_atualizacao,
    colunas_encontradas as colunas_detalhadas,
    CASE 
        WHEN total_colunas_timestamptz = 0 THEN '✅ OK - Sem campos timestamptz'
        WHEN colunas_criacao > 0 OR colunas_atualizacao > 0 THEN '⚠️  ATENÇÃO - Campos de auditoria com timestamptz'
        WHEN total_colunas_timestamptz > 0 THEN '📋 INFO - Outros campos timestamptz'
        ELSE '❓ VERIFICAR'
    END as status_recomendacao,
    CASE 
        WHEN colunas_criacao > 0 OR colunas_atualizacao > 0 THEN 'Considerar DateTimeOffset para auditoria'
        WHEN total_colunas_timestamptz > 0 THEN 'Avaliar necessidade de timezone'
        ELSE 'Nenhuma ação necessária'
    END as acao_recomendada
FROM colunas_timestamptz
ORDER BY 
    CASE 
        WHEN colunas_criacao > 0 OR colunas_atualizacao > 0 THEN 1
        WHEN total_colunas_timestamptz > 0 THEN 2
        ELSE 3
    END,
    table_name;

-- =====================================================
-- 3. ANÁLISE DAS ENTIDADES BASE (AUDITORIA)
-- =====================================================
SELECT '3. ANÁLISE DAS ENTIDADES BASE (AUDITORIA)' as secao;

-- Verificar padrões de auditoria nas tabelas
SELECT 
    'Padrões de Auditoria Identificados' as categoria,
    table_name as tabela,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = t.table_name AND column_name = 'DataCriacao' AND table_schema = 'public') THEN 'DataCriacao'
        WHEN EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = t.table_name AND column_name = 'data_criacao' AND table_schema = 'public') THEN 'data_criacao'
        ELSE 'Sem campo criação'
    END as campo_criacao,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = t.table_name AND column_name = 'DataAtualizacao' AND table_schema = 'public') THEN 'DataAtualizacao'
        WHEN EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = t.table_name AND column_name = 'data_atualizacao' AND table_schema = 'public') THEN 'data_atualizacao'
        ELSE 'Sem campo atualização'
    END as campo_atualizacao,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = t.table_name AND (column_name = 'DataCriacao' OR column_name = 'data_criacao') AND data_type = 'timestamp with time zone' AND table_schema = 'public')
        THEN '⚠️  timestamptz - Considerar DateTimeOffset'
        WHEN EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = t.table_name AND (column_name = 'DataCriacao' OR column_name = 'data_criacao') AND table_schema = 'public')
        THEN '📋 Outro tipo - Verificar'
        ELSE '✅ Sem problemas'
    END as recomendacao_criacao,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = t.table_name AND (column_name = 'DataAtualizacao' OR column_name = 'data_atualizacao') AND data_type = 'timestamp with time zone' AND table_schema = 'public')
        THEN '⚠️  timestamptz - Considerar DateTimeOffset'
        WHEN EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = t.table_name AND (column_name = 'DataAtualizacao' OR column_name = 'data_atualizacao') AND table_schema = 'public')
        THEN '📋 Outro tipo - Verificar'
        ELSE '✅ Sem problemas'
    END as recomendacao_atualizacao
FROM (
    SELECT DISTINCT table_name 
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
        AND table_type = 'BASE TABLE'
        AND table_name NOT LIKE 'backup_%'
        AND table_name NOT LIKE 'migration_%'
        AND (column_name ILIKE '%criacao%' OR column_name ILIKE '%atualizacao%' OR column_name ILIKE '%created%' OR column_name ILIKE '%updated%')
) t
ORDER BY table_name;

-- =====================================================
-- 4. IMPACTO DA CONVERSÃO DATETIME -> DATETIMEOFFSET
-- =====================================================
SELECT '4. IMPACTO DA CONVERSÃO DATETIME -> DATETIMEOFFSET' as secao;

-- Análise de impacto por categoria de campo
WITH campos_por_categoria AS (
    SELECT 
        CASE 
            WHEN column_name ILIKE '%criacao%' OR column_name ILIKE '%created%' THEN 'Auditoria - Criação'
            WHEN column_name ILIKE '%atualizacao%' OR column_name ILIKE '%updated%' THEN 'Auditoria - Atualização'
            WHEN column_name ILIKE '%login%' OR column_name ILIKE '%acesso%' THEN 'Controle de Acesso'
            WHEN column_name ILIKE '%expir%' OR column_name ILIKE '%valid%' THEN 'Expiração/Validade'
            WHEN column_name ILIKE '%inicio%' OR column_name ILIKE '%fim%' THEN 'Período/Vigência'
            ELSE 'Outros Campos Temporais'
        END as categoria,
        COUNT(*) as total_campos,
        COUNT(DISTINCT table_name) as tabelas_afetadas,
        STRING_AGG(DISTINCT table_name, ', ' ORDER BY table_name) as lista_tabelas
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
        AND data_type = 'timestamp with time zone'
        AND table_name NOT LIKE 'backup_%'
    GROUP BY 1
)
SELECT 
    categoria,
    total_campos,
    tabelas_afetadas,
    lista_tabelas,
    CASE 
        WHEN categoria LIKE 'Auditoria%' THEN 'ALTO - Campos críticos para rastreamento'
        WHEN categoria = 'Controle de Acesso' THEN 'ALTO - Segurança e sessões'
        WHEN categoria = 'Expiração/Validade' THEN 'MÉDIO - Lógica de negócio'
        WHEN categoria = 'Período/Vigência' THEN 'MÉDIO - Funcionalidades de data'
        ELSE 'BAIXO - Avaliar caso a caso'
    END as impacto_conversao,
    CASE 
        WHEN categoria LIKE 'Auditoria%' THEN 'Recomendado - DateTimeOffset para auditoria precisa'
        WHEN categoria = 'Controle de Acesso' THEN 'Recomendado - DateTimeOffset para sessões globais'
        WHEN categoria = 'Expiração/Validade' THEN 'Avaliar - Depende do contexto de negócio'
        WHEN categoria = 'Período/Vigência' THEN 'Avaliar - Considerar timezone dos usuários'
        ELSE 'Opcional - Manter DateTime se não há necessidade de timezone'
    END as recomendacao
FROM campos_por_categoria
ORDER BY 
    CASE 
        WHEN categoria LIKE 'Auditoria%' THEN 1
        WHEN categoria = 'Controle de Acesso' THEN 2
        ELSE 3
    END,
    categoria;

-- =====================================================
-- 5. SCRIPTS DE MIGRAÇÃO PARA DATETIMEOFFSET
-- =====================================================
SELECT '5. SCRIPTS DE MIGRAÇÃO PARA DATETIMEOFFSET' as secao;

-- Gerar exemplo de migração para campos de auditoria
SELECT 
    'Exemplo de Migração - Campos de Auditoria' as categoria,
    '-- Exemplo de conversão DateTime -> DateTimeOffset em entidades base
-- ANTES (DateTime):
public DateTime DataCriacao { get; private set; }
public DateTime? DataAtualizacao { get; private set; }

-- DEPOIS (DateTimeOffset):
public DateTimeOffset DataCriacao { get; private set; }
public DateTimeOffset? DataAtualizacao { get; private set; }

-- Configuração EF Core:
builder.Property(e => e.DataCriacao)
    .HasColumnType("timestamptz")
    .IsRequired();

builder.Property(e => e.DataAtualizacao)
    .HasColumnType("timestamptz");

-- Migração de dados existentes:
UPDATE tabela SET 
    "DataCriacao" = "DataCriacao" AT TIME ZONE ''UTC'',
    "DataAtualizacao" = "DataAtualizacao" AT TIME ZONE ''UTC''
WHERE "DataCriacao" IS NOT NULL;' as exemplo_migracao,
    'Conversão de campos de auditoria para DateTimeOffset' as descricao

UNION ALL

SELECT 
    'Scripts de Validação Pós-Migração' as categoria,
    '-- Validar que todos os campos timestamptz estão sendo tratados corretamente
SELECT 
    table_name,
    column_name,
    data_type
FROM information_schema.columns 
WHERE data_type = ''timestamp with time zone''
    AND table_schema = ''public''
ORDER BY table_name, column_name;

-- Verificar se há dados com timezone inconsistente
SELECT 
    ''Fornecedor'' as tabela,
    COUNT(*) as total_registros,
    COUNT(CASE WHEN EXTRACT(timezone FROM "DataCriacao") != 0 THEN 1 END) as com_timezone_nao_utc
FROM public."Fornecedor"
WHERE "DataCriacao" IS NOT NULL;' as exemplo_migracao,
    'Validação de consistência após migração' as descricao;

-- =====================================================
-- 6. RESUMO E RECOMENDAÇÕES
-- =====================================================
SELECT '6. RESUMO E RECOMENDAÇÕES' as secao;

WITH resumo_geral AS (
    SELECT 
        COUNT(*) as total_campos_timestamptz,
        COUNT(DISTINCT table_name) as tabelas_afetadas,
        COUNT(CASE WHEN column_name ILIKE '%criacao%' OR column_name ILIKE '%atualizacao%' THEN 1 END) as campos_auditoria,
        COUNT(CASE WHEN column_name ILIKE '%login%' OR column_name ILIKE '%acesso%' THEN 1 END) as campos_acesso
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
        AND data_type = 'timestamp with time zone'
        AND table_name NOT LIKE 'backup_%'
)
SELECT 
    'Resumo Geral da Análise' as aspecto,
    rg.total_campos_timestamptz as total_campos,
    rg.tabelas_afetadas as tabelas_impactadas,
    rg.campos_auditoria as campos_criticos_auditoria,
    rg.campos_acesso as campos_criticos_acesso,
    CASE 
        WHEN rg.campos_auditoria > 0 OR rg.campos_acesso > 0 THEN 'RECOMENDADO - Migrar campos críticos para DateTimeOffset'
        WHEN rg.total_campos_timestamptz > 0 THEN 'OPCIONAL - Avaliar necessidade caso a caso'
        ELSE 'DESNECESSÁRIO - Sem campos timestamptz encontrados'
    END as recomendacao_geral,
    CASE 
        WHEN rg.campos_auditoria > 0 THEN '1. Migrar campos de auditoria (DataCriacao, DataAtualizacao)'
        WHEN rg.campos_acesso > 0 THEN '2. Migrar campos de controle de acesso'
        WHEN rg.total_campos_timestamptz > 0 THEN '3. Avaliar outros campos temporais'
        ELSE 'Nenhuma ação necessária'
    END as proxima_acao
FROM resumo_geral rg;