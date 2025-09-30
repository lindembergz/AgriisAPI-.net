-- Script de validação para verificar se o fix do DateTimeOffset foi aplicado corretamente
-- Execute este script após aplicar o fix-datetimeoffset-mapping.sql

-- 1. Verificar se as colunas da tabela Catalogo foram corrigidas
SELECT 
    'Catalogo' as tabela,
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_schema = 'public' 
    AND table_name = 'Catalogo'
    AND column_name IN ('DataCriacao', 'DataAtualizacao', 'DataInicio', 'DataFim')
ORDER BY column_name;

-- 2. Verificar se as colunas da tabela CatalogoItem foram corrigidas
SELECT 
    'CatalogoItem' as tabela,
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_schema = 'public' 
    AND table_name = 'CatalogoItem'
    AND column_name IN ('DataCriacao', 'DataAtualizacao')
ORDER BY column_name;

-- 3. Listar TODAS as colunas que ainda usam timestamp without time zone
-- (estas podem precisar de correção adicional)
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable,
    'PRECISA CORREÇÃO' as status
FROM information_schema.columns 
WHERE table_schema = 'public' 
    AND data_type = 'timestamp without time zone'
    AND (column_name LIKE '%Data%' OR column_name LIKE '%Date%' OR column_name LIKE '%Time%')
ORDER BY table_name, column_name;

-- 4. Listar todas as colunas timestamp with time zone (corretas)
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable,
    'CORRETO' as status
FROM information_schema.columns 
WHERE table_schema = 'public' 
    AND data_type = 'timestamp with time zone'
ORDER BY table_name, column_name;

-- 5. Teste de inserção para verificar se não há mais erros
-- (Descomente para testar - CUIDADO: vai inserir dados de teste)
/*
INSERT INTO "Catalogo" (
    "SafraId", "PontoDistribuicaoId", "CulturaId", "CategoriaId", 
    "Moeda", "DataInicio", "DataFim", "Ativo"
) VALUES (
    1, 1, 1, 1, 
    'BRL', CURRENT_TIMESTAMP, NULL, true
);

-- Verificar se a inserção funcionou
SELECT 
    "Id", "DataCriacao", "DataInicio", "DataFim", "DataAtualizacao"
FROM "Catalogo" 
WHERE "Id" = (SELECT MAX("Id") FROM "Catalogo");

-- Limpar dados de teste
DELETE FROM "Catalogo" WHERE "Id" = (SELECT MAX("Id") FROM "Catalogo");
*/