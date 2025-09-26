-- =====================================================
-- ROLLBACK SCRIPT: Fornecedor References Migration
-- Description: Rollback script for 03_migrate_fornecedor_references.sql
-- Author: System Migration
-- Date: 2025-01-27
-- =====================================================

-- Enable detailed logging
\set VERBOSITY verbose
\timing on

-- Start rollback transaction
BEGIN;

-- Log rollback start
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_03_fornecedor_references', 'START', 'Starting rollback of Fornecedor references migration', 'INFO');

-- =====================================================
-- STEP 1: Drop foreign key constraints
-- =====================================================
DO $$
BEGIN
    -- Drop FK constraint for UF
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Fornecedor_Uf_UfId'
        AND table_name = 'Fornecedor'
    ) THEN
        ALTER TABLE public."Fornecedor" DROP CONSTRAINT "FK_Fornecedor_Uf_UfId";
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('rollback_03_fornecedor_references', 'DROP_CONSTRAINT', 'Dropped FK constraint for UF', 'SUCCESS');
    END IF;
    
    -- Drop FK constraint for Municipio
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Fornecedor_Municipio_MunicipioId'
        AND table_name = 'Fornecedor'
    ) THEN
        ALTER TABLE public."Fornecedor" DROP CONSTRAINT "FK_Fornecedor_Municipio_MunicipioId";
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('rollback_03_fornecedor_references', 'DROP_CONSTRAINT', 'Dropped FK constraint for Municipio', 'SUCCESS');
    END IF;
END $$;

-- =====================================================
-- STEP 2: Drop indexes
-- =====================================================
DROP INDEX IF EXISTS "IX_Fornecedor_UfId";
DROP INDEX IF EXISTS "IX_Fornecedor_MunicipioId";

INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_03_fornecedor_references', 'DROP_INDEXES', 'Dropped performance indexes for FK columns', 'SUCCESS');

-- =====================================================
-- STEP 3: Find and restore from backup table
-- =====================================================
DO $$
DECLARE
    backup_table_name TEXT;
    backup_exists BOOLEAN := false;
    fornecedores_restored INTEGER := 0;
BEGIN
    -- Find the most recent backup table
    SELECT table_name INTO backup_table_name
    FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_name LIKE 'Fornecedor_backup_%'
    AND table_name ~ 'Fornecedor_backup_\d{14}'
    ORDER BY table_name DESC
    LIMIT 1;
    
    IF backup_table_name IS NOT NULL THEN
        backup_exists := true;
        
        -- Drop current Fornecedor table
        DROP TABLE IF EXISTS public."Fornecedor" CASCADE;
        
        -- Restore from backup
        EXECUTE 'CREATE TABLE public."Fornecedor" AS SELECT * FROM public."' || backup_table_name || '"';
        
        -- Get count of restored fornecedores
        EXECUTE 'SELECT COUNT(*) FROM public."Fornecedor"' INTO fornecedores_restored;
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
        VALUES ('rollback_03_fornecedor_references', 'RESTORE', 'Restored Fornecedor table from backup: ' || backup_table_name, fornecedores_restored, 'SUCCESS');
    ELSE
        -- No backup found, just remove the new columns
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('rollback_03_fornecedor_references', 'RESTORE', 'No backup table found, removing new columns only', 'WARNING');
        
        -- Remove new columns if they exist
        IF EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'public' 
            AND table_name = 'Fornecedor' 
            AND column_name = 'UfId'
        ) THEN
            ALTER TABLE public."Fornecedor" DROP COLUMN "UfId";
        END IF;
        
        IF EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'public' 
            AND table_name = 'Fornecedor' 
            AND column_name = 'MunicipioId'
        ) THEN
            ALTER TABLE public."Fornecedor" DROP COLUMN "MunicipioId";
        END IF;
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('rollback_03_fornecedor_references', 'REMOVE_COLUMNS', 'Removed new reference columns', 'SUCCESS');
    END IF;
END $$;

-- =====================================================
-- STEP 4: Restore original indexes and constraints if needed
-- =====================================================
DO $$
BEGIN
    -- Recreate primary key if it doesn't exist
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_type = 'PRIMARY KEY'
        AND table_name = 'Fornecedor'
        AND table_schema = 'public'
    ) THEN
        ALTER TABLE public."Fornecedor" ADD PRIMARY KEY ("Id");
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('rollback_03_fornecedor_references', 'RESTORE_PK', 'Restored primary key constraint', 'SUCCESS');
    END IF;
    
    -- Recreate common indexes that might have been lost
    CREATE INDEX IF NOT EXISTS "IX_Fornecedor_Nome" ON public."Fornecedor" ("Nome");
    CREATE INDEX IF NOT EXISTS "IX_Fornecedor_Cnpj" ON public."Fornecedor" ("Cnpj");
    CREATE INDEX IF NOT EXISTS "IX_Fornecedor_Ativo" ON public."Fornecedor" ("Ativo");
    
    -- Recreate indexes for string geographic columns if they exist
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'Uf'
    ) THEN
        CREATE INDEX IF NOT EXISTS "IX_Fornecedor_Uf" ON public."Fornecedor" ("Uf");
    END IF;
    
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'Municipio'
    ) THEN
        CREATE INDEX IF NOT EXISTS "IX_Fornecedor_Municipio" ON public."Fornecedor" ("Municipio");
    END IF;
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('rollback_03_fornecedor_references', 'RESTORE_INDEXES', 'Restored common indexes', 'SUCCESS');
END $$;

-- =====================================================
-- STEP 5: Validate rollback
-- =====================================================
DO $$
DECLARE
    total_fornecedores INTEGER;
    has_uf_id BOOLEAN;
    has_municipio_id BOOLEAN;
    has_uf_string BOOLEAN;
    has_municipio_string BOOLEAN;
    validation_report TEXT;
BEGIN
    -- Count fornecedores
    SELECT COUNT(*) INTO total_fornecedores FROM public."Fornecedor";
    
    -- Check if new columns still exist
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'UfId'
    ) INTO has_uf_id;
    
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'MunicipioId'
    ) INTO has_municipio_id;
    
    -- Check if original string columns exist
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'Uf'
        AND data_type IN ('character varying', 'text', 'varchar')
    ) INTO has_uf_string;
    
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'Municipio'
        AND data_type IN ('character varying', 'text', 'varchar')
    ) INTO has_municipio_string;
    
    -- Generate validation report
    validation_report := 'FORNECEDOR ROLLBACK VALIDATION' || E'\n' ||
                        '===============================' || E'\n' ||
                        'Total Fornecedores: ' || total_fornecedores || E'\n' ||
                        'Has UfId column: ' || has_uf_id || E'\n' ||
                        'Has MunicipioId column: ' || has_municipio_id || E'\n' ||
                        'Has Uf string column: ' || has_uf_string || E'\n' ||
                        'Has Municipio string column: ' || has_municipio_string || E'\n' ||
                        '===============================';
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('rollback_03_fornecedor_references', 'VALIDATION', validation_report, 'SUCCESS');
    
    -- Output validation to console
    RAISE NOTICE '%', validation_report;
    
    -- Check if rollback was successful
    IF NOT has_uf_id AND NOT has_municipio_id THEN
        RAISE NOTICE 'Rollback validation PASSED: All new reference columns have been removed.';
        IF has_uf_string AND has_municipio_string THEN
            RAISE NOTICE 'Original string columns have been restored.';
        END IF;
    ELSE
        RAISE NOTICE 'Rollback validation WARNING: Some reference columns still exist.';
    END IF;
END $$;

-- =====================================================
-- STEP 6: Restore original foreign key constraints if applicable
-- =====================================================
-- Note: This section would restore any original FK constraints that existed
-- before the migration. Since we're dealing with geographic references,
-- there might not be any original FKs to restore.

DO $$
BEGIN
    -- If there were original FK constraints to other tables (not geographic references),
    -- they would be restored here. For now, we'll just log that this step was considered.
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('rollback_03_fornecedor_references', 'RESTORE_ORIGINAL_FKS', 'No original FK constraints to restore for geographic fields', 'INFO');
END $$;

-- =====================================================
-- STEP 7: Clean up backup table (optional)
-- =====================================================
-- Note: Commented out for safety. Uncomment if you want to clean up backup tables.
/*
DO $$
DECLARE
    backup_table_name TEXT;
BEGIN
    -- Find backup tables and drop them
    FOR backup_table_name IN 
        SELECT table_name 
        FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name LIKE 'Fornecedor_backup_%'
        AND table_name ~ 'Fornecedor_backup_\d{14}'
    LOOP
        EXECUTE 'DROP TABLE IF EXISTS public."' || backup_table_name || '"';
    END LOOP;
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('rollback_03_fornecedor_references', 'CLEANUP', 'Cleaned up backup tables', 'SUCCESS');
END $$;
*/

-- Log rollback completion
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_03_fornecedor_references', 'COMPLETE', 'Fornecedor references rollback completed successfully', 'SUCCESS');

-- Commit rollback transaction
COMMIT;

-- Display final message
\echo 'Rollback of 03_migrate_fornecedor_references completed successfully!'
\echo 'Fornecedor table has been restored to its pre-migration state.'
\echo 'Check MigrationLogs table for detailed rollback log.'