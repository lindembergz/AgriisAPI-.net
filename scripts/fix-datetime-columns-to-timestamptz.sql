-- Script para corrigir colunas de timestamp para timestamptz
-- Execução: psql -d agriis_db -f fix-datetime-columns-to-timestamptz.sql

BEGIN;

-- Função para converter timestamp para timestamptz assumindo UTC
CREATE OR REPLACE FUNCTION convert_timestamp_to_timestamptz(table_name text, column_name text)
RETURNS void AS $$
BEGIN
    EXECUTE format('ALTER TABLE %I ALTER COLUMN %I TYPE timestamptz USING %I AT TIME ZONE ''UTC''', 
                   table_name, column_name, column_name);
    RAISE NOTICE 'Converted %.% from timestamp to timestamptz', table_name, column_name;
END;
$$ LANGUAGE plpgsql;

-- Corrigir tabela estados_referencia (módulo Referencias)
SELECT convert_timestamp_to_timestamptz('estados_referencia', 'data_criacao');
SELECT convert_timestamp_to_timestamptz('estados_referencia', 'data_atualizacao');

-- Corrigir tabela Fornecedor (módulo Fornecedores)
SELECT convert_timestamp_to_timestamptz('Fornecedor', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('Fornecedor', 'DataAtualizacao');

-- Corrigir tabela UsuarioFornecedor (módulo Fornecedores)
SELECT convert_timestamp_to_timestamptz('UsuarioFornecedor', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('UsuarioFornecedor', 'DataAtualizacao');

-- Corrigir tabela Produtor (módulo Produtores)
SELECT convert_timestamp_to_timestamptz('Produtor', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('Produtor', 'DataAtualizacao');
SELECT convert_timestamp_to_timestamptz('Produtor', 'DataAutorizacao');

-- Corrigir tabela UsuarioProdutor (módulo Produtores)
SELECT convert_timestamp_to_timestamptz('UsuarioProdutor', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('UsuarioProdutor', 'DataAtualizacao');

-- Corrigir tabela Produto (módulo Produtos)
SELECT convert_timestamp_to_timestamptz('Produto', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('Produto', 'DataAtualizacao');

-- Corrigir tabela Categoria (módulo Produtos)
SELECT convert_timestamp_to_timestamptz('Categoria', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('Categoria', 'DataAtualizacao');

-- Corrigir outras tabelas que possam ter o mesmo problema
-- Verificar se existem outras tabelas com colunas timestamp

DO $$
DECLARE
    rec RECORD;
BEGIN
    FOR rec IN 
        SELECT table_name, column_name 
        FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND data_type = 'timestamp without time zone'
        AND column_name IN ('data_criacao', 'data_atualizacao', 'DataCriacao', 'DataAtualizacao')
        AND table_name NOT IN ('estados_referencia', 'Fornecedor', 'UsuarioFornecedor', 'Produtor', 'UsuarioProdutor', 'Produto', 'Categoria')
    LOOP
        PERFORM convert_timestamp_to_timestamptz(rec.table_name, rec.column_name);
    END LOOP;
END $$;

-- Remover função temporária
DROP FUNCTION convert_timestamp_to_timestamptz(text, text);

-- Verificar resultado
SELECT 
    table_name, 
    column_name, 
    data_type,
    is_nullable
FROM information_schema.columns 
WHERE table_schema = 'public' 
AND column_name IN ('data_criacao', 'data_atualizacao', 'DataCriacao', 'DataAtualizacao')
ORDER BY table_name, column_name;

COMMIT;

-- Mensagem final
SELECT 'Conversão de colunas timestamp para timestamptz concluída!' as status;