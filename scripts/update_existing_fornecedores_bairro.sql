-- =====================================================
-- SCRIPT: ATUALIZAR FORNECEDORES EXISTENTES COM BAIRRO
-- Data: $(date)
-- Descrição: Atualiza fornecedores existentes que não têm bairro preenchido
-- =====================================================

-- Verificar fornecedores sem bairro
SELECT 
    'FORNECEDORES SEM BAIRRO' as status,
    COUNT(*) as total_sem_bairro
FROM public."Fornecedor" 
WHERE "Bairro" IS NULL OR "Bairro" = '';

-- Mostrar alguns exemplos
SELECT 
    "Id",
    "Nome",
    "Logradouro",
    "Bairro",
    "Cep"
FROM public."Fornecedor" 
WHERE "Bairro" IS NULL OR "Bairro" = ''
LIMIT 10;

-- =====================================================
-- OPÇÃO 1: ATUALIZAR COM BAIRRO PADRÃO BASEADO NA LOCALIZAÇÃO
-- =====================================================

-- Atualizar fornecedores sem bairro com um bairro padrão baseado no município
UPDATE public."Fornecedor" 
SET "Bairro" = CASE 
    WHEN "MunicipioId" IS NOT NULL THEN 'Centro'
    ELSE 'Não informado'
END
WHERE ("Bairro" IS NULL OR "Bairro" = '') 
AND "Ativo" = true;

-- =====================================================
-- OPÇÃO 2: ATUALIZAR COM BAIRROS ESPECÍFICOS POR MUNICÍPIO
-- =====================================================

-- Para São Paulo - usar bairros conhecidos
UPDATE public."Fornecedor" 
SET "Bairro" = CASE 
    WHEN "Cep" LIKE '01%' THEN 'Centro'
    WHEN "Cep" LIKE '02%' THEN 'Santana'
    WHEN "Cep" LIKE '03%' THEN 'Bom Retiro'
    WHEN "Cep" LIKE '04%' THEN 'Vila Olímpia'
    WHEN "Cep" LIKE '05%' THEN 'Pinheiros'
    WHEN "Cep" LIKE '08%' THEN 'Vila Formosa'
    ELSE 'Centro'
END
WHERE ("Bairro" IS NULL OR "Bairro" = '') 
AND "MunicipioId" = (
    SELECT "Id" FROM public."Municipios" 
    WHERE "Nome" ILIKE '%São Paulo%' 
    LIMIT 1
);

-- Para outros municípios - usar Centro como padrão
UPDATE public."Fornecedor" 
SET "Bairro" = 'Centro'
WHERE ("Bairro" IS NULL OR "Bairro" = '') 
AND "MunicipioId" IS NOT NULL
AND "MunicipioId" != (
    SELECT "Id" FROM public."Municipios" 
    WHERE "Nome" ILIKE '%São Paulo%' 
    LIMIT 1
);

-- =====================================================
-- OPÇÃO 3: ATUALIZAR FORNECEDOR ESPECÍFICO (EXEMPLO)
-- =====================================================

-- Atualizar o fornecedor do exemplo com um bairro apropriado
UPDATE public."Fornecedor" 
SET "Bairro" = 'Tambauzinho'  -- Bairro comum em João Pessoa/PB baseado no CEP 58037755
WHERE "Id" = 6;

-- =====================================================
-- VALIDAÇÃO PÓS-ATUALIZAÇÃO
-- =====================================================

-- Verificar quantos fornecedores ainda estão sem bairro
SELECT 
    'APÓS ATUALIZAÇÃO' as status,
    COUNT(*) as total_sem_bairro
FROM public."Fornecedor" 
WHERE "Bairro" IS NULL OR "Bairro" = '';

-- Mostrar alguns fornecedores atualizados
SELECT 
    "Id",
    "Nome",
    "Logradouro",
    "Bairro",
    "Cep",
    CASE 
        WHEN "Bairro" IS NOT NULL AND "Bairro" != '' THEN '✅'
        ELSE '❌'
    END as status_bairro
FROM public."Fornecedor" 
WHERE "Ativo" = true
ORDER BY "Id"
LIMIT 10;

-- Verificar o fornecedor específico do exemplo
SELECT 
    "Id",
    "Nome",
    "Logradouro",
    "Bairro",
    "Cep",
    "MunicipioId"
FROM public."Fornecedor" 
WHERE "Id" = 6;

-- =====================================================
-- ESTATÍSTICAS FINAIS
-- =====================================================

SELECT 
    'ESTATÍSTICAS FINAIS' as relatorio,
    COUNT(*) as total_fornecedores,
    COUNT("Bairro") as fornecedores_com_bairro,
    COUNT(*) - COUNT("Bairro") as fornecedores_sem_bairro,
    ROUND(
        (COUNT("Bairro")::decimal / COUNT(*)) * 100, 2
    ) as percentual_com_bairro
FROM public."Fornecedor" 
WHERE "Ativo" = true;

RAISE NOTICE 'Script de atualização de bairros concluído!';