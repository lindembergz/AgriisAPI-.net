-- =====================================================
-- ANÁLISE ESPECÍFICA DA TABELA PRODUTO
-- Identifica e corrige inconsistências na tabela Produto
-- =====================================================

-- Cabeçalho do relatório
SELECT 'ANÁLISE ESPECÍFICA DA TABELA PRODUTO' as titulo, NOW() as data_execucao;

-- =====================================================
-- 1. ESTRUTURA ATUAL DA TABELA PRODUTO
-- =====================================================
SELECT '1. ESTRUTURA ATUAL DA TABELA PRODUTO' as secao;

SELECT 
    column_name as coluna,
    data_type as tipo_dados,
    character_maximum_length as tamanho_max,
    is_nullable as permite_null,
    column_default as valor_padrao,
    ordinal_position as posicao,
    CASE 
        WHEN column_name IN ('UnidadeMedidaId', 'EmbalagemId', 'AtividadeAgropecuariaId') THEN '🔗 FK Referências'
        WHEN column_name IN ('CategoriaId', 'FornecedorId', 'ProdutoPaiId') THEN '🔗 FK Principais'
        WHEN column_name LIKE '%Id' AND column_name != 'Id' THEN '🔗 Possível FK'
        WHEN column_name IN ('Ativo', 'ativo') THEN '⚠️  Coluna Status'
        WHEN data_type LIKE '%timestamp%' THEN '📅 Campo Temporal'
        WHEN data_type LIKE '%numeric%' OR data_type LIKE '%decimal%' THEN '🔢 Campo Numérico'
        WHEN data_type LIKE '%json%' THEN '📋 Campo JSON'
        ELSE '📝 Campo Texto'
    END as categoria,
    CASE 
        WHEN column_name = 'ativo' THEN '❌ REMOVER - Coluna duplicada'
        WHEN column_name = 'Ativo' AND EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'Produto' AND column_name = 'ativo' AND table_schema = 'public'
        ) THEN '✅ MANTER - Coluna principal'
        WHEN column_name IN ('UnidadeMedidaId', 'EmbalagemId', 'AtividadeAgropecuariaId') THEN '🔍 VERIFICAR FK'
        ELSE '✅ OK'
    END as status_recomendado
FROM information_schema.columns 
WHERE table_name = 'Produto' AND table_schema = 'public'
ORDER BY ordinal_position;

-- =====================================================
-- 2. ANÁLISE DE FOREIGN KEYS DA TABELA PRODUTO
-- =====================================================
SELECT '2. ANÁLISE DE FOREIGN KEYS DA TABELA PRODUTO' as secao;

SELECT 
    tc.constraint_name as nome_constraint,
    kcu.column_name as coluna_origem,
    ccu.table_name as tabela_destino,
    ccu.column_name as coluna_destino,
    rc.delete_rule as regra_delete,
    rc.update_rule as regra_update,
    CASE 
        WHEN kcu.column_name = 'UnidadeMedidaId' THEN
            CASE 
                WHEN ccu.table_name = 'UnidadesMedida' THEN '✅ CORRETO'
                ELSE '❌ ERRO - Deveria apontar para UnidadesMedida, mas aponta para ' || ccu.table_name
            END
        WHEN kcu.column_name = 'EmbalagemId' THEN
            CASE 
                WHEN ccu.table_name = 'Embalagens' THEN '✅ CORRETO'
                ELSE '❌ ERRO - Deveria apontar para Embalagens, mas aponta para ' || ccu.table_name
            END
        WHEN kcu.column_name = 'AtividadeAgropecuariaId' THEN
            CASE 
                WHEN ccu.table_name = 'AtividadesAgropecuarias' THEN '✅ CORRETO'
                ELSE '❌ ERRO - Deveria apontar para AtividadesAgropecuarias, mas aponta para ' || ccu.table_name
            END
        WHEN kcu.column_name = 'CategoriaId' THEN
            CASE 
                WHEN ccu.table_name = 'Categoria' THEN '✅ CORRETO'
                ELSE '❌ ERRO - Deveria apontar para Categoria, mas aponta para ' || ccu.table_name
            END
        WHEN kcu.column_name = 'FornecedorId' THEN
            CASE 
                WHEN ccu.table_name = 'Fornecedor' THEN '✅ CORRETO'
                ELSE '❌ ERRO - Deveria apontar para Fornecedor, mas aponta para ' || ccu.table_name
            END
        WHEN kcu.column_name = 'ProdutoPaiId' THEN
            CASE 
                WHEN ccu.table_name = 'Produto' THEN '✅ CORRETO - Auto-referência'
                ELSE '❌ ERRO - Deveria apontar para Produto, mas aponta para ' || ccu.table_name
            END
        ELSE '📋 INFO - Outra FK'
    END as status_validacao,
    CASE 
        WHEN ccu.table_name NOT IN ('UnidadesMedida', 'Embalagens', 'AtividadesAgropecuarias', 'Categoria', 'Fornecedor', 'Produto')
             AND kcu.column_name IN ('UnidadeMedidaId', 'EmbalagemId', 'AtividadeAgropecuariaId', 'CategoriaId', 'FornecedorId', 'ProdutoPaiId')
        THEN 'CRÍTICO - FK incorreta'
        ELSE 'OK'
    END as prioridade_correcao
FROM information_schema.table_constraints AS tc 
    JOIN information_schema.key_column_usage AS kcu
        ON tc.constraint_name = kcu.constraint_name
        AND tc.table_schema = kcu.table_schema
    JOIN information_schema.constraint_column_usage AS ccu
        ON ccu.constraint_name = tc.constraint_name
        AND ccu.table_schema = tc.table_schema
    LEFT JOIN information_schema.referential_constraints AS rc
        ON tc.constraint_name = rc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY' 
    AND tc.table_schema = 'public'
    AND tc.table_name = 'Produto'
ORDER BY 
    CASE 
        WHEN ccu.table_name NOT IN ('UnidadesMedida', 'Embalagens', 'AtividadesAgropecuarias', 'Categoria', 'Fornecedor', 'Produto') THEN 1
        ELSE 2
    END,
    kcu.column_name;

-- =====================================================
-- 3. VERIFICAÇÃO DE INTEGRIDADE REFERENCIAL
-- =====================================================
SELECT '3. VERIFICAÇÃO DE INTEGRIDADE REFERENCIAL' as secao;

-- Verificar produtos com UnidadeMedidaId inválido
SELECT 
    'Produtos com UnidadeMedidaId inválido' as verificacao,
    COUNT(*) as quantidade_problemas,
    CASE 
        WHEN COUNT(*) = 0 THEN '✅ OK - Todos os UnidadeMedidaId são válidos'
        ELSE '❌ ERRO - Há UnidadeMedidaId inválidos'
    END as status
FROM public."Produto" p
LEFT JOIN public."UnidadesMedida" um ON p."UnidadeMedidaId" = um."Id"
WHERE p."UnidadeMedidaId" IS NOT NULL AND um."Id" IS NULL

UNION ALL

-- Verificar produtos com EmbalagemId inválido
SELECT 
    'Produtos com EmbalagemId inválido' as verificacao,
    COUNT(*) as quantidade_problemas,
    CASE 
        WHEN COUNT(*) = 0 THEN '✅ OK - Todos os EmbalagemId são válidos'
        ELSE '❌ ERRO - Há EmbalagemId inválidos'
    END as status
FROM public."Produto" p
LEFT JOIN public."Embalagens" e ON p."EmbalagemId" = e."Id"
WHERE p."EmbalagemId" IS NOT NULL AND e."Id" IS NULL

UNION ALL

-- Verificar produtos com AtividadeAgropecuariaId inválido
SELECT 
    'Produtos com AtividadeAgropecuariaId inválido' as verificacao,
    COUNT(*) as quantidade_problemas,
    CASE 
        WHEN COUNT(*) = 0 THEN '✅ OK - Todos os AtividadeAgropecuariaId são válidos'
        ELSE '❌ ERRO - Há AtividadeAgropecuariaId inválidos'
    END as status
FROM public."Produto" p
LEFT JOIN public."AtividadesAgropecuarias" aa ON p."AtividadeAgropecuariaId" = aa."Id"
WHERE p."AtividadeAgropecuariaId" IS NOT NULL AND aa."Id" IS NULL;

-- =====================================================
-- 4. ANÁLISE DE COLUNAS DUPLICADAS NA TABELA PRODUTO
-- =====================================================
SELECT '4. ANÁLISE DE COLUNAS DUPLICADAS NA TABELA PRODUTO' as secao;

WITH colunas_produto AS (
    SELECT 
        column_name,
        data_type,
        is_nullable,
        column_default
    FROM information_schema.columns 
    WHERE table_name = 'Produto' AND table_schema = 'public'
        AND column_name IN ('Ativo', 'ativo')
)
SELECT 
    column_name as coluna,
    data_type as tipo,
    is_nullable as permite_null,
    column_default as valor_padrao,
    CASE 
        WHEN column_name = 'Ativo' THEN '✅ MANTER - Coluna principal (PascalCase)'
        WHEN column_name = 'ativo' THEN '❌ REMOVER - Coluna duplicada (lowercase)'
        ELSE '❓ VERIFICAR'
    END as acao_recomendada
FROM colunas_produto
ORDER BY column_name;

-- Verificar se há divergências nos dados entre as colunas
SELECT 
    'Verificação de Divergências entre Ativo e ativo' as verificacao,
    COUNT(*) as total_produtos,
    COUNT(CASE WHEN "Ativo" = true THEN 1 END) as ativo_pascal_true,
    COALESCE(
        (SELECT COUNT(*) FROM public."Produto" WHERE ativo = true), 
        0
    ) as ativo_lower_true,
    COALESCE(
        (SELECT COUNT(*) FROM public."Produto" WHERE "Ativo" != COALESCE(ativo, "Ativo")), 
        0
    ) as registros_divergentes,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produto' AND column_name = 'ativo')
        THEN 'Coluna "ativo" existe - verificar divergências'
        ELSE 'Coluna "ativo" não existe - sem problemas'
    END as status
FROM public."Produto";

-- =====================================================
-- 5. SCRIPTS DE CORREÇÃO PARA TABELA PRODUTO
-- =====================================================
SELECT '5. SCRIPTS DE CORREÇÃO PARA TABELA PRODUTO' as secao;

-- Script de backup
SELECT 
    'Backup da Tabela Produto' as categoria,
    'CREATE TABLE backup_produto_' || TO_CHAR(NOW(), 'YYYYMMDD_HH24MI') || ' AS SELECT * FROM public."Produto";' as comando_sql,
    'Criar backup completo da tabela Produto antes das alterações' as descricao,
    '1 - Preparação' as prioridade

UNION ALL

-- Script para remover coluna duplicada (se existir)
SELECT 
    'Remover Coluna Duplicada' as categoria,
    'ALTER TABLE public."Produto" DROP COLUMN IF EXISTS ativo;' as comando_sql,
    'Remove a coluna "ativo" (lowercase) duplicada da tabela Produto' as descricao,
    '2 - Crítico' as prioridade
WHERE EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name = 'Produto' AND column_name = 'ativo' AND table_schema = 'public'
)

UNION ALL

-- Script de validação pós-correção
SELECT 
    'Validação Pós-Correção' as categoria,
    'SELECT COUNT(*) as total_produtos, COUNT(CASE WHEN "Ativo" = true THEN 1 END) as produtos_ativos FROM public."Produto";' as comando_sql,
    'Validar que a tabela Produto mantém integridade após correções' as descricao,
    '3 - Validação' as prioridade

ORDER BY prioridade;

-- =====================================================
-- 6. RESUMO E RECOMENDAÇÕES PARA TABELA PRODUTO
-- =====================================================
SELECT '6. RESUMO E RECOMENDAÇÕES PARA TABELA PRODUTO' as secao;

WITH 
-- Verificar se há coluna duplicada
coluna_duplicada AS (
    SELECT COUNT(*) as tem_duplicada
    FROM information_schema.columns 
    WHERE table_name = 'Produto' AND column_name = 'ativo' AND table_schema = 'public'
),
-- Verificar FKs problemáticas
fks_problematicas AS (
    SELECT COUNT(*) as total_problemas
    FROM information_schema.table_constraints tc
        JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name = ccu.constraint_name
    WHERE tc.constraint_type = 'FOREIGN KEY' 
        AND tc.table_name = 'Produto'
        AND tc.table_schema = 'public'
        AND ccu.table_name NOT IN ('UnidadesMedida', 'Embalagens', 'AtividadesAgropecuarias', 'Categoria', 'Fornecedor', 'Produto')
        AND tc.constraint_name LIKE '%UnidadeMedida%' OR tc.constraint_name LIKE '%Embalagem%' OR tc.constraint_name LIKE '%AtividadeAgropecuaria%'
),
-- Contar produtos
total_produtos AS (
    SELECT COUNT(*) as total FROM public."Produto"
)
SELECT 
    'Tabela Produto - Status Geral' as aspecto,
    tp.total as total_registros,
    CASE 
        WHEN cd.tem_duplicada > 0 AND fp.total_problemas > 0 THEN '❌ CRÍTICO - Colunas duplicadas E FKs incorretas'
        WHEN cd.tem_duplicada > 0 THEN '⚠️  MÉDIO - Apenas colunas duplicadas'
        WHEN fp.total_problemas > 0 THEN '⚠️  MÉDIO - Apenas FKs incorretas'
        ELSE '✅ OK - Sem problemas detectados'
    END as status_geral,
    CASE 
        WHEN cd.tem_duplicada > 0 THEN 'Remover coluna "ativo" duplicada'
        WHEN fp.total_problemas > 0 THEN 'Corrigir FKs incorretas'
        ELSE 'Nenhuma ação necessária'
    END as acao_prioritaria
FROM coluna_duplicada cd, fks_problematicas fp, total_produtos tp;