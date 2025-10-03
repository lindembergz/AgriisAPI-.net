-- =====================================================
-- SCRIPT DE MIGRAÇÃO: ADICIONAR CAMPO BAIRRO NA TABELA FORNECEDOR
-- Data: $(date)
-- Descrição: Adiciona a coluna Bairro na tabela Fornecedor
-- =====================================================

-- Verificar se a coluna já existe antes de adicionar
DO $$
BEGIN
    -- Verificar se a coluna Bairro já existe
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'Fornecedor' 
        AND column_name = 'Bairro' 
        AND table_schema = 'public'
    ) THEN
        -- Adicionar a coluna Bairro
        ALTER TABLE public."Fornecedor" 
        ADD COLUMN "Bairro" VARCHAR(100) NULL;
        
        RAISE NOTICE 'Coluna Bairro adicionada com sucesso na tabela Fornecedor';
    ELSE
        RAISE NOTICE 'Coluna Bairro já existe na tabela Fornecedor';
    END IF;
END $$;

-- Criar índice para melhorar performance de consultas por bairro
DO $$
BEGIN
    -- Verificar se o índice já existe
    IF NOT EXISTS (
        SELECT 1 
        FROM pg_indexes 
        WHERE tablename = 'Fornecedor' 
        AND indexname = 'IX_Fornecedor_Bairro'
        AND schemaname = 'public'
    ) THEN
        -- Criar índice
        CREATE INDEX "IX_Fornecedor_Bairro" ON public."Fornecedor" ("Bairro")
        WHERE "Bairro" IS NOT NULL;
        
        RAISE NOTICE 'Índice IX_Fornecedor_Bairro criado com sucesso';
    ELSE
        RAISE NOTICE 'Índice IX_Fornecedor_Bairro já existe';
    END IF;
END $$;

-- Adicionar comentário na coluna
COMMENT ON COLUMN public."Fornecedor"."Bairro" IS 'Bairro do endereço do fornecedor';

-- =====================================================
-- VALIDAÇÃO PÓS-MIGRAÇÃO
-- =====================================================

-- Verificar se a coluna foi criada corretamente
SELECT 
    'VALIDAÇÃO - Estrutura da coluna Bairro' as validacao,
    column_name,
    data_type,
    character_maximum_length,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'Fornecedor' 
AND column_name = 'Bairro' 
AND table_schema = 'public';

-- Verificar se o índice foi criado
SELECT 
    'VALIDAÇÃO - Índice da coluna Bairro' as validacao,
    indexname,
    tablename,
    indexdef
FROM pg_indexes 
WHERE tablename = 'Fornecedor' 
AND indexname = 'IX_Fornecedor_Bairro'
AND schemaname = 'public';

-- Contar registros na tabela para verificar integridade
SELECT 
    'VALIDAÇÃO - Integridade dos dados' as validacao,
    COUNT(*) as total_fornecedores,
    COUNT("Bairro") as fornecedores_com_bairro,
    COUNT(*) - COUNT("Bairro") as fornecedores_sem_bairro
FROM public."Fornecedor";

-- =====================================================
-- SCRIPT DE ROLLBACK (COMENTADO - DESCOMENTE SE NECESSÁRIO)
-- =====================================================

/*
-- ATENÇÃO: Execute apenas se precisar reverter a migração
-- Este script remove a coluna Bairro e seu índice

-- Remover índice
DROP INDEX IF EXISTS public."IX_Fornecedor_Bairro";

-- Remover coluna
ALTER TABLE public."Fornecedor" DROP COLUMN IF EXISTS "Bairro";

-- Verificar se foi removido
SELECT 
    CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM information_schema.columns 
            WHERE table_name = 'Fornecedor' 
            AND column_name = 'Bairro' 
            AND table_schema = 'public'
        )
        THEN '❌ ERRO: Coluna Bairro ainda existe'
        ELSE '✅ OK: Coluna Bairro removida com sucesso'
    END as status_rollback;
*/

RAISE NOTICE 'Migração concluída com sucesso!';