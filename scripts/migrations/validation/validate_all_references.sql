-- =====================================================
-- VALIDATION SCRIPT: All Reference Entities
-- Description: Comprehensive validation of all reference entity migrations
-- Author: System Migration
-- Date: 2025-01-27
-- Requirements: 12.5, 12.6
-- =====================================================

-- Enable detailed logging
\set VERBOSITY verbose
\timing on

-- Create validation results table if not exists
CREATE TABLE IF NOT EXISTS public."ValidationResults" (
    "Id" SERIAL PRIMARY KEY,
    "ValidationName" VARCHAR(200) NOT NULL,
    "Category" VARCHAR(100) NOT NULL,
    "TestName" VARCHAR(200) NOT NULL,
    "Status" VARCHAR(20) NOT NULL, -- PASS, FAIL, WARNING
    "Message" TEXT NOT NULL,
    "RecordsAffected" INTEGER DEFAULT 0,
    "ExpectedCount" INTEGER DEFAULT NULL,
    "ActualCount" INTEGER DEFAULT NULL,
    "ExecutionTime" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Start validation
BEGIN;

-- Log validation start
INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message")
VALUES ('validate_all_references', 'SYSTEM', 'START', 'INFO', 'Starting comprehensive reference validation');

-- =====================================================
-- CATEGORY 1: REFERENCE TABLES EXISTENCE
-- =====================================================

-- Test 1.1: Check if all reference tables exist
DO $$
DECLARE
    expected_tables TEXT[] := ARRAY['Paises', 'Ufs', 'Municipios', 'Moedas', 'UnidadesMedida', 'AtividadesAgropecuarias', 'Embalagens'];
    table_name TEXT;
    missing_tables TEXT[] := ARRAY[]::TEXT[];
    existing_count INTEGER := 0;
BEGIN
    FOREACH table_name IN ARRAY expected_tables
    LOOP
        IF EXISTS (
            SELECT 1 FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name = table_name
        ) THEN
            existing_count := existing_count + 1;
        ELSE
            missing_tables := array_append(missing_tables, table_name);
        END IF;
    END LOOP;
    
    IF array_length(missing_tables, 1) IS NULL THEN
        INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ExpectedCount", "ActualCount")
        VALUES ('validate_all_references', 'TABLE_EXISTENCE', 'All reference tables exist', 'PASS', 'All expected reference tables are present', array_length(expected_tables, 1), existing_count);
    ELSE
        INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ExpectedCount", "ActualCount")
        VALUES ('validate_all_references', 'TABLE_EXISTENCE', 'Missing reference tables', 'FAIL', 'Missing tables: ' || array_to_string(missing_tables, ', '), array_length(expected_tables, 1), existing_count);
    END IF;
END $$;

-- Test 1.2: Check if tables have data
DO $$
DECLARE
    table_counts RECORD;
    empty_tables TEXT[] := ARRAY[]::TEXT[];
    total_records INTEGER := 0;
BEGIN
    FOR table_counts IN
        SELECT 
            table_name,
            CASE 
                WHEN table_name = 'Paises' THEN (SELECT COUNT(*) FROM public."Paises")
                WHEN table_name = 'Ufs' THEN (SELECT COUNT(*) FROM public."Ufs")
                WHEN table_name = 'Municipios' THEN (SELECT COUNT(*) FROM public."Municipios")
                WHEN table_name = 'Moedas' THEN (SELECT COUNT(*) FROM public."Moedas")
                WHEN table_name = 'UnidadesMedida' THEN (SELECT COUNT(*) FROM public."UnidadesMedida")
                WHEN table_name = 'AtividadesAgropecuarias' THEN (SELECT COUNT(*) FROM public."AtividadesAgropecuarias")
                WHEN table_name = 'Embalagens' THEN (SELECT COUNT(*) FROM public."Embalagens")
                ELSE 0
            END as record_count
        FROM (VALUES ('Paises'), ('Ufs'), ('Municipios'), ('Moedas'), ('UnidadesMedida'), ('AtividadesAgropecuarias'), ('Embalagens')) AS t(table_name)
        WHERE EXISTS (
            SELECT 1 FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND information_schema.tables.table_name = t.table_name
        )
    LOOP
        total_records := total_records + table_counts.record_count;
        
        IF table_counts.record_count = 0 THEN
            empty_tables := array_append(empty_tables, table_counts.table_name);
        END IF;
        
        INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
        VALUES ('validate_all_references', 'TABLE_DATA', table_counts.table_name || ' record count', 
                CASE WHEN table_counts.record_count > 0 THEN 'PASS' ELSE 'WARNING' END,
                table_counts.table_name || ' has ' || table_counts.record_count || ' records',
                table_counts.record_count);
    END LOOP;
    
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'TABLE_DATA', 'Total reference records', 'INFO', 'Total records across all reference tables: ' || total_records, total_records);
END $$;

-- =====================================================
-- CATEGORY 2: FOREIGN KEY CONSTRAINTS
-- =====================================================

-- Test 2.1: Check foreign key constraints exist
DO $$
DECLARE
    expected_fks RECORD;
    missing_fks TEXT[] := ARRAY[]::TEXT[];
    existing_fks INTEGER := 0;
BEGIN
    FOR expected_fks IN
        SELECT 
            constraint_name,
            table_name,
            EXISTS (
                SELECT 1 FROM information_schema.table_constraints 
                WHERE constraint_type = 'FOREIGN KEY'
                AND constraint_schema = 'public'
                AND information_schema.table_constraints.constraint_name = expected_fks.constraint_name
                AND information_schema.table_constraints.table_name = expected_fks.table_name
            ) as fk_exists
        FROM (VALUES 
            ('FK_Ufs_Paises_PaisId', 'Ufs'),
            ('FK_Municipios_Ufs_UfId', 'Municipios'),
            ('FK_Embalagens_UnidadesMedida_UnidadeMedidaId', 'Embalagens'),
            ('FK_Produto_UnidadeMedida_UnidadeMedidaId', 'Produto'),
            ('FK_Produto_Embalagem_EmbalagemId', 'Produto'),
            ('FK_Produto_AtividadeAgropecuaria_AtividadeAgropecuariaId', 'Produto'),
            ('FK_Fornecedor_Uf_UfId', 'Fornecedor'),
            ('FK_Fornecedor_Municipio_MunicipioId', 'Fornecedor')
        ) AS fk_list(constraint_name, table_name)
    LOOP
        IF expected_fks.fk_exists THEN
            existing_fks := existing_fks + 1;
        ELSE
            missing_fks := array_append(missing_fks, expected_fks.constraint_name);
        END IF;
        
        INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message")
        VALUES ('validate_all_references', 'FOREIGN_KEYS', expected_fks.constraint_name, 
                CASE WHEN expected_fks.fk_exists THEN 'PASS' ELSE 'FAIL' END,
                'FK constraint ' || expected_fks.constraint_name || ' on ' || expected_fks.table_name || 
                CASE WHEN expected_fks.fk_exists THEN ' exists' ELSE ' is missing' END);
    END LOOP;
    
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'FOREIGN_KEYS', 'FK constraints summary', 
            CASE WHEN array_length(missing_fks, 1) IS NULL THEN 'PASS' ELSE 'FAIL' END,
            'Foreign key constraints - Existing: ' || existing_fks || 
            CASE WHEN array_length(missing_fks, 1) IS NOT NULL THEN ', Missing: ' || array_to_string(missing_fks, ', ') ELSE '' END,
            existing_fks);
END $$;

-- =====================================================
-- CATEGORY 3: REFERENTIAL INTEGRITY
-- =====================================================

-- Test 3.1: Check for orphaned records in Ufs
DO $$
DECLARE
    orphaned_ufs INTEGER;
BEGIN
    SELECT COUNT(*) INTO orphaned_ufs
    FROM public."Ufs" u
    LEFT JOIN public."Paises" p ON u."PaisId" = p."Id"
    WHERE p."Id" IS NULL;
    
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'REFERENTIAL_INTEGRITY', 'Orphaned UFs', 
            CASE WHEN orphaned_ufs = 0 THEN 'PASS' ELSE 'FAIL' END,
            'UFs without valid País reference: ' || orphaned_ufs,
            orphaned_ufs);
END $$;

-- Test 3.2: Check for orphaned records in Municipios
DO $$
DECLARE
    orphaned_municipios INTEGER;
BEGIN
    SELECT COUNT(*) INTO orphaned_municipios
    FROM public."Municipios" m
    LEFT JOIN public."Ufs" u ON m."UfId" = u."Id"
    WHERE u."Id" IS NULL;
    
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'REFERENTIAL_INTEGRITY', 'Orphaned Municípios', 
            CASE WHEN orphaned_municipios = 0 THEN 'PASS' ELSE 'FAIL' END,
            'Municípios without valid UF reference: ' || orphaned_municipios,
            orphaned_municipios);
END $$;

-- Test 3.3: Check for orphaned records in Embalagens
DO $$
DECLARE
    orphaned_embalagens INTEGER;
BEGIN
    SELECT COUNT(*) INTO orphaned_embalagens
    FROM public."Embalagens" e
    LEFT JOIN public."UnidadesMedida" um ON e."UnidadeMedidaId" = um."Id"
    WHERE um."Id" IS NULL;
    
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'REFERENTIAL_INTEGRITY', 'Orphaned Embalagens', 
            CASE WHEN orphaned_embalagens = 0 THEN 'PASS' ELSE 'FAIL' END,
            'Embalagens without valid UnidadeMedida reference: ' || orphaned_embalagens,
            orphaned_embalagens);
END $$;

-- Test 3.4: Check Produto references
DO $$
DECLARE
    produtos_invalid_unidade INTEGER := 0;
    produtos_invalid_embalagem INTEGER := 0;
    produtos_invalid_atividade INTEGER := 0;
BEGIN
    -- Check UnidadeMedida references
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produto' AND column_name = 'UnidadeMedidaId') THEN
        SELECT COUNT(*) INTO produtos_invalid_unidade
        FROM public."Produto" p
        LEFT JOIN public."UnidadesMedida" um ON p."UnidadeMedidaId" = um."Id"
        WHERE p."UnidadeMedidaId" IS NOT NULL AND um."Id" IS NULL;
    END IF;
    
    -- Check Embalagem references
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produto' AND column_name = 'EmbalagemId') THEN
        SELECT COUNT(*) INTO produtos_invalid_embalagem
        FROM public."Produto" p
        LEFT JOIN public."Embalagens" e ON p."EmbalagemId" = e."Id"
        WHERE p."EmbalagemId" IS NOT NULL AND e."Id" IS NULL;
    END IF;
    
    -- Check AtividadeAgropecuaria references
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produto' AND column_name = 'AtividadeAgropecuariaId') THEN
        SELECT COUNT(*) INTO produtos_invalid_atividade
        FROM public."Produto" p
        LEFT JOIN public."AtividadesAgropecuarias" aa ON p."AtividadeAgropecuariaId" = aa."Id"
        WHERE p."AtividadeAgropecuariaId" IS NOT NULL AND aa."Id" IS NULL;
    END IF;
    
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'REFERENTIAL_INTEGRITY', 'Produto UnidadeMedida references', 
            CASE WHEN produtos_invalid_unidade = 0 THEN 'PASS' ELSE 'FAIL' END,
            'Produtos with invalid UnidadeMedida reference: ' || produtos_invalid_unidade,
            produtos_invalid_unidade);
            
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'REFERENTIAL_INTEGRITY', 'Produto Embalagem references', 
            CASE WHEN produtos_invalid_embalagem = 0 THEN 'PASS' ELSE 'FAIL' END,
            'Produtos with invalid Embalagem reference: ' || produtos_invalid_embalagem,
            produtos_invalid_embalagem);
            
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'REFERENTIAL_INTEGRITY', 'Produto AtividadeAgropecuaria references', 
            CASE WHEN produtos_invalid_atividade = 0 THEN 'PASS' ELSE 'FAIL' END,
            'Produtos with invalid AtividadeAgropecuaria reference: ' || produtos_invalid_atividade,
            produtos_invalid_atividade);
END $$;

-- Test 3.5: Check Fornecedor references
DO $$
DECLARE
    fornecedores_invalid_uf INTEGER := 0;
    fornecedores_invalid_municipio INTEGER := 0;
    fornecedores_inconsistent INTEGER := 0;
BEGIN
    -- Check UF references
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Fornecedor' AND column_name = 'UfId') THEN
        SELECT COUNT(*) INTO fornecedores_invalid_uf
        FROM public."Fornecedor" f
        LEFT JOIN public."Ufs" u ON f."UfId" = u."Id"
        WHERE f."UfId" IS NOT NULL AND u."Id" IS NULL;
    END IF;
    
    -- Check Municipio references
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Fornecedor' AND column_name = 'MunicipioId') THEN
        SELECT COUNT(*) INTO fornecedores_invalid_municipio
        FROM public."Fornecedor" f
        LEFT JOIN public."Municipios" m ON f."MunicipioId" = m."Id"
        WHERE f."MunicipioId" IS NOT NULL AND m."Id" IS NULL;
    END IF;
    
    -- Check UF-Municipio consistency
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Fornecedor' AND column_name = 'UfId') 
       AND EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Fornecedor' AND column_name = 'MunicipioId') THEN
        SELECT COUNT(*) INTO fornecedores_inconsistent
        FROM public."Fornecedor" f
        INNER JOIN public."Municipios" m ON f."MunicipioId" = m."Id"
        WHERE f."UfId" IS NOT NULL AND f."UfId" != m."UfId";
    END IF;
    
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'REFERENTIAL_INTEGRITY', 'Fornecedor UF references', 
            CASE WHEN fornecedores_invalid_uf = 0 THEN 'PASS' ELSE 'FAIL' END,
            'Fornecedores with invalid UF reference: ' || fornecedores_invalid_uf,
            fornecedores_invalid_uf);
            
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'REFERENTIAL_INTEGRITY', 'Fornecedor Municipio references', 
            CASE WHEN fornecedores_invalid_municipio = 0 THEN 'PASS' ELSE 'FAIL' END,
            'Fornecedores with invalid Municipio reference: ' || fornecedores_invalid_municipio,
            fornecedores_invalid_municipio);
            
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'REFERENTIAL_INTEGRITY', 'Fornecedor UF-Municipio consistency', 
            CASE WHEN fornecedores_inconsistent = 0 THEN 'PASS' ELSE 'FAIL' END,
            'Fornecedores with inconsistent UF-Municipio relationship: ' || fornecedores_inconsistent,
            fornecedores_inconsistent);
END $$;

-- =====================================================
-- CATEGORY 4: DATA QUALITY
-- =====================================================

-- Test 4.1: Check for duplicate codes in reference tables
DO $$
DECLARE
    duplicate_moedas INTEGER := 0;
    duplicate_ufs INTEGER := 0;
    duplicate_atividades INTEGER := 0;
    duplicate_unidades INTEGER := 0;
BEGIN
    -- Check Moedas duplicates
    SELECT COUNT(*) - COUNT(DISTINCT "Codigo") INTO duplicate_moedas FROM public."Moedas";
    
    -- Check UFs duplicates
    SELECT COUNT(*) - COUNT(DISTINCT "Codigo") INTO duplicate_ufs FROM public."Ufs";
    
    -- Check AtividadesAgropecuarias duplicates
    SELECT COUNT(*) - COUNT(DISTINCT "Codigo") INTO duplicate_atividades FROM public."AtividadesAgropecuarias";
    
    -- Check UnidadesMedida duplicates
    SELECT COUNT(*) - COUNT(DISTINCT "Simbolo") INTO duplicate_unidades FROM public."UnidadesMedida";
    
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'DATA_QUALITY', 'Duplicate Moeda codes', 
            CASE WHEN duplicate_moedas = 0 THEN 'PASS' ELSE 'FAIL' END,
            'Duplicate Moeda codes found: ' || duplicate_moedas,
            duplicate_moedas);
            
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'DATA_QUALITY', 'Duplicate UF codes', 
            CASE WHEN duplicate_ufs = 0 THEN 'PASS' ELSE 'FAIL' END,
            'Duplicate UF codes found: ' || duplicate_ufs,
            duplicate_ufs);
            
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'DATA_QUALITY', 'Duplicate AtividadeAgropecuaria codes', 
            CASE WHEN duplicate_atividades = 0 THEN 'PASS' ELSE 'FAIL' END,
            'Duplicate AtividadeAgropecuaria codes found: ' || duplicate_atividades,
            duplicate_atividades);
            
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'DATA_QUALITY', 'Duplicate UnidadeMedida symbols', 
            CASE WHEN duplicate_unidades = 0 THEN 'PASS' ELSE 'FAIL' END,
            'Duplicate UnidadeMedida symbols found: ' || duplicate_unidades,
            duplicate_unidades);
END $$;

-- Test 4.2: Check for null/empty required fields
DO $$
DECLARE
    null_checks RECORD;
BEGIN
    FOR null_checks IN
        SELECT 
            table_name,
            column_name,
            null_count,
            CASE WHEN null_count = 0 THEN 'PASS' ELSE 'FAIL' END as status
        FROM (
            SELECT 'Paises' as table_name, 'Codigo' as column_name, 
                   (SELECT COUNT(*) FROM public."Paises" WHERE "Codigo" IS NULL OR TRIM("Codigo") = '') as null_count
            UNION ALL
            SELECT 'Paises', 'Nome', 
                   (SELECT COUNT(*) FROM public."Paises" WHERE "Nome" IS NULL OR TRIM("Nome") = '')
            UNION ALL
            SELECT 'Ufs', 'Codigo', 
                   (SELECT COUNT(*) FROM public."Ufs" WHERE "Codigo" IS NULL OR TRIM("Codigo") = '')
            UNION ALL
            SELECT 'Ufs', 'Nome', 
                   (SELECT COUNT(*) FROM public."Ufs" WHERE "Nome" IS NULL OR TRIM("Nome") = '')
            UNION ALL
            SELECT 'Municipios', 'Nome', 
                   (SELECT COUNT(*) FROM public."Municipios" WHERE "Nome" IS NULL OR TRIM("Nome") = '')
            UNION ALL
            SELECT 'Moedas', 'Codigo', 
                   (SELECT COUNT(*) FROM public."Moedas" WHERE "Codigo" IS NULL OR TRIM("Codigo") = '')
            UNION ALL
            SELECT 'UnidadesMedida', 'Simbolo', 
                   (SELECT COUNT(*) FROM public."UnidadesMedida" WHERE "Simbolo" IS NULL OR TRIM("Simbolo") = '')
            UNION ALL
            SELECT 'AtividadesAgropecuarias', 'Codigo', 
                   (SELECT COUNT(*) FROM public."AtividadesAgropecuarias" WHERE "Codigo" IS NULL OR TRIM("Codigo") = '')
        ) checks
        WHERE EXISTS (
            SELECT 1 FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name = checks.table_name
        )
    LOOP
        INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
        VALUES ('validate_all_references', 'DATA_QUALITY', null_checks.table_name || '.' || null_checks.column_name || ' null check', 
                null_checks.status,
                'Null/empty values in ' || null_checks.table_name || '.' || null_checks.column_name || ': ' || null_checks.null_count,
                null_checks.null_count);
    END LOOP;
END $$;

-- =====================================================
-- CATEGORY 5: MIGRATION COMPLETENESS
-- =====================================================

-- Test 5.1: Check migration coverage for Produto
DO $$
DECLARE
    total_produtos INTEGER := 0;
    produtos_with_unidade INTEGER := 0;
    produtos_with_embalagem INTEGER := 0;
    produtos_with_atividade INTEGER := 0;
    migration_coverage DECIMAL;
BEGIN
    SELECT COUNT(*) INTO total_produtos FROM public."Produto";
    
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produto' AND column_name = 'UnidadeMedidaId') THEN
        SELECT COUNT(*) INTO produtos_with_unidade FROM public."Produto" WHERE "UnidadeMedidaId" IS NOT NULL;
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produto' AND column_name = 'EmbalagemId') THEN
        SELECT COUNT(*) INTO produtos_with_embalagem FROM public."Produto" WHERE "EmbalagemId" IS NOT NULL;
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produto' AND column_name = 'AtividadeAgropecuariaId') THEN
        SELECT COUNT(*) INTO produtos_with_atividade FROM public."Produto" WHERE "AtividadeAgropecuariaId" IS NOT NULL;
    END IF;
    
    IF total_produtos > 0 THEN
        migration_coverage := ROUND((produtos_with_unidade::DECIMAL / total_produtos * 100), 2);
    ELSE
        migration_coverage := 0;
    END IF;
    
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ExpectedCount", "ActualCount")
    VALUES ('validate_all_references', 'MIGRATION_COMPLETENESS', 'Produto UnidadeMedida migration coverage', 
            CASE WHEN migration_coverage >= 95 THEN 'PASS' WHEN migration_coverage >= 80 THEN 'WARNING' ELSE 'FAIL' END,
            'Produto UnidadeMedida migration coverage: ' || migration_coverage || '% (' || produtos_with_unidade || '/' || total_produtos || ')',
            total_produtos, produtos_with_unidade);
            
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ExpectedCount", "ActualCount")
    VALUES ('validate_all_references', 'MIGRATION_COMPLETENESS', 'Produto Embalagem migration coverage', 'INFO',
            'Produto Embalagem migration coverage: ' || ROUND((produtos_with_embalagem::DECIMAL / GREATEST(total_produtos, 1) * 100), 2) || '% (' || produtos_with_embalagem || '/' || total_produtos || ')',
            total_produtos, produtos_with_embalagem);
            
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ExpectedCount", "ActualCount")
    VALUES ('validate_all_references', 'MIGRATION_COMPLETENESS', 'Produto AtividadeAgropecuaria migration coverage', 'INFO',
            'Produto AtividadeAgropecuaria migration coverage: ' || ROUND((produtos_with_atividade::DECIMAL / GREATEST(total_produtos, 1) * 100), 2) || '% (' || produtos_with_atividade || '/' || total_produtos || ')',
            total_produtos, produtos_with_atividade);
END $$;

-- Test 5.2: Check migration coverage for Fornecedor
DO $$
DECLARE
    total_fornecedores INTEGER := 0;
    fornecedores_with_uf INTEGER := 0;
    fornecedores_with_municipio INTEGER := 0;
    uf_coverage DECIMAL;
    municipio_coverage DECIMAL;
BEGIN
    SELECT COUNT(*) INTO total_fornecedores FROM public."Fornecedor";
    
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Fornecedor' AND column_name = 'UfId') THEN
        SELECT COUNT(*) INTO fornecedores_with_uf FROM public."Fornecedor" WHERE "UfId" IS NOT NULL;
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Fornecedor' AND column_name = 'MunicipioId') THEN
        SELECT COUNT(*) INTO fornecedores_with_municipio FROM public."Fornecedor" WHERE "MunicipioId" IS NOT NULL;
    END IF;
    
    IF total_fornecedores > 0 THEN
        uf_coverage := ROUND((fornecedores_with_uf::DECIMAL / total_fornecedores * 100), 2);
        municipio_coverage := ROUND((fornecedores_with_municipio::DECIMAL / total_fornecedores * 100), 2);
    ELSE
        uf_coverage := 0;
        municipio_coverage := 0;
    END IF;
    
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ExpectedCount", "ActualCount")
    VALUES ('validate_all_references', 'MIGRATION_COMPLETENESS', 'Fornecedor UF migration coverage', 
            CASE WHEN uf_coverage >= 95 THEN 'PASS' WHEN uf_coverage >= 80 THEN 'WARNING' ELSE 'FAIL' END,
            'Fornecedor UF migration coverage: ' || uf_coverage || '% (' || fornecedores_with_uf || '/' || total_fornecedores || ')',
            total_fornecedores, fornecedores_with_uf);
            
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ExpectedCount", "ActualCount")
    VALUES ('validate_all_references', 'MIGRATION_COMPLETENESS', 'Fornecedor Municipio migration coverage', 
            CASE WHEN municipio_coverage >= 90 THEN 'PASS' WHEN municipio_coverage >= 70 THEN 'WARNING' ELSE 'FAIL' END,
            'Fornecedor Municipio migration coverage: ' || municipio_coverage || '% (' || fornecedores_with_municipio || '/' || total_fornecedores || ')',
            total_fornecedores, fornecedores_with_municipio);
END $$;

-- =====================================================
-- GENERATE FINAL VALIDATION REPORT
-- =====================================================
DO $$
DECLARE
    validation_summary RECORD;
    total_tests INTEGER;
    passed_tests INTEGER;
    failed_tests INTEGER;
    warning_tests INTEGER;
    success_rate DECIMAL;
    final_status TEXT;
    report_text TEXT;
BEGIN
    -- Get validation summary
    SELECT 
        COUNT(*) as total,
        COUNT(*) FILTER (WHERE "Status" = 'PASS') as passed,
        COUNT(*) FILTER (WHERE "Status" = 'FAIL') as failed,
        COUNT(*) FILTER (WHERE "Status" = 'WARNING') as warnings
    INTO total_tests, passed_tests, failed_tests, warning_tests
    FROM public."ValidationResults" 
    WHERE "ValidationName" = 'validate_all_references' 
    AND "Status" IN ('PASS', 'FAIL', 'WARNING');
    
    -- Calculate success rate
    IF total_tests > 0 THEN
        success_rate := ROUND((passed_tests::DECIMAL / total_tests * 100), 2);
    ELSE
        success_rate := 0;
    END IF;
    
    -- Determine final status
    IF failed_tests = 0 AND warning_tests = 0 THEN
        final_status := 'EXCELLENT';
    ELSIF failed_tests = 0 AND warning_tests <= 2 THEN
        final_status := 'GOOD';
    ELSIF failed_tests <= 2 THEN
        final_status := 'ACCEPTABLE';
    ELSE
        final_status := 'NEEDS_ATTENTION';
    END IF;
    
    -- Generate final report
    report_text := 'REFERENCE VALIDATION SUMMARY REPORT' || E'\n' ||
                   '====================================' || E'\n' ||
                   'Total Tests: ' || total_tests || E'\n' ||
                   'Passed: ' || passed_tests || ' (' || success_rate || '%)' || E'\n' ||
                   'Failed: ' || failed_tests || E'\n' ||
                   'Warnings: ' || warning_tests || E'\n' ||
                   'Overall Status: ' || final_status || E'\n' ||
                   '====================================' || E'\n' ||
                   'Categories Tested:' || E'\n' ||
                   '- Table Existence' || E'\n' ||
                   '- Foreign Key Constraints' || E'\n' ||
                   '- Referential Integrity' || E'\n' ||
                   '- Data Quality' || E'\n' ||
                   '- Migration Completeness' || E'\n' ||
                   '====================================';
    
    INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message", "ActualCount")
    VALUES ('validate_all_references', 'SUMMARY', 'Final validation report', final_status, report_text, total_tests);
    
    -- Output report to console
    RAISE NOTICE '%', report_text;
    
    -- Additional recommendations
    IF failed_tests > 0 THEN
        RAISE NOTICE 'RECOMMENDATION: Review failed tests and fix issues before proceeding to production.';
    END IF;
    
    IF warning_tests > 0 THEN
        RAISE NOTICE 'RECOMMENDATION: Review warnings and consider addressing them for optimal data quality.';
    END IF;
    
    RAISE NOTICE 'Detailed validation results are available in the ValidationResults table.';
    RAISE NOTICE 'Query: SELECT * FROM public."ValidationResults" WHERE "ValidationName" = ''validate_all_references'' ORDER BY "Id";';
END $$;

-- Log validation completion
INSERT INTO public."ValidationResults" ("ValidationName", "Category", "TestName", "Status", "Message")
VALUES ('validate_all_references', 'SYSTEM', 'COMPLETE', 'SUCCESS', 'Reference validation completed successfully');

-- Commit validation transaction
COMMIT;

-- Display final message
\echo 'Reference validation completed!'
\echo 'Check ValidationResults table for detailed results.'
\echo 'Query: SELECT * FROM public."ValidationResults" WHERE "ValidationName" = ''validate_all_references'' ORDER BY "Category", "TestName";'