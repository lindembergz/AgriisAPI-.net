-- Script para adicionar a coluna 'ativo' em todas as tabelas cujas entidades herdam de EntidadeBase
-- Este script verifica se a coluna existe antes de adicioná-la

-- Função auxiliar para adicionar coluna ativo se não existir
CREATE OR REPLACE FUNCTION add_ativo_column_if_not_exists(table_name_param TEXT, schema_name_param TEXT DEFAULT 'public')
RETURNS TEXT AS $$
DECLARE
    result_message TEXT;
BEGIN
    -- Verificar se a coluna já existe
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = schema_name_param 
        AND table_name = table_name_param 
        AND column_name = 'ativo'
    ) THEN
        -- Adicionar a coluna
        EXECUTE format('ALTER TABLE %I.%I ADD COLUMN ativo BOOLEAN NOT NULL DEFAULT true', schema_name_param, table_name_param);
        result_message := format('✓ Coluna ativo adicionada à tabela %s.%s', schema_name_param, table_name_param);
    ELSE
        result_message := format('- Coluna ativo já existe na tabela %s.%s', schema_name_param, table_name_param);
    END IF;
    
    RAISE NOTICE '%', result_message;
    RETURN result_message;
END;
$$ LANGUAGE plpgsql;

-- Adicionar coluna ativo nas tabelas do módulo Referencias
SELECT add_ativo_column_if_not_exists('paises');
SELECT add_ativo_column_if_not_exists('estados_referencia');
SELECT add_ativo_column_if_not_exists('municipios_referencia');
SELECT add_ativo_column_if_not_exists('Moedas');
SELECT add_ativo_column_if_not_exists('AtividadesAgropecuarias');
SELECT add_ativo_column_if_not_exists('UnidadesMedida');
SELECT add_ativo_column_if_not_exists('Embalagens');

-- Adicionar coluna ativo nas tabelas do módulo Usuarios
SELECT add_ativo_column_if_not_exists('usuarios');
SELECT add_ativo_column_if_not_exists('usuario_roles');

-- Adicionar coluna ativo nas tabelas do módulo Autenticacao
SELECT add_ativo_column_if_not_exists('refresh_tokens');

-- Adicionar coluna ativo nas tabelas do módulo Culturas
SELECT add_ativo_column_if_not_exists('Cultura');

-- Adicionar coluna ativo nas tabelas do módulo Propriedades
SELECT add_ativo_column_if_not_exists('Propriedade');
SELECT add_ativo_column_if_not_exists('PropriedadeCultura');
SELECT add_ativo_column_if_not_exists('Talhao');

-- Adicionar coluna ativo nas tabelas do módulo Produtores
SELECT add_ativo_column_if_not_exists('Produtor');
SELECT add_ativo_column_if_not_exists('UsuarioProdutor');

-- Adicionar coluna ativo nas tabelas do módulo Fornecedores
SELECT add_ativo_column_if_not_exists('Fornecedor');
SELECT add_ativo_column_if_not_exists('UsuarioFornecedor');
SELECT add_ativo_column_if_not_exists('UsuarioFornecedorTerritorio');

-- Adicionar coluna ativo nas tabelas do módulo Enderecos
SELECT add_ativo_column_if_not_exists('estados');
SELECT add_ativo_column_if_not_exists('municipios');
SELECT add_ativo_column_if_not_exists('enderecos');

-- Adicionar coluna ativo nas tabelas do módulo Produtos
SELECT add_ativo_column_if_not_exists('Produto');
SELECT add_ativo_column_if_not_exists('Categorias');
SELECT add_ativo_column_if_not_exists('ProdutosCulturas');

-- Adicionar coluna ativo nas tabelas do módulo Catalogos
SELECT add_ativo_column_if_not_exists('Catalogo');
SELECT add_ativo_column_if_not_exists('CatalogoItem');

-- Adicionar coluna ativo nas tabelas do módulo Pagamentos
SELECT add_ativo_column_if_not_exists('forma_pagamento');
SELECT add_ativo_column_if_not_exists('cultura_forma_pagamento');

-- Adicionar coluna ativo nas tabelas do módulo Combos
SELECT add_ativo_column_if_not_exists('Combo');
SELECT add_ativo_column_if_not_exists('ComboItem');
SELECT add_ativo_column_if_not_exists('ComboLocalRecebimento');
SELECT add_ativo_column_if_not_exists('ComboCategoriaDesconto');

-- Adicionar coluna ativo nas tabelas do módulo Safras
SELECT add_ativo_column_if_not_exists('Safra');

-- Adicionar coluna ativo nas tabelas do módulo Segmentacoes
SELECT add_ativo_column_if_not_exists('Segmentacao');
SELECT add_ativo_column_if_not_exists('Grupo');
SELECT add_ativo_column_if_not_exists('GrupoSegmentacao');

-- Adicionar coluna ativo nas tabelas do módulo PontosDistribuicao
SELECT add_ativo_column_if_not_exists('PontoDistribuicao');

-- Adicionar coluna ativo nas tabelas do módulo Pedidos
SELECT add_ativo_column_if_not_exists('Pedido');
SELECT add_ativo_column_if_not_exists('PedidoItem');
SELECT add_ativo_column_if_not_exists('PedidoItemTransporte');
SELECT add_ativo_column_if_not_exists('Proposta');

-- Remover a função auxiliar
DROP FUNCTION add_ativo_column_if_not_exists(TEXT, TEXT);

-- Verificar quais tabelas agora têm a coluna ativo
SELECT 
    table_schema,
    table_name,
    'ativo' as column_name,
    data_type,
    column_default,
    is_nullable
FROM information_schema.columns 
WHERE column_name = 'ativo'
AND table_schema = 'public'
ORDER BY table_name;

-- Contar quantas tabelas têm a coluna ativo
SELECT 
    COUNT(*) as total_tables_with_ativo,
    'Tabelas com coluna ativo adicionada' as description
FROM information_schema.columns 
WHERE column_name = 'ativo'
AND table_schema = 'public';