-- Script para verificar os nomes corretos das tabelas no banco
-- Execução: psql -d agriis_db -f verificar-nomes-tabelas.sql

-- Verificar todas as tabelas que podem ter problemas de nomenclatura
SELECT 
    table_name,
    COUNT(*) as total_colunas
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name ILIKE '%categoria%'
GROUP BY table_name
ORDER BY table_name;

-- Verificar tabelas relacionadas aos módulos que estamos corrigindo
SELECT 
    table_name,
    COUNT(*) as total_colunas
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND (
    table_name ILIKE '%fornecedor%' OR
    table_name ILIKE '%produtor%' OR
    table_name ILIKE '%produto%' OR
    table_name ILIKE '%categoria%' OR
    table_name ILIKE '%estados%'
)
GROUP BY table_name
ORDER BY table_name;

-- Verificar especificamente as colunas de data nas tabelas suspeitas
SELECT 
    t.table_name,
    c.column_name,
    c.data_type,
    c.is_nullable
FROM information_schema.tables t
JOIN information_schema.columns c ON t.table_name = c.table_name
WHERE t.table_schema = 'public' 
AND c.table_schema = 'public'
AND (
    t.table_name ILIKE '%categoria%' OR
    t.table_name = 'Produto' OR
    t.table_name = 'Produtor' OR
    t.table_name = 'Fornecedor'
)
AND c.column_name IN ('DataCriacao', 'DataAtualizacao', 'data_criacao', 'data_atualizacao')
ORDER BY t.table_name, c.column_name;