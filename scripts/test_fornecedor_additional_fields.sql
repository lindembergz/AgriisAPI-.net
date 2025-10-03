-- Script de teste para verificar os novos campos do Fornecedor
-- Executa testes básicos de inserção e consulta

-- Verificar estrutura da tabela
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default,
    character_maximum_length
FROM information_schema.columns 
WHERE table_name = 'Fornecedor' 
AND column_name IN ('NomeFantasia', 'RamosAtividade', 'EnderecoCorrespondencia')
ORDER BY column_name;

-- Verificar índices criados
SELECT 
    indexname,
    indexdef
FROM pg_indexes 
WHERE tablename = 'Fornecedor' 
AND indexname IN ('IX_Fornecedor_RamosAtividade', 'IX_Fornecedor_EnderecoCorrespondencia');

-- Teste de inserção com novos campos (exemplo)
-- ATENÇÃO: Este é apenas um exemplo - ajuste os valores conforme necessário
/*
INSERT INTO "Fornecedor" (
    "Nome", 
    "Cnpj", 
    "NomeFantasia", 
    "RamosAtividade", 
    "EnderecoCorrespondencia",
    "Ativo",
    "DataCriacao",
    "DataModificacao"
) VALUES (
    'Empresa Teste Ltda',
    '12345678000190',
    'Teste Agro',
    ARRAY['Sementes', 'Fertilizantes'],
    'MesmoFaturamento',
    true,
    NOW(),
    NOW()
);
*/

-- Consulta para verificar fornecedores existentes com novos campos
SELECT 
    "Id",
    "Nome",
    "NomeFantasia",
    "RamosAtividade",
    "EnderecoCorrespondencia"
FROM "Fornecedor" 
LIMIT 5;

-- Teste de busca por ramo de atividade
-- SELECT * FROM "Fornecedor" WHERE 'Sementes' = ANY("RamosAtividade");

-- Teste de busca por endereço de correspondência
-- SELECT * FROM "Fornecedor" WHERE "EnderecoCorrespondencia" = 'DiferenteFaturamento';