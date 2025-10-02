-- =====================================================
-- Geographic Tables Unification - PaisId Migration
-- =====================================================
-- This script handles the migration of pais_id field from estados_referencia
-- =====================================================

-- Log start of pais_id migration
INSERT INTO migration_log (step, status, message) 
VALUES ('pais_id_migration', 'started', 'Starting pais_id field migration');

-- Step 1: Ensure pais_id column exists in estados table
-- =====================================================
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'estados' AND column_name = 'pais_id'
    ) THEN
        -- Add pais_id column with default value 1 (Brasil)
        ALTER TABLE estados ADD COLUMN pais_id int4 DEFAULT 1 NOT NULL;
        
        INSERT INTO migration_log (step, status, message) 
        VALUES ('add_pais_id_column', 'completed', 'Added pais_id column to estados table');
    ELSE
        INSERT INTO migration_log (step, status, message) 
        VALUES ('add_pais_id_column', 'skipped', 'pais_id column already exists');
    END IF;
END $$;

-- Step 2: Migrate pais_id data from estados_referencia
-- =====================================================
DO $$
DECLARE
    updated_count INTEGER := 0;
BEGIN
    -- Update pais_id for existing estados based on estados_referencia data
    UPDATE estados 
    SET pais_id = er.pais_id,
        data_atualizacao = CURRENT_TIMESTAMP
    FROM estados_referencia er
    WHERE estados.uf = er.uf
      AND er.pais_id IS NOT NULL
      AND er.pais_id != estados.pais_id;
    
    GET DIAGNOSTICS updated_count = ROW_COUNT;
    
    INSERT INTO migration_log (step, status, message) 
    VALUES ('migrate_pais_id_data', 'completed', 
            FORMAT('Updated pais_id for %s estados records', updated_count));
END $$;

-- Step 3: Validate pais_id migration
-- =====================================================
DO $$
DECLARE
    null_pais_count INTEGER;
    invalid_pais_count INTEGER;
BEGIN
    -- Check for NULL pais_id values
    SELECT COUNT(*) INTO null_pais_count
    FROM estados 
    WHERE pais_id IS NULL;
    
    -- Check for invalid pais_id values (assuming valid range is > 0)
    SELECT COUNT(*) INTO invalid_pais_count
    FROM estados 
    WHERE pais_id <= 0;
    
    IF null_pais_count > 0 THEN
        INSERT INTO migration_log (step, status, message, error_details) 
        VALUES ('pais_id_validation', 'warning', 
                FORMAT('Found %s estados with NULL pais_id', null_pais_count),
                'Setting NULL pais_id values to 1 (Brasil)');
        
        -- Fix NULL values
        UPDATE estados SET pais_id = 1 WHERE pais_id IS NULL;
    END IF;
    
    IF invalid_pais_count > 0 THEN
        INSERT INTO migration_log (step, status, message, error_details) 
        VALUES ('pais_id_validation', 'warning', 
                FORMAT('Found %s estados with invalid pais_id', invalid_pais_count),
                'Setting invalid pais_id values to 1 (Brasil)');
        
        -- Fix invalid values
        UPDATE estados SET pais_id = 1 WHERE pais_id <= 0;
    END IF;
    
    IF null_pais_count = 0 AND invalid_pais_count = 0 THEN
        INSERT INTO migration_log (step, status, message) 
        VALUES ('pais_id_validation', 'success', 'All pais_id values are valid');
    END IF;
END $$;

-- Step 4: Create foreign key constraint if paises table exists
-- =====================================================
DO $$
BEGIN
    -- Check if paises table exists
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'paises') THEN
        -- Check if foreign key constraint already exists
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.table_constraints 
            WHERE constraint_name = 'fk_estados_pais' 
            AND table_name = 'estados'
        ) THEN
            -- Add foreign key constraint
            ALTER TABLE estados 
            ADD CONSTRAINT fk_estados_pais 
            FOREIGN KEY (pais_id) REFERENCES paises(id);
            
            INSERT INTO migration_log (step, status, message) 
            VALUES ('create_pais_fk', 'completed', 'Created foreign key constraint to paises table');
        ELSE
            INSERT INTO migration_log (step, status, message) 
            VALUES ('create_pais_fk', 'skipped', 'Foreign key constraint already exists');
        END IF;
    ELSE
        INSERT INTO migration_log (step, status, message) 
        VALUES ('create_pais_fk', 'skipped', 'paises table does not exist');
    END IF;
END $$;

-- Step 5: Create index for pais_id if it doesn't exist
-- =====================================================
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE tablename = 'estados' AND indexname = 'IX_estados_pais_id'
    ) THEN
        CREATE INDEX IX_estados_pais_id ON estados(pais_id);
        
        INSERT INTO migration_log (step, status, message) 
        VALUES ('create_pais_id_index', 'completed', 'Created index for pais_id column');
    ELSE
        INSERT INTO migration_log (step, status, message) 
        VALUES ('create_pais_id_index', 'skipped', 'Index already exists');
    END IF;
END $$;

-- Step 6: Generate pais_id migration report
-- =====================================================
SELECT 
    'PaisId Migration Report' as report_type,
    'Total estados with pais_id' as metric,
    COUNT(*) as count
FROM estados
WHERE pais_id IS NOT NULL
UNION ALL
SELECT 
    'PaisId Migration Report',
    'Estados with pais_id = 1 (Brasil)',
    COUNT(*)
FROM estados
WHERE pais_id = 1
UNION ALL
SELECT 
    'PaisId Migration Report',
    'Estados with other pais_id',
    COUNT(*)
FROM estados
WHERE pais_id != 1
UNION ALL
SELECT 
    'PaisId Migration Report',
    'Unique pais_id values',
    COUNT(DISTINCT pais_id)::text::integer
FROM estados;

-- Log completion of pais_id migration
INSERT INTO migration_log (step, status, message) 
VALUES ('pais_id_migration', 'completed', 'PaisId field migration completed successfully');

-- Final validation message
SELECT 
    'PAIS_ID MIGRATION COMPLETED' as status,
    'All estados now have valid pais_id values' as message,
    'Default value is 1 for Brasil' as note;