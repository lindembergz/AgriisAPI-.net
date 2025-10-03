-- Script para adicionar campo QuantidadeEmbalagem à tabela Produto
-- Criado para implementar a funcionalidade de detalhes de produto

-- Verificar se o campo já existe antes de adicionar
DO $$
BEGIN
    -- Verificar se a coluna QuantidadeEmbalagem já existe
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Produto' 
        AND column_name = 'QuantidadeEmbalagem'
    ) THEN
        -- Adicionar a coluna QuantidadeEmbalagem
        ALTER TABLE public."Produto" 
        ADD COLUMN "QuantidadeEmbalagem" decimal(18,4) NOT NULL DEFAULT 1.0;
        
        -- Adicionar comentário na coluna
        COMMENT ON COLUMN public."Produto"."QuantidadeEmbalagem" IS 'Quantidade de produto por embalagem';
        
        -- Criar índice para performance (opcional, mas recomendado para consultas)
        CREATE INDEX IF NOT EXISTS "IX_Produto_QuantidadeEmbalagem" 
        ON public."Produto" ("QuantidadeEmbalagem");
        
        RAISE NOTICE 'Campo QuantidadeEmbalagem adicionado com sucesso à tabela Produto';
    ELSE
        RAISE NOTICE 'Campo QuantidadeEmbalagem já existe na tabela Produto';
    END IF;
    
    -- Verificar se há produtos com EmbalagemId NULL e QuantidadeEmbalagem = 1
    -- Isso pode indicar produtos que precisam de configuração manual
    PERFORM 1 FROM public."Produto" 
    WHERE "EmbalagemId" IS NULL AND "QuantidadeEmbalagem" = 1;
    
    IF FOUND THEN
        RAISE NOTICE 'Existem produtos sem embalagem definida. Considere configurar as embalagens apropriadas.';
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
AND table_name = 'Produto' 
AND column_name IN ('EmbalagemId', 'QuantidadeEmbalagem')
ORDER BY column_name;