-- Script para adicionar novos campos na tabela Fornecedor
-- NomeFantasia, RamosAtividade e EnderecoCorrespondencia
-- Data: 2025-01-03

BEGIN;

-- Adicionar coluna NomeFantasia (string até 200 caracteres)
ALTER TABLE "Fornecedor" 
ADD COLUMN "NomeFantasia" VARCHAR(200) NULL;

-- Adicionar coluna RamosAtividade (array de strings)
ALTER TABLE "Fornecedor" 
ADD COLUMN "RamosAtividade" TEXT[] DEFAULT '{}' NOT NULL;

-- Adicionar coluna EnderecoCorrespondencia (enum como string)
ALTER TABLE "Fornecedor" 
ADD COLUMN "EnderecoCorrespondencia" VARCHAR(20) DEFAULT 'MesmoFaturamento' NOT NULL;

-- Adicionar comentários nas colunas para documentação
COMMENT ON COLUMN "Fornecedor"."NomeFantasia" IS 'Nome fantasia/comercial do fornecedor (até 200 caracteres)';
COMMENT ON COLUMN "Fornecedor"."RamosAtividade" IS 'Array com os ramos de atividade do fornecedor';
COMMENT ON COLUMN "Fornecedor"."EnderecoCorrespondencia" IS 'Configuração do endereço de correspondência: MesmoFaturamento ou DiferenteFaturamento';

-- Criar índice para busca por ramos de atividade (usando GIN para arrays)
CREATE INDEX "IX_Fornecedor_RamosAtividade" ON "Fornecedor" USING GIN ("RamosAtividade");

-- Criar índice para EnderecoCorrespondencia
CREATE INDEX "IX_Fornecedor_EnderecoCorrespondencia" ON "Fornecedor" ("EnderecoCorrespondencia");

-- Verificar se as colunas foram criadas corretamente
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'Fornecedor' 
AND column_name IN ('NomeFantasia', 'RamosAtividade', 'EnderecoCorrespondencia')
ORDER BY column_name;

COMMIT;

-- Script de rollback (comentado - descomente se precisar reverter)
/*
BEGIN;

-- Remover índices
DROP INDEX IF EXISTS "IX_Fornecedor_RamosAtividade";
DROP INDEX IF EXISTS "IX_Fornecedor_EnderecoCorrespondencia";

-- Remover colunas
ALTER TABLE "Fornecedor" DROP COLUMN IF EXISTS "NomeFantasia";
ALTER TABLE "Fornecedor" DROP COLUMN IF EXISTS "RamosAtividade";
ALTER TABLE "Fornecedor" DROP COLUMN IF EXISTS "EnderecoCorrespondencia";

COMMIT;
*/