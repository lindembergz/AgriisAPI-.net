-- =====================================================
-- COMPREHENSIVE DATABASE ANALYSIS SCRIPT
-- Análise completa do banco para identificar todas as inconsistências
-- =====================================================

-- Cabeçalho do relatório
SELECT 'COMPREHENSIVE DATABASE ANALYSIS REPORT' as titulo, NOW() as data_execucao;

-- =====================================================
-- 1. ANÁLISE COMPLETA DE COLUNAS DUPLICADAS
-- =====================================================
SELECT '1. ANÁLISE COMPLETA DE COLUNAS DUPLICADAS' as secao;

-- Identificar TODAS as tabelas com colunas "ativo" duplicadas
WITH todas_colunas_ativo AS (
    SELECT 
        table_name,
        column_name,
        data_type,
        is_nullable,
        column_default,
        ordinal_position
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
        AND column_name IN ('Ativo', 'ativo')
),
tabelas_com_duplicatas AS (
    SELECT 
        table_name,
        COUNT(*) as total_colunas_ativo,
        COUNT(CASE WHEN column_name = 'Ativo' THEN 1 END) as ativo_pascal,
        COUNT(CASE WHEN column_name = 'ativo' THEN 1 END) as ativo_lower,
        STRING_AGG(
            column_name || ' (' || data_type || 
            CASE WHEN column_default IS NOT NULL THEN ', default: ' || column_default ELSE '' END || ')',
            ' | '
            ORDER BY ordinal_position
        ) as detalhes_colunas
    FROM todas_colunas_ativo
    GROUP BY table_name
)
SELECT 
    table_name as tabela,
    total_colunas_ativo as total_colunas,
    ativo_pascal as "Ativo_PascalCase",
    ativo_lower as "ativo_lowercase", 
    CASE 
        WHEN total_colunas_ativo > 1 THEN '❌ CRÍTICO - Duplicada'
        WHEN ativo_pascal = 1 AND ativo_lower = 0 THEN '✅ OK - Apenas PascalCase'
        WHEN ativo_pascal = 0 AND ativo_lower = 1 THEN '⚠️  ATENÇÃO - Apenas lowercase'
        ELSE '❓ ESTADO INESPERADO'
    END as status,
    detalhes_colunas,
    CASE 
        WHEN total_colunas_ativo > 1 THEN 'ALTER TABLE public."' || table_name || '" DROP COLUMN IF EXISTS ativo;'
        ELSE 'Nenhuma ação necessária'
    END as comando_correcao
FROM tabelas_com_duplicatas
ORDER BY 
    CASE 
        WHEN total_colunas_ativo > 1 THEN 1  -- Problemas primeiro
        WHEN ativo_pascal = 0 AND ativo_lower = 1 THEN 2  -- Atenção depois
        ELSE 3  -- OK por último
    END,
    table_name;

-- =====================================================
-- 2. ANÁLISE ESPECÍFICA DA TABELA PRODUTO
-- =====================================================
SELECT '2. ANÁLISE ESPECÍFICA DA TABELA PRODUTO' as secao;

-- Verificar estrutura da tabela Produto
SELECT 
    'Estrutura da Tabela Produto' as categoria,
    column_name as coluna,
    data_type as tipo,
    is_nullable as permite_null,
    column_default as valor_padrao,
    CASE 
        WHEN column_name IN ('UnidadeMedidaId', 'EmbalagemId', 'AtividadeAgropecuariaId') THEN '🔗 FK para referências'
        WHEN column_name LIKE '%Id' THEN '🔗 Possível FK'
        WHEN column_name IN ('Ativo', 'ativo') THEN '⚠️  Coluna ativo'
        WHEN data_type LIKE '%timestamp%' THEN '📅 Campo de data'
        ELSE '📋 Campo regular'
    END as categoria_campo
FROM information_schema.columns 
WHERE table_name = 'Produto' AND table_schema = 'public'
ORDER BY ordinal_position;

-- Verificar FKs da tabela Produto
SELECT 
    'Foreign Keys da Tabela Produto' as categoria,
    tc.constraint_name as nome_constraint,
    kcu.column_name as coluna_origem,
    ccu.table_name as tabela_destino,
    ccu.column_name as coluna_destino,
    CASE 
        WHEN tc.constraint_name LIKE '%UnidadeMedida%' OR tc.constraint_name LIKE '%UnidadesMedida%' THEN
            CASE 
                WHEN ccu.table_name = 'UnidadesMedida' THEN '✅ OK - Aponta para UnidadesMedida'
                ELSE '❌ ERRO - Tabela incorreta: ' || ccu.table_name
            END
        WHEN tc.constraint_name LIKE '%Embalagem%' THEN
            CASE 
                WHEN ccu.table_name = 'Embalagens' THEN '✅ OK - Aponta para Embalagens'
                ELSE '❌ ERRO - Tabela incorreta: ' || ccu.table_name
            END
        WHEN tc.constraint_name LIKE '%AtividadeAgropecuaria%' THEN
            CASE 
                WHEN ccu.table_name = 'AtividadesAgropecuarias' THEN '✅ OK - Aponta para AtividadesAgropecuarias'
                ELSE '❌ ERRO - Tabela incorreta: ' || ccu.table_name
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
    AND tc.table_name = 'Produto'
ORDER BY tc.constraint_name;

-- =====================================================
-- 3. ANÁLISE DE TODAS AS FOREIGN KEYS PROBLEMÁTICAS
-- =====================================================
SELECT '3. ANÁLISE DE TODAS AS FOREIGN KEYS PROBLEMÁTICAS' as secao;

SELECT 
    tc.table_name as tabela_origem,
    tc.constraint_name as nome_constraint,
    kcu.column_name as coluna_origem,
    ccu.table_name as tabela_destino,
    ccu.column_name as coluna_destino,
    CASE 
        -- Verificar FKs do Fornecedor (já corrigidas)
        WHEN tc.table_name = 'Fornecedor' AND tc.constraint_name LIKE '%Municipios%' THEN
            CASE 
                WHEN ccu.table_name = 'municipios' THEN '✅ CORRETO - Aponta para municipios'
                WHEN ccu.table_name = 'municipios_referencia' THEN '❌ ERRO - Aponta para municipios_referencia'
                ELSE '❓ INESPERADO - ' || ccu.table_name
            END
        WHEN tc.table_name = 'Fornecedor' AND tc.constraint_name LIKE '%Estados%' THEN
            CASE 
                WHEN ccu.table_name = 'estados' THEN '✅ CORRETO - Aponta para estados'
                ELSE '❌ ERRO - Não aponta para estados'
            END
        -- Verificar FKs do Produto
        WHEN tc.table_name = 'Produto' AND kcu.column_name = 'UnidadeMedidaId' THEN
            CASE 
                WHEN ccu.table_name = 'UnidadesMedida' THEN '✅ CORRETO - Aponta para UnidadesMedida'
                ELSE '❌ ERRO - Tabela incorreta: ' || ccu.table_name
            END
        WHEN tc.table_name = 'Produto' AND kcu.column_name = 'EmbalagemId' THEN
            CASE 
                WHEN ccu.table_name = 'Embalagens' THEN '✅ CORRETO - Aponta para Embalagens'
                ELSE '❌ ERRO - Tabela incorreta: ' || ccu.table_name
            END
        WHEN tc.table_name = 'Produto' AND kcu.column_name = 'AtividadeAgropecuariaId' THEN
            CASE 
                WHEN ccu.table_name = 'AtividadesAgropecuarias' THEN '✅ CORRETO - Aponta para AtividadesAgropecuarias'
                ELSE '❌ ERRO - Tabela incorreta: ' || ccu.table_name
            END
        ELSE '📋 INFO - Outra FK'
    END as status_validacao,
    CASE 
        WHEN tc.constraint_name LIKE '%MunicipiosReferencia%' THEN 'CRÍTICO - Corrigir FK Fornecedor'
        WHEN ccu.table_name NOT IN ('municipios', 'estados', 'UnidadesMedida', 'Embalagens', 'AtividadesAgropecuarias') 
             AND tc.table_name IN ('Fornecedor', 'Produto') THEN 'ALTO - Verificar FK'
        ELSE 'BAIXO - Monitorar'
    END as prioridade
FROM information_schema.table_constraints AS tc 
    JOIN information_schema.key_column_usage AS kcu
        ON tc.constraint_name = kcu.constraint_name
        AND tc.table_schema = kcu.table_schema
    JOIN information_schema.constraint_column_usage AS ccu
        ON ccu.constraint_name = tc.constraint_name
        AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY' 
    AND tc.table_schema = 'public'
    AND tc.table_name IN ('Fornecedor', 'Produto')
ORDER BY 
    CASE 
        WHEN tc.constraint_name LIKE '%MunicipiosReferencia%' THEN 1
        WHEN ccu.table_name NOT IN ('municipios', 'estados', 'UnidadesMedida', 'Embalagens', 'AtividadesAgropecuarias') THEN 2
        ELSE 3
    END,
    tc.table_name, tc.constraint_name;

-- =====================================================
-- 4. GERAÇÃO DE SCRIPTS DE MIGRAÇÃO
-- =====================================================
SELECT '4. GERAÇÃO DE SCRIPTS DE MIGRAÇÃO' as secao;

-- Script para remover colunas duplicadas
SELECT 
    'Scripts para Remover Colunas Duplicadas' as categoria,
    'ALTER TABLE public."' || table_name || '" DROP COLUMN IF EXISTS ativo;' as comando_sql,
    'Remove coluna "ativo" (lowercase) duplicada da tabela ' || table_name as descricao,
    '1 - Crítico' as prioridade
FROM (
    SELECT DISTINCT table_name
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
        AND column_name IN ('Ativo', 'ativo')
    GROUP BY table_name
    HAVING COUNT(DISTINCT column_name) > 1
) tabelas_duplicadas

UNION ALL

-- Script para backup antes das alterações
SELECT 
    'Scripts de Backup' as categoria,
    'CREATE TABLE backup_' || table_name || '_' || TO_CHAR(NOW(), 'YYYYMMDD') || ' AS SELECT * FROM public."' || table_name || '";' as comando_sql,
    'Backup da tabela ' || table_name || ' antes das alterações' as descricao,
    '0 - Preparação' as prioridade
FROM (
    SELECT DISTINCT table_name
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
        AND column_name IN ('Ativo', 'ativo')
    GROUP BY table_name
    HAVING COUNT(DISTINCT column_name) > 1
) tabelas_duplicadas

ORDER BY prioridade, categoria;

-- =====================================================
-- 5. ANÁLISE DE IMPACTO DAS MUDANÇAS
-- =====================================================
SELECT '5. ANÁLISE DE IMPACTO DAS MUDANÇAS' as secao;

-- Contar registros afetados
WITH tabelas_afetadas AS (
    SELECT table_name
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
        AND column_name IN ('Ativo', 'ativo')
    GROUP BY table_name
    HAVING COUNT(DISTINCT column_name) > 1
)
SELECT 
    ta.table_name as tabela,
    CASE ta.table_name
        WHEN 'AtividadesAgropecuarias' THEN (SELECT COUNT(*) FROM public."AtividadesAgropecuarias")
        WHEN 'Catalogo' THEN (SELECT COUNT(*) FROM public."Catalogo")
        WHEN 'Combo' THEN (SELECT COUNT(*) FROM public."Combo")
        WHEN 'Cultura' THEN (SELECT COUNT(*) FROM public."Cultura")
        WHEN 'Moedas' THEN (SELECT COUNT(*) FROM public."Moedas")
        WHEN 'UnidadesMedida' THEN (SELECT COUNT(*) FROM public."UnidadesMedida")
        WHEN 'Embalagens' THEN (SELECT COUNT(*) FROM public."Embalagens")
        ELSE 0
    END as total_registros,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = ta.table_name AND column_name = 'ativo')
        THEN 'Tem coluna "ativo" para remover'
        ELSE 'Sem coluna duplicada'
    END as acao_necessaria,
    CASE 
        WHEN ta.table_name IN ('Produto', 'Fornecedor') THEN 'ALTO - Tabela crítica'
        WHEN ta.table_name IN ('Cultura', 'Catalogo') THEN 'MÉDIO - Tabela importante'
        ELSE 'BAIXO - Tabela de referência'
    END as impacto_negocio
FROM tabelas_afetadas ta
ORDER BY 
    CASE 
        WHEN ta.table_name IN ('Produto', 'Fornecedor') THEN 1
        WHEN ta.table_name IN ('Cultura', 'Catalogo') THEN 2
        ELSE 3
    END,
    ta.table_name;

-- =====================================================
-- 6. RESUMO EXECUTIVO E PLANO DE AÇÃO
-- =====================================================
SELECT '6. RESUMO EXECUTIVO E PLANO DE AÇÃO' as secao;

WITH 
-- Contar problemas por categoria
problemas_fk AS (
    SELECT COUNT(*) as total
    FROM information_schema.table_constraints tc
        JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name = ccu.constraint_name
    WHERE tc.constraint_type = 'FOREIGN KEY' 
        AND tc.table_name IN ('Fornecedor', 'Produto')
        AND (tc.constraint_name LIKE '%MunicipiosReferencia%' 
             OR (tc.table_name = 'Produto' AND ccu.table_name NOT IN ('UnidadesMedida', 'Embalagens', 'AtividadesAgropecuarias', 'Categoria')))
),
problemas_colunas AS (
    SELECT COUNT(*) as total
    FROM (
        SELECT table_name
        FROM information_schema.columns 
        WHERE table_schema = 'public' AND column_name IN ('Ativo', 'ativo')
        GROUP BY table_name
        HAVING COUNT(DISTINCT column_name) > 1
    ) t
)
SELECT 
    'Foreign Keys Incorretas' as categoria_problema,
    pf.total as quantidade,
    CASE WHEN pf.total > 0 THEN '❌ CRÍTICO' ELSE '✅ OK' END as severidade,
    CASE WHEN pf.total > 0 THEN 'Gerar migração EF Core para corrigir FKs' ELSE 'Nenhuma ação necessária' END as acao_recomendada
FROM problemas_fk pf

UNION ALL

SELECT 
    'Colunas Duplicadas' as categoria_problema,
    pc.total as quantidade,
    CASE WHEN pc.total > 0 THEN '❌ CRÍTICO' ELSE '✅ OK' END as severidade,
    CASE WHEN pc.total > 0 THEN 'Executar scripts DROP COLUMN para remover duplicatas' ELSE 'Nenhuma ação necessária' END as acao_recomendada
FROM problemas_colunas pc;

-- Plano de execução recomendado
SELECT 
    'PLANO DE EXECUÇÃO RECOMENDADO' as titulo,
    '1. Fazer backup das tabelas afetadas' as passo_1,
    '2. Executar scripts DROP COLUMN para remover colunas duplicadas' as passo_2,
    '3. Gerar migração EF Core para aplicar mudanças de FK' as passo_3,
    '4. Executar testes de validação pós-migração' as passo_4,
    '5. Validar integridade referencial' as passo_5;