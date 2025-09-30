-- Script para testar as correções de DateTimeOffset nos módulos Produtores e Produtos
-- Execução: psql -d agriis_db -f test-produtores-produtos-datetime.sql

-- Verificar tipos de colunas específicas dos módulos Produtores e Produtos
SELECT 
    table_name, 
    column_name, 
    data_type,
    datetime_precision,
    is_nullable
FROM information_schema.columns 
WHERE table_schema = 'public' 
AND table_name IN ('Produtor', 'UsuarioProdutor', 'Produto', 'Categoria')
AND column_name IN ('DataCriacao', 'DataAtualizacao', 'DataAutorizacao')
ORDER BY table_name, column_name;

-- Verificar se há registros nas tabelas e seus tipos de data
SELECT 
    'Produtor' as tabela,
    COUNT(*) as total_registros,
    MIN("DataCriacao") as primeira_data_criacao,
    MAX("DataCriacao") as ultima_data_criacao,
    COUNT("DataAtualizacao") as registros_com_atualizacao
FROM "Produtor"
WHERE "DataCriacao" IS NOT NULL

UNION ALL

SELECT 
    'UsuarioProdutor' as tabela,
    COUNT(*) as total_registros,
    MIN("DataCriacao") as primeira_data_criacao,
    MAX("DataCriacao") as ultima_data_criacao,
    COUNT("DataAtualizacao") as registros_com_atualizacao
FROM "UsuarioProdutor"
WHERE "DataCriacao" IS NOT NULL

UNION ALL

SELECT 
    'Produto' as tabela,
    COUNT(*) as total_registros,
    MIN("DataCriacao") as primeira_data_criacao,
    MAX("DataCriacao") as ultima_data_criacao,
    COUNT("DataAtualizacao") as registros_com_atualizacao
FROM "Produto"
WHERE "DataCriacao" IS NOT NULL

UNION ALL

SELECT 
    'Categoria' as tabela,
    COUNT(*) as total_registros,
    MIN("DataCriacao") as primeira_data_criacao,
    MAX("DataCriacao") as ultima_data_criacao,
    COUNT("DataAtualizacao") as registros_com_atualizacao
FROM "Categoria"
WHERE "DataCriacao" IS NOT NULL;

-- Testar inserção com timezone em uma categoria de teste
INSERT INTO "Categoria" ("Nome", "Tipo", "Ativo", "Ordem", "DataCriacao") 
VALUES ('Categoria Teste DateTime', 1, true, 999, CURRENT_TIMESTAMP)
ON CONFLICT ("Nome") DO NOTHING;

-- Verificar se a inserção funcionou
SELECT "Id", "Nome", "DataCriacao", "DataAtualizacao" 
FROM "Categoria" 
WHERE "Nome" = 'Categoria Teste DateTime';

-- Testar atualização
UPDATE "Categoria" 
SET "DataAtualizacao" = CURRENT_TIMESTAMP 
WHERE "Nome" = 'Categoria Teste DateTime';

-- Verificar atualização
SELECT "Id", "Nome", "DataCriacao", "DataAtualizacao" 
FROM "Categoria" 
WHERE "Nome" = 'Categoria Teste DateTime';

-- Limpar teste
DELETE FROM "Categoria" WHERE "Nome" = 'Categoria Teste DateTime';

-- Verificar se há problemas de timezone em registros existentes
SELECT 
    table_name,
    column_name,
    COUNT(*) as total_registros
FROM information_schema.columns c
JOIN (
    SELECT 'Produtor' as table_name, 'DataCriacao' as column_name
    UNION ALL SELECT 'Produtor', 'DataAtualizacao'
    UNION ALL SELECT 'Produtor', 'DataAutorizacao'
    UNION ALL SELECT 'UsuarioProdutor', 'DataCriacao'
    UNION ALL SELECT 'UsuarioProdutor', 'DataAtualizacao'
    UNION ALL SELECT 'Produto', 'DataCriacao'
    UNION ALL SELECT 'Produto', 'DataAtualizacao'
    UNION ALL SELECT 'Categoria', 'DataCriacao'
    UNION ALL SELECT 'Categoria', 'DataAtualizacao'
) t ON c.table_name = t.table_name AND c.column_name = t.column_name
WHERE c.table_schema = 'public'
AND c.data_type = 'timestamp with time zone'
GROUP BY c.table_name, c.column_name
ORDER BY c.table_name, c.column_name;

SELECT 'Teste de DateTimeOffset para Produtores e Produtos concluído!' as status;