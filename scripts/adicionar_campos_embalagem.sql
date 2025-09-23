-- Script SQL para adicionar campos de embalagem na tabela Produtos
-- Compatibilidade com sistema Python original
-- Execute este script no PostgreSQL para adicionar os campos necessários

-- Adicionar campos de embalagem na tabela Produtos
ALTER TABLE "Produtos" 
ADD COLUMN IF NOT EXISTS "PesoEmbalagem" DECIMAL(10,3) NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS "Pms" DECIMAL(10,3) NULL,
ADD COLUMN IF NOT EXISTS "QuantidadeMinima" DECIMAL(10,3) NOT NULL DEFAULT 1,
ADD COLUMN IF NOT EXISTS "Embalagem" VARCHAR(100) NOT NULL DEFAULT '',
ADD COLUMN IF NOT EXISTS "FaixaDensidadeInicial" DECIMAL(10,3) NULL,
ADD COLUMN IF NOT EXISTS "FaixaDensidadeFinal" DECIMAL(10,3) NULL;

-- Comentários para documentar os campos
COMMENT ON COLUMN "Produtos"."PesoEmbalagem" IS 'Peso da embalagem em quilogramas (usado nos cálculos de frete)';
COMMENT ON COLUMN "Produtos"."Pms" IS 'Peso de mil sementes em gramas (PMS - usado para cálculo de peso de sementes)';
COMMENT ON COLUMN "Produtos"."QuantidadeMinima" IS 'Quantidade mínima por embalagem';
COMMENT ON COLUMN "Produtos"."Embalagem" IS 'Tipo de embalagem (saco, caixa, tambor, etc.)';
COMMENT ON COLUMN "Produtos"."FaixaDensidadeInicial" IS 'Densidade inicial para cálculo cúbico (kg/m³)';
COMMENT ON COLUMN "Produtos"."FaixaDensidadeFinal" IS 'Densidade final para cálculo cúbico (kg/m³)';

-- Atualizar valores padrão para produtos existentes (se houver)
-- Copiar PesoNominal para PesoEmbalagem como valor inicial
UPDATE "Produtos" 
SET "PesoEmbalagem" = "PesoNominal"
WHERE "PesoEmbalagem" = 0;

-- Definir embalagem padrão baseada na categoria (se necessário)
UPDATE "Produtos" 
SET "Embalagem" = 'Saco'
WHERE "Embalagem" = '' AND EXISTS (
    SELECT 1 FROM "Categorias" c 
    WHERE c."Id" = "Produtos"."CategoriaId" 
    AND c."Nome" ILIKE '%semente%'
);

UPDATE "Produtos" 
SET "Embalagem" = 'Tambor'
WHERE "Embalagem" = '' AND EXISTS (
    SELECT 1 FROM "Categorias" c 
    WHERE c."Id" = "Produtos"."CategoriaId" 
    AND (c."Nome" ILIKE '%defensivo%' OR c."Nome" ILIKE '%agrotóxico%')
);

UPDATE "Produtos" 
SET "Embalagem" = 'Caixa'
WHERE "Embalagem" = '';

-- Verificar se os campos foram criados corretamente
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default,
    character_maximum_length,
    numeric_precision,
    numeric_scale
FROM information_schema.columns 
WHERE table_name = 'Produtos' 
AND column_name IN ('PesoEmbalagem', 'Pms', 'QuantidadeMinima', 'Embalagem', 'FaixaDensidadeInicial', 'FaixaDensidadeFinal')
ORDER BY column_name;

-- Verificar dados após a atualização
SELECT 
    "Id",
    "Nome",
    "PesoNominal",
    "PesoEmbalagem",
    "QuantidadeMinima",
    "Embalagem",
    "Pms",
    "FaixaDensidadeInicial",
    "FaixaDensidadeFinal"
FROM "Produtos" 
LIMIT 5;