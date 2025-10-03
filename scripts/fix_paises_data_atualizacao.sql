-- Script para corrigir a coluna data_atualizacao na tabela paises
-- Verifica se a coluna existe e a cria se necessário

-- Verificar se a coluna data_atualizacao existe
DO $$
BEGIN
    -- Verificar se a coluna existe
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'paises' 
        AND column_name = 'data_atualizacao'
    ) THEN
        -- Adicionar a coluna data_atualizacao
        ALTER TABLE public.paises 
        ADD COLUMN data_atualizacao timestamptz NULL;
        
        RAISE NOTICE 'Coluna data_atualizacao adicionada à tabela paises';
    ELSE
        RAISE NOTICE 'Coluna data_atualizacao já existe na tabela paises';
    END IF;
END $$;

-- Verificar se a coluna data_criacao tem o tipo correto
DO $$
BEGIN
    -- Verificar se a coluna data_criacao tem o tipo timestamptz
    IF EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'paises' 
        AND column_name = 'data_criacao'
        AND data_type != 'timestamp with time zone'
    ) THEN
        -- Alterar o tipo da coluna data_criacao
        ALTER TABLE public.paises 
        ALTER COLUMN data_criacao TYPE timestamptz USING data_criacao AT TIME ZONE 'UTC';
        
        RAISE NOTICE 'Tipo da coluna data_criacao alterado para timestamptz';
    ELSE
        RAISE NOTICE 'Coluna data_criacao já tem o tipo correto';
    END IF;
END $$;

-- Criar índice para data_atualizacao se não existir
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM pg_indexes 
        WHERE schemaname = 'public' 
        AND tablename = 'paises' 
        AND indexname = 'IX_paises_data_atualizacao'
    ) THEN
        CREATE INDEX IX_paises_data_atualizacao ON public.paises (data_atualizacao);
        RAISE NOTICE 'Índice IX_paises_data_atualizacao criado';
    ELSE
        RAISE NOTICE 'Índice IX_paises_data_atualizacao já existe';
    END IF;
END $$;

-- Verificar a estrutura final da tabela
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_schema = 'public' 
AND table_name = 'paises'
ORDER BY ordinal_position;