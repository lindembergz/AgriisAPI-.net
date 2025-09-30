-- Script completo para corrigir TODAS as tabelas com problemas de DateTimeOffset
-- Este script identifica e corrige todas as colunas de auditoria que herdam de EntidadeBase

-- IMPORTANTE: 
-- 1. Execute primeiro em ambiente de desenvolvimento!
-- 2. Faça backup completo do banco antes de executar em produção!
-- 3. Teste a aplicação após executar o script!

BEGIN;

-- Função para gerar comandos ALTER TABLE dinamicamente
DO $$
DECLARE
    rec RECORD;
    sql_cmd TEXT;
BEGIN
    -- Buscar todas as tabelas que têm colunas DataCriacao ou DataAtualizacao
    -- com timestamp without time zone
    FOR rec IN 
        SELECT DISTINCT table_name
        FROM information_schema.columns 
        WHERE table_schema = 'public' 
            AND data_type = 'timestamp without time zone'
            AND column_name IN ('DataCriacao', 'DataAtualizacao')
        ORDER BY table_name
    LOOP
        -- Gerar comando ALTER TABLE para cada tabela
        sql_cmd := 'ALTER TABLE "' || rec.table_name || '"';
        
        -- Verificar se tem DataCriacao
        IF EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'public' 
                AND table_name = rec.table_name 
                AND column_name = 'DataCriacao'
                AND data_type = 'timestamp without time zone'
        ) THEN
            sql_cmd := sql_cmd || ' ALTER COLUMN "DataCriacao" TYPE timestamp with time zone USING "DataCriacao" AT TIME ZONE ''UTC''';
        END IF;
        
        -- Verificar se tem DataAtualizacao
        IF EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'public' 
                AND table_name = rec.table_name 
                AND column_name = 'DataAtualizacao'
                AND data_type = 'timestamp without time zone'
        ) THEN
            -- Adicionar vírgula se já tem DataCriacao
            IF sql_cmd LIKE '%DataCriacao%' THEN
                sql_cmd := sql_cmd || ',';
            END IF;
            sql_cmd := sql_cmd || ' ALTER COLUMN "DataAtualizacao" TYPE timestamp with time zone USING "DataAtualizacao" AT TIME ZONE ''UTC''';
        END IF;
        
        -- Executar o comando se foi construído
        IF sql_cmd != 'ALTER TABLE "' || rec.table_name || '"' THEN
            RAISE NOTICE 'Executando: %', sql_cmd;
            EXECUTE sql_cmd;
        END IF;
    END LOOP;
END $$;

-- Corrigir também outras colunas de data específicas que podem usar DateTimeOffset
-- Tabela Catalogo - DataInicio e DataFim
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
            AND table_name = 'Catalogo' 
            AND column_name IN ('DataInicio', 'DataFim')
            AND data_type = 'timestamp without time zone'
    ) THEN
        ALTER TABLE "Catalogo" 
            ALTER COLUMN "DataInicio" TYPE timestamp with time zone USING "DataInicio" AT TIME ZONE 'UTC',
            ALTER COLUMN "DataFim" TYPE timestamp with time zone USING "DataFim" AT TIME ZONE 'UTC';
        RAISE NOTICE 'Corrigidas colunas DataInicio e DataFim da tabela Catalogo';
    END IF;
END $$;

-- Atualizar defaults para CURRENT_TIMESTAMP onde apropriado
DO $$
DECLARE
    rec RECORD;
BEGIN
    FOR rec IN 
        SELECT table_name
        FROM information_schema.columns 
        WHERE table_schema = 'public' 
            AND column_name = 'DataCriacao'
            AND data_type = 'timestamp with time zone'
        ORDER BY table_name
    LOOP
        EXECUTE 'ALTER TABLE "' || rec.table_name || '" ALTER COLUMN "DataCriacao" SET DEFAULT CURRENT_TIMESTAMP';
        RAISE NOTICE 'Atualizado default para DataCriacao na tabela %', rec.table_name;
    END LOOP;
END $$;

COMMIT;

-- Relatório final de validação
SELECT 
    'RESUMO DA CORREÇÃO' as tipo,
    COUNT(*) as total_colunas_corrigidas
FROM information_schema.columns 
WHERE table_schema = 'public' 
    AND data_type = 'timestamp with time zone'
    AND column_name IN ('DataCriacao', 'DataAtualizacao');

-- Listar tabelas que ainda podem ter problemas
SELECT 
    'TABELAS COM POSSÍVEIS PROBLEMAS RESTANTES' as tipo,
    table_name,
    column_name,
    data_type
FROM information_schema.columns 
WHERE table_schema = 'public' 
    AND data_type = 'timestamp without time zone'
    AND (column_name LIKE '%Data%' OR column_name LIKE '%Date%')
ORDER BY table_name, column_name;