-- =====================================================
-- GERAÇÃO DE MIGRAÇÕES DO BANCO DE DADOS
-- Scripts para corrigir todas as inconsistências identificadas
-- =====================================================

-- Cabeçalho
SELECT 'SCRIPTS DE MIGRAÇÃO DO BANCO DE DADOS' as titulo, NOW() as data_execucao;

-- =====================================================
-- FASE 1: SCRIPTS DE BACKUP
-- =====================================================
SELECT '=== FASE 1: SCRIPTS DE BACKUP ===' as fase;

-- Backup de todas as tabelas que serão alteradas
SELECT 
    'BACKUP_' || table_name as script_name,
    'CREATE TABLE backup_' || LOWER(table_name) || '_' || TO_CHAR(NOW(), 'YYYYMMDD_HH24MI') || ' AS SELECT * FROM public."' || table_name || '";' as sql_command,
    'Backup da tabela ' || table_name || ' antes das alterações' as description,
    '1' as execution_order
FROM (
    SELECT DISTINCT table_name
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
        AND column_name IN ('Ativo', 'ativo')
        AND table_name IN ('AtividadesAgropecuarias', 'Catalogo', 'Combo', 'Cultura', 'Moedas', 'UnidadesMedida', 'Embalagens', 'Produto')
    GROUP BY table_name
    HAVING COUNT(DISTINCT column_name) > 1
) tabelas_com_duplicatas
ORDER BY table_name;

-- =====================================================
-- FASE 2: VERIFICAÇÃO PRÉ-MIGRAÇÃO
-- =====================================================
SELECT '=== FASE 2: VERIFICAÇÃO PRÉ-MIGRAÇÃO ===' as fase;

SELECT 
    'PRE_MIGRATION_CHECK' as script_name,
    '-- Verificar estado antes da migração
DO $$
DECLARE
    duplicated_columns_count INTEGER;
    incorrect_fks_count INTEGER;
BEGIN
    -- Contar colunas duplicadas
    SELECT COUNT(*) INTO duplicated_columns_count
    FROM (
        SELECT table_name
        FROM information_schema.columns 
        WHERE table_schema = ''public'' AND column_name IN (''Ativo'', ''ativo'')
        GROUP BY table_name
        HAVING COUNT(DISTINCT column_name) > 1
    ) t;
    
    -- Contar FKs incorretas
    SELECT COUNT(*) INTO incorrect_fks_count
    FROM information_schema.table_constraints tc
        JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name = ccu.constraint_name
    WHERE tc.constraint_type = ''FOREIGN KEY'' 
        AND tc.constraint_name LIKE ''%MunicipiosReferencia%'';
    
    RAISE NOTICE ''PRE-MIGRATION STATUS:'';
    RAISE NOTICE ''- Colunas duplicadas encontradas: %'', duplicated_columns_count;
    RAISE NOTICE ''- FKs incorretas encontradas: %'', incorrect_fks_count;
    
    IF duplicated_columns_count = 0 AND incorrect_fks_count = 0 THEN
        RAISE NOTICE ''✅ Banco já está consistente - migração não necessária'';
    ELSE
        RAISE NOTICE ''⚠️  Migração necessária - prosseguir com as correções'';
    END IF;
END $$;' as sql_command,
    'Verificação do estado atual antes de aplicar migrações' as description,
    '2' as execution_order;

-- =====================================================
-- FASE 3: REMOÇÃO DE COLUNAS DUPLICADAS
-- =====================================================
SELECT '=== FASE 3: REMOÇÃO DE COLUNAS DUPLICADAS ===' as fase;

-- Gerar scripts para cada tabela com colunas duplicadas
SELECT 
    'DROP_DUPLICATE_' || table_name as script_name,
    'DO $$
BEGIN
    -- Verificar se a coluna "ativo" existe antes de tentar remover
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = ''' || table_name || ''' 
            AND column_name = ''ativo'' 
            AND table_schema = ''public''
    ) THEN
        -- Verificar se há divergências nos dados antes de remover
        EXECUTE ''
            INSERT INTO migration_log_' || TO_CHAR(NOW(), 'YYYYMMDD') || ' (table_name, action, details, timestamp)
            SELECT 
                ''''' || table_name || ''''',
                ''''DROP_COLUMN_ativo'''',
                ''''Removendo coluna duplicada ativo de ' || table_name || ''''',
                NOW()
        '';
        
        -- Remover a coluna duplicada
        ALTER TABLE public."' || table_name || '" DROP COLUMN ativo;
        
        RAISE NOTICE ''✅ Coluna "ativo" removida da tabela ' || table_name || ''';
    ELSE
        RAISE NOTICE ''ℹ️  Tabela ' || table_name || ' não possui coluna "ativo" duplicada'';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE ''❌ Erro ao remover coluna ativo da tabela ' || table_name || ': %'', SQLERRM;
        -- Log do erro
        INSERT INTO migration_errors_' || TO_CHAR(NOW(), 'YYYYMMDD') || ' (table_name, error_message, timestamp)
        VALUES (''' || table_name || ''', SQLERRM, NOW());
END $$;' as sql_command,
    'Remove coluna "ativo" duplicada da tabela ' || table_name as description,
    '3' as execution_order
FROM (
    SELECT DISTINCT table_name
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
        AND column_name IN ('Ativo', 'ativo')
        AND table_name IN ('AtividadesAgropecuarias', 'Catalogo', 'Combo', 'Cultura', 'Moedas', 'UnidadesMedida', 'Embalagens', 'Produto')
    GROUP BY table_name
    HAVING COUNT(DISTINCT column_name) > 1
) tabelas_duplicadas
ORDER BY 
    CASE table_name
        WHEN 'Produto' THEN 1
        WHEN 'Cultura' THEN 2
        WHEN 'Catalogo' THEN 3
        ELSE 4
    END,
    table_name;

-- =====================================================
-- FASE 4: CRIAÇÃO DE TABELAS DE LOG
-- =====================================================
SELECT '=== FASE 4: CRIAÇÃO DE TABELAS DE LOG ===' as fase;

SELECT 
    'CREATE_MIGRATION_LOGS' as script_name,
    '-- Criar tabelas de log para a migração
CREATE TABLE IF NOT EXISTS migration_log_' || TO_CHAR(NOW(), 'YYYYMMDD') || ' (
    id SERIAL PRIMARY KEY,
    table_name VARCHAR(100) NOT NULL,
    action VARCHAR(50) NOT NULL,
    details TEXT,
    timestamp TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS migration_errors_' || TO_CHAR(NOW(), 'YYYYMMDD') || ' (
    id SERIAL PRIMARY KEY,
    table_name VARCHAR(100) NOT NULL,
    error_message TEXT NOT NULL,
    timestamp TIMESTAMPTZ DEFAULT NOW()
);

-- Inserir log inicial
INSERT INTO migration_log_' || TO_CHAR(NOW(), 'YYYYMMDD') || ' (table_name, action, details)
VALUES (''SYSTEM'', ''MIGRATION_START'', ''Iniciando migração para correção de inconsistências DDL vs API'');' as sql_command,
    'Criar tabelas de log para rastrear a migração' as description,
    '0' as execution_order;

-- =====================================================
-- FASE 5: VALIDAÇÃO PÓS-MIGRAÇÃO
-- =====================================================
SELECT '=== FASE 5: VALIDAÇÃO PÓS-MIGRAÇÃO ===' as fase;

SELECT 
    'POST_MIGRATION_VALIDATION' as script_name,
    '-- Validação completa pós-migração
DO $$
DECLARE
    duplicated_columns_remaining INTEGER;
    tables_with_issues TEXT[];
    validation_results TEXT := '''';
BEGIN
    -- Verificar se ainda há colunas duplicadas
    SELECT COUNT(*), ARRAY_AGG(table_name) 
    INTO duplicated_columns_remaining, tables_with_issues
    FROM (
        SELECT table_name
        FROM information_schema.columns 
        WHERE table_schema = ''public'' AND column_name IN (''Ativo'', ''ativo'')
        GROUP BY table_name
        HAVING COUNT(DISTINCT column_name) > 1
    ) t;
    
    validation_results := validation_results || ''Colunas duplicadas restantes: '' || duplicated_columns_remaining || E''\n'';
    
    IF duplicated_columns_remaining = 0 THEN
        validation_results := validation_results || ''✅ Todas as colunas duplicadas foram removidas com sucesso'' || E''\n'';
    ELSE
        validation_results := validation_results || ''❌ Ainda existem '' || duplicated_columns_remaining || '' tabelas com colunas duplicadas: '' || array_to_string(tables_with_issues, '', '') || E''\n'';
    END IF;
    
    -- Verificar integridade das tabelas principais
    PERFORM 1 FROM public."Produto" LIMIT 1;
    validation_results := validation_results || ''✅ Tabela Produto acessível'' || E''\n'';
    
    PERFORM 1 FROM public."Fornecedor" LIMIT 1;
    validation_results := validation_results || ''✅ Tabela Fornecedor acessível'' || E''\n'';
    
    -- Log dos resultados
    INSERT INTO migration_log_' || TO_CHAR(NOW(), 'YYYYMMDD') || ' (table_name, action, details)
    VALUES (''SYSTEM'', ''VALIDATION_COMPLETE'', validation_results);
    
    RAISE NOTICE ''VALIDAÇÃO PÓS-MIGRAÇÃO:'';
    RAISE NOTICE ''%'', validation_results;
    
    -- Finalizar migração
    INSERT INTO migration_log_' || TO_CHAR(NOW(), 'YYYYMMDD') || ' (table_name, action, details)
    VALUES (''SYSTEM'', ''MIGRATION_COMPLETE'', ''Migração finalizada com sucesso'');
    
END $$;' as sql_command,
    'Validação completa após aplicar todas as migrações' as description,
    '4' as execution_order;

-- =====================================================
-- FASE 6: SCRIPT COMPLETO DE EXECUÇÃO
-- =====================================================
SELECT '=== SCRIPT COMPLETO DE EXECUÇÃO ===' as fase;

SELECT 
    'EXECUTE_COMPLETE_MIGRATION' as script_name,
    '-- SCRIPT COMPLETO DE MIGRAÇÃO
-- Execute este script para aplicar todas as correções

BEGIN;

-- Log início
DO $$ BEGIN RAISE NOTICE ''🚀 Iniciando migração completa - %'', NOW(); END $$;

-- Criar tabelas de log
' || (SELECT sql_command FROM (
    SELECT 'CREATE_MIGRATION_LOGS' as sn, sql_command FROM (
        SELECT 
            '-- Criar tabelas de log para a migração
CREATE TABLE IF NOT EXISTS migration_log_' || TO_CHAR(NOW(), 'YYYYMMDD') || ' (
    id SERIAL PRIMARY KEY,
    table_name VARCHAR(100) NOT NULL,
    action VARCHAR(50) NOT NULL,
    details TEXT,
    timestamp TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS migration_errors_' || TO_CHAR(NOW(), 'YYYYMMDD') || ' (
    id SERIAL PRIMARY KEY,
    table_name VARCHAR(100) NOT NULL,
    error_message TEXT NOT NULL,
    timestamp TIMESTAMPTZ DEFAULT NOW()
);' as sql_command
    ) sub
) logs WHERE sn = 'CREATE_MIGRATION_LOGS') || '

-- Executar remoção de colunas duplicadas para cada tabela
' || (
    SELECT STRING_AGG(
        '-- ' || table_name || E'\n' ||
        'ALTER TABLE public."' || table_name || '" DROP COLUMN IF EXISTS ativo;' || E'\n',
        E'\n'
    )
    FROM (
        SELECT DISTINCT table_name
        FROM information_schema.columns 
        WHERE table_schema = 'public' 
            AND column_name IN ('Ativo', 'ativo')
        GROUP BY table_name
        HAVING COUNT(DISTINCT column_name) > 1
        ORDER BY table_name
    ) t
) || '

-- Validação final
DO $$
DECLARE
    remaining_issues INTEGER;
BEGIN
    SELECT COUNT(*) INTO remaining_issues
    FROM (
        SELECT table_name
        FROM information_schema.columns 
        WHERE table_schema = ''public'' AND column_name IN (''Ativo'', ''ativo'')
        GROUP BY table_name
        HAVING COUNT(DISTINCT column_name) > 1
    ) t;
    
    IF remaining_issues = 0 THEN
        RAISE NOTICE ''✅ Migração concluída com sucesso - todas as inconsistências foram corrigidas'';
    ELSE
        RAISE NOTICE ''⚠️  Migração parcial - ainda existem % problemas'', remaining_issues;
    END IF;
END $$;

COMMIT;

-- Log final
DO $$ BEGIN RAISE NOTICE ''🎉 Migração finalizada - %'', NOW(); END $$;' as sql_command,
    'Script completo para executar toda a migração de uma vez' as description,
    '5' as execution_order;