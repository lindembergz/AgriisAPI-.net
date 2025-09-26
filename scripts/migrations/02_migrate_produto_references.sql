-- =====================================================
-- MIGRATION SCRIPT: Migrate Produto References
-- Description: Maps Produto string fields to reference IDs with conflict resolution
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
VALUES ('02_migrate_produto_references', 'START', 'Starting Produto references migration', 'INFO');

-- =====================================================
-- STEP 1: Create backup of Produto table
-- =====================================================
DO $$
DECLARE
    backup_table_name TEXT;
BEGIN
    backup_table_name := 'Produto_backup_' || to_char(CURRENT_TIMESTAMP, 'YYYYMMDDHH24MISS');
    
    -- Create backup table
    EXECUTE 'CREATE TABLE public."' || backup_table_name || '" AS SELECT * FROM public."Produto"';
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('02_migrate_produto_references', 'BACKUP', 'Created backup table: ' || backup_table_name, 'SUCCESS');
END $$;

-- =====================================================
-- STEP 2: Create temporary mapping tables for conflict resolution
-- =====================================================

-- Create temporary table to track Produto migration issues
CREATE TEMP TABLE produto_migration_issues (
    produto_id INTEGER,
    issue_type VARCHAR(50),
    issue_description TEXT,
    old_value TEXT,
    suggested_value TEXT,
    resolved BOOLEAN DEFAULT false
);

-- Create temporary table for UnidadeMedida mapping
CREATE TEMP TABLE unidade_medida_mapping (
    old_enum_value INTEGER,
    old_enum_name VARCHAR(50),
    new_unidade_id INTEGER,
    simbolo VARCHAR(10)
);

-- =====================================================
-- STEP 3: Populate UnidadeMedida mapping table
-- =====================================================
INSERT INTO unidade_medida_mapping (old_enum_value, old_enum_name, new_unidade_id, simbolo)
SELECT 
    enum_val,
    enum_name,
    um."Id",
    um."Simbolo"
FROM (
    VALUES 
        (0, 'Sementes', 'SEMENTES'),
        (1, 'Quilo', 'KG'),
        (2, 'Tonelada', 'T'),
        (3, 'Litro', 'L'),
        (4, 'Hectare', 'HA'),
        (5, 'Dose', 'DOSE'),
        (6, 'Frasco', 'FRASCO'),
        (7, 'Ovos', 'OVOS'),
        (8, 'Parasitoide', 'PARASITOIDE')
) AS enum_mapping(enum_val, enum_name, simbolo_lookup)
LEFT JOIN public."UnidadesMedida" um ON um."Simbolo" = simbolo_lookup;

INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
VALUES ('02_migrate_produto_references', 'MAPPING', 'Created UnidadeMedida mapping table', 9, 'SUCCESS');

-- =====================================================
-- STEP 4: Check if Produto table has the old Unidade enum column
-- =====================================================
DO $$
DECLARE
    has_unidade_column BOOLEAN := false;
    produtos_to_migrate INTEGER := 0;
BEGIN
    -- Check if Unidade column exists
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Produto' 
        AND column_name = 'Unidade'
    ) INTO has_unidade_column;
    
    IF has_unidade_column THEN
        -- Count products that need migration
        EXECUTE 'SELECT COUNT(*) FROM public."Produto" WHERE "UnidadeMedidaId" IS NULL OR "UnidadeMedidaId" = 0' INTO produtos_to_migrate;
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
        VALUES ('02_migrate_produto_references', 'CHECK', 'Found Unidade column, ' || produtos_to_migrate || ' products need migration', produtos_to_migrate, 'INFO');
    ELSE
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('02_migrate_produto_references', 'CHECK', 'Unidade column not found, checking existing UnidadeMedidaId values', 'INFO');
    END IF;
END $$;

-- =====================================================
-- STEP 5: Add new columns if they don't exist
-- =====================================================
DO $$
BEGIN
    -- Add UnidadeMedidaId if not exists
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Produto' 
        AND column_name = 'UnidadeMedidaId'
    ) THEN
        ALTER TABLE public."Produto" ADD COLUMN "UnidadeMedidaId" INTEGER;
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('02_migrate_produto_references', 'ALTER', 'Added UnidadeMedidaId column to Produto table', 'SUCCESS');
    END IF;
    
    -- Add EmbalagemId if not exists
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Produto' 
        AND column_name = 'EmbalagemId'
    ) THEN
        ALTER TABLE public."Produto" ADD COLUMN "EmbalagemId" INTEGER;
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('02_migrate_produto_references', 'ALTER', 'Added EmbalagemId column to Produto table', 'SUCCESS');
    END IF;
    
    -- Add AtividadeAgropecuariaId if not exists
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Produto' 
        AND column_name = 'AtividadeAgropecuariaId'
    ) THEN
        ALTER TABLE public."Produto" ADD COLUMN "AtividadeAgropecuariaId" INTEGER;
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('02_migrate_produto_references', 'ALTER', 'Added AtividadeAgropecuariaId column to Produto table', 'SUCCESS');
    END IF;
END $$;

-- =====================================================
-- STEP 6: Migrate UnidadeMedida references
-- =====================================================
DO $$
DECLARE
    produtos_updated INTEGER := 0;
    has_unidade_column BOOLEAN := false;
    default_kg_id INTEGER;
BEGIN
    -- Get default KG unit ID
    SELECT "Id" INTO default_kg_id FROM public."UnidadesMedida" WHERE "Simbolo" = 'kg' LIMIT 1;
    
    -- Check if old Unidade column exists
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Produto' 
        AND column_name = 'Unidade'
    ) INTO has_unidade_column;
    
    IF has_unidade_column THEN
        -- Migrate from enum values
        UPDATE public."Produto" 
        SET "UnidadeMedidaId" = COALESCE(umm.new_unidade_id, default_kg_id)
        FROM unidade_medida_mapping umm
        WHERE public."Produto"."Unidade" = umm.old_enum_value
        AND (public."Produto"."UnidadeMedidaId" IS NULL OR public."Produto"."UnidadeMedidaId" = 0);
        
        GET DIAGNOSTICS produtos_updated = ROW_COUNT;
        
        -- Handle products with unmapped enum values
        UPDATE public."Produto" 
        SET "UnidadeMedidaId" = default_kg_id
        WHERE ("UnidadeMedidaId" IS NULL OR "UnidadeMedidaId" = 0);
        
    ELSE
        -- Set default for products without UnidadeMedidaId
        UPDATE public."Produto" 
        SET "UnidadeMedidaId" = default_kg_id
        WHERE ("UnidadeMedidaId" IS NULL OR "UnidadeMedidaId" = 0);
        
        GET DIAGNOSTICS produtos_updated = ROW_COUNT;
    END IF;
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
    VALUES ('02_migrate_produto_references', 'UNIDADE_MEDIDA', 'Migrated UnidadeMedida references', produtos_updated, 'SUCCESS');
END $$;

-- =====================================================
-- STEP 7: Migrate Embalagem references (from string field if exists)
-- =====================================================
DO $$
DECLARE
    embalagens_mapped INTEGER := 0;
    has_embalagem_string BOOLEAN := false;
BEGIN
    -- Check if there's a string Embalagem column
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Produto' 
        AND column_name = 'Embalagem'
        AND data_type IN ('character varying', 'text', 'varchar')
    ) INTO has_embalagem_string;
    
    IF has_embalagem_string THEN
        -- Try to map string embalagem values to Embalagem IDs
        UPDATE public."Produto" p
        SET "EmbalagemId" = e."Id"
        FROM public."Embalagens" e
        WHERE p."EmbalagemId" IS NULL
        AND p."Embalagem" IS NOT NULL
        AND TRIM(p."Embalagem") != ''
        AND LOWER(TRIM(p."Embalagem")) = LOWER(e."Nome");
        
        GET DIAGNOSTICS embalagens_mapped = ROW_COUNT;
        
        -- Log unmapped embalagem values for manual review
        INSERT INTO produto_migration_issues (produto_id, issue_type, issue_description, old_value)
        SELECT 
            p."Id",
            'EMBALAGEM_NOT_MAPPED',
            'Embalagem string value could not be mapped to reference',
            p."Embalagem"
        FROM public."Produto" p
        WHERE p."EmbalagemId" IS NULL
        AND p."Embalagem" IS NOT NULL
        AND TRIM(p."Embalagem") != ''
        AND NOT EXISTS (
            SELECT 1 FROM public."Embalagens" e 
            WHERE LOWER(TRIM(p."Embalagem")) = LOWER(e."Nome")
        );
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
        VALUES ('02_migrate_produto_references', 'EMBALAGEM', 'Mapped Embalagem string values to references', embalagens_mapped, 'SUCCESS');
    ELSE
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('02_migrate_produto_references', 'EMBALAGEM', 'No string Embalagem column found, skipping mapping', 'INFO');
    END IF;
END $$;

-- =====================================================
-- STEP 8: Map AtividadeAgropecuaria based on Categoria
-- =====================================================
DO $$
DECLARE
    atividades_mapped INTEGER := 0;
BEGIN
    -- Map based on category names (heuristic mapping)
    UPDATE public."Produto" p
    SET "AtividadeAgropecuariaId" = aa."Id"
    FROM public."Categoria" c, public."AtividadesAgropecuarias" aa
    WHERE p."CategoriaId" = c."Id"
    AND p."AtividadeAgropecuariaId" IS NULL
    AND (
        (LOWER(c."Nome") LIKE '%soja%' AND aa."Codigo" = 'SOJA') OR
        (LOWER(c."Nome") LIKE '%milho%' AND aa."Codigo" = 'MILHO') OR
        (LOWER(c."Nome") LIKE '%algod%' AND aa."Codigo" = 'ALGODAO') OR
        (LOWER(c."Nome") LIKE '%cana%' AND aa."Codigo" = 'CANA') OR
        (LOWER(c."Nome") LIKE '%caf%' AND aa."Codigo" = 'CAFE') OR
        (LOWER(c."Nome") LIKE '%bovino%' AND aa."Codigo" = 'BOVINO') OR
        (LOWER(c."Nome") LIKE '%su%no%' AND aa."Codigo" = 'SUINO') OR
        (LOWER(c."Nome") LIKE '%ave%' AND aa."Codigo" = 'AVES') OR
        (LOWER(c."Nome") LIKE '%semente%' AND aa."Codigo" = 'SOJA') -- Default seeds to soja
    );
    
    GET DIAGNOSTICS atividades_mapped = ROW_COUNT;
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
    VALUES ('02_migrate_produto_references', 'ATIVIDADE_AGROPECUARIA', 'Mapped AtividadeAgropecuaria based on category names', atividades_mapped, 'SUCCESS');
END $$;

-- =====================================================
-- STEP 9: Create foreign key constraints
-- =====================================================
DO $$
BEGIN
    -- Add FK constraint for UnidadeMedida
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Produto_UnidadeMedida_UnidadeMedidaId'
        AND table_name = 'Produto'
    ) THEN
        ALTER TABLE public."Produto" 
        ADD CONSTRAINT "FK_Produto_UnidadeMedida_UnidadeMedidaId" 
        FOREIGN KEY ("UnidadeMedidaId") 
        REFERENCES public."UnidadesMedida" ("Id") 
        ON DELETE RESTRICT;
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('02_migrate_produto_references', 'FK_CONSTRAINT', 'Added FK constraint for UnidadeMedida', 'SUCCESS');
    END IF;
    
    -- Add FK constraint for Embalagem
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Produto_Embalagem_EmbalagemId'
        AND table_name = 'Produto'
    ) THEN
        ALTER TABLE public."Produto" 
        ADD CONSTRAINT "FK_Produto_Embalagem_EmbalagemId" 
        FOREIGN KEY ("EmbalagemId") 
        REFERENCES public."Embalagens" ("Id") 
        ON DELETE RESTRICT;
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('02_migrate_produto_references', 'FK_CONSTRAINT', 'Added FK constraint for Embalagem', 'SUCCESS');
    END IF;
    
    -- Add FK constraint for AtividadeAgropecuaria
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Produto_AtividadeAgropecuaria_AtividadeAgropecuariaId'
        AND table_name = 'Produto'
    ) THEN
        ALTER TABLE public."Produto" 
        ADD CONSTRAINT "FK_Produto_AtividadeAgropecuaria_AtividadeAgropecuariaId" 
        FOREIGN KEY ("AtividadeAgropecuariaId") 
        REFERENCES public."AtividadesAgropecuarias" ("Id") 
        ON DELETE RESTRICT;
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('02_migrate_produto_references', 'FK_CONSTRAINT', 'Added FK constraint for AtividadeAgropecuaria', 'SUCCESS');
    END IF;
END $$;

-- =====================================================
-- STEP 10: Create indexes for performance
-- =====================================================
CREATE INDEX IF NOT EXISTS "IX_Produto_UnidadeMedidaId" ON public."Produto" ("UnidadeMedidaId");
CREATE INDEX IF NOT EXISTS "IX_Produto_EmbalagemId" ON public."Produto" ("EmbalagemId");
CREATE INDEX IF NOT EXISTS "IX_Produto_AtividadeAgropecuariaId" ON public."Produto" ("AtividadeAgropecuariaId");

INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('02_migrate_produto_references', 'INDEXES', 'Created performance indexes for new FK columns', 'SUCCESS');

-- =====================================================
-- STEP 11: Remove old Unidade enum column if exists
-- =====================================================
DO $$
DECLARE
    has_unidade_column BOOLEAN := false;
BEGIN
    -- Check if old Unidade column exists
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Produto' 
        AND column_name = 'Unidade'
    ) INTO has_unidade_column;
    
    IF has_unidade_column THEN
        -- Only drop if all products have been migrated
        IF NOT EXISTS (
            SELECT 1 FROM public."Produto" 
            WHERE "UnidadeMedidaId" IS NULL OR "UnidadeMedidaId" = 0
        ) THEN
            ALTER TABLE public."Produto" DROP COLUMN "Unidade";
            INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
            VALUES ('02_migrate_produto_references', 'CLEANUP', 'Dropped old Unidade enum column', 'SUCCESS');
        ELSE
            INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
            VALUES ('02_migrate_produto_references', 'CLEANUP', 'Kept old Unidade column due to unmigrated records', 'WARNING');
        END IF;
    END IF;
END $$;

-- =====================================================
-- STEP 12: Generate migration report
-- =====================================================
DO $$
DECLARE
    total_produtos INTEGER;
    produtos_with_unidade INTEGER;
    produtos_with_embalagem INTEGER;
    produtos_with_atividade INTEGER;
    migration_issues INTEGER;
    report_text TEXT;
BEGIN
    -- Count migration results
    SELECT COUNT(*) INTO total_produtos FROM public."Produto";
    SELECT COUNT(*) INTO produtos_with_unidade FROM public."Produto" WHERE "UnidadeMedidaId" IS NOT NULL;
    SELECT COUNT(*) INTO produtos_with_embalagem FROM public."Produto" WHERE "EmbalagemId" IS NOT NULL;
    SELECT COUNT(*) INTO produtos_with_atividade FROM public."Produto" WHERE "AtividadeAgropecuariaId" IS NOT NULL;
    SELECT COUNT(*) INTO migration_issues FROM produto_migration_issues WHERE NOT resolved;
    
    -- Generate report
    report_text := 'PRODUTO MIGRATION REPORT' || E'\n' ||
                   '========================' || E'\n' ||
                   'Total Products: ' || total_produtos || E'\n' ||
                   'Products with UnidadeMedida: ' || produtos_with_unidade || ' (' || ROUND((produtos_with_unidade::DECIMAL / total_produtos * 100), 2) || '%)' || E'\n' ||
                   'Products with Embalagem: ' || produtos_with_embalagem || ' (' || ROUND((produtos_with_embalagem::DECIMAL / total_produtos * 100), 2) || '%)' || E'\n' ||
                   'Products with AtividadeAgropecuaria: ' || produtos_with_atividade || ' (' || ROUND((produtos_with_atividade::DECIMAL / total_produtos * 100), 2) || '%)' || E'\n' ||
                   'Migration Issues: ' || migration_issues || E'\n' ||
                   '========================';
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('02_migrate_produto_references', 'REPORT', report_text, 'SUCCESS');
    
    -- Output report to console
    RAISE NOTICE '%', report_text;
    
    -- Show migration issues if any
    IF migration_issues > 0 THEN
        RAISE NOTICE 'Migration issues found. Check produto_migration_issues temp table for details.';
        RAISE NOTICE 'Query: SELECT * FROM produto_migration_issues WHERE NOT resolved;';
    END IF;
END $$;

-- Log migration completion
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('02_migrate_produto_references', 'COMPLETE', 'Produto references migration completed successfully', 'SUCCESS');

-- Commit transaction
COMMIT;

-- Display final message
\echo 'Migration 02_migrate_produto_references completed successfully!'
\echo 'Check MigrationLogs table for detailed execution log.'
\echo 'If there were migration issues, review the produto_migration_issues temp table.'