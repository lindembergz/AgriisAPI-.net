-- =====================================================
-- ROLLBACK SCRIPT: Reference Tables Population
-- Description: Rollback script for 01_populate_reference_tables.sql
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
VALUES ('rollback_01_reference_tables', 'START', 'Starting rollback of reference tables population', 'INFO');

-- =====================================================
-- STEP 1: Drop foreign key constraints first
-- =====================================================
DO $$
BEGIN
    -- Drop FK constraints from Embalagens
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Embalagens_UnidadesMedida_UnidadeMedidaId'
        AND table_name = 'Embalagens'
    ) THEN
        ALTER TABLE public."Embalagens" DROP CONSTRAINT "FK_Embalagens_UnidadesMedida_UnidadeMedidaId";
    END IF;
    
    -- Drop FK constraints from Municipios
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Municipios_Ufs_UfId'
        AND table_name = 'Municipios'
    ) THEN
        ALTER TABLE public."Municipios" DROP CONSTRAINT "FK_Municipios_Ufs_UfId";
    END IF;
    
    -- Drop FK constraints from Ufs
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Ufs_Paises_PaisId'
        AND table_name = 'Ufs'
    ) THEN
        ALTER TABLE public."Ufs" DROP CONSTRAINT "FK_Ufs_Paises_PaisId";
    END IF;
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('rollback_01_reference_tables', 'DROP_CONSTRAINTS', 'Dropped foreign key constraints', 'SUCCESS');
END $$;

-- =====================================================
-- STEP 2: Drop reference tables in reverse order
-- =====================================================

-- Drop Embalagens table
DROP TABLE IF EXISTS public."Embalagens" CASCADE;
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_01_reference_tables', 'DROP_TABLE', 'Dropped Embalagens table', 'SUCCESS');

-- Drop AtividadesAgropecuarias table
DROP TABLE IF EXISTS public."AtividadesAgropecuarias" CASCADE;
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_01_reference_tables', 'DROP_TABLE', 'Dropped AtividadesAgropecuarias table', 'SUCCESS');

-- Drop UnidadesMedida table
DROP TABLE IF EXISTS public."UnidadesMedida" CASCADE;
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_01_reference_tables', 'DROP_TABLE', 'Dropped UnidadesMedida table', 'SUCCESS');

-- Drop Moedas table
DROP TABLE IF EXISTS public."Moedas" CASCADE;
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_01_reference_tables', 'DROP_TABLE', 'Dropped Moedas table', 'SUCCESS');

-- Drop Municipios table
DROP TABLE IF EXISTS public."Municipios" CASCADE;
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_01_reference_tables', 'DROP_TABLE', 'Dropped Municipios table', 'SUCCESS');

-- Drop Ufs table
DROP TABLE IF EXISTS public."Ufs" CASCADE;
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_01_reference_tables', 'DROP_TABLE', 'Dropped Ufs table', 'SUCCESS');

-- Drop Paises table
DROP TABLE IF EXISTS public."Paises" CASCADE;
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_01_reference_tables', 'DROP_TABLE', 'Dropped Paises table', 'SUCCESS');

-- =====================================================
-- STEP 3: Restore from backup tables if they exist
-- =====================================================
DO $$
DECLARE
    backup_table RECORD;
    restore_count INTEGER := 0;
BEGIN
    -- Look for backup tables and restore them
    FOR backup_table IN 
        SELECT table_name 
        FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name LIKE '%_backup_%'
        AND table_name ~ '(Moedas|Paises|Ufs|Municipios|UnidadesMedida|AtividadesAgropecuarias|Embalagens)_backup_\d{14}'
    LOOP
        -- Extract original table name
        DECLARE
            original_table_name TEXT;
        BEGIN
            original_table_name := split_part(backup_table.table_name, '_backup_', 1);
            
            -- Restore table
            EXECUTE 'CREATE TABLE public."' || original_table_name || '" AS SELECT * FROM public."' || backup_table.table_name || '"';
            
            restore_count := restore_count + 1;
            
            INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
            VALUES ('rollback_01_reference_tables', 'RESTORE', 'Restored ' || original_table_name || ' from backup', 'SUCCESS');
        END;
    END LOOP;
    
    IF restore_count = 0 THEN
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('rollback_01_reference_tables', 'RESTORE', 'No backup tables found to restore', 'INFO');
    ELSE
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
        VALUES ('rollback_01_reference_tables', 'RESTORE', 'Restored tables from backup', restore_count, 'SUCCESS');
    END IF;
END $$;

-- =====================================================
-- STEP 4: Clean up backup tables (optional)
-- =====================================================
-- Note: Commented out for safety. Uncomment if you want to clean up backup tables.
/*
DO $$
DECLARE
    backup_table RECORD;
BEGIN
    FOR backup_table IN 
        SELECT table_name 
        FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name LIKE '%_backup_%'
        AND table_name ~ '(Moedas|Paises|Ufs|Municipios|UnidadesMedida|AtividadesAgropecuarias|Embalagens)_backup_\d{14}'
    LOOP
        EXECUTE 'DROP TABLE IF EXISTS public."' || backup_table.table_name || '"';
    END LOOP;
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('rollback_01_reference_tables', 'CLEANUP', 'Cleaned up backup tables', 'SUCCESS');
END $$;
*/

-- Log rollback completion
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_01_reference_tables', 'COMPLETE', 'Reference tables rollback completed successfully', 'SUCCESS');

-- Commit rollback transaction
COMMIT;

-- Display final message
\echo 'Rollback of 01_populate_reference_tables completed successfully!'
\echo 'All reference tables have been dropped and restored from backup if available.'
\echo 'Check MigrationLogs table for detailed rollback log.'