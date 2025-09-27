-- Script para verificar a consistência dos índices entre banco e configurações EF
-- Data: 2025-01-27
-- Objetivo: Comparar índices existentes com os esperados pelas configurações

-- =====================================================
-- VERIFICAÇÃO DE ÍNDICES PRINCIPAIS
-- =====================================================

-- Verificar índices da tabela Produto
SELECT 
    'Produto' as tabela,
    'IX_Produtos_Codigo' as indice_esperado,
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Produtos_Codigo' AND tablename = 'Produto') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END as status
UNION ALL
SELECT 
    'Produto',
    'IX_Produtos_Nome',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Produtos_Nome' AND tablename = 'Produto') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END
UNION ALL
SELECT 
    'Produto',
    'IX_Produtos_FornecedorId',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Produtos_FornecedorId' AND tablename = 'Produto') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END
UNION ALL
SELECT 
    'Produto',
    'IX_Produto_CategoriaId',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Produto_CategoriaId' AND tablename = 'Produto') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END
UNION ALL
SELECT 
    'Produto',
    'IX_Produto_UnidadesMedidaId',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Produto_UnidadesMedidaId' AND tablename = 'Produto') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END

-- Verificar índices da tabela Fornecedor
UNION ALL
SELECT 
    'Fornecedor',
    'IX_Fornecedor_Cnpj',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Fornecedor_Cnpj' AND tablename = 'Fornecedor') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END
UNION ALL
SELECT 
    'Fornecedor',
    'IX_Fornecedor_Nome',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Fornecedor_Nome' AND tablename = 'Fornecedor') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END
UNION ALL
SELECT 
    'Fornecedor',
    'IX_Fornecedor_Ativo',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Fornecedor_Ativo' AND tablename = 'Fornecedor') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END

-- Verificar índices da tabela Produtor
UNION ALL
SELECT 
    'Produtor',
    'IX_Produtor_Cpf',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Produtor_Cpf' AND tablename = 'Produtor') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END
UNION ALL
SELECT 
    'Produtor',
    'IX_Produtor_Cnpj',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Produtor_Cnpj' AND tablename = 'Produtor') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END
UNION ALL
SELECT 
    'Produtor',
    'IX_Produtor_Status',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Produtor_Status' AND tablename = 'Produtor') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END

-- Verificar índices da tabela usuarios
UNION ALL
SELECT 
    'usuarios',
    'ix_usuarios_email',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_usuarios_email' AND tablename = 'usuarios') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END
UNION ALL
SELECT 
    'usuarios',
    'ix_usuarios_cpf',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_usuarios_cpf' AND tablename = 'usuarios') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END
UNION ALL
SELECT 
    'usuarios',
    'ix_usuarios_ativo',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_usuarios_ativo' AND tablename = 'usuarios') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END

ORDER BY tabela, indice_esperado;

-- =====================================================
-- VERIFICAÇÃO DE ÍNDICES DE AUDITORIA (DataCriacao)
-- =====================================================

SELECT 
    '=== ÍNDICES DE AUDITORIA (DataCriacao) ===' as secao,
    '' as tabela,
    '' as indice_esperado,
    '' as status

UNION ALL

-- Verificar índices de DataCriacao para tabelas principais
SELECT 
    'AUDITORIA' as secao,
    'usuarios' as tabela,
    'IX_Usuarios_DataCriacao' as indice_esperado,
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Usuarios_DataCriacao' AND tablename = 'usuarios') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END as status
UNION ALL
SELECT 
    'AUDITORIA',
    'Produto',
    'IX_Produto_DataCriacao',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Produto_DataCriacao' AND tablename = 'Produto') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END
UNION ALL
SELECT 
    'AUDITORIA',
    'Fornecedor',
    'IX_Fornecedor_DataCriacao',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Fornecedor_DataCriacao' AND tablename = 'Fornecedor') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END
UNION ALL
SELECT 
    'AUDITORIA',
    'Produtor',
    'IX_Produtor_DataCriacao',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Produtor_DataCriacao' AND tablename = 'Produtor') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END
UNION ALL
SELECT 
    'AUDITORIA',
    'Cultura',
    'IX_Culturas_DataCriacao',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Culturas_DataCriacao' AND tablename = 'Cultura') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END
UNION ALL
SELECT 
    'AUDITORIA',
    'Safra',
    'IX_Safras_DataCriacao',
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Safras_DataCriacao' AND tablename = 'Safra') 
        THEN '✓ EXISTE' 
        ELSE '✗ FALTANDO' 
    END

ORDER BY secao, tabela, indice_esperado;

-- =====================================================
-- RESUMO ESTATÍSTICO
-- =====================================================

SELECT 
    '=== RESUMO ESTATÍSTICO ===' as info,
    '' as valor

UNION ALL

SELECT 
    'Total de índices no schema public:',
    COUNT(*)::text
FROM pg_indexes 
WHERE schemaname = 'public' 
    AND indexname NOT LIKE 'pg_%'

UNION ALL

SELECT 
    'Índices únicos:',
    COUNT(*)::text
FROM pg_indexes 
WHERE schemaname = 'public' 
    AND indexname NOT LIKE 'pg_%'
    AND indexdef LIKE '%UNIQUE%'

UNION ALL

SELECT 
    'Índices de chave primária:',
    COUNT(*)::text
FROM pg_indexes 
WHERE schemaname = 'public' 
    AND indexname LIKE '%_pkey'

UNION ALL

SELECT 
    'Índices de DataCriacao:',
    COUNT(*)::text
FROM pg_indexes 
WHERE schemaname = 'public' 
    AND indexname LIKE '%DataCriacao%';

-- =====================================================
-- ÍNDICES ÓRFÃOS (existem no banco mas não no EF)
-- =====================================================

SELECT 
    '=== POSSÍVEIS ÍNDICES ÓRFÃOS ===' as info,
    '' as detalhes

UNION ALL

SELECT 
    'Tabela: ' || tablename as info,
    'Índice: ' || indexname as detalhes
FROM pg_indexes 
WHERE schemaname = 'public' 
    AND indexname NOT LIKE 'pg_%'
    AND indexname NOT LIKE '%_pkey'
    AND indexname NOT LIKE 'IX_%'
    AND indexname NOT LIKE 'ix_%'
ORDER BY tablename, indexname;

-- =====================================================
-- ÍNDICES COM PROBLEMAS DE PERFORMANCE
-- =====================================================

SELECT 
    '=== ANÁLISE DE PERFORMANCE DOS ÍNDICES ===' as info,
    '' as detalhes

UNION ALL

SELECT 
    'Índices não utilizados (podem ser removidos):' as info,
    '' as detalhes

UNION ALL

SELECT 
    'Tabela: ' || schemaname || '.' || tablename as info,
    'Índice: ' || indexname || ' (Tamanho: ' || pg_size_pretty(pg_relation_size(indexrelid)) || ')' as detalhes
FROM pg_stat_user_indexes 
WHERE schemaname = 'public'
    AND idx_scan = 0
    AND idx_tup_read = 0
ORDER BY pg_relation_size(indexrelid) DESC
LIMIT 10;