-- =====================================================
-- SCRIPT DE DEBUG: VERIFICAR DADOS DE BAIRRO NO BANCO
-- Data: $(date)
-- Descrição: Verifica se os dados de bairro estão realmente no banco
-- =====================================================

-- Verificar a estrutura da tabela Fornecedor
SELECT 
    'ESTRUTURA DA TABELA FORNECEDOR' as info,
    column_name,
    data_type,
    character_maximum_length,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'Fornecedor' 
AND table_schema = 'public'
AND column_name IN ('Id', 'Nome', 'Bairro', 'Logradouro', 'UfId', 'MunicipioId')
ORDER BY ordinal_position;

-- Verificar dados do fornecedor específico (ID 6)
SELECT 
    'DADOS DO FORNECEDOR ID 6' as info,
    "Id",
    "Nome",
    "Logradouro",
    "Bairro",
    "UfId",
    "MunicipioId",
    "Cep",
    "Complemento",
    "Ativo",
    "DataCriacao"
FROM public."Fornecedor" 
WHERE "Id" = 6;

-- Verificar todos os fornecedores e seus bairros
SELECT 
    'TODOS OS FORNECEDORES' as info,
    "Id",
    "Nome",
    "Bairro",
    CASE 
        WHEN "Bairro" IS NULL THEN '❌ NULL'
        WHEN "Bairro" = '' THEN '❌ VAZIO'
        ELSE '✅ PREENCHIDO'
    END as status_bairro,
    "Logradouro",
    "UfId",
    "MunicipioId"
FROM public."Fornecedor" 
WHERE "Ativo" = true
ORDER BY "Id";

-- Verificar se há algum problema de encoding ou caracteres especiais
SELECT 
    'ANÁLISE DETALHADA DO BAIRRO' as info,
    "Id",
    "Nome",
    "Bairro",
    LENGTH("Bairro") as tamanho_bairro,
    ASCII(LEFT("Bairro", 1)) as primeiro_char_ascii,
    ENCODE("Bairro"::bytea, 'hex') as bairro_hex
FROM public."Fornecedor" 
WHERE "Id" = 6;

-- Tentar atualizar o fornecedor ID 6 com um bairro de teste
UPDATE public."Fornecedor" 
SET "Bairro" = 'Tambauzinho'
WHERE "Id" = 6;

-- Verificar se a atualização funcionou
SELECT 
    'APÓS ATUALIZAÇÃO' as info,
    "Id",
    "Nome",
    "Bairro",
    "DataAtualizacao"
FROM public."Fornecedor" 
WHERE "Id" = 6;

-- Verificar se há triggers ou constraints que podem estar interferindo
SELECT 
    'TRIGGERS NA TABELA' as info,
    trigger_name,
    event_manipulation,
    action_timing,
    action_statement
FROM information_schema.triggers 
WHERE event_object_table = 'Fornecedor'
AND event_object_schema = 'public';

-- Verificar constraints
SELECT 
    'CONSTRAINTS NA TABELA' as info,
    constraint_name,
    constraint_type,
    column_name
FROM information_schema.table_constraints tc
JOIN information_schema.constraint_column_usage ccu 
    ON tc.constraint_name = ccu.constraint_name
WHERE tc.table_name = 'Fornecedor'
AND tc.table_schema = 'public'
AND ccu.column_name = 'Bairro';

-- Estatísticas finais
SELECT 
    'ESTATÍSTICAS FINAIS' as info,
    COUNT(*) as total_fornecedores,
    COUNT("Bairro") as com_bairro_nao_null,
    COUNT(CASE WHEN "Bairro" IS NOT NULL AND "Bairro" != '' THEN 1 END) as com_bairro_preenchido,
    COUNT(CASE WHEN "Bairro" IS NULL THEN 1 END) as com_bairro_null,
    COUNT(CASE WHEN "Bairro" = '' THEN 1 END) as com_bairro_vazio
FROM public."Fornecedor" 
WHERE "Ativo" = true;