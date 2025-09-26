-- Script para adicionar campos de dimensões na tabela Produto
-- Data: 2025-01-27
-- Objetivo: Sincronizar tabela Produto com o mapeamento da API .NET

-- Adicionar campos de dimensões físicas do produto
ALTER TABLE public."Produto" 
ADD COLUMN IF NOT EXISTS "Altura" numeric(10, 2) DEFAULT 0 NOT NULL,
ADD COLUMN IF NOT EXISTS "Largura" numeric(10, 2) DEFAULT 0 NOT NULL,
ADD COLUMN IF NOT EXISTS "Comprimento" numeric(10, 2) DEFAULT 0 NOT NULL,
ADD COLUMN IF NOT EXISTS "PesoNominal" numeric(10, 3) DEFAULT 0 NOT NULL;

-- Adicionar comentários para documentar os novos campos
COMMENT ON COLUMN public."Produto"."Altura" IS 'Altura do produto em centímetros';
COMMENT ON COLUMN public."Produto"."Largura" IS 'Largura do produto em centímetros';
COMMENT ON COLUMN public."Produto"."Comprimento" IS 'Comprimento do produto em centímetros';
COMMENT ON COLUMN public."Produto"."PesoNominal" IS 'Peso nominal do produto em quilogramas';

-- Criar índices para otimizar consultas por dimensões (opcional)
CREATE INDEX IF NOT EXISTS "IX_Produto_Dimensoes" ON public."Produto" USING btree ("Altura", "Largura", "Comprimento");
CREATE INDEX IF NOT EXISTS "IX_Produto_PesoNominal" ON public."Produto" USING btree ("PesoNominal");

-- Atualizar produtos existentes com valores padrão baseados no peso da embalagem
-- (assumindo que produtos sem dimensões terão dimensões calculadas baseadas no peso)
UPDATE public."Produto" 
SET 
    "Altura" = CASE 
        WHEN "PesoEmbalagem" <= 1 THEN 10.0  -- Produtos leves: 10cm altura
        WHEN "PesoEmbalagem" <= 5 THEN 15.0  -- Produtos médios: 15cm altura
        ELSE 20.0                            -- Produtos pesados: 20cm altura
    END,
    "Largura" = CASE 
        WHEN "PesoEmbalagem" <= 1 THEN 15.0  -- Produtos leves: 15cm largura
        WHEN "PesoEmbalagem" <= 5 THEN 25.0  -- Produtos médios: 25cm largura
        ELSE 35.0                            -- Produtos pesados: 35cm largura
    END,
    "Comprimento" = CASE 
        WHEN "PesoEmbalagem" <= 1 THEN 20.0  -- Produtos leves: 20cm comprimento
        WHEN "PesoEmbalagem" <= 5 THEN 30.0  -- Produtos médios: 30cm comprimento
        ELSE 40.0                            -- Produtos pesados: 40cm comprimento
    END,
    "PesoNominal" = "PesoEmbalagem"          -- Peso nominal igual ao peso da embalagem inicialmente
WHERE 
    "Altura" = 0 OR "Largura" = 0 OR "Comprimento" = 0 OR "PesoNominal" = 0;

-- Verificar se as alterações foram aplicadas corretamente
SELECT 
    column_name,
    data_type,
    numeric_precision,
    numeric_scale,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'Produto' 
    AND table_schema = 'public'
    AND column_name IN ('Altura', 'Largura', 'Comprimento', 'PesoNominal')
ORDER BY column_name;

-- Verificar alguns registros atualizados
SELECT 
    "Id",
    "Nome",
    "PesoEmbalagem",
    "Altura",
    "Largura", 
    "Comprimento",
    "PesoNominal"
FROM public."Produto" 
LIMIT 5;