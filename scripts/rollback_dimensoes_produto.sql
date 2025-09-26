-- Script de rollback para remover campos de dimensões da tabela Produto
-- Data: 2025-01-27
-- Objetivo: Reverter alterações caso necessário

-- ATENÇÃO: Este script remove colunas e dados permanentemente!
-- Certifique-se de ter um backup antes de executar

-- Verificar se as colunas existem antes de tentar removê-las
DO $$
BEGIN
    -- Remover índices criados
    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Produto_Dimensoes') THEN
        DROP INDEX IF EXISTS public."IX_Produto_Dimensoes";
        RAISE NOTICE 'Índice IX_Produto_Dimensoes removido';
    END IF;
    
    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Produto_PesoNominal') THEN
        DROP INDEX IF EXISTS public."IX_Produto_PesoNominal";
        RAISE NOTICE 'Índice IX_Produto_PesoNominal removido';
    END IF;
    
    -- Remover colunas se existirem
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produto' AND column_name = 'Altura' AND table_schema = 'public') THEN
        ALTER TABLE public."Produto" DROP COLUMN "Altura";
        RAISE NOTICE 'Coluna Altura removida';
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produto' AND column_name = 'Largura' AND table_schema = 'public') THEN
        ALTER TABLE public."Produto" DROP COLUMN "Largura";
        RAISE NOTICE 'Coluna Largura removida';
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produto' AND column_name = 'Comprimento' AND table_schema = 'public') THEN
        ALTER TABLE public."Produto" DROP COLUMN "Comprimento";
        RAISE NOTICE 'Coluna Comprimento removida';
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produto' AND column_name = 'PesoNominal' AND table_schema = 'public') THEN
        ALTER TABLE public."Produto" DROP COLUMN "PesoNominal";
        RAISE NOTICE 'Coluna PesoNominal removida';
    END IF;
    
    RAISE NOTICE 'Rollback concluído com sucesso';
END $$;

-- Verificar se as colunas foram removidas
SELECT 
    column_name,
    data_type
FROM information_schema.columns 
WHERE table_name = 'Produto' 
    AND table_schema = 'public'
    AND column_name IN ('Altura', 'Largura', 'Comprimento', 'PesoNominal')
ORDER BY column_name;

-- Se não retornar nenhuma linha, o rollback foi bem-sucedido