-- =====================================================
-- MIGRATION SCRIPT: Migrate Fornecedor References
-- Description: Maps Fornecedor geographic strings to reference IDs
-- Author: System Migration
-- Date: 2025-01-27
-- Requirements: 12.1, 12.2, 12.3, 12.4, 12.5, 12.6
-- =====================================================

-- Enable detailed logging
\set VERBOSITY verbose
\timing on

-- Start migration transaction
BEGIN;

-- Log migration start
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('03_migrate_fornecedor_references', 'START', 'Starting Fornecedor references migration', 'INFO');

-- =====================================================
-- STEP 1: Create backup of Fornecedor table
-- =====================================================
DO $$
DECLARE
    backup_table_name TEXT;
BEGIN
    backup_table_name := 'Fornecedor_backup_' || to_char(CURRENT_TIMESTAMP, 'YYYYMMDDHH24MISS');
    
    -- Create backup table
    EXECUTE 'CREATE TABLE public."' || backup_table_name || '" AS SELECT * FROM public."Fornecedor"';
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('03_migrate_fornecedor_references', 'BACKUP', 'Created backup table: ' || backup_table_name, 'SUCCESS');
END $$;

-- =====================================================
-- STEP 2: Create temporary tables for conflict resolution
-- =====================================================

-- Create temporary table to track Fornecedor migration issues
CREATE TEMP TABLE fornecedor_migration_issues (
    fornecedor_id INTEGER,
    issue_type VARCHAR(50),
    issue_description TEXT,
    old_uf_value TEXT,
    old_municipio_value TEXT,
    suggested_uf_id INTEGER,
    suggested_municipio_id INTEGER,
    resolved BOOLEAN DEFAULT false
);

-- Create temporary table for UF mapping statistics
CREATE TEMP TABLE uf_mapping_stats (
    original_uf_string VARCHAR(100),
    mapped_uf_id INTEGER,
    mapped_uf_codigo VARCHAR(2),
    fornecedor_count INTEGER
);

-- Create temporary table for Municipio mapping statistics
CREATE TEMP TABLE municipio_mapping_stats (
    original_municipio_string VARCHAR(200),
    original_uf_string VARCHAR(100),
    mapped_municipio_id INTEGER,
    mapped_uf_id INTEGER,
    fornecedor_count INTEGER
);

-- =====================================================
-- STEP 3: Check current Fornecedor table structure
-- =====================================================
DO $$
DECLARE
    has_uf_string BOOLEAN := false;
    has_municipio_string BOOLEAN := false;
    has_uf_id BOOLEAN := false;
    has_municipio_id BOOLEAN := false;
    fornecedores_count INTEGER := 0;
BEGIN
    -- Check for string UF column
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'Uf'
        AND data_type IN ('character varying', 'text', 'varchar')
    ) INTO has_uf_string;
    
    -- Check for string Municipio column
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'Municipio'
        AND data_type IN ('character varying', 'text', 'varchar')
    ) INTO has_municipio_string;
    
    -- Check for UfId column
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'UfId'
    ) INTO has_uf_id;
    
    -- Check for MunicipioId column
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'MunicipioId'
    ) INTO has_municipio_id;
    
    -- Count total fornecedores
    SELECT COUNT(*) INTO fornecedores_count FROM public."Fornecedor";
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('03_migrate_fornecedor_references', 'STRUCTURE_CHECK', 
            'Table structure - UF string: ' || has_uf_string || 
            ', Municipio string: ' || has_municipio_string || 
            ', UfId: ' || has_uf_id || 
            ', MunicipioId: ' || has_municipio_id || 
            ', Total fornecedores: ' || fornecedores_count, 'INFO');
END $$;

-- =====================================================
-- STEP 4: Add new reference columns if they don't exist
-- =====================================================
DO $$
BEGIN
    -- Add UfId column if not exists
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'UfId'
    ) THEN
        ALTER TABLE public."Fornecedor" ADD COLUMN "UfId" INTEGER;
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('03_migrate_fornecedor_references', 'ALTER', 'Added UfId column to Fornecedor table', 'SUCCESS');
    END IF;
    
    -- Add MunicipioId column if not exists
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'MunicipioId'
    ) THEN
        ALTER TABLE public."Fornecedor" ADD COLUMN "MunicipioId" INTEGER;
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('03_migrate_fornecedor_references', 'ALTER', 'Added MunicipioId column to Fornecedor table', 'SUCCESS');
    END IF;
END $$;

-- =====================================================
-- STEP 5: Migrate UF references
-- =====================================================
DO $$
DECLARE
    ufs_mapped INTEGER := 0;
    has_uf_string BOOLEAN := false;
BEGIN
    -- Check if string UF column exists
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'Uf'
        AND data_type IN ('character varying', 'text', 'varchar')
    ) INTO has_uf_string;
    
    IF has_uf_string THEN
        -- First, collect mapping statistics
        INSERT INTO uf_mapping_stats (original_uf_string, mapped_uf_id, mapped_uf_codigo, fornecedor_count)
        SELECT 
            f."Uf" as original_uf_string,
            u."Id" as mapped_uf_id,
            u."Codigo" as mapped_uf_codigo,
            COUNT(*) as fornecedor_count
        FROM public."Fornecedor" f
        LEFT JOIN public."Ufs" u ON (
            UPPER(TRIM(f."Uf")) = UPPER(u."Codigo") OR
            UPPER(TRIM(f."Uf")) = UPPER(u."Nome") OR
            -- Handle common variations
            (UPPER(TRIM(f."Uf")) = 'SAO PAULO' AND u."Codigo" = 'SP') OR
            (UPPER(TRIM(f."Uf")) = 'RIO DE JANEIRO' AND u."Codigo" = 'RJ') OR
            (UPPER(TRIM(f."Uf")) = 'MINAS GERAIS' AND u."Codigo" = 'MG') OR
            (UPPER(TRIM(f."Uf")) = 'RIO GRANDE DO SUL' AND u."Codigo" = 'RS') OR
            (UPPER(TRIM(f."Uf")) = 'RIO GRANDE DO NORTE' AND u."Codigo" = 'RN') OR
            (UPPER(TRIM(f."Uf")) = 'MATO GROSSO' AND u."Codigo" = 'MT') OR
            (UPPER(TRIM(f."Uf")) = 'MATO GROSSO DO SUL' AND u."Codigo" = 'MS') OR
            (UPPER(TRIM(f."Uf")) = 'ESPIRITO SANTO' AND u."Codigo" = 'ES') OR
            (UPPER(TRIM(f."Uf")) = 'DISTRITO FEDERAL' AND u."Codigo" = 'DF') OR
            (UPPER(TRIM(f."Uf")) = 'SANTA CATARINA' AND u."Codigo" = 'SC')
        )
        WHERE f."Uf" IS NOT NULL 
        AND TRIM(f."Uf") != ''
        AND f."UfId" IS NULL
        GROUP BY f."Uf", u."Id", u."Codigo"
        ORDER BY f."Uf";
        
        -- Perform the actual mapping
        UPDATE public."Fornecedor" f
        SET "UfId" = u."Id"
        FROM public."Ufs" u
        WHERE f."UfId" IS NULL
        AND f."Uf" IS NOT NULL 
        AND TRIM(f."Uf") != ''
        AND (
            UPPER(TRIM(f."Uf")) = UPPER(u."Codigo") OR
            UPPER(TRIM(f."Uf")) = UPPER(u."Nome") OR
            -- Handle common variations
            (UPPER(TRIM(f."Uf")) = 'SAO PAULO' AND u."Codigo" = 'SP') OR
            (UPPER(TRIM(f."Uf")) = 'RIO DE JANEIRO' AND u."Codigo" = 'RJ') OR
            (UPPER(TRIM(f."Uf")) = 'MINAS GERAIS' AND u."Codigo" = 'MG') OR
            (UPPER(TRIM(f."Uf")) = 'RIO GRANDE DO SUL' AND u."Codigo" = 'RS') OR
            (UPPER(TRIM(f."Uf")) = 'RIO GRANDE DO NORTE' AND u."Codigo" = 'RN') OR
            (UPPER(TRIM(f."Uf")) = 'MATO GROSSO' AND u."Codigo" = 'MT') OR
            (UPPER(TRIM(f."Uf")) = 'MATO GROSSO DO SUL' AND u."Codigo" = 'MS') OR
            (UPPER(TRIM(f."Uf")) = 'ESPIRITO SANTO' AND u."Codigo" = 'ES') OR
            (UPPER(TRIM(f."Uf")) = 'DISTRITO FEDERAL' AND u."Codigo" = 'DF') OR
            (UPPER(TRIM(f."Uf")) = 'SANTA CATARINA' AND u."Codigo" = 'SC')
        );
        
        GET DIAGNOSTICS ufs_mapped = ROW_COUNT;
        
        -- Log unmapped UF values
        INSERT INTO fornecedor_migration_issues (fornecedor_id, issue_type, issue_description, old_uf_value)
        SELECT 
            f."Id",
            'UF_NOT_MAPPED',
            'UF string value could not be mapped to reference',
            f."Uf"
        FROM public."Fornecedor" f
        WHERE f."UfId" IS NULL
        AND f."Uf" IS NOT NULL
        AND TRIM(f."Uf") != '';
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
        VALUES ('03_migrate_fornecedor_references', 'UF_MAPPING', 'Mapped UF string values to references', ufs_mapped, 'SUCCESS');
    ELSE
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('03_migrate_fornecedor_references', 'UF_MAPPING', 'No string UF column found, skipping UF mapping', 'INFO');
    END IF;
END $$;

-- =====================================================
-- STEP 6: Migrate Municipio references
-- =====================================================
DO $$
DECLARE
    municipios_mapped INTEGER := 0;
    has_municipio_string BOOLEAN := false;
BEGIN
    -- Check if string Municipio column exists
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'Municipio'
        AND data_type IN ('character varying', 'text', 'varchar')
    ) INTO has_municipio_string;
    
    IF has_municipio_string THEN
        -- First, collect mapping statistics
        INSERT INTO municipio_mapping_stats (original_municipio_string, original_uf_string, mapped_municipio_id, mapped_uf_id, fornecedor_count)
        SELECT 
            f."Municipio" as original_municipio_string,
            f."Uf" as original_uf_string,
            m."Id" as mapped_municipio_id,
            f."UfId" as mapped_uf_id,
            COUNT(*) as fornecedor_count
        FROM public."Fornecedor" f
        LEFT JOIN public."Municipios" m ON (
            UPPER(TRIM(f."Municipio")) = UPPER(TRIM(m."Nome"))
            AND (f."UfId" = m."UfId" OR f."UfId" IS NULL)
        )
        WHERE f."Municipio" IS NOT NULL 
        AND TRIM(f."Municipio") != ''
        AND f."MunicipioId" IS NULL
        GROUP BY f."Municipio", f."Uf", m."Id", f."UfId"
        ORDER BY f."Municipio";
        
        -- Perform the actual mapping (prioritize matches within the same UF)
        UPDATE public."Fornecedor" f
        SET "MunicipioId" = m."Id"
        FROM public."Municipios" m
        WHERE f."MunicipioId" IS NULL
        AND f."Municipio" IS NOT NULL 
        AND TRIM(f."Municipio") != ''
        AND UPPER(TRIM(f."Municipio")) = UPPER(TRIM(m."Nome"))
        AND (
            -- Prefer exact UF match
            (f."UfId" IS NOT NULL AND f."UfId" = m."UfId") OR
            -- If no UF match, take any municipio with that name (will be validated later)
            (f."UfId" IS NULL)
        );
        
        GET DIAGNOSTICS municipios_mapped = ROW_COUNT;
        
        -- For municipios mapped without UF validation, update UfId to match
        UPDATE public."Fornecedor" f
        SET "UfId" = m."UfId"
        FROM public."Municipios" m
        WHERE f."MunicipioId" = m."Id"
        AND (f."UfId" IS NULL OR f."UfId" != m."UfId");
        
        -- Log unmapped Municipio values
        INSERT INTO fornecedor_migration_issues (fornecedor_id, issue_type, issue_description, old_municipio_value, old_uf_value)
        SELECT 
            f."Id",
            'MUNICIPIO_NOT_MAPPED',
            'Municipio string value could not be mapped to reference',
            f."Municipio",
            f."Uf"
        FROM public."Fornecedor" f
        WHERE f."MunicipioId" IS NULL
        AND f."Municipio" IS NOT NULL
        AND TRIM(f."Municipio") != '';
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
        VALUES ('03_migrate_fornecedor_references', 'MUNICIPIO_MAPPING', 'Mapped Municipio string values to references', municipios_mapped, 'SUCCESS');
    ELSE
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('03_migrate_fornecedor_references', 'MUNICIPIO_MAPPING', 'No string Municipio column found, skipping Municipio mapping', 'INFO');
    END IF;
END $$;

-- =====================================================
-- STEP 7: Validate UF-Municipio consistency
-- =====================================================
DO $$
DECLARE
    inconsistent_records INTEGER := 0;
    fixed_records INTEGER := 0;
BEGIN
    -- Find records where Municipio.UfId doesn't match Fornecedor.UfId
    SELECT COUNT(*) INTO inconsistent_records
    FROM public."Fornecedor" f
    INNER JOIN public."Municipios" m ON f."MunicipioId" = m."Id"
    WHERE f."UfId" != m."UfId";
    
    IF inconsistent_records > 0 THEN
        -- Log inconsistent records
        INSERT INTO fornecedor_migration_issues (fornecedor_id, issue_type, issue_description, suggested_uf_id)
        SELECT 
            f."Id",
            'UF_MUNICIPIO_INCONSISTENT',
            'Fornecedor UfId (' || f."UfId" || ') does not match Municipio UfId (' || m."UfId" || ')',
            m."UfId"
        FROM public."Fornecedor" f
        INNER JOIN public."Municipios" m ON f."MunicipioId" = m."Id"
        WHERE f."UfId" != m."UfId";
        
        -- Fix by updating Fornecedor.UfId to match Municipio.UfId
        UPDATE public."Fornecedor" f
        SET "UfId" = m."UfId"
        FROM public."Municipios" m
        WHERE f."MunicipioId" = m."Id"
        AND f."UfId" != m."UfId";
        
        GET DIAGNOSTICS fixed_records = ROW_COUNT;
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
        VALUES ('03_migrate_fornecedor_references', 'CONSISTENCY_FIX', 'Fixed UF-Municipio consistency issues', fixed_records, 'SUCCESS');
    ELSE
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('03_migrate_fornecedor_references', 'CONSISTENCY_CHECK', 'No UF-Municipio consistency issues found', 'SUCCESS');
    END IF;
END $$;

-- =====================================================
-- STEP 8: Create foreign key constraints
-- =====================================================
DO $$
BEGIN
    -- Add FK constraint for UF
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Fornecedor_Uf_UfId'
        AND table_name = 'Fornecedor'
    ) THEN
        ALTER TABLE public."Fornecedor" 
        ADD CONSTRAINT "FK_Fornecedor_Uf_UfId" 
        FOREIGN KEY ("UfId") 
        REFERENCES public."Ufs" ("Id") 
        ON DELETE RESTRICT;
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('03_migrate_fornecedor_references', 'FK_CONSTRAINT', 'Added FK constraint for UF', 'SUCCESS');
    END IF;
    
    -- Add FK constraint for Municipio
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Fornecedor_Municipio_MunicipioId'
        AND table_name = 'Fornecedor'
    ) THEN
        ALTER TABLE public."Fornecedor" 
        ADD CONSTRAINT "FK_Fornecedor_Municipio_MunicipioId" 
        FOREIGN KEY ("MunicipioId") 
        REFERENCES public."Municipios" ("Id") 
        ON DELETE RESTRICT;
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('03_migrate_fornecedor_references', 'FK_CONSTRAINT', 'Added FK constraint for Municipio', 'SUCCESS');
    END IF;
END $$;

-- =====================================================
-- STEP 9: Create indexes for performance
-- =====================================================
CREATE INDEX IF NOT EXISTS "IX_Fornecedor_UfId" ON public."Fornecedor" ("UfId");
CREATE INDEX IF NOT EXISTS "IX_Fornecedor_MunicipioId" ON public."Fornecedor" ("MunicipioId");

INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('03_migrate_fornecedor_references', 'INDEXES', 'Created performance indexes for new FK columns', 'SUCCESS');

-- =====================================================
-- STEP 10: Generate migration report
-- =====================================================
DO $$
DECLARE
    total_fornecedores INTEGER;
    fornecedores_with_uf INTEGER;
    fornecedores_with_municipio INTEGER;
    migration_issues INTEGER;
    uf_mapping_success_rate DECIMAL;
    municipio_mapping_success_rate DECIMAL;
    report_text TEXT;
BEGIN
    -- Count migration results
    SELECT COUNT(*) INTO total_fornecedores FROM public."Fornecedor";
    SELECT COUNT(*) INTO fornecedores_with_uf FROM public."Fornecedor" WHERE "UfId" IS NOT NULL;
    SELECT COUNT(*) INTO fornecedores_with_municipio FROM public."Fornecedor" WHERE "MunicipioId" IS NOT NULL;
    SELECT COUNT(*) INTO migration_issues FROM fornecedor_migration_issues WHERE NOT resolved;
    
    -- Calculate success rates
    IF total_fornecedores > 0 THEN
        uf_mapping_success_rate := ROUND((fornecedores_with_uf::DECIMAL / total_fornecedores * 100), 2);
        municipio_mapping_success_rate := ROUND((fornecedores_with_municipio::DECIMAL / total_fornecedores * 100), 2);
    ELSE
        uf_mapping_success_rate := 0;
        municipio_mapping_success_rate := 0;
    END IF;
    
    -- Generate report
    report_text := 'FORNECEDOR MIGRATION REPORT' || E'\n' ||
                   '============================' || E'\n' ||
                   'Total Fornecedores: ' || total_fornecedores || E'\n' ||
                   'Fornecedores with UF: ' || fornecedores_with_uf || ' (' || uf_mapping_success_rate || '%)' || E'\n' ||
                   'Fornecedores with Municipio: ' || fornecedores_with_municipio || ' (' || municipio_mapping_success_rate || '%)' || E'\n' ||
                   'Migration Issues: ' || migration_issues || E'\n' ||
                   '============================';
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('03_migrate_fornecedor_references', 'REPORT', report_text, 'SUCCESS');
    
    -- Output report to console
    RAISE NOTICE '%', report_text;
    
    -- Show migration issues if any
    IF migration_issues > 0 THEN
        RAISE NOTICE 'Migration issues found. Check fornecedor_migration_issues temp table for details.';
        RAISE NOTICE 'Query: SELECT * FROM fornecedor_migration_issues WHERE NOT resolved;';
    END IF;
    
    -- Show mapping statistics
    RAISE NOTICE 'UF Mapping Statistics:';
    RAISE NOTICE 'Query: SELECT * FROM uf_mapping_stats ORDER BY fornecedor_count DESC;';
    
    RAISE NOTICE 'Municipio Mapping Statistics:';
    RAISE NOTICE 'Query: SELECT * FROM municipio_mapping_stats ORDER BY fornecedor_count DESC;';
END $$;

-- =====================================================
-- STEP 11: Optional cleanup of old string columns
-- =====================================================
-- Note: This step is commented out for safety. 
-- Uncomment only after thorough validation of the migration results.

/*
DO $$
DECLARE
    has_uf_string BOOLEAN := false;
    has_municipio_string BOOLEAN := false;
BEGIN
    -- Check if old string columns exist
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
    
    -- Only drop columns if migration was successful for most records
    IF has_uf_string AND (
        SELECT COUNT(*) FROM public."Fornecedor" WHERE "UfId" IS NOT NULL
    ) >= (
        SELECT COUNT(*) * 0.9 FROM public."Fornecedor" WHERE "Uf" IS NOT NULL AND TRIM("Uf") != ''
    ) THEN
        ALTER TABLE public."Fornecedor" DROP COLUMN "Uf";
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('03_migrate_fornecedor_references', 'CLEANUP', 'Dropped old Uf string column', 'SUCCESS');
    END IF;
    
    IF has_municipio_string AND (
        SELECT COUNT(*) FROM public."Fornecedor" WHERE "MunicipioId" IS NOT NULL
    ) >= (
        SELECT COUNT(*) * 0.9 FROM public."Fornecedor" WHERE "Municipio" IS NOT NULL AND TRIM("Municipio") != ''
    ) THEN
        ALTER TABLE public."Fornecedor" DROP COLUMN "Municipio";
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('03_migrate_fornecedor_references', 'CLEANUP', 'Dropped old Municipio string column', 'SUCCESS');
    END IF;
END $$;
*/

-- Log migration completion
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('03_migrate_fornecedor_references', 'COMPLETE', 'Fornecedor references migration completed successfully', 'SUCCESS');

-- Commit transaction
COMMIT;

-- Display final message
\echo 'Migration 03_migrate_fornecedor_references completed successfully!'
\echo 'Check MigrationLogs table for detailed execution log.'
\echo 'If there were migration issues, review the fornecedor_migration_issues temp table.'
\echo 'Review mapping statistics in uf_mapping_stats and municipio_mapping_stats temp tables.'