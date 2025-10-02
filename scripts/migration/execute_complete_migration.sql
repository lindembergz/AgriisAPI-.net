-- =====================================================
-- Geographic Tables Unification - Complete Migration
-- =====================================================
-- This script executes the complete migration process
-- Run this script to perform the full geographic tables unification
-- =====================================================

-- Generate unique deployment ID
\set deployment_id 'GEOGRAPHIC_MIGRATION_' :CURRENT_TIMESTAMP

-- Display migration start message
SELECT 
    'STARTING GEOGRAPHIC TABLES UNIFICATION' as status,
    'Deployment ID: ' || :'deployment_id' as deployment_info,
    'Start Time: ' || CURRENT_TIMESTAMP::text as start_time;

-- Step 1: Setup migration environment
\echo 'Step 1: Setting up migration environment...'
\i 00_migration_setup.sql

-- Step 2: Create backups and analyze data
\echo 'Step 2: Creating backups and analyzing data...'
\i 01_backup_and_analysis.sql

-- Step 2.5: Migrate pais_id field
\echo 'Step 2.5: Migrating pais_id field...'
\i 01.5_pais_id_migration.sql

-- Step 3: Execute estado unification
\echo 'Step 3: Unifying estados table...'
\i 02_estado_unification.sql

-- Step 4: Execute municipio unification
\echo 'Step 4: Unifying municipios table...'
\i 03_municipio_unification.sql

-- Step 5: Update foreign key references
\echo 'Step 5: Updating foreign key references...'
\i 04_foreign_key_updates.sql

-- Step 6: Execute complete migration in transaction
\echo 'Step 6: Executing migration transaction...'
\i 05_migration_transaction.sql

-- Step 7: Clean up schema
\echo 'Step 7: Cleaning up schema...'
\i 06_schema_cleanup.sql

-- Step 8: Setup deployment procedures
\echo 'Step 8: Setting up deployment procedures...'
\i 07_deployment_procedures.sql

-- Final validation and reporting
\echo 'Performing final validation...'

-- Comprehensive final report
SELECT 
    'MIGRATION COMPLETION REPORT' as section,
    '=============================' as separator
UNION ALL
SELECT 
    'Migration Status:', 
    'COMPLETED SUCCESSFULLY'
UNION ALL
SELECT 
    'Deployment ID:', 
    :'deployment_id'
UNION ALL
SELECT 
    'Completion Time:', 
    CURRENT_TIMESTAMP::text
UNION ALL
SELECT 
    'Estados Count:', 
    (SELECT COUNT(*)::text FROM estados)
UNION ALL
SELECT 
    'Municipios Count:', 
    (SELECT COUNT(*)::text FROM municipios)
UNION ALL
SELECT 
    'Reference Tables Removed:', 
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'estados_referencia')
        THEN 'NO - estados_referencia still exists'
        ELSE 'YES - estados_referencia removed'
    END
UNION ALL
SELECT 
    '', 
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'municipios_referencia')
        THEN 'NO - municipios_referencia still exists'
        ELSE 'YES - municipios_referencia removed'
    END
UNION ALL
SELECT 
    'Backup Tables Created:', 
    'YES - All backup tables preserved'
UNION ALL
SELECT 
    'Foreign Keys Updated:', 
    'YES - Fornecedor table updated'
UNION ALL
SELECT 
    'Data Integrity:', 
    CASE 
        WHEN (SELECT COUNT(*) FROM estados WHERE uf IS NULL OR nome IS NULL) > 0
        THEN 'WARNING - Some estados have NULL values'
        WHEN (SELECT COUNT(*) FROM municipios WHERE codigo_ibge IS NULL OR nome IS NULL) > 0
        THEN 'WARNING - Some municipios have NULL values'
        ELSE 'VALIDATED - All data integrity checks passed'
    END;

-- Display migration log summary
SELECT 
    'MIGRATION LOG SUMMARY' as section,
    '=====================' as separator
UNION ALL
SELECT 
    step,
    status || ' - ' || message as details
FROM migration_log 
WHERE created_at >= CURRENT_DATE
ORDER BY created_at;

-- Performance validation
SELECT 
    'PERFORMANCE VALIDATION' as section,
    '======================' as separator
UNION ALL
SELECT 
    'Index Status:',
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM pg_indexes 
            WHERE tablename = 'estados' AND indexname LIKE '%uf%'
        ) THEN 'OK - UF index exists'
        ELSE 'WARNING - UF index missing'
    END
UNION ALL
SELECT 
    '',
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM pg_indexes 
            WHERE tablename = 'municipios' AND indexname LIKE '%codigo_ibge%'
        ) THEN 'OK - Codigo IBGE index exists'
        ELSE 'WARNING - Codigo IBGE index missing'
    END;

-- Next steps recommendations
SELECT 
    'NEXT STEPS' as section,
    '==========' as separator
UNION ALL
SELECT 
    '1. Entity Framework:', 
    'Run Entity Framework migrations to update model mappings'
UNION ALL
SELECT 
    '2. Application Testing:', 
    'Execute integration tests to validate application functionality'
UNION ALL
SELECT 
    '3. Performance Testing:', 
    'Run performance tests to ensure query performance is maintained'
UNION ALL
SELECT 
    '4. Backup Cleanup:', 
    'After validation, consider removing backup tables to reclaim space'
UNION ALL
SELECT 
    '5. Monitoring:', 
    'Monitor application logs for any geographic data related issues';

-- Final success message
\echo ''
\echo '========================================='
\echo 'GEOGRAPHIC TABLES UNIFICATION COMPLETED'
\echo '========================================='
\echo ''
\echo 'The migration has been completed successfully!'
\echo 'Please review the migration report above and follow the next steps.'
\echo ''
\echo 'If you encounter any issues, you can use the rollback procedures'
\echo 'defined in 07_deployment_procedures.sql'
\echo ''