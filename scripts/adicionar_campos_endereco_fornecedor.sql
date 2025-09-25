-- Script para adicionar campos de endereço na tabela Fornecedor
-- Execute este script manualmente se a migration falhar

-- Adicionar novos campos de endereço na tabela Fornecedor
ALTER TABLE public."Fornecedor" 
ADD COLUMN IF NOT EXISTS "Municipio" character varying(100),
ADD COLUMN IF NOT EXISTS "Uf" character varying(2),
ADD COLUMN IF NOT EXISTS "Cep" character varying(10),
ADD COLUMN IF NOT EXISTS "Complemento" character varying(200),
ADD COLUMN IF NOT EXISTS "Latitude" numeric(10,8),
ADD COLUMN IF NOT EXISTS "Longitude" numeric(11,8);

-- Verificar se as colunas foram criadas
SELECT column_name, data_type, character_maximum_length, is_nullable
FROM information_schema.columns 
WHERE table_name = 'Fornecedor' 
  AND table_schema = 'public'
  AND column_name IN ('Municipio', 'Uf', 'Cep', 'Complemento', 'Latitude', 'Longitude')
ORDER BY column_name;

-- Comentários nas colunas para documentação
COMMENT ON COLUMN public."Fornecedor"."Municipio" IS 'Município do fornecedor';
COMMENT ON COLUMN public."Fornecedor"."Uf" IS 'UF (Estado) do fornecedor';
COMMENT ON COLUMN public."Fornecedor"."Cep" IS 'CEP do fornecedor';
COMMENT ON COLUMN public."Fornecedor"."Complemento" IS 'Complemento do endereço';
COMMENT ON COLUMN public."Fornecedor"."Latitude" IS 'Latitude da localização';
COMMENT ON COLUMN public."Fornecedor"."Longitude" IS 'Longitude da localização';

-- Atualizar a tabela __EFMigrationsHistory para marcar a migration como aplicada
INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250925005430_AdicionarCamposEnderecoFornecedor', '9.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;

-- Verificar se a migration foi registrada
SELECT "MigrationId", "ProductVersion" 
FROM public."__EFMigrationsHistory" 
WHERE "MigrationId" = '20250925005430_AdicionarCamposEnderecoFornecedor';

COMMIT;