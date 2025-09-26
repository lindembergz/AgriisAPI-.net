-- =====================================================
-- ROLLBACK SCRIPT: Produto References Migration
-- Description: Rollback script for 02_migrate_produto_references.sql
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
VALUES ('rollback_02_produto_references', 'START', 'Starting rollback of Produto references migration', 'INFO');

-- =====================================================
-- STEP 1: Drop foreign key constraints
-- =====================================================
DO $$
BEGIN
    -- Drop FK constraint for UnidadeMedida
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Produto_UnidadeMedida_UnidadeMedidaId'
        AND table_name = 'Produto'
    ) THEN
        ALTER TABLE public."Produto" DROP CONSTRAINT "FK_Produto_UnidadeMedida_UnidadeMedidaId";
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('rollback_02_produto_references', 'DROP_CONSTRAINT', 'Dropped FK constraint for UnidadeMedida', 'SUCCESS');
    END IF;
    
    -- Drop FK constraint for Embalagem
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Produto_Embalagem_EmbalagemId'
        AND table_name = 'Produto'
    ) THEN
        ALTER TABLE public."Produto" DROP CONSTRAINT "FK_Produto_Embalagem_EmbalagemId";
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('rollback_02_produto_references', 'DROP_CONSTRAINT', 'Dropped FK constraint for Embalagem', 'SUCCESS');
    END IF;
    
    -- Drop FK constraint for AtividadeAgropecuaria
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Produto_AtividadeAgropecuaria_AtividadeAgropecuariaId'
        AND table_name = 'Produto'
    ) THEN
        ALTER TABLE public."Produto" DROP CONSTRAINT "FK_Produto_AtividadeAgropecuaria_AtividadeAgropecuariaId";
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('rollback_02_produto_references', 'DROP_CONSTRAINT', 'Dropped FK constraint for AtividadeAgropecuaria', 'SUCCESS');
    END IF;
END $$;

-- =====================================================
-- STEP 2: Drop indexes
-- =====================================================
DROP INDEX IF EXISTS "IX_Produto_UnidadeMedidaId";
DROP INDEX IF EXISTS "IX_Produto_EmbalagemId";
DROP INDEX IF EXISTS "IX_Produto_AtividadeAgropecuariaId";

INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_02_produto_references', 'DROP_INDEXES', 'Dropped performance indexes for FK columns', 'SUCCESS');

-- =====================================================
-- STEP 3: Find and restore from backup table
-- =====================================================
DO $$
DECLARE
    backup_table_name TEXT;
    backup_exists BOOLEAN := false;
    produtos_restored INTEGER := 0;
BEGIN
    -- Find the most recent backup table
    SELECT table_name INTO backup_table_name
    FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_name LIKE 'Produto_backup_%'
    AND table_name ~ 'Produto_backup_\d{14}'
    ORDER BY table_name DESC
    LIMIT 1;
    
    IF backup_table_name IS NOT NULL THEN
        backup_exists := true;
        
        -- Drop current Produto table
        DROP TABLE IF EXISTS public."Produto" CASCADE;
        
        -- Restore from backup
        EXECUTE 'CREATE TABLE public."Produto" AS SELECT * FROM public."' || backup_table_name || '"';
        
        -- Get count of restored products
        EXECUTE 'SELECT COUNT(*) FROM public."Produto"' INTO produtos_restored;
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
        VALUES ('rollback_02_produto_references', 'RESTORE', 'Restored Produto table from backup: ' || backup_table_name, produtos_restored, 'SUCCESS');
    ELSE
        -- No backup found, just remove the new columns
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('rollback_02_produto_references', 'RESTORE', 'No backup table found, removing new columns only', 'WARNING');
        
        -- Remove new columns if they exist
        IF EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'public' 
            AND table_name = 'Produto' 
            AND column_name = 'UnidadeMedidaId'
        ) THEN
            ALTER TABLE public."Produto" DROP COLUMN "UnidadeMedidaId";
        END IF;
        
        IF EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'public' 
            AND table_name = 'Produto' 
            AND column_name = 'EmbalagemId'
        ) THEN
            ALTER TABLE public."Produto" DROP COLUMN "EmbalagemId";
        END IF;
        
        IF EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'public' 
            AND table_name = 'Produto' 
            AND column_name = 'AtividadeAgropecuariaId'
        ) THEN
            ALTER TABLE public."Produto" DROP COLUMN "AtividadeAgropecuariaId";
        END IF;
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('rollback_02_produto_references', 'REMOVE_COLUMNS', 'Removed new reference columns', 'SUCCESS');
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
        AND table_name = 'Produto'
        AND table_schema = 'public'
    ) THEN
        ALTER TABLE public."Produto" ADD PRIMARY KEY ("Id");
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('rollback_02_produto_references', 'RESTORE_PK', 'Restored primary key constraint', 'SUCCESS');
    END IF;
    
    -- Recreate common indexes that might have been lost
    CREATE INDEX IF NOT EXISTS "IX_Produto_Nome" ON public."Produto" ("Nome");
    CREATE INDEX IF NOT EXISTS "IX_Produto_Codigo" ON public."Produto" ("Codigo");
    CREATE INDEX IF NOT EXISTS "IX_Produto_CategoriaId" ON public."Produto" ("CategoriaId");
    CREATE INDEX IF NOT EXISTS "IX_Produto_FornecedorId" ON public."Produto" ("FornecedorId");
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('rollback_02_produto_references', 'RESTORE_INDEXES', 'Restored common indexes', 'SUCCESS');
END $$;

-- =====================================================
-- STEP 5: Validate rollback
-- =====================================================
DO $$
DECLARE
    total_produtos INTEGER;
    has_unidade_medida_id BOOLEAN;
    has_embalagem_id BOOLEAN;
    has_atividade_agropecuaria_id BOOLEAN;
    validation_report TEXT;
BEGIN
    -- Count products
    SELECT COUNT(*) INTO total_produtos FROM public."Produto";
    
    -- Check if new columns still exist
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Produto' 
        AND column_name = 'UnidadeMedidaId'
    ) INTO has_unidade_medida_id;
    
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Produto' 
        AND column_name = 'EmbalagemId'
    ) INTO has_embalagem_id;
    
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Produto' 
        AND column_name = 'AtividadeAgropecuariaId'
    ) INTO has_atividade_agropecuaria_id;
    
    -- Generate validation report
    validation_report := 'PRODUTO ROLLBACK VALIDATION' || E'\n' ||
                        '===========================' || E'\n' ||
                        'Total Products: ' || total_produtos || E'\n' ||
                        'Has UnidadeMedidaId column: ' || has_unidade_medida_id || E'\n' ||
                        'Has EmbalagemId column: ' || has_embalagem_id || E'\n' ||
                        'Has AtividadeAgropecuariaId column: ' || has_atividade_agropecuaria_id || E'\n' ||
                        '===========================';
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('rollback_02_produto_references', 'VALIDATION', validation_report, 'SUCCESS');
    
    -- Output validation to console
    RAISE NOTICE '%', validation_report;
    
    -- Check if rollback was successful
    IF NOT has_unidade_medida_id AND NOT has_embalagem_id AND NOT has_atividade_agropecuaria_id THEN
        RAISE NOTICE 'Rollback validation PASSED: All new reference columns have been removed.';
    ELSE
        RAISE NOTICE 'Rollback validation WARNING: Some reference columns still exist.';
    END IF;
END $$;

-- =====================================================
-- STEP 6: Clean up backup table (optional)
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
        AND table_name LIKE 'Produto_backup_%'
        AND table_name ~ 'Produto_backup_\d{14}'
    LOOP
        EXECUTE 'DROP TABLE IF EXISTS public."' || backup_table_name || '"';
    END LOOP;
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('rollback_02_produto_references', 'CLEANUP', 'Cleaned up backup tables', 'SUCCESS');
END $$;
*/

-- Log rollback completion
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_02_produto_references', 'COMPLETE', 'Produto references rollback completed successfully', 'SUCCESS');

-- Commit rollback transaction
COMMIT;

-- Display final message
\echo 'Rollback of 02_migrate_produto_references completed successfully!'
\echo 'Produto table has been restored to its pre-migration state.'
\echo 'Check MigrationLogs table for detailed rollback log.'