-- Script para corrigir mapeamento DateTimeOffset no PostgreSQL
-- Este script altera as colunas de timestamp without time zone para timestamp with time zone
-- para compatibilidade com propriedades DateTimeOffset do Entity Framework Core

-- IMPORTANTE: Execute este script em ambiente de desenvolvimento primeiro!
-- Faça backup do banco antes de executar em produção!

BEGIN;

-- 1. Corrigir tabela Catalogo
-- Alterar DataCriacao, DataAtualizacao, DataInicio e DataFim para timestamp with time zone
ALTER TABLE "Catalogo" 
    ALTER COLUMN "DataCriacao" TYPE timestamp with time zone USING "DataCriacao" AT TIME ZONE 'UTC',
    ALTER COLUMN "DataAtualizacao" TYPE timestamp with time zone USING "DataAtualizacao" AT TIME ZONE 'UTC',
    ALTER COLUMN "DataInicio" TYPE timestamp with time zone USING "DataInicio" AT TIME ZONE 'UTC',
    ALTER COLUMN "DataFim" TYPE timestamp with time zone USING "DataFim" AT TIME ZONE 'UTC';

-- 2. Corrigir tabela CatalogoItem
-- Alterar DataCriacao e DataAtualizacao para timestamp with time zone
ALTER TABLE "CatalogoItem" 
    ALTER COLUMN "DataCriacao" TYPE timestamp with time zone USING "DataCriacao" AT TIME ZONE 'UTC',
    ALTER COLUMN "DataAtualizacao" TYPE timestamp with time zone USING "DataAtualizacao" AT TIME ZONE 'UTC';

-- 3. Verificar se existem outras tabelas com o mesmo problema
-- Este query identifica todas as colunas que ainda usam timestamp without time zone
-- e que podem precisar ser corrigidas
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns 
WHERE table_schema = 'public' 
    AND data_type = 'timestamp without time zone'
    AND (column_name LIKE '%Data%' OR column_name LIKE '%Date%')
ORDER BY table_name, column_name;

-- 4. Atualizar valores default para usar timezone
-- Remover e recriar constraints de default se necessário
ALTER TABLE "Catalogo" ALTER COLUMN "DataCriacao" SET DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE "CatalogoItem" ALTER COLUMN "DataCriacao" SET DEFAULT CURRENT_TIMESTAMP;

-- 5. Verificação final - listar todas as colunas timestamp with time zone
SELECT 
    table_name,
    column_name,
    data_type,
    column_default,
    is_nullable
FROM information_schema.columns 
WHERE table_schema = 'public' 
    AND data_type = 'timestamp with time zone'
ORDER BY table_name, column_name;

COMMIT;

-- Script de rollback (execute apenas se necessário reverter as mudanças)
/*
BEGIN;

-- Reverter Catalogo
ALTER TABLE "Catalogo" 
    ALTER COLUMN "DataCriacao" TYPE timestamp without time zone,
    ALTER COLUMN "DataAtualizacao" TYPE timestamp without time zone,
    ALTER COLUMN "DataInicio" TYPE timestamp without time zone,
    ALTER COLUMN "DataFim" TYPE timestamp without time zone;

-- Reverter CatalogoItem
ALTER TABLE "CatalogoItem" 
    ALTER COLUMN "DataCriacao" TYPE timestamp without time zone,
    ALTER COLUMN "DataAtualizacao" TYPE timestamp without time zone;

COMMIT;
*/