-- =====================================================
-- VALIDAÇÃO DA CORREÇÃO FK FORNECEDOR-MUNICIPIO
-- =====================================================

-- Verificar se a tabela Fornecedor existe
SELECT 'FORNECEDOR - Verificação da Tabela' as validacao;
SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Fornecedor' AND table_schema = 'public')
        THEN '✅ OK: Tabela Fornecedor existe'
        ELSE '❌ ERRO: Tabela Fornecedor não existe'
    END as status_tabela;

-- Verificar se as colunas FK existem
SELECT 'FORNECEDOR - Verificação Colunas FK' as validacao;
SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.columns 
                    WHERE table_name = 'Fornecedor' AND column_name = 'UfId' AND table_schema = 'public')
        THEN '✅ OK: Campo UfId existe'
        ELSE '❌ ERRO: Campo UfId não existe'
    END as status_uf_id,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.columns 
                    WHERE table_name = 'Fornecedor' AND column_name = 'MunicipioId' AND table_schema = 'public')
        THEN '✅ OK: Campo MunicipioId existe'
        ELSE '❌ ERRO: Campo MunicipioId não existe'
    END as status_municipio_id;

-- Verificar se as tabelas de referência existem
SELECT 'TABELAS DE REFERÊNCIA - Verificação' as validacao;
SELECT 
    'estados' as tabela,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'estados' AND table_schema = 'public')
        THEN '✅ Existe'
        ELSE '❌ Não existe'
    END as status
UNION ALL
SELECT 
    'municipios' as tabela,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'municipios' AND table_schema = 'public')
        THEN '✅ Existe'
        ELSE '❌ Não existe'
    END as status
UNION ALL
SELECT 
    'municipios_referencia' as tabela,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'municipios_referencia' AND table_schema = 'public')
        THEN '✅ Existe'
        ELSE '❌ Não existe'
    END as status;

-- Verificar constraints FK do Fornecedor
SELECT 'FORNECEDOR - Constraints FK' as validacao;
SELECT 
    tc.constraint_name,
    tc.table_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name,
    CASE 
        WHEN tc.constraint_name = 'FK_Fornecedor_Estados_UfId' AND ccu.table_name = 'estados'
        THEN '✅ OK: FK UfId aponta para estados'
        WHEN tc.constraint_name = 'FK_Fornecedor_Municipios_MunicipioId' AND ccu.table_name = 'municipios'
        THEN '✅ OK: FK MunicipioId aponta para municipios'
        WHEN tc.constraint_name LIKE 'FK_Fornecedor_%' 
        THEN '⚠️  ATENÇÃO: FK pode estar incorreta'
        ELSE '❓ INFO: Outra constraint'
    END as status
FROM information_schema.table_constraints AS tc 
    JOIN information_schema.key_column_usage AS kcu
        ON tc.constraint_name = kcu.constraint_name
        AND tc.table_schema = kcu.table_schema
    JOIN information_schema.constraint_column_usage AS ccu
        ON ccu.constraint_name = tc.constraint_name
        AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY' 
    AND tc.table_name = 'Fornecedor'
    AND tc.table_schema = 'public'
    AND kcu.column_name IN ('UfId', 'MunicipioId')
ORDER BY tc.constraint_name;

-- Verificar se há dados de teste para validar o relacionamento
SELECT 'DADOS DE TESTE - Verificação' as validacao;
SELECT 
    'Fornecedores' as tabela,
    COUNT(*) as total_registros
FROM public."Fornecedor"
WHERE "UfId" IS NOT NULL OR "MunicipioId" IS NOT NULL
UNION ALL
SELECT 
    'Estados' as tabela,
    COUNT(*) as total_registros
FROM public.estados
UNION ALL
SELECT 
    'Municipios' as tabela,
    COALESCE((SELECT COUNT(*) FROM public.municipios), 0) as total_registros;

-- RESUMO FINAL
SELECT 'RESUMO DA VALIDAÇÃO' as validacao;

WITH validacao_constraints AS (
    SELECT 
        COUNT(CASE WHEN tc.constraint_name = 'FK_Fornecedor_Estados_UfId' AND ccu.table_name = 'estados' THEN 1 END) as fk_uf_ok,
        COUNT(CASE WHEN tc.constraint_name = 'FK_Fornecedor_Municipios_MunicipioId' AND ccu.table_name = 'municipios' THEN 1 END) as fk_municipio_ok,
        COUNT(CASE WHEN tc.constraint_name LIKE 'FK_Fornecedor_MunicipiosReferencia%' THEN 1 END) as fk_incorreta
    FROM information_schema.table_constraints AS tc 
        JOIN information_schema.key_column_usage AS kcu
            ON tc.constraint_name = kcu.constraint_name
        JOIN information_schema.constraint_column_usage AS ccu
            ON ccu.constraint_name = tc.constraint_name
    WHERE tc.constraint_type = 'FOREIGN KEY' 
        AND tc.table_name = 'Fornecedor'
        AND tc.table_schema = 'public'
        AND kcu.column_name IN ('UfId', 'MunicipioId')
)
SELECT 
    CASE 
        WHEN vc.fk_uf_ok = 1 AND vc.fk_municipio_ok = 1 AND vc.fk_incorreta = 0
        THEN '🎉 SUCESSO: Todas as FKs estão corretas!'
        WHEN vc.fk_uf_ok = 1 AND vc.fk_municipio_ok = 0
        THEN '⚠️  PARCIAL: FK UF OK, mas FK Município precisa ser corrigida'
        WHEN vc.fk_uf_ok = 0 AND vc.fk_municipio_ok = 1
        THEN '⚠️  PARCIAL: FK Município OK, mas FK UF precisa ser corrigida'
        WHEN vc.fk_incorreta > 0
        THEN '❌ ERRO: Ainda existem FKs incorretas (MunicipiosReferencia)'
        ELSE '❌ ERRO: Nenhuma FK foi encontrada ou está correta'
    END as resultado_final
FROM validacao_constraints vc;