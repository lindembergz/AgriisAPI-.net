-- =====================================================
-- IDENTIFICAÇÃO DE COLUNAS DUPLICADAS
-- Script para identificar todas as inconsistências de colunas
-- =====================================================

-- =====================================================
-- 1. IDENTIFICAR TODAS AS COLUNAS "ATIVO" DUPLICADAS
-- =====================================================
SELECT 'ANÁLISE DETALHADA DE COLUNAS ATIVO DUPLICADAS' as titulo;

WITH colunas_ativo AS (
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
        STRING_AGG(
            column_name || ' (' || data_type || ', default: ' || COALESCE(column_default, 'NULL') || ')',
            ' | '
            ORDER BY ordinal_position
        ) as detalhes_colunas
    FROM colunas_ativo
    GROUP BY table_name
    HAVING COUNT(*) > 1
)
SELECT 
    table_name as tabela_com_problema,
    total_colunas_ativo as quantidade_colunas,
    detalhes_colunas as detalhes,
    'REMOVER coluna "ativo" (lowercase)' as acao_recomendada
FROM tabelas_com_duplicatas
ORDER BY table_name;

-- =====================================================
-- 2. SCRIPT PARA GERAR COMANDOS DE LIMPEZA
-- =====================================================
SELECT 'COMANDOS SQL PARA CORREÇÃO' as titulo;

WITH tabelas_com_duplicatas AS (
    SELECT DISTINCT table_name
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
        AND column_name IN ('Ativo', 'ativo')
    GROUP BY table_name
    HAVING COUNT(DISTINCT column_name) > 1
)
SELECT 
    'ALTER TABLE public."' || table_name || '" DROP COLUMN IF EXISTS ativo;' as comando_sql,
    table_name as tabela_afetada,
    'Remove coluna duplicada "ativo" (lowercase)' as descricao
FROM tabelas_com_duplicatas
ORDER BY table_name;

-- =====================================================
-- 3. VERIFICAR DADOS NAS COLUNAS DUPLICADAS
-- =====================================================
SELECT 'VERIFICAÇÃO DE DADOS NAS COLUNAS DUPLICADAS' as titulo;

-- Para cada tabela com duplicata, verificar se há diferenças nos dados
-- AtividadesAgropecuarias
SELECT 
    'AtividadesAgropecuarias' as tabela,
    COUNT(*) as total_registros,
    COUNT(CASE WHEN "Ativo" = true THEN 1 END) as ativo_pascal_true,
    COUNT(CASE WHEN ativo = true THEN 1 END) as ativo_lower_true,
    COUNT(CASE WHEN "Ativo" != ativo THEN 1 END) as registros_divergentes
FROM public."AtividadesAgropecuarias"
WHERE EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name = 'AtividadesAgropecuarias' 
        AND column_name = 'ativo' 
        AND table_schema = 'public'
)

UNION ALL

-- Catalogo
SELECT 
    'Catalogo' as tabela,
    COUNT(*) as total_registros,
    COUNT(CASE WHEN "Ativo" = true THEN 1 END) as ativo_pascal_true,
    COUNT(CASE WHEN ativo = true THEN 1 END) as ativo_lower_true,
    COUNT(CASE WHEN "Ativo" != ativo THEN 1 END) as registros_divergentes
FROM public."Catalogo"
WHERE EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name = 'Catalogo' 
        AND column_name = 'ativo' 
        AND table_schema = 'public'
)

UNION ALL

-- Combo
SELECT 
    'Combo' as tabela,
    COUNT(*) as total_registros,
    NULL as ativo_pascal_true,  -- Combo pode não ter coluna Ativo PascalCase
    COUNT(CASE WHEN ativo = true THEN 1 END) as ativo_lower_true,
    NULL as registros_divergentes
FROM public."Combo"
WHERE EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name = 'Combo' 
        AND column_name = 'ativo' 
        AND table_schema = 'public'
)

UNION ALL

-- Cultura
SELECT 
    'Cultura' as tabela,
    COUNT(*) as total_registros,
    COUNT(CASE WHEN "Ativo" = true THEN 1 END) as ativo_pascal_true,
    COUNT(CASE WHEN ativo = true THEN 1 END) as ativo_lower_true,
    COUNT(CASE WHEN "Ativo" != ativo THEN 1 END) as registros_divergentes
FROM public."Cultura"
WHERE EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name = 'Cultura' 
        AND column_name = 'ativo' 
        AND table_schema = 'public'
);

-- =====================================================
-- 4. IDENTIFICAR OUTRAS POSSÍVEIS DUPLICATAS
-- =====================================================
SELECT 'OUTRAS POSSÍVEIS COLUNAS DUPLICADAS' as titulo;

WITH colunas_suspeitas AS (
    SELECT 
        table_name,
        LOWER(column_name) as coluna_normalizada,
        COUNT(*) as quantidade,
        STRING_AGG(column_name, ', ' ORDER BY column_name) as variantes
    FROM information_schema.columns 
    WHERE table_schema = 'public'
        AND table_name IN (
            'Fornecedor', 'AtividadesAgropecuarias', 'Catalogo', 'Combo', 
            'Cultura', 'Moedas', 'UnidadesMedida', 'Embalagens'
        )
    GROUP BY table_name, LOWER(column_name)
    HAVING COUNT(*) > 1
)
SELECT 
    table_name as tabela,
    coluna_normalizada as coluna_base,
    quantidade as total_variantes,
    variantes as nomes_encontrados,
    'Verificar se são duplicatas' as acao_recomendada
FROM colunas_suspeitas
ORDER BY table_name, coluna_normalizada;

-- =====================================================
-- 5. RESUMO PARA MIGRAÇÃO
-- =====================================================
SELECT 'RESUMO PARA PLANEJAMENTO DE MIGRAÇÃO' as titulo;

WITH resumo_problemas AS (
    SELECT 
        'Colunas ativo duplicadas' as tipo_problema,
        COUNT(DISTINCT table_name) as tabelas_afetadas
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
        AND column_name IN ('Ativo', 'ativo')
    GROUP BY table_name
    HAVING COUNT(DISTINCT column_name) > 1
)
SELECT 
    tipo_problema,
    tabelas_afetadas,
    CASE 
        WHEN tabelas_afetadas > 0 THEN 'ALTA - Requer migração de dados'
        ELSE 'BAIXA - Sem problemas detectados'
    END as prioridade,
    CASE 
        WHEN tabelas_afetadas > 0 THEN 'Executar scripts de limpeza antes da migração EF Core'
        ELSE 'Nenhuma ação necessária'
    END as acao_recomendada
FROM resumo_problemas;