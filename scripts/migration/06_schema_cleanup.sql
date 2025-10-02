-- =====================================================
-- Geographic Tables Unification - Schema Cleanup
-- =====================================================
-- This script removes duplicate tables and performs final cleanup
-- =====================================================

-- Log start of schema cleanup
INSERT INTO migration_log (step, status, message) 
VALUES ('schema_cleanup', 'started', 'Starting schema cleanup process');

-- Step 1: Final validation before cleanup
-- =====================================================

-- Verify no remaining references to reference tables
DO $$
DECLARE
    remaining_refs INTEGER := 0;
    error_message TEXT := '';
BEGIN
    -- Check for any remaining foreign key references to estados_referencia
    SELECT COUNT(*) INTO remaining_refs
    FROM information_schema.key_column_usage kcu
    JOIN information_schema.table_constraints tc ON kcu.constraint_name = tc.constraint_name
    WHERE tc.constraint_type = 'FOREIGN KEY'
      AND kcu.referenced_table_name = 'estados_referencia';
    
    IF remaining_refs > 0 THEN
        error_message := FORMAT('Found %s remaining FK references to estados_referencia', remaining_refs);
        INSERT INTO migration_log (step, status, message, error_details) 
        VALUES ('cleanup_validation', 'error', 'Cleanup validation failed', error_message);
        RAISE EXCEPTION '%', error_message;
    END IF;
    
    -- Check for any remaining foreign key references to municipios_referencia
    SELECT COUNT(*) INTO remaining_refs
    FROM information_schema.key_column_usage kcu
    JOIN information_schema.table_constraints tc ON kcu.constraint_name = tc.constraint_name
    WHERE tc.constraint_type = 'FOREIGN KEY'
      AND kcu.referenced_table_name = 'municipios_referencia';
    
    IF remaining_refs > 0 THEN
        error_message := FORMAT('Found %s remaining FK references to municipios_referencia', remaining_refs);
        INSERT INTO migration_log (step, status, message, error_details) 
        VALUES ('cleanup_validation', 'error', 'Cleanup validation failed', error_message);
        RAISE EXCEPTION '%', error_message;
    END IF;
    
    INSERT INTO migration_log (step, status, message) 
    VALUES ('cleanup_validation', 'success', 'No remaining references to reference tables found');
END $$;

-- Step 2: Drop reference tables
-- =====================================================

-- Drop municipios_referencia table
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'municipios_referencia') THEN
        DROP TABLE municipios_referencia CASCADE;
        INSERT INTO migration_log (step, status, message) 
        VALUES ('drop_municipios_referencia', 'completed', 'municipios_referencia table dropped successfully');
    ELSE
        INSERT INTO migration_log (step, status, message) 
        VALUES ('drop_municipios_referencia', 'skipped', 'municipios_referencia table does not exist');
    END IF;
END $$;

-- Drop estados_referencia table
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'estados_referencia') THEN
        DROP TABLE estados_referencia CASCADE;
        INSERT INTO migration_log (step, status, message) 
        VALUES ('drop_estados_referencia', 'completed', 'estados_referencia table dropped successfully');
    ELSE
        INSERT INTO migration_log (step, status, message) 
        VALUES ('drop_estados_referencia', 'skipped', 'estados_referencia table does not exist');
    END IF;
END $$;

-- Step 3: Clean up backup tables (optional - keep for safety)
-- =====================================================

-- Log backup table status
INSERT INTO migration_log (step, status, message) 
VALUES ('backup_tables_status', 'info', 
        FORMAT('Backup tables preserved: estados_backup (%s rows), municipios_backup (%s rows), estados_referencia_backup (%s rows), municipios_referencia_backup (%s rows)',
               (SELECT COUNT(*) FROM estados_backup),
               (SELECT COUNT(*) FROM municipios_backup),
               (SELECT COUNT(*) FROM estados_referencia_backup),
               (SELECT COUNT(*) FROM municipios_referencia_backup)));

-- Step 4: Optimize unified tables
-- =====================================================

-- Update table statistics for better query planning
ANALYZE estados;
ANALYZE municipios;

-- Vacuum tables to reclaim space and update statistics
VACUUM ANALYZE estados;
VACUUM ANALYZE municipios;

INSERT INTO migration_log (step, status, message) 
VALUES ('table_optimization', 'completed', 'Table statistics updated and space reclaimed');

-- Step 5: Verify final schema state
-- =====================================================

-- Verify unified tables exist and have expected structure
DO $$
DECLARE
    estados_count INTEGER;
    municipios_count INTEGER;
    estados_columns INTEGER;
    municipios_columns INTEGER;
BEGIN
    -- Check table counts
    SELECT COUNT(*) INTO estados_count FROM estados;
    SELECT COUNT(*) INTO municipios_count FROM municipios;
    
    -- Check table structure
    SELECT COUNT(*) INTO estados_columns 
    FROM information_schema.columns 
    WHERE table_name = 'estados' AND table_schema = 'public';
    
    SELECT COUNT(*) INTO municipios_columns 
    FROM information_schema.columns 
    WHERE table_name = 'municipios' AND table_schema = 'public';
    
    -- Validate counts
    IF estados_count < 27 THEN
        RAISE EXCEPTION 'Estados table has insufficient records: %', estados_count;
    END IF;
    
    IF municipios_count < 5000 THEN
        RAISE EXCEPTION 'Municipios table has insufficient records: %', municipios_count;
    END IF;
    
    -- Validate structure
    IF estados_columns < 7 THEN
        RAISE EXCEPTION 'Estados table has insufficient columns: %', estados_columns;
    END IF;
    
    IF municipios_columns < 9 THEN
        RAISE EXCEPTION 'Municipios table has insufficient columns: %', municipios_columns;
    END IF;
    
    INSERT INTO migration_log (step, status, message) 
    VALUES ('final_validation', 'success', 
            FORMAT('Final validation passed: Estados=%s rows/%s cols, Municipios=%s rows/%s cols',
                   estados_count, estados_columns, municipios_count, municipios_columns));
END $$;

-- Step 6: Verify foreign key constraints
-- =====================================================

-- Check that all foreign key constraints are working
DO $$
DECLARE
    fk_count INTEGER;
BEGIN
    -- Count foreign key constraints on unified tables
    SELECT COUNT(*) INTO fk_count
    FROM information_schema.table_constraints tc
    WHERE tc.constraint_type = 'FOREIGN KEY'
      AND tc.table_name IN ('estados', 'municipios', 'Fornecedor', 'enderecos')
      AND tc.constraint_name LIKE '%estado%' OR tc.constraint_name LIKE '%municipio%';
    
    IF fk_count < 4 THEN
        RAISE EXCEPTION 'Insufficient foreign key constraints found: %', fk_count;
    END IF;
    
    INSERT INTO migration_log (step, status, message) 
    VALUES ('fk_constraints_validation', 'success', 
            FORMAT('Foreign key constraints validated: %s constraints found', fk_count));
END $$;

-- Step 7: Create final migration summary
-- =====================================================

-- Generate comprehensive migration report
SELECT 
    'MIGRATION SUMMARY REPORT' as section,
    '' as detail
UNION ALL
SELECT 
    '========================',
    ''
UNION ALL
SELECT 
    'Tables Processed:',
    'estados, municipios, estados_referencia, municipios_referencia'
UNION ALL
SELECT 
    'Tables Dropped:',
    'estados_referencia, municipios_referencia'
UNION ALL
SELECT 
    'Tables Preserved:',
    'estados, municipios'
UNION ALL
SELECT 
    'Backup Tables Created:',
    'estados_backup, municipios_backup, estados_referencia_backup, municipios_referencia_backup'
UNION ALL
SELECT 
    'Final Estados Count:',
    (SELECT COUNT(*)::text FROM estados)
UNION ALL
SELECT 
    'Final Municipios Count:',
    (SELECT COUNT(*)::text FROM municipios)
UNION ALL
SELECT 
    'Foreign Keys Updated:',
    'Fornecedor.UfId, Fornecedor.MunicipioId'
UNION ALL
SELECT 
    'Migration Status:',
    'COMPLETED SUCCESSFULLY'
UNION ALL
SELECT 
    'Migration Date:',
    CURRENT_TIMESTAMP::text;

-- Log completion of schema cleanup
INSERT INTO migration_log (step, status, message) 
VALUES ('schema_cleanup', 'completed', 'Schema cleanup completed successfully');

-- Final success message
SELECT 
    'SCHEMA CLEANUP COMPLETED' as status,
    'Geographic tables have been successfully unified' as message,
    'Reference tables have been removed' as action_taken,
    'Backup tables preserved for safety' as safety_note;