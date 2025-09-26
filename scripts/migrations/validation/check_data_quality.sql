-- =====================================================
-- DATA QUALITY CHECKER
-- Description: Comprehensive data quality validation for reference entities
-- Author: System Migration
-- Date: 2025-01-27
-- Requirements: 12.5, 12.6
-- =====================================================

-- Enable detailed logging
\set VERBOSITY verbose
\timing on

-- Create data quality results table if not exists
CREATE TABLE IF NOT EXISTS public."DataQualityResults" (
    "Id" SERIAL PRIMARY KEY,
    "CheckName" VARCHAR(200) NOT NULL,
    "Category" VARCHAR(100) NOT NULL,
    "TableName" VARCHAR(100) NOT NULL,
    "ColumnName" VARCHAR(100) NULL,
    "IssueType" VARCHAR(100) NOT NULL,
    "Severity" VARCHAR(20) NOT NULL, -- CRITICAL, HIGH, MEDIUM, LOW, INFO
    "IssueCount" INTEGER NOT NULL DEFAULT 0,
    "SampleValues" TEXT NULL,
    "RecommendedAction" TEXT NULL,
    "CheckedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Start data quality check
BEGIN;

-- Clear previous results
DELETE FROM public."DataQualityResults" WHERE "CheckName" = 'reference_entities_quality_check';

-- =====================================================
-- CATEGORY 1: NULL AND EMPTY VALUE CHECKS
-- =====================================================

-- Check 1.1: Required fields with NULL values
DO $$
DECLARE
    null_check RECORD;
BEGIN
    FOR null_check IN
        SELECT 
            table_name,
            column_name,
            null_count,
            sample_values
        FROM (
            -- Paises checks
            SELECT 'Paises' as table_name, 'Codigo' as column_name,
                   (SELECT COUNT(*) FROM public."Paises" WHERE "Codigo" IS NULL OR TRIM("Codigo") = '') as null_count,
                   (SELECT string_agg(DISTINCT "Nome", ', ') FROM public."Paises" WHERE "Codigo" IS NULL OR TRIM("Codigo") = '' LIMIT 5) as sample_values
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Paises' AND table_schema = 'public')
            
            UNION ALL
            SELECT 'Paises', 'Nome',
                   (SELECT COUNT(*) FROM public."Paises" WHERE "Nome" IS NULL OR TRIM("Nome") = ''),
                   (SELECT string_agg(DISTINCT "Codigo", ', ') FROM public."Paises" WHERE "Nome" IS NULL OR TRIM("Nome") = '' LIMIT 5)
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Paises' AND table_schema = 'public')
            
            -- Ufs checks
            UNION ALL
            SELECT 'Ufs', 'Codigo',
                   (SELECT COUNT(*) FROM public."Ufs" WHERE "Codigo" IS NULL OR TRIM("Codigo") = ''),
                   (SELECT string_agg(DISTINCT "Nome", ', ') FROM public."Ufs" WHERE "Codigo" IS NULL OR TRIM("Codigo") = '' LIMIT 5)
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Ufs' AND table_schema = 'public')
            
            UNION ALL
            SELECT 'Ufs', 'Nome',
                   (SELECT COUNT(*) FROM public."Ufs" WHERE "Nome" IS NULL OR TRIM("Nome") = ''),
                   (SELECT string_agg(DISTINCT "Codigo", ', ') FROM public."Ufs" WHERE "Nome" IS NULL OR TRIM("Nome") = '' LIMIT 5)
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Ufs' AND table_schema = 'public')
            
            -- Municipios checks
            UNION ALL
            SELECT 'Municipios', 'Nome',
                   (SELECT COUNT(*) FROM public."Municipios" WHERE "Nome" IS NULL OR TRIM("Nome") = ''),
                   (SELECT string_agg(DISTINCT "Id"::TEXT, ', ') FROM public."Municipios" WHERE "Nome" IS NULL OR TRIM("Nome") = '' LIMIT 5)
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Municipios' AND table_schema = 'public')
            
            -- Moedas checks
            UNION ALL
            SELECT 'Moedas', 'Codigo',
                   (SELECT COUNT(*) FROM public."Moedas" WHERE "Codigo" IS NULL OR TRIM("Codigo") = ''),
                   (SELECT string_agg(DISTINCT "Nome", ', ') FROM public."Moedas" WHERE "Codigo" IS NULL OR TRIM("Codigo") = '' LIMIT 5)
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Moedas' AND table_schema = 'public')
            
            -- UnidadesMedida checks
            UNION ALL
            SELECT 'UnidadesMedida', 'Simbolo',
                   (SELECT COUNT(*) FROM public."UnidadesMedida" WHERE "Simbolo" IS NULL OR TRIM("Simbolo") = ''),
                   (SELECT string_agg(DISTINCT "Nome", ', ') FROM public."UnidadesMedida" WHERE "Simbolo" IS NULL OR TRIM("Simbolo") = '' LIMIT 5)
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UnidadesMedida' AND table_schema = 'public')
            
            -- AtividadesAgropecuarias checks
            UNION ALL
            SELECT 'AtividadesAgropecuarias', 'Codigo',
                   (SELECT COUNT(*) FROM public."AtividadesAgropecuarias" WHERE "Codigo" IS NULL OR TRIM("Codigo") = ''),
                   (SELECT string_agg(DISTINCT "Descricao", ', ') FROM public."AtividadesAgropecuarias" WHERE "Codigo" IS NULL OR TRIM("Codigo") = '' LIMIT 5)
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AtividadesAgropecuarias' AND table_schema = 'public')
        ) checks
        WHERE null_count > 0
    LOOP
        INSERT INTO public."DataQualityResults" 
        ("CheckName", "Category", "TableName", "ColumnName", "IssueType", "Severity", "IssueCount", "SampleValues", "RecommendedAction")
        VALUES (
            'reference_entities_quality_check',
            'NULL_EMPTY_VALUES',
            null_check.table_name,
            null_check.column_name,
            'NULL_OR_EMPTY_REQUIRED_FIELD',
            'CRITICAL',
            null_check.null_count,
            null_check.sample_values,
            'Update records to provide valid values for required field: ' || null_check.column_name
        );
    END LOOP;
END $$;

-- =====================================================
-- CATEGORY 2: DUPLICATE VALUE CHECKS
-- =====================================================

-- Check 2.1: Duplicate codes and unique constraints
DO $$
DECLARE
    duplicate_check RECORD;
BEGIN
    FOR duplicate_check IN
        SELECT 
            table_name,
            column_name,
            duplicate_count,
            sample_duplicates
        FROM (
            -- Moedas duplicate codes
            SELECT 'Moedas' as table_name, 'Codigo' as column_name,
                   (SELECT COUNT(*) - COUNT(DISTINCT "Codigo") FROM public."Moedas") as duplicate_count,
                   (SELECT string_agg(DISTINCT "Codigo", ', ') FROM public."Moedas" GROUP BY "Codigo" HAVING COUNT(*) > 1 LIMIT 5) as sample_duplicates
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Moedas' AND table_schema = 'public')
            
            UNION ALL
            SELECT 'Moedas', 'Nome',
                   (SELECT COUNT(*) - COUNT(DISTINCT "Nome") FROM public."Moedas"),
                   (SELECT string_agg(DISTINCT "Nome", ', ') FROM public."Moedas" GROUP BY "Nome" HAVING COUNT(*) > 1 LIMIT 5)
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Moedas' AND table_schema = 'public')
            
            UNION ALL
            SELECT 'Moedas', 'Simbolo',
                   (SELECT COUNT(*) - COUNT(DISTINCT "Simbolo") FROM public."Moedas"),
                   (SELECT string_agg(DISTINCT "Simbolo", ', ') FROM public."Moedas" GROUP BY "Simbolo" HAVING COUNT(*) > 1 LIMIT 5)
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Moedas' AND table_schema = 'public')
            
            -- Ufs duplicate codes
            UNION ALL
            SELECT 'Ufs', 'Codigo',
                   (SELECT COUNT(*) - COUNT(DISTINCT "Codigo") FROM public."Ufs"),
                   (SELECT string_agg(DISTINCT "Codigo", ', ') FROM public."Ufs" GROUP BY "Codigo" HAVING COUNT(*) > 1 LIMIT 5)
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Ufs' AND table_schema = 'public')
            
            -- UnidadesMedida duplicate symbols
            UNION ALL
            SELECT 'UnidadesMedida', 'Simbolo',
                   (SELECT COUNT(*) - COUNT(DISTINCT "Simbolo") FROM public."UnidadesMedida"),
                   (SELECT string_agg(DISTINCT "Simbolo", ', ') FROM public."UnidadesMedida" GROUP BY "Simbolo" HAVING COUNT(*) > 1 LIMIT 5)
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UnidadesMedida' AND table_schema = 'public')
            
            -- AtividadesAgropecuarias duplicate codes
            UNION ALL
            SELECT 'AtividadesAgropecuarias', 'Codigo',
                   (SELECT COUNT(*) - COUNT(DISTINCT "Codigo") FROM public."AtividadesAgropecuarias"),
                   (SELECT string_agg(DISTINCT "Codigo", ', ') FROM public."AtividadesAgropecuarias" GROUP BY "Codigo" HAVING COUNT(*) > 1 LIMIT 5)
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AtividadesAgropecuarias' AND table_schema = 'public')
            
            -- Municipios duplicate names within same UF
            UNION ALL
            SELECT 'Municipios', 'Nome+UfId',
                   (SELECT COUNT(*) - COUNT(DISTINCT "Nome", "UfId") FROM public."Municipios"),
                   (SELECT string_agg(DISTINCT "Nome" || ' (UF:' || "UfId"::TEXT || ')', ', ') 
                    FROM public."Municipios" 
                    GROUP BY "Nome", "UfId" HAVING COUNT(*) > 1 LIMIT 5)
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Municipios' AND table_schema = 'public')
        ) checks
        WHERE duplicate_count > 0
    LOOP
        INSERT INTO public."DataQualityResults" 
        ("CheckName", "Category", "TableName", "ColumnName", "IssueType", "Severity", "IssueCount", "SampleValues", "RecommendedAction")
        VALUES (
            'reference_entities_quality_check',
            'DUPLICATE_VALUES',
            duplicate_check.table_name,
            duplicate_check.column_name,
            'DUPLICATE_UNIQUE_FIELD',
            'HIGH',
            duplicate_check.duplicate_count,
            duplicate_check.sample_duplicates,
            'Remove or merge duplicate records for unique field: ' || duplicate_check.column_name
        );
    END LOOP;
END $$;

-- =====================================================
-- CATEGORY 3: FORMAT AND LENGTH VALIDATION
-- =====================================================

-- Check 3.1: Invalid formats and lengths
DO $$
DECLARE
    format_check RECORD;
BEGIN
    FOR format_check IN
        SELECT 
            table_name,
            column_name,
            issue_type,
            invalid_count,
            sample_values,
            recommended_action
        FROM (
            -- UF codes should be exactly 2 characters
            SELECT 'Ufs' as table_name, 'Codigo' as column_name, 'INVALID_LENGTH' as issue_type,
                   (SELECT COUNT(*) FROM public."Ufs" WHERE LENGTH("Codigo") != 2) as invalid_count,
                   (SELECT string_agg(DISTINCT "Codigo", ', ') FROM public."Ufs" WHERE LENGTH("Codigo") != 2 LIMIT 5) as sample_values,
                   'UF codes must be exactly 2 characters' as recommended_action
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Ufs' AND table_schema = 'public')
            
            -- Moeda codes should be exactly 3 characters
            UNION ALL
            SELECT 'Moedas', 'Codigo', 'INVALID_LENGTH',
                   (SELECT COUNT(*) FROM public."Moedas" WHERE LENGTH("Codigo") != 3),
                   (SELECT string_agg(DISTINCT "Codigo", ', ') FROM public."Moedas" WHERE LENGTH("Codigo") != 3 LIMIT 5),
                   'Currency codes must be exactly 3 characters (ISO 4217)'
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Moedas' AND table_schema = 'public')
            
            -- UF codes should be uppercase
            UNION ALL
            SELECT 'Ufs', 'Codigo', 'INVALID_CASE',
                   (SELECT COUNT(*) FROM public."Ufs" WHERE "Codigo" != UPPER("Codigo")),
                   (SELECT string_agg(DISTINCT "Codigo", ', ') FROM public."Ufs" WHERE "Codigo" != UPPER("Codigo") LIMIT 5),
                   'UF codes should be uppercase'
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Ufs' AND table_schema = 'public')
            
            -- Moeda codes should be uppercase
            UNION ALL
            SELECT 'Moedas', 'Codigo', 'INVALID_CASE',
                   (SELECT COUNT(*) FROM public."Moedas" WHERE "Codigo" != UPPER("Codigo")),
                   (SELECT string_agg(DISTINCT "Codigo", ', ') FROM public."Moedas" WHERE "Codigo" != UPPER("Codigo") LIMIT 5),
                   'Currency codes should be uppercase'
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Moedas' AND table_schema = 'public')
            
            -- Check for leading/trailing spaces
            UNION ALL
            SELECT 'Municipios', 'Nome', 'WHITESPACE_ISSUES',
                   (SELECT COUNT(*) FROM public."Municipios" WHERE "Nome" != TRIM("Nome")),
                   (SELECT string_agg(DISTINCT '"' || "Nome" || '"', ', ') FROM public."Municipios" WHERE "Nome" != TRIM("Nome") LIMIT 5),
                   'Remove leading/trailing whitespace from municipality names'
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Municipios' AND table_schema = 'public')
            
            -- Check for very long names (potential data issues)
            UNION ALL
            SELECT 'Municipios', 'Nome', 'EXCESSIVE_LENGTH',
                   (SELECT COUNT(*) FROM public."Municipios" WHERE LENGTH("Nome") > 100),
                   (SELECT string_agg(DISTINCT LEFT("Nome", 50) || '...', ', ') FROM public."Municipios" WHERE LENGTH("Nome") > 100 LIMIT 3),
                   'Review municipality names longer than 100 characters'
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Municipios' AND table_schema = 'public')
        ) checks
        WHERE invalid_count > 0
    LOOP
        INSERT INTO public."DataQualityResults" 
        ("CheckName", "Category", "TableName", "ColumnName", "IssueType", "Severity", "IssueCount", "SampleValues", "RecommendedAction")
        VALUES (
            'reference_entities_quality_check',
            'FORMAT_VALIDATION',
            format_check.table_name,
            format_check.column_name,
            format_check.issue_type,
            CASE 
                WHEN format_check.issue_type IN ('INVALID_LENGTH', 'INVALID_CASE') THEN 'HIGH'
                WHEN format_check.issue_type = 'WHITESPACE_ISSUES' THEN 'MEDIUM'
                ELSE 'LOW'
            END,
            format_check.invalid_count,
            format_check.sample_values,
            format_check.recommended_action
        );
    END LOOP;
END $$;

-- =====================================================
-- CATEGORY 4: BUSINESS RULE VALIDATION
-- =====================================================

-- Check 4.1: Business logic validation
DO $$
DECLARE
    business_check RECORD;
BEGIN
    FOR business_check IN
        SELECT 
            table_name,
            issue_type,
            invalid_count,
            sample_values,
            recommended_action
        FROM (
            -- UnidadesMedida with invalid conversion factors
            SELECT 'UnidadesMedida' as table_name, 'INVALID_CONVERSION_FACTOR' as issue_type,
                   (SELECT COUNT(*) FROM public."UnidadesMedida" WHERE "FatorConversao" IS NOT NULL AND "FatorConversao" <= 0) as invalid_count,
                   (SELECT string_agg(DISTINCT "Simbolo" || ':' || "FatorConversao"::TEXT, ', ') 
                    FROM public."UnidadesMedida" WHERE "FatorConversao" IS NOT NULL AND "FatorConversao" <= 0 LIMIT 5) as sample_values,
                   'Conversion factors must be positive numbers' as recommended_action
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UnidadesMedida' AND table_schema = 'public')
            
            -- AtividadesAgropecuarias with invalid types
            UNION ALL
            SELECT 'AtividadesAgropecuarias', 'INVALID_TIPO',
                   (SELECT COUNT(*) FROM public."AtividadesAgropecuarias" 
                    WHERE "Tipo" NOT IN ('Agricultura', 'Pecuaria', 'Mista')),
                   (SELECT string_agg(DISTINCT "Tipo", ', ') FROM public."AtividadesAgropecuarias" 
                    WHERE "Tipo" NOT IN ('Agricultura', 'Pecuaria', 'Mista') LIMIT 5),
                   'Activity type must be one of: Agricultura, Pecuaria, Mista'
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AtividadesAgropecuarias' AND table_schema = 'public')
            
            -- UnidadesMedida with invalid types
            UNION ALL
            SELECT 'UnidadesMedida', 'INVALID_TIPO',
                   (SELECT COUNT(*) FROM public."UnidadesMedida" 
                    WHERE "Tipo" NOT IN ('Peso', 'Volume', 'Area', 'Unidade')),
                   (SELECT string_agg(DISTINCT "Tipo", ', ') FROM public."UnidadesMedida" 
                    WHERE "Tipo" NOT IN ('Peso', 'Volume', 'Area', 'Unidade') LIMIT 5),
                   'Unit type must be one of: Peso, Volume, Area, Unidade'
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UnidadesMedida' AND table_schema = 'public')
            
            -- Municipios with invalid IBGE codes (should be numeric if provided)
            UNION ALL
            SELECT 'Municipios', 'INVALID_CODIGO_IBGE',
                   (SELECT COUNT(*) FROM public."Municipios" 
                    WHERE "CodigoIbge" IS NOT NULL AND "CodigoIbge" !~ '^[0-9]+$'),
                   (SELECT string_agg(DISTINCT "CodigoIbge", ', ') FROM public."Municipios" 
                    WHERE "CodigoIbge" IS NOT NULL AND "CodigoIbge" !~ '^[0-9]+$' LIMIT 5),
                   'IBGE codes should contain only numeric characters'
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Municipios' AND table_schema = 'public')
        ) checks
        WHERE invalid_count > 0
    LOOP
        INSERT INTO public."DataQualityResults" 
        ("CheckName", "Category", "TableName", "ColumnName", "IssueType", "Severity", "IssueCount", "SampleValues", "RecommendedAction")
        VALUES (
            'reference_entities_quality_check',
            'BUSINESS_RULES',
            business_check.table_name,
            NULL,
            business_check.issue_type,
            'MEDIUM',
            business_check.invalid_count,
            business_check.sample_values,
            business_check.recommended_action
        );
    END LOOP;
END $$;

-- =====================================================
-- CATEGORY 5: REFERENTIAL INTEGRITY DEEP CHECK
-- =====================================================

-- Check 5.1: Orphaned records and circular references
DO $$
DECLARE
    integrity_check RECORD;
BEGIN
    FOR integrity_check IN
        SELECT 
            table_name,
            issue_type,
            invalid_count,
            sample_values,
            recommended_action
        FROM (
            -- Embalagens referencing non-existent UnidadesMedida
            SELECT 'Embalagens' as table_name, 'ORPHANED_REFERENCE' as issue_type,
                   (SELECT COUNT(*) FROM public."Embalagens" e 
                    LEFT JOIN public."UnidadesMedida" um ON e."UnidadeMedidaId" = um."Id" 
                    WHERE um."Id" IS NULL) as invalid_count,
                   (SELECT string_agg(DISTINCT e."Nome" || ' (UMId:' || e."UnidadeMedidaId"::TEXT || ')', ', ') 
                    FROM public."Embalagens" e 
                    LEFT JOIN public."UnidadesMedida" um ON e."UnidadeMedidaId" = um."Id" 
                    WHERE um."Id" IS NULL LIMIT 5) as sample_values,
                   'Fix or remove Embalagens with invalid UnidadeMedida references' as recommended_action
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Embalagens' AND table_schema = 'public')
            
            -- Municipios referencing non-existent UFs
            UNION ALL
            SELECT 'Municipios', 'ORPHANED_REFERENCE',
                   (SELECT COUNT(*) FROM public."Municipios" m 
                    LEFT JOIN public."Ufs" u ON m."UfId" = u."Id" 
                    WHERE u."Id" IS NULL),
                   (SELECT string_agg(DISTINCT m."Nome" || ' (UfId:' || m."UfId"::TEXT || ')', ', ') 
                    FROM public."Municipios" m 
                    LEFT JOIN public."Ufs" u ON m."UfId" = u."Id" 
                    WHERE u."Id" IS NULL LIMIT 5),
                   'Fix or remove Municipios with invalid UF references'
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Municipios' AND table_schema = 'public')
            
            -- UFs referencing non-existent Paises
            UNION ALL
            SELECT 'Ufs', 'ORPHANED_REFERENCE',
                   (SELECT COUNT(*) FROM public."Ufs" u 
                    LEFT JOIN public."Paises" p ON u."PaisId" = p."Id" 
                    WHERE p."Id" IS NULL),
                   (SELECT string_agg(DISTINCT u."Nome" || ' (PaisId:' || u."PaisId"::TEXT || ')', ', ') 
                    FROM public."Ufs" u 
                    LEFT JOIN public."Paises" p ON u."PaisId" = p."Id" 
                    WHERE p."Id" IS NULL LIMIT 5),
                   'Fix or remove UFs with invalid Pais references'
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Ufs' AND table_schema = 'public')
        ) checks
        WHERE invalid_count > 0
    LOOP
        INSERT INTO public."DataQualityResults" 
        ("CheckName", "Category", "TableName", "ColumnName", "IssueType", "Severity", "IssueCount", "SampleValues", "RecommendedAction")
        VALUES (
            'reference_entities_quality_check',
            'REFERENTIAL_INTEGRITY',
            integrity_check.table_name,
            NULL,
            integrity_check.issue_type,
            'CRITICAL',
            integrity_check.invalid_count,
            integrity_check.sample_values,
            integrity_check.recommended_action
        );
    END LOOP;
END $$;

-- =====================================================
-- CATEGORY 6: STATISTICAL ANOMALIES
-- =====================================================

-- Check 6.1: Statistical outliers and anomalies
DO $$
DECLARE
    stats_check RECORD;
BEGIN
    FOR stats_check IN
        SELECT 
            table_name,
            issue_type,
            anomaly_count,
            sample_values,
            recommended_action
        FROM (
            -- UFs with unusually high number of municipalities
            SELECT 'Ufs' as table_name, 'HIGH_MUNICIPIO_COUNT' as issue_type,
                   (SELECT COUNT(*) FROM (
                       SELECT u."Id", u."Nome", COUNT(m."Id") as municipio_count
                       FROM public."Ufs" u
                       LEFT JOIN public."Municipios" m ON u."Id" = m."UfId"
                       GROUP BY u."Id", u."Nome"
                       HAVING COUNT(m."Id") > 500
                   ) high_counts) as anomaly_count,
                   (SELECT string_agg(u."Nome" || ':' || COUNT(m."Id")::TEXT, ', ') 
                    FROM public."Ufs" u
                    LEFT JOIN public."Municipios" m ON u."Id" = m."UfId"
                    GROUP BY u."Id", u."Nome"
                    HAVING COUNT(m."Id") > 500 LIMIT 3) as sample_values,
                   'Review UFs with unusually high municipality counts' as recommended_action
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Ufs' AND table_schema = 'public')
            AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Municipios' AND table_schema = 'public')
            
            -- UFs with no municipalities
            UNION ALL
            SELECT 'Ufs', 'NO_MUNICIPIOS',
                   (SELECT COUNT(*) FROM public."Ufs" u
                    LEFT JOIN public."Municipios" m ON u."Id" = m."UfId"
                    WHERE m."Id" IS NULL),
                   (SELECT string_agg(DISTINCT u."Nome", ', ') FROM public."Ufs" u
                    LEFT JOIN public."Municipios" m ON u."Id" = m."UfId"
                    WHERE m."Id" IS NULL LIMIT 5),
                   'Review UFs with no municipalities - may indicate incomplete data'
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Ufs' AND table_schema = 'public')
            AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Municipios' AND table_schema = 'public')
            
            -- Embalagens with no associated UnidadeMedida type
            UNION ALL
            SELECT 'Embalagens', 'UNMATCHED_UNIT_TYPE',
                   (SELECT COUNT(*) FROM public."Embalagens" e
                    INNER JOIN public."UnidadesMedida" um ON e."UnidadeMedidaId" = um."Id"
                    WHERE (LOWER(e."Nome") LIKE '%litro%' OR LOWER(e."Nome") LIKE '%ml%' OR LOWER(e."Nome") LIKE '%galao%') 
                    AND um."Tipo" != 'Volume'),
                   (SELECT string_agg(DISTINCT e."Nome" || ' (' || um."Tipo" || ')', ', ') 
                    FROM public."Embalagens" e
                    INNER JOIN public."UnidadesMedida" um ON e."UnidadeMedidaId" = um."Id"
                    WHERE (LOWER(e."Nome") LIKE '%litro%' OR LOWER(e."Nome") LIKE '%ml%' OR LOWER(e."Nome") LIKE '%galao%') 
                    AND um."Tipo" != 'Volume' LIMIT 5),
                   'Review embalagens with mismatched unit types'
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Embalagens' AND table_schema = 'public')
        ) checks
        WHERE anomaly_count > 0
    LOOP
        INSERT INTO public."DataQualityResults" 
        ("CheckName", "Category", "TableName", "ColumnName", "IssueType", "Severity", "IssueCount", "SampleValues", "RecommendedAction")
        VALUES (
            'reference_entities_quality_check',
            'STATISTICAL_ANOMALIES',
            stats_check.table_name,
            NULL,
            stats_check.issue_type,
            'LOW',
            stats_check.anomaly_count,
            stats_check.sample_values,
            stats_check.recommended_action
        );
    END LOOP;
END $$;

-- =====================================================
-- GENERATE DATA QUALITY SUMMARY REPORT
-- =====================================================
DO $$
DECLARE
    quality_summary RECORD;
    total_issues INTEGER;
    critical_issues INTEGER;
    high_issues INTEGER;
    medium_issues INTEGER;
    low_issues INTEGER;
    tables_checked INTEGER;
    overall_score DECIMAL;
    quality_grade TEXT;
    summary_report TEXT;
BEGIN
    -- Get issue counts by severity
    SELECT 
        COUNT(*) as total,
        COUNT(*) FILTER (WHERE "Severity" = 'CRITICAL') as critical,
        COUNT(*) FILTER (WHERE "Severity" = 'HIGH') as high,
        COUNT(*) FILTER (WHERE "Severity" = 'MEDIUM') as medium,
        COUNT(*) FILTER (WHERE "Severity" = 'LOW') as low,
        COUNT(DISTINCT "TableName") as tables
    INTO total_issues, critical_issues, high_issues, medium_issues, low_issues, tables_checked
    FROM public."DataQualityResults" 
    WHERE "CheckName" = 'reference_entities_quality_check';
    
    -- Calculate quality score (100 - weighted penalty)
    overall_score := 100 - (critical_issues * 20 + high_issues * 10 + medium_issues * 5 + low_issues * 1);
    overall_score := GREATEST(0, overall_score);
    
    -- Determine quality grade
    quality_grade := CASE 
        WHEN overall_score >= 95 THEN 'A+ (Excellent)'
        WHEN overall_score >= 90 THEN 'A (Very Good)'
        WHEN overall_score >= 80 THEN 'B (Good)'
        WHEN overall_score >= 70 THEN 'C (Acceptable)'
        WHEN overall_score >= 60 THEN 'D (Poor)'
        ELSE 'F (Critical Issues)'
    END;
    
    -- Generate summary report
    summary_report := 'DATA QUALITY ASSESSMENT REPORT' || E'\n' ||
                     '===============================' || E'\n' ||
                     'Overall Quality Score: ' || overall_score || '/100' || E'\n' ||
                     'Quality Grade: ' || quality_grade || E'\n' ||
                     'Tables Checked: ' || tables_checked || E'\n' ||
                     'Total Issues Found: ' || total_issues || E'\n' ||
                     E'\n' ||
                     'ISSUES BY SEVERITY:' || E'\n' ||
                     '🔴 Critical: ' || critical_issues || ' (Immediate attention required)' || E'\n' ||
                     '🟠 High: ' || high_issues || ' (Should be fixed soon)' || E'\n' ||
                     '🟡 Medium: ' || medium_issues || ' (Should be addressed)' || E'\n' ||
                     '🟢 Low: ' || low_issues || ' (Minor improvements)' || E'\n' ||
                     E'\n' ||
                     'CATEGORIES CHECKED:' || E'\n' ||
                     '✓ Null and Empty Values' || E'\n' ||
                     '✓ Duplicate Values' || E'\n' ||
                     '✓ Format and Length Validation' || E'\n' ||
                     '✓ Business Rule Validation' || E'\n' ||
                     '✓ Referential Integrity' || E'\n' ||
                     '✓ Statistical Anomalies' || E'\n' ||
                     E'\n' ||
                     'RECOMMENDATIONS:' || E'\n' ||
                     CASE 
                         WHEN critical_issues > 0 THEN '❗ Address critical issues immediately before production deployment' || E'\n'
                         ELSE ''
                     END ||
                     CASE 
                         WHEN high_issues > 0 THEN '⚠️ Fix high-priority issues to improve data reliability' || E'\n'
                         ELSE ''
                     END ||
                     CASE 
                         WHEN medium_issues + low_issues > 0 THEN '💡 Consider addressing remaining issues for optimal data quality' || E'\n'
                         ELSE ''
                     END ||
                     CASE 
                         WHEN total_issues = 0 THEN '🎉 Excellent! No data quality issues found.' || E'\n'
                         ELSE ''
                     END;
    
    -- Save summary
    INSERT INTO public."DataQualityResults" 
    ("CheckName", "Category", "TableName", "ColumnName", "IssueType", "Severity", "IssueCount", "SampleValues", "RecommendedAction")
    VALUES (
        'reference_entities_quality_check',
        'SUMMARY',
        'ALL_TABLES',
        NULL,
        'QUALITY_SUMMARY',
        'INFO',
        total_issues,
        'Score: ' || overall_score || '/100, Grade: ' || quality_grade,
        summary_report
    );
    
    -- Output summary to console
    RAISE NOTICE '%', summary_report;
    
    -- Additional console messages
    IF critical_issues > 0 THEN
        RAISE NOTICE 'URGENT: % critical data quality issues require immediate attention!', critical_issues;
    END IF;
    
    IF total_issues = 0 THEN
        RAISE NOTICE 'SUCCESS: All data quality checks passed!';
    ELSE
        RAISE NOTICE 'Review detailed results: SELECT * FROM public."DataQualityResults" WHERE "CheckName" = ''reference_entities_quality_check'' ORDER BY "Severity", "Category";';
    END IF;
END $$;

-- Commit data quality check
COMMIT;

-- Display final message
\echo '========================================='
\echo 'DATA QUALITY CHECK COMPLETED!'
\echo '========================================='
\echo 'Comprehensive data quality assessment finished.'
\echo ''
\echo 'View summary:'
\echo 'SELECT "RecommendedAction" FROM public."DataQualityResults" WHERE "IssueType" = ''QUALITY_SUMMARY'';'
\echo ''
\echo 'View all issues:'
\echo 'SELECT "Severity", "Category", "TableName", "IssueType", "IssueCount", "RecommendedAction" FROM public."DataQualityResults" WHERE "CheckName" = ''reference_entities_quality_check'' AND "IssueType" != ''QUALITY_SUMMARY'' ORDER BY "Severity", "IssueCount" DESC;'
\echo ''
\echo 'View issues by table:'
\echo 'SELECT "TableName", COUNT(*) as issue_count, string_agg("IssueType", '', '') as issues FROM public."DataQualityResults" WHERE "CheckName" = ''reference_entities_quality_check'' AND "IssueType" != ''QUALITY_SUMMARY'' GROUP BY "TableName" ORDER BY issue_count DESC;'
\echo '========================================='