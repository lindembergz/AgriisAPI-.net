-- =====================================================
-- PRE-MIGRATION VALIDATION SCRIPT
-- Valida o estado atual do banco antes das correções
-- =====================================================

-- Cabeçalho do relatório
SELECT 'PRE-MIGRATION VALIDATION REPORT' as titulo, NOW() as data_execucao;

-- =====================================================
-- 1. VALIDAÇÃO DE TABELAS EXISTENTES
-- =====================================================
SELECT '1. VALIDAÇÃO DE TABELAS EXISTENTES' as secao;

SELECT 
    'Tabelas Principais' as categoria,
    table_name as tabela,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = t.table_name AND table_schema = 'public')
        THEN '✅ Existe'
        ELSE '❌ Não existe'
    END as status
FROM (
    VALUES 
        ('Fornecedor'),
        ('estados'),
        ('municipios'),
        ('municipios_referencia'),
        ('AtividadesAgropecuarias'),
        ('Catalogo'),
        ('Combo'),
        ('Cultura'),
        ('Moedas'),
        ('UnidadesMedida'),
        ('Embalagens')
) AS t(table_name)
ORDER BY t.table_name;

-- =====================================================
-- 2. ANÁLISE DE COLUNAS DUPLICADAS "ATIVO"
-- =====================================================
SELECT '2. ANÁLISE DE COLUNAS DUPLICADAS "ATIVO"' as secao;

WITH tabelas_com_ativo AS (
    SELECT 
        table_name,
        COUNT(CASE WHEN column_name = 'Ativo' THEN 1 END) as ativo_pascal,
        COUNT(CASE WHEN column_name = 'ativo' THEN 1 END) as ativo_lower,
        STRING_AGG(
            CASE 
                WHEN column_name IN ('Ativo', 'ativo') 
                THEN column_name || ':' || data_type || ':' || COALESCE(column_default, 'NULL')
            END, 
            ', '
        ) as detalhes_colunas
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
        AND column_name IN ('Ativo', 'ativo')
    GROUP BY table_name
)
SELECT 
    table_name as tabela,
    ativo_pascal as "Ativo (PascalCase)",
    ativo_lower as "ativo (lowercase)", 
    CASE 
        WHEN ativo_pascal = 1 AND ativo_lower = 0 THEN '✅ OK - Apenas PascalCase'
        WHEN ativo_pascal = 1 AND ativo_lower = 1 THEN '⚠️  PROBLEMA - Duplicada'
        WHEN ativo_pascal = 0 AND ativo_lower = 1 THEN '❓ INFO - Apenas lowercase'
        ELSE '❌ ERRO - Estado inesperado'
    END as status,
    detalhes_colunas
FROM tabelas_com_ativo
ORDER BY 
    CASE 
        WHEN ativo_pascal = 1 AND ativo_lower = 1 THEN 1  -- Problemas primeiro
        WHEN ativo_pascal = 0 AND ativo_lower = 1 THEN 2  -- Info depois
        ELSE 3  -- OK por último
    END,
    table_name;

-- =====================================================
-- 3. ANÁLISE DE FOREIGN KEYS EXISTENTES
-- =====================================================
SELECT '3. ANÁLISE DE FOREIGN KEYS EXISTENTES' as secao;

SELECT 
    tc.table_name as tabela_origem,
    kcu.column_name as coluna_origem,
    tc.constraint_name as nome_constraint,
    ccu.table_name as tabela_destino,
    ccu.column_name as coluna_destino,
    CASE 
        WHEN tc.constraint_name LIKE '%Fornecedor%' AND tc.constraint_name LIKE '%Municipios%' THEN
            CASE 
                WHEN ccu.table_name = 'municipios' THEN '✅ OK - Aponta para municipios'
                WHEN ccu.table_name = 'municipios_referencia' THEN '❌ ERRO - Aponta para municipios_referencia'
                ELSE '❓ INFO - Tabela inesperada'
            END
        WHEN tc.constraint_name LIKE '%Fornecedor%' AND tc.constraint_name LIKE '%Estados%' THEN
            CASE 
                WHEN ccu.table_name = 'estados' THEN '✅ OK - Aponta para estados'
                ELSE '❌ ERRO - Não aponta para estados'
            END
        ELSE '📋 INFO - Outra FK'
    END as status_validacao
FROM information_schema.table_constraints AS tc 
    JOIN information_schema.key_column_usage AS kcu
        ON tc.constraint_name = kcu.constraint_name
        AND tc.table_schema = kcu.table_schema
    JOIN information_schema.constraint_column_usage AS ccu
        ON ccu.constraint_name = tc.constraint_name
        AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY' 
    AND tc.table_schema = 'public'
    AND (tc.table_name IN ('Fornecedor') OR tc.constraint_name LIKE '%Fornecedor%')
ORDER BY tc.table_name, tc.constraint_name;

-- =====================================================
-- 4. ANÁLISE DE TIPOS DE DADOS DATETIME
-- =====================================================
SELECT '4. ANÁLISE DE TIPOS DE DADOS DATETIME' as secao;

SELECT 
    table_name as tabela,
    column_name as coluna,
    data_type as tipo_atual,
    CASE 
        WHEN data_type = 'timestamp with time zone' THEN '✅ OK - timestamptz'
        WHEN data_type = 'timestamp without time zone' THEN '⚠️  ATENÇÃO - timestamp sem timezone'
        WHEN data_type = 'date' THEN '📋 INFO - date'
        ELSE '❓ INFO - Outro tipo'
    END as status_timezone,
    column_default as valor_padrao
FROM information_schema.columns 
WHERE table_schema = 'public' 
    AND (
        column_name ILIKE '%data%' 
        OR column_name ILIKE '%date%'
        OR data_type LIKE '%timestamp%'
        OR data_type = 'date'
    )
    AND table_name IN (
        'Fornecedor', 'AtividadesAgropecuarias', 'Catalogo', 'Combo', 
        'Cultura', 'Moedas', 'UnidadesMedida', 'Embalagens'
    )
ORDER BY table_name, column_name;

-- =====================================================
-- 5. ANÁLISE DE NOMES COM ASPAS
-- =====================================================
SELECT '5. ANÁLISE DE NOMES COM ASPAS' as secao;

SELECT 
    'Tabelas com Aspas' as categoria,
    table_name as nome,
    CASE 
        WHEN table_name ~ '^[A-Z]' THEN '⚠️  PascalCase - Provavelmente com aspas'
        WHEN table_name ~ '^[a-z]' THEN '✅ OK - snake_case'
        ELSE '❓ INFO - Formato misto'
    END as status_naming
FROM information_schema.tables 
WHERE table_schema = 'public' 
    AND table_type = 'BASE TABLE'
ORDER BY 
    CASE 
        WHEN table_name ~ '^[A-Z]' THEN 1  -- Problemas primeiro
        ELSE 2
    END,
    table_name;

-- =====================================================
-- 6. CONTAGEM DE DADOS PARA VALIDAÇÃO
-- =====================================================
SELECT '6. CONTAGEM DE DADOS PARA VALIDAÇÃO' as secao;

-- Verificar se há dados nas tabelas principais
SELECT 
    'Fornecedor' as tabela,
    COUNT(*) as total_registros,
    COUNT(CASE WHEN "UfId" IS NOT NULL THEN 1 END) as com_uf,
    COUNT(CASE WHEN "MunicipioId" IS NOT NULL THEN 1 END) as com_municipio
FROM public."Fornecedor"
UNION ALL
SELECT 
    'estados' as tabela,
    COUNT(*) as total_registros,
    NULL as com_uf,
    NULL as com_municipio
FROM public.estados
UNION ALL
SELECT 
    'municipios' as tabela,
    COALESCE((SELECT COUNT(*) FROM public.municipios), 0) as total_registros,
    NULL as com_uf,
    NULL as com_municipio
UNION ALL
SELECT 
    'municipios_referencia' as tabela,
    COALESCE((SELECT COUNT(*) FROM public.municipios_referencia), 0) as total_registros,
    NULL as com_uf,
    NULL as com_municipio;

-- =====================================================
-- 7. RESUMO EXECUTIVO
-- =====================================================
SELECT '7. RESUMO EXECUTIVO' as secao;

WITH 
-- Contar tabelas com colunas duplicadas
tabelas_duplicadas AS (
    SELECT COUNT(*) as total
    FROM (
        SELECT table_name
        FROM information_schema.columns 
        WHERE table_schema = 'public' 
            AND column_name IN ('Ativo', 'ativo')
        GROUP BY table_name
        HAVING COUNT(DISTINCT column_name) > 1
    ) t
),
-- Contar FKs incorretas
fks_incorretas AS (
    SELECT COUNT(*) as total
    FROM information_schema.table_constraints AS tc 
        JOIN information_schema.constraint_column_usage AS ccu
            ON ccu.constraint_name = tc.constraint_name
    WHERE tc.constraint_type = 'FOREIGN KEY' 
        AND tc.table_schema = 'public'
        AND tc.constraint_name LIKE '%Fornecedor%MunicipiosReferencia%'
),
-- Contar tabelas com PascalCase
tabelas_pascalcase AS (
    SELECT COUNT(*) as total
    FROM information_schema.tables 
    WHERE table_schema = 'public' 
        AND table_type = 'BASE TABLE'
        AND table_name ~ '^[A-Z]'
)
SELECT 
    'Colunas "ativo" duplicadas' as problema,
    td.total as quantidade,
    CASE WHEN td.total > 0 THEN '❌ CRÍTICO' ELSE '✅ OK' END as severidade
FROM tabelas_duplicadas td
UNION ALL
SELECT 
    'FKs Fornecedor incorretas' as problema,
    fi.total as quantidade,
    CASE WHEN fi.total > 0 THEN '❌ CRÍTICO' ELSE '✅ OK' END as severidade
FROM fks_incorretas fi
UNION ALL
SELECT 
    'Tabelas com PascalCase' as problema,
    tp.total as quantidade,
    CASE WHEN tp.total > 0 THEN '⚠️  MÉDIO' ELSE '✅ OK' END as severidade
FROM tabelas_pascalcase tp;

-- =====================================================
-- 8. RECOMENDAÇÕES
-- =====================================================
SELECT '8. RECOMENDAÇÕES' as secao;

SELECT 
    'RECOMENDAÇÕES BASEADAS NA ANÁLISE:' as titulo,
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'public' AND column_name IN ('Ativo', 'ativo')
            GROUP BY table_name HAVING COUNT(DISTINCT column_name) > 1
        ) THEN '1. Remover colunas "ativo" duplicadas (lowercase)'
        ELSE '1. ✅ Não há colunas duplicadas'
    END as recomendacao_1,
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM information_schema.table_constraints 
            WHERE constraint_name LIKE '%Fornecedor%MunicipiosReferencia%'
        ) THEN '2. Corrigir FK Fornecedor-Municipio'
        ELSE '2. ✅ FK Fornecedor-Municipio está correta'
    END as recomendacao_2,
    '3. Considerar migração para DateTimeOffset' as recomendacao_3,
    '4. Planejar migração para snake_case' as recomendacao_4;