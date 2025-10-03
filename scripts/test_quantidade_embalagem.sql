-- Script de teste para verificar se o campo QuantidadeEmbalagem foi adicionado corretamente

-- 1. Verificar se a coluna existe
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default,
    character_maximum_length,
    numeric_precision,
    numeric_scale
FROM information_schema.columns 
WHERE table_schema = 'public' 
AND table_name = 'Produto' 
AND column_name = 'QuantidadeEmbalagem';

-- 2. Verificar se o índice foi criado
SELECT 
    indexname,
    indexdef
FROM pg_indexes 
WHERE schemaname = 'public' 
AND tablename = 'Produto' 
AND indexname = 'IX_Produto_QuantidadeEmbalagem';

-- 3. Verificar alguns produtos existentes (se houver)
SELECT 
    "Id",
    "Nome",
    "Codigo",
    "EmbalagemId",
    "QuantidadeEmbalagem"
FROM public."Produto" 
LIMIT 5;

-- 4. Testar inserção de um produto com QuantidadeEmbalagem (apenas para teste - remover depois)
-- NOTA: Este INSERT é apenas para teste. Remova após verificar que funciona.
/*
INSERT INTO public."Produto" (
    "Nome", "Codigo", "Tipo", "Status", "UnidadeMedidaId", "CategoriaId", "FornecedorId",
    "Altura", "Largura", "Comprimento", "PesoNominal", "PesoEmbalagem", 
    "QuantidadeMinima", "Embalagem", "QuantidadeEmbalagem", "DataCriacao"
) VALUES (
    'Produto Teste QuantidadeEmbalagem', 
    'TEST-QE-001', 
    0, -- TipoProduto.Fabricante
    0, -- StatusProduto.Ativo
    1, -- Assumindo que existe UnidadeMedida com ID 1
    1, -- Assumindo que existe Categoria com ID 1
    1, -- Assumindo que existe Fornecedor com ID 1
    10.0, 20.0, 30.0, -- Dimensões
    1.5, 2.0, -- Pesos
    1.0, 'Saco', -- Quantidade mínima e embalagem
    25.0, -- QuantidadeEmbalagem (novo campo)
    NOW()
);

-- Verificar se o produto foi inserido
SELECT 
    "Id", "Nome", "Codigo", "QuantidadeEmbalagem"
FROM public."Produto" 
WHERE "Codigo" = 'TEST-QE-001';

-- Limpar o produto de teste
DELETE FROM public."Produto" WHERE "Codigo" = 'TEST-QE-001';
*/

-- 5. Verificar constraints e foreign keys relacionadas
SELECT 
    tc.constraint_name,
    tc.constraint_type,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc 
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
LEFT JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
    AND ccu.table_schema = tc.table_schema
WHERE tc.table_name = 'Produto' 
AND tc.table_schema = 'public'
AND (kcu.column_name = 'QuantidadeEmbalagem' OR kcu.column_name = 'EmbalagemId')
ORDER BY tc.constraint_type, tc.constraint_name;