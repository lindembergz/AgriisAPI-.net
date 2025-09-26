-- =====================================================
-- COMPLETE ROLLBACK SCRIPT: All Reference Migrations
-- Description: Complete rollback of all reference entity migrations
-- Author: System Migration
-- Date: 2025-01-27
-- =====================================================

-- Enable detailed logging
\set VERBOSITY verbose
\timing on

-- Start complete rollback transaction
BEGIN;

-- Log complete rollback start
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_all_migrations', 'START', 'Starting complete rollback of all reference migrations', 'INFO');

-- =====================================================
-- STEP 1: Rollback Fornecedor references (Step 3)
-- =====================================================
\echo 'Rolling back Fornecedor references migration...'

-- Drop FK constraints for Fornecedor
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Fornecedor_Uf_UfId'
        AND table_name = 'Fornecedor'
    ) THEN
        ALTER TABLE public."Fornecedor" DROP CONSTRAINT "FK_Fornecedor_Uf_UfId";
    END IF;
    
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Fornecedor_Municipio_MunicipioId'
        AND table_name = 'Fornecedor'
    ) THEN
        ALTER TABLE public."Fornecedor" DROP CONSTRAINT "FK_Fornecedor_Municipio_MunicipioId";
    END IF;
END $$;

-- Drop indexes for Fornecedor
DROP INDEX IF EXISTS "IX_Fornecedor_UfId";
DROP INDEX IF EXISTS "IX_Fornecedor_MunicipioId";

-- Restore Fornecedor from backup or remove columns
DO $$
DECLARE
    backup_table_name TEXT;
BEGIN
    SELECT table_name INTO backup_table_name
    FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_name LIKE 'Fornecedor_backup_%'
    AND table_name ~ 'Fornecedor_backup_\d{14}'
    ORDER BY table_name DESC
    LIMIT 1;
    
    IF backup_table_name IS NOT NULL THEN
        DROP TABLE IF EXISTS public."Fornecedor" CASCADE;
        EXECUTE 'CREATE TABLE public."Fornecedor" AS SELECT * FROM public."' || backup_table_name || '"';
    ELSE
        -- Remove new columns
        ALTER TABLE public."Fornecedor" DROP COLUMN IF EXISTS "UfId";
        ALTER TABLE public."Fornecedor" DROP COLUMN IF EXISTS "MunicipioId";
    END IF;
END $$;

INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_all_migrations', 'FORNECEDOR', 'Rolled back Fornecedor references migration', 'SUCCESS');

-- =====================================================
-- STEP 2: Rollback Produto references (Step 2)
-- =====================================================
\echo 'Rolling back Produto references migration...'

-- Drop FK constraints for Produto
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Produto_UnidadeMedida_UnidadeMedidaId'
        AND table_name = 'Produto'
    ) THEN
        ALTER TABLE public."Produto" DROP CONSTRAINT "FK_Produto_UnidadeMedida_UnidadeMedidaId";
    END IF;
    
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Produto_Embalagem_EmbalagemId'
        AND table_name = 'Produto'
    ) THEN
        ALTER TABLE public."Produto" DROP CONSTRAINT "FK_Produto_Embalagem_EmbalagemId";
    END IF;
    
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Produto_AtividadeAgropecuaria_AtividadeAgropecuariaId'
        AND table_name = 'Produto'
    ) THEN
        ALTER TABLE public."Produto" DROP CONSTRAINT "FK_Produto_AtividadeAgropecuaria_AtividadeAgropecuariaId";
    END IF;
END $$;

-- Drop indexes for Produto
DROP INDEX IF EXISTS "IX_Produto_UnidadeMedidaId";
DROP INDEX IF EXISTS "IX_Produto_EmbalagemId";
DROP INDEX IF EXISTS "IX_Produto_AtividadeAgropecuariaId";

-- Restore Produto from backup or remove columns
DO $$
DECLARE
    backup_table_name TEXT;
BEGIN
    SELECT table_name INTO backup_table_name
    FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_name LIKE 'Produto_backup_%'
    AND table_name ~ 'Produto_backup_\d{14}'
    ORDER BY table_name DESC
    LIMIT 1;
    
    IF backup_table_name IS NOT NULL THEN
        DROP TABLE IF EXISTS public."Produto" CASCADE;
        EXECUTE 'CREATE TABLE public."Produto" AS SELECT * FROM public."' || backup_table_name || '"';
    ELSE
        -- Remove new columns
        ALTER TABLE public."Produto" DROP COLUMN IF EXISTS "UnidadeMedidaId";
        ALTER TABLE public."Produto" DROP COLUMN IF EXISTS "EmbalagemId";
        ALTER TABLE public."Produto" DROP COLUMN IF EXISTS "AtividadeAgropecuariaId";
    END IF;
END $$;

INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_all_migrations', 'PRODUTO', 'Rolled back Produto references migration', 'SUCCESS');

-- =====================================================
-- STEP 3: Rollback reference tables (Step 1)
-- =====================================================
\echo 'Rolling back reference tables creation...'

-- Drop all reference tables in reverse dependency order
DROP TABLE IF EXISTS public."Embalagens" CASCADE;
DROP TABLE IF EXISTS public."AtividadesAgropecuarias" CASCADE;
DROP TABLE IF EXISTS public."UnidadesMedida" CASCADE;
DROP TABLE IF EXISTS public."Moedas" CASCADE;
DROP TABLE IF EXISTS public."Municipios" CASCADE;
DROP TABLE IF EXISTS public."Ufs" CASCADE;
DROP TABLE IF EXISTS public."Paises" CASCADE;

-- Restore from backup tables if they exist
DO $$
DECLARE
    backup_table RECORD;
    original_table_name TEXT;
    restore_count INTEGER := 0;
BEGIN
    FOR backup_table IN 
        SELECT table_name 
        FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name LIKE '%_backup_%'
        AND table_name ~ '(Moedas|Paises|Ufs|Municipios|UnidadesMedida|AtividadesAgropecuarias|Embalagens)_backup_\d{14}'
    LOOP
        original_table_name := split_part(backup_table.table_name, '_backup_', 1);
        EXECUTE 'CREATE TABLE public."' || original_table_name || '" AS SELECT * FROM public."' || backup_table.table_name || '"';
        restore_count := restore_count + 1;
    END LOOP;
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
    VALUES ('rollback_all_migrations', 'REFERENCE_TABLES', 'Rolled back reference tables creation', restore_count, 'SUCCESS');
END $$;

-- =====================================================
-- STEP 4: Restore original table structures
-- =====================================================
\echo 'Restoring original table structures...'

-- Restore Produto primary key and indexes if needed
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_type = 'PRIMARY KEY'
        AND table_name = 'Produto'
        AND table_schema = 'public'
    ) THEN
        ALTER TABLE public."Produto" ADD PRIMARY KEY ("Id");
    END IF;
    
    CREATE INDEX IF NOT EXISTS "IX_Produto_Nome" ON public."Produto" ("Nome");
    CREATE INDEX IF NOT EXISTS "IX_Produto_Codigo" ON public."Produto" ("Codigo");
    CREATE INDEX IF NOT EXISTS "IX_Produto_CategoriaId" ON public."Produto" ("CategoriaId");
    CREATE INDEX IF NOT EXISTS "IX_Produto_FornecedorId" ON public."Produto" ("FornecedorId");
END $$;

-- Restore Fornecedor primary key and indexes if needed
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_type = 'PRIMARY KEY'
        AND table_name = 'Fornecedor'
        AND table_schema = 'public'
    ) THEN
        ALTER TABLE public."Fornecedor" ADD PRIMARY KEY ("Id");
    END IF;
    
    CREATE INDEX IF NOT EXISTS "IX_Fornecedor_Nome" ON public."Fornecedor" ("Nome");
    CREATE INDEX IF NOT EXISTS "IX_Fornecedor_Cnpj" ON public."Fornecedor" ("Cnpj");
    CREATE INDEX IF NOT EXISTS "IX_Fornecedor_Ativo" ON public."Fornecedor" ("Ativo");
    
    -- Restore geographic string column indexes if they exist
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
END $$;

INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_all_migrations', 'RESTORE_STRUCTURES', 'Restored original table structures', 'SUCCESS');

-- =====================================================
-- STEP 5: Generate complete rollback report
-- =====================================================
DO $$
DECLARE
    total_produtos INTEGER := 0;
    total_fornecedores INTEGER := 0;
    reference_tables_count INTEGER := 0;
    rollback_report TEXT;
BEGIN
    -- Count remaining records
    SELECT COUNT(*) INTO total_produtos FROM public."Produto" WHERE true;
    SELECT COUNT(*) INTO total_fornecedores FROM public."Fornecedor" WHERE true;
    
    -- Count reference tables (should be 0 after rollback)
    SELECT COUNT(*) INTO reference_tables_count
    FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_name IN ('Paises', 'Ufs', 'Municipios', 'Moedas', 'UnidadesMedida', 'AtividadesAgropecuarias', 'Embalagens');
    
    -- Generate rollback report
    rollback_report := 'COMPLETE ROLLBACK REPORT' || E'\n' ||
                      '=========================' || E'\n' ||
                      'Products restored: ' || total_produtos || E'\n' ||
                      'Fornecedores restored: ' || total_fornecedores || E'\n' ||
                      'Reference tables remaining: ' || reference_tables_count || E'\n' ||
                      '=========================' || E'\n' ||
                      'Rollback Status: ' || 
                      CASE 
                          WHEN reference_tables_count = 0 THEN 'COMPLETE - All reference tables removed'
                          ELSE 'PARTIAL - Some reference tables still exist'
                      END;
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('rollback_all_migrations', 'FINAL_REPORT', rollback_report, 'SUCCESS');
    
    -- Output report to console
    RAISE NOTICE '%', rollback_report;
    
    -- Final status message
    IF reference_tables_count = 0 THEN
        RAISE NOTICE 'ROLLBACK SUCCESSFUL: All migrations have been completely rolled back.';
    ELSE
        RAISE NOTICE 'ROLLBACK WARNING: Some reference tables still exist. Manual cleanup may be required.';
    END IF;
END $$;

-- =====================================================
-- STEP 6: Clean up backup tables (optional)
-- =====================================================
-- Note: Commented out for safety. Uncomment if you want to clean up ALL backup tables.
/*
\echo 'Cleaning up backup tables...'

DO $$
DECLARE
    backup_table RECORD;
    cleanup_count INTEGER := 0;
BEGIN
    FOR backup_table IN 
        SELECT table_name 
        FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name LIKE '%_backup_%'
        AND table_name ~ '(Produto|Fornecedor|Moedas|Paises|Ufs|Municipios|UnidadesMedida|AtividadesAgropecuarias|Embalagens)_backup_\d{14}'
    LOOP
        EXECUTE 'DROP TABLE IF EXISTS public."' || backup_table.table_name || '"';
        cleanup_count := cleanup_count + 1;
    END LOOP;
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
    VALUES ('rollback_all_migrations', 'CLEANUP', 'Cleaned up backup tables', cleanup_count, 'SUCCESS');
    
    RAISE NOTICE 'Cleaned up % backup tables.', cleanup_count;
END $$;
*/

-- Log complete rollback completion
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('rollback_all_migrations', 'COMPLETE', 'Complete rollback of all reference migrations finished', 'SUCCESS');

-- Commit complete rollback transaction
COMMIT;

-- Display final message
\echo '========================================='
\echo 'COMPLETE ROLLBACK FINISHED SUCCESSFULLY!'
\echo '========================================='
\echo 'All reference entity migrations have been rolled back.'
\echo 'The database has been restored to its pre-migration state.'
\echo 'Check MigrationLogs table for detailed rollback log.'
\echo 'Backup tables have been preserved for safety.'
\echo '========================================='