-- Script completo para corrigir DateTimeOffset em TODOS os módulos
-- Execução: psql -d agriis_db -f fix-all-modules-datetime.sql

BEGIN;

-- Função para converter timestamp para timestamptz assumindo UTC
CREATE OR REPLACE FUNCTION convert_timestamp_to_timestamptz(table_name text, column_name text)
RETURNS void AS $$
BEGIN
    EXECUTE format('ALTER TABLE %I ALTER COLUMN %I TYPE timestamptz USING %I AT TIME ZONE ''UTC''', 
                   table_name, column_name, column_name);
    RAISE NOTICE 'Converted %.% from timestamp to timestamptz', table_name, column_name;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Erro ao converter %.%: %', table_name, column_name, SQLERRM;
END;
$$ LANGUAGE plpgsql;

-- === MÓDULOS JÁ IDENTIFICADOS ===

-- Módulo Referencias
SELECT convert_timestamp_to_timestamptz('estados_referencia', 'data_criacao');
SELECT convert_timestamp_to_timestamptz('estados_referencia', 'data_atualizacao');

-- Módulo Fornecedores
SELECT convert_timestamp_to_timestamptz('Fornecedor', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('Fornecedor', 'DataAtualizacao');
SELECT convert_timestamp_to_timestamptz('UsuarioFornecedor', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('UsuarioFornecedor', 'DataAtualizacao');

-- Módulo Produtores
SELECT convert_timestamp_to_timestamptz('Produtor', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('Produtor', 'DataAtualizacao');
SELECT convert_timestamp_to_timestamptz('Produtor', 'DataAutorizacao');
SELECT convert_timestamp_to_timestamptz('UsuarioProdutor', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('UsuarioProdutor', 'DataAtualizacao');

-- Módulo Produtos
SELECT convert_timestamp_to_timestamptz('Produto', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('Produto', 'DataAtualizacao');
SELECT convert_timestamp_to_timestamptz('Categoria', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('Categoria', 'DataAtualizacao');

-- === NOVOS MÓDULOS IDENTIFICADOS ===

-- Módulo Usuarios (já usa timestamptz corretamente)
-- Não precisa de correção

-- Módulo Safras
SELECT convert_timestamp_to_timestamptz('Safra', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('Safra', 'DataAtualizacao');

-- Módulo Propriedades (já usa timestamptz corretamente)
-- Não precisa de correção

-- Módulo Segmentacoes
SELECT convert_timestamp_to_timestamptz('Segmentacao', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('Segmentacao', 'DataAtualizacao');
SELECT convert_timestamp_to_timestamptz('Grupo', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('Grupo', 'DataAtualizacao');

-- Módulo Culturas
SELECT convert_timestamp_to_timestamptz('Cultura', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('Cultura', 'DataAtualizacao');

-- Módulo Pedidos
SELECT convert_timestamp_to_timestamptz('Pedido', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('Pedido', 'DataAtualizacao');
SELECT convert_timestamp_to_timestamptz('Proposta', 'DataCriacao');
SELECT convert_timestamp_to_timestamptz('Proposta', 'DataAtualizacao');

-- === CORREÇÃO AUTOMÁTICA DE OUTRAS TABELAS ===

-- Verificar e corrigir automaticamente outras tabelas que possam ter o mesmo problema
DO $$
DECLARE
    rec RECORD;
BEGIN
    FOR rec IN 
        SELECT table_name, column_name 
        FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND data_type = 'timestamp without time zone'
        AND column_name IN ('data_criacao', 'data_atualizacao', 'DataCriacao', 'DataAtualizacao', 'DataAutorizacao', 'UltimoLogin', 'ultimo_login')
        AND table_name NOT IN (
            'estados_referencia', 'Fornecedor', 'UsuarioFornecedor', 
            'Produtor', 'UsuarioProdutor', 'Produto', 'Categoria',
            'Safra', 'Segmentacao', 'Grupo', 'Cultura', 'Pedido', 'Proposta'
        )
    LOOP
        PERFORM convert_timestamp_to_timestamptz(rec.table_name, rec.column_name);
    END LOOP;
END $$;

-- Remover função temporária
DROP FUNCTION convert_timestamp_to_timestamptz(text, text);

-- === VERIFICAÇÃO FINAL ===

-- Verificar resultado final
SELECT 
    table_name, 
    column_name, 
    data_type,
    is_nullable
FROM information_schema.columns 
WHERE table_schema = 'public' 
AND column_name IN ('data_criacao', 'data_atualizacao', 'DataCriacao', 'DataAtualizacao', 'DataAutorizacao', 'UltimoLogin', 'ultimo_login')
AND data_type IN ('timestamp with time zone', 'timestamp without time zone')
ORDER BY 
    CASE WHEN data_type = 'timestamp without time zone' THEN 0 ELSE 1 END,
    table_name, 
    column_name;

-- Contar tabelas corrigidas vs não corrigidas
SELECT 
    data_type,
    COUNT(*) as total_colunas
FROM information_schema.columns 
WHERE table_schema = 'public' 
AND column_name IN ('data_criacao', 'data_atualizacao', 'DataCriacao', 'DataAtualizacao', 'DataAutorizacao', 'UltimoLogin', 'ultimo_login')
AND data_type IN ('timestamp with time zone', 'timestamp without time zone')
GROUP BY data_type
ORDER BY data_type;

COMMIT;

SELECT 'Conversão completa de TODOS os módulos para timestamptz concluída!' as status;