-- Script para testar as correções de DateTimeOffset
-- Execução: psql -d agriis_db -f test-datetime-fixes.sql

-- Verificar tipos de colunas após correção
SELECT 
    table_name, 
    column_name, 
    data_type,
    datetime_precision,
    is_nullable
FROM information_schema.columns 
WHERE table_schema = 'public' 
AND column_name IN ('data_criacao', 'data_atualizacao', 'DataCriacao', 'DataAtualizacao')
ORDER BY table_name, column_name;

-- Testar inserção com timezone
INSERT INTO estados_referencia (uf, nome, pais_id, ativo, data_criacao) 
VALUES ('TS', 'Estado Teste', 1, true, CURRENT_TIMESTAMP)
ON CONFLICT (uf) DO NOTHING;

-- Verificar se a inserção funcionou
SELECT id, uf, nome, data_criacao, data_atualizacao 
FROM estados_referencia 
WHERE uf = 'TS';

-- Testar atualização
UPDATE estados_referencia 
SET data_atualizacao = CURRENT_TIMESTAMP 
WHERE uf = 'TS';

-- Verificar atualização
SELECT id, uf, nome, data_criacao, data_atualizacao 
FROM estados_referencia 
WHERE uf = 'TS';

-- Limpar teste
DELETE FROM estados_referencia WHERE uf = 'TS';

-- Verificar algumas linhas existentes para confirmar formato
SELECT 
    'estados_referencia' as tabela,
    COUNT(*) as total_registros,
    MIN(data_criacao) as primeira_data,
    MAX(data_criacao) as ultima_data
FROM estados_referencia
WHERE data_criacao IS NOT NULL

UNION ALL

SELECT 
    'Fornecedor' as tabela,
    COUNT(*) as total_registros,
    MIN("DataCriacao") as primeira_data,
    MAX("DataCriacao") as ultima_data
FROM "Fornecedor"
WHERE "DataCriacao" IS NOT NULL

UNION ALL

SELECT 
    'UsuarioFornecedor' as tabela,
    COUNT(*) as total_registros,
    MIN("DataCriacao") as primeira_data,
    MAX("DataCriacao") as ultima_data
FROM "UsuarioFornecedor"
WHERE "DataCriacao" IS NOT NULL;

SELECT 'Teste de DateTimeOffset concluído!' as status;