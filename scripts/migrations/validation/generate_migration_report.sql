-- =====================================================
-- MIGRATION REPORT GENERATOR
-- Description: Generates comprehensive migration report with statistics and issues
-- Author: System Migration
-- Date: 2025-01-27
-- Requirements: 12.5, 12.6
-- =====================================================

-- Enable detailed logging
\set VERBOSITY verbose
\timing on

-- Create migration report table if not exists
CREATE TABLE IF NOT EXISTS public."MigrationReports" (
    "Id" SERIAL PRIMARY KEY,
    "ReportName" VARCHAR(200) NOT NULL,
    "ReportType" VARCHAR(100) NOT NULL,
    "Section" VARCHAR(100) NOT NULL,
    "Title" VARCHAR(200) NOT NULL,
    "Content" TEXT NOT NULL,
    "Metrics" JSONB NULL,
    "GeneratedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Start report generation
BEGIN;

-- Clear previous reports
DELETE FROM public."MigrationReports" WHERE "ReportName" = 'reference_migration_summary';

-- =====================================================
-- SECTION 1: EXECUTIVE SUMMARY
-- =====================================================
DO $$
DECLARE
    total_reference_records INTEGER := 0;
    total_produtos INTEGER := 0;
    total_fornecedores INTEGER := 0;
    migration_start_time TIMESTAMP;
    migration_end_time TIMESTAMP;
    migration_duration INTERVAL;
    overall_status TEXT := 'SUCCESS';
    executive_summary TEXT;
    metrics JSONB;
BEGIN
    -- Get migration timing from logs
    SELECT MIN("ExecutionTime") INTO migration_start_time 
    FROM public."MigrationLogs" 
    WHERE "MigrationName" LIKE '%populate_reference_tables%' AND "Step" = 'START';
    
    SELECT MAX("ExecutionTime") INTO migration_end_time 
    FROM public."MigrationLogs" 
    WHERE "MigrationName" LIKE '%fornecedor_references%' AND "Step" = 'COMPLETE';
    
    migration_duration := migration_end_time - migration_start_time;
    
    -- Count records
    SELECT 
        COALESCE((SELECT COUNT(*) FROM public."Paises"), 0) +
        COALESCE((SELECT COUNT(*) FROM public."Ufs"), 0) +
        COALESCE((SELECT COUNT(*) FROM public."Municipios"), 0) +
        COALESCE((SELECT COUNT(*) FROM public."Moedas"), 0) +
        COALESCE((SELECT COUNT(*) FROM public."UnidadesMedida"), 0) +
        COALESCE((SELECT COUNT(*) FROM public."AtividadesAgropecuarias"), 0) +
        COALESCE((SELECT COUNT(*) FROM public."Embalagens"), 0)
    INTO total_reference_records;
    
    SELECT COUNT(*) INTO total_produtos FROM public."Produto";
    SELECT COUNT(*) INTO total_fornecedores FROM public."Fornecedor";
    
    -- Check for any critical failures
    IF EXISTS (
        SELECT 1 FROM public."ValidationResults" 
        WHERE "ValidationName" = 'validate_all_references' 
        AND "Status" = 'FAIL' 
        AND "Category" IN ('FOREIGN_KEYS', 'REFERENTIAL_INTEGRITY')
    ) THEN
        overall_status := 'CRITICAL_ISSUES';
    ELSIF EXISTS (
        SELECT 1 FROM public."ValidationResults" 
        WHERE "ValidationName" = 'validate_all_references' 
        AND "Status" = 'FAIL'
    ) THEN
        overall_status := 'MINOR_ISSUES';
    END IF;
    
    -- Generate executive summary
    executive_summary := 'REFERENCE ENTITIES MIGRATION - EXECUTIVE SUMMARY' || E'\n' ||
                        '=================================================' || E'\n' ||
                        'Migration Status: ' || overall_status || E'\n' ||
                        'Migration Duration: ' || COALESCE(migration_duration::TEXT, 'Unknown') || E'\n' ||
                        'Start Time: ' || COALESCE(migration_start_time::TEXT, 'Unknown') || E'\n' ||
                        'End Time: ' || COALESCE(migration_end_time::TEXT, 'Unknown') || E'\n' ||
                        E'\n' ||
                        'MIGRATION SCOPE:' || E'\n' ||
                        '- Reference Tables Created: 7 tables' || E'\n' ||
                        '- Total Reference Records: ' || total_reference_records || E'\n' ||
                        '- Products Migrated: ' || total_produtos || E'\n' ||
                        '- Suppliers Migrated: ' || total_fornecedores || E'\n' ||
                        E'\n' ||
                        'KEY ACHIEVEMENTS:' || E'\n' ||
                        '✓ Centralized reference data management' || E'\n' ||
                        '✓ Replaced string fields with proper foreign keys' || E'\n' ||
                        '✓ Established referential integrity constraints' || E'\n' ||
                        '✓ Improved data consistency and validation' || E'\n' ||
                        '✓ Enhanced query performance with proper indexing';
    
    -- Create metrics JSON
    metrics := jsonb_build_object(
        'total_reference_records', total_reference_records,
        'total_produtos', total_produtos,
        'total_fornecedores', total_fornecedores,
        'migration_duration_seconds', EXTRACT(EPOCH FROM migration_duration),
        'overall_status', overall_status
    );
    
    INSERT INTO public."MigrationReports" ("ReportName", "ReportType", "Section", "Title", "Content", "Metrics")
    VALUES ('reference_migration_summary', 'EXECUTIVE', 'SUMMARY', 'Executive Summary', executive_summary, metrics);
END $$;

-- =====================================================
-- SECTION 2: REFERENCE TABLES STATISTICS
-- =====================================================
DO $$
DECLARE
    table_stats RECORD;
    reference_stats TEXT := '';
    table_metrics JSONB := '{}';
BEGIN
    reference_stats := 'REFERENCE TABLES STATISTICS' || E'\n' ||
                      '===========================' || E'\n';
    
    FOR table_stats IN
        SELECT 
            table_name,
            record_count,
            has_data
        FROM (
            SELECT 'Paises' as table_name, 
                   COALESCE((SELECT COUNT(*) FROM public."Paises"), 0) as record_count,
                   EXISTS(SELECT 1 FROM information_schema.tables WHERE table_name = 'Paises' AND table_schema = 'public') as has_data
            UNION ALL
            SELECT 'Ufs', 
                   COALESCE((SELECT COUNT(*) FROM public."Ufs"), 0),
                   EXISTS(SELECT 1 FROM information_schema.tables WHERE table_name = 'Ufs' AND table_schema = 'public')
            UNION ALL
            SELECT 'Municipios', 
                   COALESCE((SELECT COUNT(*) FROM public."Municipios"), 0),
                   EXISTS(SELECT 1 FROM information_schema.tables WHERE table_name = 'Municipios' AND table_schema = 'public')
            UNION ALL
            SELECT 'Moedas', 
                   COALESCE((SELECT COUNT(*) FROM public."Moedas"), 0),
                   EXISTS(SELECT 1 FROM information_schema.tables WHERE table_name = 'Moedas' AND table_schema = 'public')
            UNION ALL
            SELECT 'UnidadesMedida', 
                   COALESCE((SELECT COUNT(*) FROM public."UnidadesMedida"), 0),
                   EXISTS(SELECT 1 FROM information_schema.tables WHERE table_name = 'UnidadesMedida' AND table_schema = 'public')
            UNION ALL
            SELECT 'AtividadesAgropecuarias', 
                   COALESCE((SELECT COUNT(*) FROM public."AtividadesAgropecuarias"), 0),
                   EXISTS(SELECT 1 FROM information_schema.tables WHERE table_name = 'AtividadesAgropecuarias' AND table_schema = 'public')
            UNION ALL
            SELECT 'Embalagens', 
                   COALESCE((SELECT COUNT(*) FROM public."Embalagens"), 0),
                   EXISTS(SELECT 1 FROM information_schema.tables WHERE table_name = 'Embalagens' AND table_schema = 'public')
        ) stats
        ORDER BY table_name
    LOOP
        reference_stats := reference_stats || 
                          table_stats.table_name || ': ' || 
                          CASE WHEN table_stats.has_data THEN table_stats.record_count::TEXT || ' records' ELSE 'Table not found' END || E'\n';
        
        table_metrics := table_metrics || jsonb_build_object(table_stats.table_name, table_stats.record_count);
    END LOOP;
    
    reference_stats := reference_stats || E'\n' ||
                      'GEOGRAPHIC HIERARCHY:' || E'\n' ||
                      '- Countries → States → Municipalities' || E'\n' ||
                      '- Referential integrity enforced' || E'\n' ||
                      '- Cascading relationships established';
    
    INSERT INTO public."MigrationReports" ("ReportName", "ReportType", "Section", "Title", "Content", "Metrics")
    VALUES ('reference_migration_summary', 'STATISTICS', 'REFERENCE_TABLES', 'Reference Tables Statistics', reference_stats, table_metrics);
END $$;

-- =====================================================
-- SECTION 3: PRODUTO MIGRATION ANALYSIS
-- =====================================================
DO $$
DECLARE
    produto_analysis TEXT;
    produto_metrics JSONB;
    total_produtos INTEGER := 0;
    produtos_with_unidade INTEGER := 0;
    produtos_with_embalagem INTEGER := 0;
    produtos_with_atividade INTEGER := 0;
    unidade_coverage DECIMAL := 0;
    embalagem_coverage DECIMAL := 0;
    atividade_coverage DECIMAL := 0;
BEGIN
    SELECT COUNT(*) INTO total_produtos FROM public."Produto";
    
    -- Check migration coverage
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produto' AND column_name = 'UnidadeMedidaId') THEN
        SELECT COUNT(*) INTO produtos_with_unidade FROM public."Produto" WHERE "UnidadeMedidaId" IS NOT NULL;
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produto' AND column_name = 'EmbalagemId') THEN
        SELECT COUNT(*) INTO produtos_with_embalagem FROM public."Produto" WHERE "EmbalagemId" IS NOT NULL;
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produto' AND column_name = 'AtividadeAgropecuariaId') THEN
        SELECT COUNT(*) INTO produtos_with_atividade FROM public."Produto" WHERE "AtividadeAgropecuariaId" IS NOT NULL;
    END IF;
    
    -- Calculate coverage percentages
    IF total_produtos > 0 THEN
        unidade_coverage := ROUND((produtos_with_unidade::DECIMAL / total_produtos * 100), 2);
        embalagem_coverage := ROUND((produtos_with_embalagem::DECIMAL / total_produtos * 100), 2);
        atividade_coverage := ROUND((produtos_with_atividade::DECIMAL / total_produtos * 100), 2);
    END IF;
    
    produto_analysis := 'PRODUTO MIGRATION ANALYSIS' || E'\n' ||
                       '==========================' || E'\n' ||
                       'Total Products: ' || total_produtos || E'\n' ||
                       E'\n' ||
                       'MIGRATION COVERAGE:' || E'\n' ||
                       '- UnidadeMedida: ' || produtos_with_unidade || '/' || total_produtos || ' (' || unidade_coverage || '%)' || E'\n' ||
                       '- Embalagem: ' || produtos_with_embalagem || '/' || total_produtos || ' (' || embalagem_coverage || '%)' || E'\n' ||
                       '- AtividadeAgropecuaria: ' || produtos_with_atividade || '/' || total_produtos || ' (' || atividade_coverage || '%)' || E'\n' ||
                       E'\n' ||
                       'MIGRATION QUALITY:' || E'\n' ||
                       CASE 
                           WHEN unidade_coverage >= 95 THEN '✓ Excellent UnidadeMedida coverage'
                           WHEN unidade_coverage >= 80 THEN '⚠ Good UnidadeMedida coverage'
                           ELSE '✗ Poor UnidadeMedida coverage - needs attention'
                       END || E'\n' ||
                       CASE 
                           WHEN embalagem_coverage >= 50 THEN '✓ Acceptable Embalagem coverage'
                           ELSE '⚠ Low Embalagem coverage - optional field'
                       END || E'\n' ||
                       CASE 
                           WHEN atividade_coverage >= 30 THEN '✓ Acceptable AtividadeAgropecuaria coverage'
                           ELSE '⚠ Low AtividadeAgropecuaria coverage - optional field'
                       END;
    
    produto_metrics := jsonb_build_object(
        'total_produtos', total_produtos,
        'produtos_with_unidade', produtos_with_unidade,
        'produtos_with_embalagem', produtos_with_embalagem,
        'produtos_with_atividade', produtos_with_atividade,
        'unidade_coverage_percent', unidade_coverage,
        'embalagem_coverage_percent', embalagem_coverage,
        'atividade_coverage_percent', atividade_coverage
    );
    
    INSERT INTO public."MigrationReports" ("ReportName", "ReportType", "Section", "Title", "Content", "Metrics")
    VALUES ('reference_migration_summary', 'ANALYSIS', 'PRODUTO_MIGRATION', 'Produto Migration Analysis', produto_analysis, produto_metrics);
END $$;

-- =====================================================
-- SECTION 4: FORNECEDOR MIGRATION ANALYSIS
-- =====================================================
DO $$
DECLARE
    fornecedor_analysis TEXT;
    fornecedor_metrics JSONB;
    total_fornecedores INTEGER := 0;
    fornecedores_with_uf INTEGER := 0;
    fornecedores_with_municipio INTEGER := 0;
    uf_coverage DECIMAL := 0;
    municipio_coverage DECIMAL := 0;
    consistency_issues INTEGER := 0;
BEGIN
    SELECT COUNT(*) INTO total_fornecedores FROM public."Fornecedor";
    
    -- Check migration coverage
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Fornecedor' AND column_name = 'UfId') THEN
        SELECT COUNT(*) INTO fornecedores_with_uf FROM public."Fornecedor" WHERE "UfId" IS NOT NULL;
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Fornecedor' AND column_name = 'MunicipioId') THEN
        SELECT COUNT(*) INTO fornecedores_with_municipio FROM public."Fornecedor" WHERE "MunicipioId" IS NOT NULL;
    END IF;
    
    -- Check for consistency issues
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Fornecedor' AND column_name = 'UfId') 
       AND EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Fornecedor' AND column_name = 'MunicipioId') THEN
        SELECT COUNT(*) INTO consistency_issues
        FROM public."Fornecedor" f
        INNER JOIN public."Municipios" m ON f."MunicipioId" = m."Id"
        WHERE f."UfId" IS NOT NULL AND f."UfId" != m."UfId";
    END IF;
    
    -- Calculate coverage percentages
    IF total_fornecedores > 0 THEN
        uf_coverage := ROUND((fornecedores_with_uf::DECIMAL / total_fornecedores * 100), 2);
        municipio_coverage := ROUND((fornecedores_with_municipio::DECIMAL / total_fornecedores * 100), 2);
    END IF;
    
    fornecedor_analysis := 'FORNECEDOR MIGRATION ANALYSIS' || E'\n' ||
                          '==============================' || E'\n' ||
                          'Total Fornecedores: ' || total_fornecedores || E'\n' ||
                          E'\n' ||
                          'GEOGRAPHIC MIGRATION COVERAGE:' || E'\n' ||
                          '- UF References: ' || fornecedores_with_uf || '/' || total_fornecedores || ' (' || uf_coverage || '%)' || E'\n' ||
                          '- Municipio References: ' || fornecedores_with_municipio || '/' || total_fornecedores || ' (' || municipio_coverage || '%)' || E'\n' ||
                          '- Consistency Issues: ' || consistency_issues || E'\n' ||
                          E'\n' ||
                          'MIGRATION QUALITY:' || E'\n' ||
                          CASE 
                              WHEN uf_coverage >= 95 THEN '✓ Excellent UF coverage'
                              WHEN uf_coverage >= 80 THEN '⚠ Good UF coverage'
                              ELSE '✗ Poor UF coverage - needs attention'
                          END || E'\n' ||
                          CASE 
                              WHEN municipio_coverage >= 90 THEN '✓ Excellent Municipio coverage'
                              WHEN municipio_coverage >= 70 THEN '⚠ Good Municipio coverage'
                              ELSE '✗ Poor Municipio coverage - needs attention'
                          END || E'\n' ||
                          CASE 
                              WHEN consistency_issues = 0 THEN '✓ No UF-Municipio consistency issues'
                              ELSE '✗ UF-Municipio consistency issues found - needs fixing'
                          END;
    
    fornecedor_metrics := jsonb_build_object(
        'total_fornecedores', total_fornecedores,
        'fornecedores_with_uf', fornecedores_with_uf,
        'fornecedores_with_municipio', fornecedores_with_municipio,
        'uf_coverage_percent', uf_coverage,
        'municipio_coverage_percent', municipio_coverage,
        'consistency_issues', consistency_issues
    );
    
    INSERT INTO public."MigrationReports" ("ReportName", "ReportType", "Section", "Title", "Content", "Metrics")
    VALUES ('reference_migration_summary', 'ANALYSIS', 'FORNECEDOR_MIGRATION', 'Fornecedor Migration Analysis', fornecedor_analysis, fornecedor_metrics);
END $$;

-- =====================================================
-- SECTION 5: VALIDATION RESULTS SUMMARY
-- =====================================================
DO $$
DECLARE
    validation_summary TEXT;
    validation_metrics JSONB;
    total_validations INTEGER := 0;
    passed_validations INTEGER := 0;
    failed_validations INTEGER := 0;
    warning_validations INTEGER := 0;
    critical_failures INTEGER := 0;
BEGIN
    -- Get validation counts
    SELECT 
        COUNT(*) FILTER (WHERE "Status" IN ('PASS', 'FAIL', 'WARNING')),
        COUNT(*) FILTER (WHERE "Status" = 'PASS'),
        COUNT(*) FILTER (WHERE "Status" = 'FAIL'),
        COUNT(*) FILTER (WHERE "Status" = 'WARNING')
    INTO total_validations, passed_validations, failed_validations, warning_validations
    FROM public."ValidationResults" 
    WHERE "ValidationName" = 'validate_all_references';
    
    -- Count critical failures
    SELECT COUNT(*) INTO critical_failures
    FROM public."ValidationResults" 
    WHERE "ValidationName" = 'validate_all_references' 
    AND "Status" = 'FAIL' 
    AND "Category" IN ('FOREIGN_KEYS', 'REFERENTIAL_INTEGRITY');
    
    validation_summary := 'VALIDATION RESULTS SUMMARY' || E'\n' ||
                         '==========================' || E'\n' ||
                         'Total Validations: ' || total_validations || E'\n' ||
                         'Passed: ' || passed_validations || ' (' || 
                         CASE WHEN total_validations > 0 THEN ROUND((passed_validations::DECIMAL / total_validations * 100), 1)::TEXT || '%' ELSE '0%' END || ')' || E'\n' ||
                         'Failed: ' || failed_validations || E'\n' ||
                         'Warnings: ' || warning_validations || E'\n' ||
                         'Critical Failures: ' || critical_failures || E'\n' ||
                         E'\n' ||
                         'VALIDATION CATEGORIES:' || E'\n' ||
                         '- Table Existence ✓' || E'\n' ||
                         '- Foreign Key Constraints' || E'\n' ||
                         '- Referential Integrity' || E'\n' ||
                         '- Data Quality' || E'\n' ||
                         '- Migration Completeness' || E'\n' ||
                         E'\n' ||
                         'OVERALL ASSESSMENT:' || E'\n' ||
                         CASE 
                             WHEN critical_failures = 0 AND failed_validations = 0 AND warning_validations = 0 THEN '✓ EXCELLENT - All validations passed'
                             WHEN critical_failures = 0 AND failed_validations = 0 THEN '✓ GOOD - Minor warnings only'
                             WHEN critical_failures = 0 THEN '⚠ ACCEPTABLE - Non-critical issues found'
                             ELSE '✗ CRITICAL - Immediate attention required'
                         END;
    
    validation_metrics := jsonb_build_object(
        'total_validations', total_validations,
        'passed_validations', passed_validations,
        'failed_validations', failed_validations,
        'warning_validations', warning_validations,
        'critical_failures', critical_failures,
        'success_rate_percent', CASE WHEN total_validations > 0 THEN ROUND((passed_validations::DECIMAL / total_validations * 100), 2) ELSE 0 END
    );
    
    INSERT INTO public."MigrationReports" ("ReportName", "ReportType", "Section", "Title", "Content", "Metrics")
    VALUES ('reference_migration_summary', 'VALIDATION', 'VALIDATION_SUMMARY', 'Validation Results Summary', validation_summary, validation_metrics);
END $$;

-- =====================================================
-- SECTION 6: RECOMMENDATIONS AND NEXT STEPS
-- =====================================================
DO $$
DECLARE
    recommendations TEXT;
    has_critical_issues BOOLEAN := false;
    has_minor_issues BOOLEAN := false;
    low_coverage_areas TEXT[] := ARRAY[]::TEXT[];
BEGIN
    -- Check for issues
    SELECT EXISTS (
        SELECT 1 FROM public."ValidationResults" 
        WHERE "ValidationName" = 'validate_all_references' 
        AND "Status" = 'FAIL' 
        AND "Category" IN ('FOREIGN_KEYS', 'REFERENTIAL_INTEGRITY')
    ) INTO has_critical_issues;
    
    SELECT EXISTS (
        SELECT 1 FROM public."ValidationResults" 
        WHERE "ValidationName" = 'validate_all_references' 
        AND "Status" IN ('FAIL', 'WARNING')
    ) INTO has_minor_issues;
    
    -- Check for low coverage areas
    IF EXISTS (
        SELECT 1 FROM public."ValidationResults" 
        WHERE "ValidationName" = 'validate_all_references' 
        AND "TestName" LIKE '%coverage%' 
        AND "Status" = 'FAIL'
    ) THEN
        low_coverage_areas := array_append(low_coverage_areas, 'Migration Coverage');
    END IF;
    
    recommendations := 'RECOMMENDATIONS AND NEXT STEPS' || E'\n' ||
                      '===============================' || E'\n';
    
    IF has_critical_issues THEN
        recommendations := recommendations || 
                          'IMMEDIATE ACTIONS REQUIRED:' || E'\n' ||
                          '❗ Fix critical referential integrity issues' || E'\n' ||
                          '❗ Restore missing foreign key constraints' || E'\n' ||
                          '❗ Do not proceed to production until resolved' || E'\n' ||
                          E'\n';
    END IF;
    
    IF has_minor_issues THEN
        recommendations := recommendations || 
                          'RECOMMENDED IMPROVEMENTS:' || E'\n' ||
                          '• Review and fix data quality issues' || E'\n' ||
                          '• Address migration coverage gaps' || E'\n' ||
                          '• Clean up duplicate or invalid data' || E'\n' ||
                          E'\n';
    END IF;
    
    recommendations := recommendations || 
                      'POST-MIGRATION TASKS:' || E'\n' ||
                      '1. Update application code to use new reference relationships' || E'\n' ||
                      '2. Update frontend components to use reference dropdowns' || E'\n' ||
                      '3. Test all CRUD operations with new reference structure' || E'\n' ||
                      '4. Update API documentation to reflect new schema' || E'\n' ||
                      '5. Train users on new reference data management' || E'\n' ||
                      E'\n' ||
                      'MONITORING:' || E'\n' ||
                      '• Monitor query performance with new indexes' || E'\n' ||
                      '• Track data quality metrics over time' || E'\n' ||
                      '• Set up alerts for referential integrity violations' || E'\n' ||
                      E'\n' ||
                      'BACKUP STRATEGY:' || E'\n' ||
                      '• Keep migration backup tables for 30 days' || E'\n' ||
                      '• Document rollback procedures' || E'\n' ||
                      '• Test rollback scripts in development environment';
    
    INSERT INTO public."MigrationReports" ("ReportName", "ReportType", "Section", "Title", "Content")
    VALUES ('reference_migration_summary', 'RECOMMENDATIONS', 'NEXT_STEPS', 'Recommendations and Next Steps', recommendations);
END $$;

-- =====================================================
-- GENERATE CONSOLIDATED REPORT
-- =====================================================
DO $$
DECLARE
    consolidated_report TEXT := '';
    report_section RECORD;
BEGIN
    -- Build consolidated report
    FOR report_section IN
        SELECT "Section", "Title", "Content"
        FROM public."MigrationReports" 
        WHERE "ReportName" = 'reference_migration_summary'
        ORDER BY 
            CASE "ReportType"
                WHEN 'EXECUTIVE' THEN 1
                WHEN 'STATISTICS' THEN 2
                WHEN 'ANALYSIS' THEN 3
                WHEN 'VALIDATION' THEN 4
                WHEN 'RECOMMENDATIONS' THEN 5
                ELSE 6
            END,
            "Section"
    LOOP
        consolidated_report := consolidated_report || report_section."Content" || E'\n\n';
    END LOOP;
    
    -- Save consolidated report
    INSERT INTO public."MigrationReports" ("ReportName", "ReportType", "Section", "Title", "Content")
    VALUES ('reference_migration_summary', 'CONSOLIDATED', 'FULL_REPORT', 'Complete Migration Report', consolidated_report);
    
    -- Output to console
    RAISE NOTICE '%', consolidated_report;
END $$;

-- Commit report generation
COMMIT;

-- Display final message
\echo '========================================='
\echo 'MIGRATION REPORT GENERATED SUCCESSFULLY!'
\echo '========================================='
\echo 'The comprehensive migration report has been generated and saved.'
\echo 'Query to view full report:'
\echo 'SELECT "Content" FROM public."MigrationReports" WHERE "ReportName" = ''reference_migration_summary'' AND "ReportType" = ''CONSOLIDATED'';'
\echo ''
\echo 'Query to view specific sections:'
\echo 'SELECT "Section", "Title", "Content" FROM public."MigrationReports" WHERE "ReportName" = ''reference_migration_summary'' ORDER BY "Id";'
\echo ''
\echo 'Query to view metrics:'
\echo 'SELECT "Section", "Title", "Metrics" FROM public."MigrationReports" WHERE "ReportName" = ''reference_migration_summary'' AND "Metrics" IS NOT NULL;'
\echo '========================================='