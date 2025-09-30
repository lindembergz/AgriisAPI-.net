-- Script para adicionar a coluna 'ativo' nas tabelas de referência se não existir

-- 1. Adicionar coluna 'ativo' na tabela estados_referencia se não existir
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'estados_referencia' 
        AND column_name = 'ativo'
    ) THEN
        ALTER TABLE public.estados_referencia 
        ADD COLUMN ativo BOOLEAN NOT NULL DEFAULT true;
        
        RAISE NOTICE 'Coluna ativo adicionada à tabela estados_referencia';
    ELSE
        RAISE NOTICE 'Coluna ativo já existe na tabela estados_referencia';
    END IF;
END $$;

-- 2. Adicionar coluna 'ativo' na tabela municipios_referencia se não existir
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'municipios_referencia' 
        AND column_name = 'ativo'
    ) THEN
        ALTER TABLE public.municipios_referencia 
        ADD COLUMN ativo BOOLEAN NOT NULL DEFAULT true;
        
        RAISE NOTICE 'Coluna ativo adicionada à tabela municipios_referencia';
    ELSE
        RAISE NOTICE 'Coluna ativo já existe na tabela municipios_referencia';
    END IF;
END $$;

-- 3. Adicionar coluna 'ativo' na tabela paises se não existir
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'paises' 
        AND column_name = 'ativo'
    ) THEN
        ALTER TABLE public.paises 
        ADD COLUMN ativo BOOLEAN NOT NULL DEFAULT true;
        
        RAISE NOTICE 'Coluna ativo adicionada à tabela paises';
    ELSE
        RAISE NOTICE 'Coluna ativo já existe na tabela paises';
    END IF;
END $$;

-- 4. Verificar as colunas das tabelas
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_schema = 'public' 
AND table_name IN ('paises', 'estados_referencia', 'municipios_referencia')
ORDER BY table_name, ordinal_position;