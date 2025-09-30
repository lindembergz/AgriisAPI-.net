-- =====================================================
-- POST-MIGRATION VALIDATION SCRIPT
-- Valida se as correções foram aplicadas corretamente
-- =====================================================

-- Cabeçalho do relatório
SELECT 'POST-MIGRATION VALIDATION REPORT' as titulo, NOW() as data_execucao;

-- =====================================================
-- 1. VALIDAÇÃO FK FORNECEDOR CORRIGIDA
-- =====================================================
SELECT '1. VALIDAÇÃO FK FORNECEDOR CORRIGIDA' as secao;

SELECT 
    tc.constraint_name as nome_constraint,
    tc.table_name as tabela_origem,
    kcu.column_name as coluna_origem,
    ccu.table_name as tabela_destino,
    ccu.column_name as coluna_destino,
    CASE 
        WHEN tc.constraint_name = 'FK_Fornecedor_Municipios_MunicipioId' AND ccu.table_name = 'municipios'
        THEN '✅ CORRETO - FK Municipio aponta para municipios'
        WHEN tc.constraint_name = 'FK_Fornecedor_Estados_UfId' AND ccu.table_name = 'estados'
        THEN '✅ CORRETO - FK Estado aponta para estados'
        WHEN tc.constraint_name LIKE '%Fornecedor%' AND tc.constraint_name LIKE '%MunicipiosReferencia%'
        THEN '❌ ERRO - FK ainda aponta para municipios_referencia'
        ELSE '📋 INFO - Outra constraint'
    END as status_validacao
FROM information_schema.table_constraints AS tc 
    JOIN information_schema.key_column_usage AS kcu
        ON tc.constraint_name = kcu.constraint_name
        AND tc.table_schema = kcu.table_schema
    JOIN information_schema.constraint_column_usage AS ccu
        ON ccu.constraint_name = tc.constraint_name
        AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY' 
    AND tc.table_schema = 'public'
    AND tc.table_name = 'Fornecedor'
    AND kcu.column_name IN ('UfId', 'MunicipioId')
ORDER BY tc.constraint_name;

-- =====================================================
-- 2. TESTE DE INTEGRIDADE REFERENCIAL
-- =====================================================
SELECT '2. TESTE DE INTEGRIDADE REFERENCIAL' as secao;

-- Verificar se há fornecedores com UfId que não existem em estados
SELECT 
    'Fornecedores com UfId inválido' as teste,
    COUNT(*) as quantidade_problemas,
    CASE 
        WHEN COUNT(*) = 0 THEN '✅ OK - Todos os UfId são válidos'
        ELSE '❌ ERRO - Há UfId inválidos'
    END as status
FROM public."Fornecedor" f
LEFT JOIN public.estados e ON f."UfId" = e.id
WHERE f."UfId" IS NOT NULL AND e.id IS NULL

UNION ALL

-- Verificar se há fornecedores com MunicipioId que não existem em municipios
SELECT 
    'Fornecedores com MunicipioId inválido' as teste,
    COUNT(*) as quantidade_problemas,
    CASE 
        WHEN COUNT(*) = 0 THEN '✅ OK - Todos os MunicipioId são válidos'
        ELSE '❌ ERRO - Há MunicipioId inválidos'
    END as status
FROM public."Fornecedor" f
LEFT JOIN public.municipios m ON f."MunicipioId" = m.id
WHERE f."MunicipioId" IS NOT NULL AND m.id IS NULL;

-- =====================================================
-- 3. VALIDAÇÃO DE COLUNAS DUPLICADAS REMOVIDAS
-- =====================================================
SELECT '3. VALIDAÇÃO DE COLUNAS DUPLICADAS REMOVIDAS' as secao;

WITH tabelas_verificar AS (
    SELECT unnest(ARRAY['AtividadesAgropecuarias', 'Catalogo', 'Combo', 'Cultura', 'Moedas', 'UnidadesMedida', 'Embalagens']) as table_name
),
colunas_ativo AS (
    SELECT 
        tv.table_name,
        COUNT(CASE WHEN c.column_name = 'Ativo' THEN 1 END) as ativo_pascal,
        COUNT(CASE WHEN c.column_name = 'ativo' THEN 1 END) as ativo_lower
    FROM tabelas_verificar tv
    LEFT JOIN information_schema.columns c 
        ON c.table_name = tv.table_name 
        AND c.table_schema = 'public' 
        AND c.column_name IN ('Ativo', 'ativo')
    GROUP BY tv.table_name
)
SELECT 
    table_name as tabela,
    ativo_pascal as "Ativo (PascalCase)",
    ativo_lower as "ativo (lowercase)",
    CASE 
        WHEN ativo_pascal = 1 AND ativo_lower = 0 THEN '✅ OK - Apenas PascalCase'
        WHEN ativo_pascal = 1 AND ativo_lower = 1 THEN '❌ ERRO - Ainda duplicada'
        WHEN ativo_pascal = 0 AND ativo_lower = 1 THEN '⚠️  ATENÇÃO - Apenas lowercase'
        WHEN ativo_pascal = 0 AND ativo_lower = 0 THEN '📋 INFO - Sem coluna ativo'
        ELSE '❓ ESTADO INESPERADO'
    END as status
FROM colunas_ativo
ORDER BY 
    CASE 
        WHEN ativo_pascal = 1 AND ativo_lower = 1 THEN 1  -- Erros primeiro
        WHEN ativo_pascal = 0 AND ativo_lower = 1 THEN 2  -- Atenção depois
        ELSE 3  -- OK por último
    END,
    table_name;

-- =====================================================
-- 4. TESTE FUNCIONAL - SIMULAÇÃO DE QUERY COM INCLUDE
-- =====================================================
SELECT '4. TESTE FUNCIONAL - SIMULAÇÃO DE QUERY COM INCLUDE' as secao;

-- Simular uma query que faria Include(f => f.Municipio) e Include(f => f.Estado)
SELECT 
    'Teste de JOIN Fornecedor-Municipio-Estado' as teste,
    COUNT(*) as fornecedores_com_dados_completos,
    CASE 
        WHEN COUNT(*) > 0 THEN '✅ OK - JOIN funciona corretamente'
        ELSE '📋 INFO - Sem dados para testar'
    END as status
FROM public."Fornecedor" f
LEFT JOIN public.municipios m ON f."MunicipioId" = m.id
LEFT JOIN public.estados e ON f."UfId" = e.id
WHERE f."UfId" IS NOT NULL AND f."MunicipioId" IS NOT NULL;

-- =====================================================
-- 5. RESUMO EXECUTIVO DA VALIDAÇÃO
-- =====================================================
SELECT '5. RESUMO EXECUTIVO DA VALIDAÇÃO' as secao;

WITH 
-- Verificar FKs corretas
fks_corretas AS (
    SELECT 
        COUNT(CASE WHEN tc.constraint_name = 'FK_Fornecedor_Municipios_MunicipioId' AND ccu.table_name = 'municipios' THEN 1 END) as fk_municipio_ok,
        COUNT(CASE WHEN tc.constraint_name = 'FK_Fornecedor_Estados_UfId' AND ccu.table_name = 'estados' THEN 1 END) as fk_estado_ok,
        COUNT(CASE WHEN tc.constraint_name LIKE '%MunicipiosReferencia%' THEN 1 END) as fk_incorretas
    FROM information_schema.table_constraints AS tc 
        JOIN information_schema.constraint_column_usage AS ccu
            ON ccu.constraint_name = tc.constraint_name
    WHERE tc.constraint_type = 'FOREIGN KEY' 
        AND tc.table_name = 'Fornecedor'
        AND tc.table_schema = 'public'
),
-- Verificar colunas duplicadas
colunas_duplicadas AS (
    SELECT COUNT(*) as total
    FROM (
        SELECT table_name
        FROM information_schema.columns 
        WHERE table_schema = 'public' 
            AND column_name IN ('Ativo', 'ativo')
            AND table_name IN ('AtividadesAgropecuarias', 'Catalogo', 'Combo', 'Cultura', 'Moedas', 'UnidadesMedida', 'Embalagens')
        GROUP BY table_name
        HAVING COUNT(DISTINCT column_name) > 1
    ) t
),
-- Verificar integridade referencial
integridade AS (
    SELECT 
        (SELECT COUNT(*) FROM public."Fornecedor" f LEFT JOIN public.estados e ON f."UfId" = e.id WHERE f."UfId" IS NOT NULL AND e.id IS NULL) as uf_invalidos,
        (SELECT COUNT(*) FROM public."Fornecedor" f LEFT JOIN public.municipios m ON f."MunicipioId" = m.id WHERE f."MunicipioId" IS NOT NULL AND m.id IS NULL) as municipio_invalidos
)
SELECT 
    'FK Fornecedor-Municipio' as aspecto,
    CASE WHEN fc.fk_municipio_ok = 1 THEN '✅ CORRETO' ELSE '❌ INCORRETO' END as status
FROM fks_corretas fc
UNION ALL
SELECT 
    'FK Fornecedor-Estado' as aspecto,
    CASE WHEN fc.fk_estado_ok = 1 THEN '✅ CORRETO' ELSE '❌ INCORRETO' END as status
FROM fks_corretas fc
UNION ALL
SELECT 
    'FKs Incorretas Removidas' as aspecto,
    CASE WHEN fc.fk_incorretas = 0 THEN '✅ CORRETO' ELSE '❌ AINDA EXISTEM' END as status
FROM fks_corretas fc
UNION ALL
SELECT 
    'Colunas Duplicadas Removidas' as aspecto,
    CASE WHEN cd.total = 0 THEN '✅ CORRETO' ELSE '❌ AINDA EXISTEM' END as status
FROM colunas_duplicadas cd
UNION ALL
SELECT 
    'Integridade Referencial UF' as aspecto,
    CASE WHEN i.uf_invalidos = 0 THEN '✅ CORRETO' ELSE '❌ DADOS INVÁLIDOS' END as status
FROM integridade i
UNION ALL
SELECT 
    'Integridade Referencial Municipio' as aspecto,
    CASE WHEN i.municipio_invalidos = 0 THEN '✅ CORRETO' ELSE '❌ DADOS INVÁLIDOS' END as status
FROM integridade i;

-- =====================================================
-- 6. PRÓXIMOS PASSOS RECOMENDADOS
-- =====================================================
SELECT '6. PRÓXIMOS PASSOS RECOMENDADOS' as secao;

WITH status_geral AS (
    SELECT 
        (SELECT COUNT(*) FROM information_schema.table_constraints WHERE constraint_name = 'FK_Fornecedor_Municipios_MunicipioId') as fk_municipio_existe,
        (SELECT COUNT(*) FROM information_schema.table_constraints WHERE constraint_name LIKE '%MunicipiosReferencia%') as fk_incorretas_existem,
        (SELECT COUNT(*) FROM (
            SELECT table_name FROM information_schema.columns 
            WHERE table_schema = 'public' AND column_name IN ('Ativo', 'ativo')
            GROUP BY table_name HAVING COUNT(DISTINCT column_name) > 1
        ) t) as colunas_duplicadas_existem
)
SELECT 
    CASE 
        WHEN sg.fk_municipio_existe = 1 AND sg.fk_incorretas_existem = 0 AND sg.colunas_duplicadas_existem = 0
        THEN '🎉 SUCESSO TOTAL: Todas as correções foram aplicadas com sucesso!'
        WHEN sg.fk_municipio_existe = 1 AND sg.fk_incorretas_existem = 0
        THEN '✅ FK CORRIGIDA: Próximo passo - remover colunas duplicadas'
        WHEN sg.fk_municipio_existe = 0 OR sg.fk_incorretas_existem > 0
        THEN '❌ FK PENDENTE: Aplicar correção de FK Fornecedor-Municipio'
        ELSE '⚠️  ESTADO PARCIAL: Revisar correções aplicadas'
    END as status_geral,
    CASE 
        WHEN sg.colunas_duplicadas_existem > 0
        THEN 'Executar: ALTER TABLE ... DROP COLUMN ativo; para remover colunas duplicadas'
        WHEN sg.fk_incorretas_existem > 0
        THEN 'Gerar migração EF Core para corrigir FKs'
        ELSE 'Prosseguir para próximas inconsistências (DateTime, naming)'
    END as proxima_acao
FROM status_geral sg;