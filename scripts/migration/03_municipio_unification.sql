-- =====================================================
-- Geographic Tables Unification - Municipio Migration
-- =====================================================
-- This script merges municipios_referencia data into municipios table
-- =====================================================

-- Log start of municipio migration
INSERT INTO migration_log (step, status, message) 
VALUES ('municipio_unification', 'started', 'Starting municipio unification process');

-- Step 1: Validate and clean municipios_referencia data
-- =====================================================

-- Check for invalid codigo_ibge values
CREATE TEMPORARY TABLE invalid_municipios AS
SELECT id, nome, codigo_ibge, uf_id
FROM municipios_referencia 
WHERE codigo_ibge IS NULL 
   OR codigo_ibge = '' 
   OR codigo_ibge !~ '^[0-9]+$';

-- Log invalid records count
INSERT INTO migration_log (step, status, message) 
VALUES ('municipio_validation', 'info', 
        FORMAT('Found %s invalid codigo_ibge records', 
               (SELECT COUNT(*) FROM invalid_municipios)));

-- Step 2: Add missing municipios from municipios_referencia
-- =====================================================
INSERT INTO municipios (nome, codigo_ibge, estado_id, data_criacao, data_atualizacao, ativo)
SELECT DISTINCT 
    mr.nome,
    mr.codigo_ibge::integer as codigo_ibge,
    e.id as estado_id,
    mr.data_criacao,
    mr.data_atualizacao,
    mr.ativo
FROM municipios_referencia mr
INNER JOIN estados_referencia er ON mr.uf_id = er.id
INNER JOIN estados e ON e.uf = er.uf
LEFT JOIN municipios m ON m.codigo_ibge = mr.codigo_ibge::integer
WHERE m.id IS NULL
  AND mr.codigo_ibge IS NOT NULL 
  AND mr.codigo_ibge != ''
  AND mr.codigo_ibge ~ '^[0-9]+$'  -- Only numeric codes
  AND mr.nome IS NOT NULL
  AND mr.nome != ''
  AND mr.uf_id IS NOT NULL;

-- Get count of inserted records
SELECT 
    'Municipio Migration' as step,
    'Records inserted' as metric,
    COUNT(*) as count
FROM municipios m
INNER JOIN estados e ON m.estado_id = e.id
INNER JOIN estados_referencia er ON e.uf = er.uf
INNER JOIN municipios_referencia mr ON mr.uf_id = er.id AND m.codigo_ibge = mr.codigo_ibge::integer
WHERE m.data_criacao >= (SELECT MAX(created_at) FROM migration_log WHERE step = 'municipio_unification');

-- Step 3: Update existing municipios with missing data
-- =====================================================
UPDATE municipios 
SET 
    nome = COALESCE(municipios.nome, mr.nome),
    data_atualizacao = CURRENT_TIMESTAMP
FROM municipios_referencia mr
INNER JOIN estados_referencia er ON mr.uf_id = er.id
INNER JOIN estados e ON e.uf = er.uf
WHERE municipios.codigo_ibge = mr.codigo_ibge::integer
  AND municipios.estado_id = e.id
  AND (municipios.nome IS NULL OR municipios.nome = '')
  AND mr.nome IS NOT NULL 
  AND mr.nome != ''
  AND mr.codigo_ibge ~ '^[0-9]+$';

-- Step 4: Validate municipio unification results
-- =====================================================
DO $$
DECLARE
    missing_count INTEGER;
    duplicate_count INTEGER;
    invalid_count INTEGER;
    orphaned_count INTEGER;
BEGIN
    -- Check for missing municipios (excluding invalid ones)
    SELECT COUNT(*) INTO missing_count
    FROM municipios_referencia mr
    INNER JOIN estados_referencia er ON mr.uf_id = er.id
    INNER JOIN estados e ON e.uf = er.uf
    LEFT JOIN municipios m ON m.codigo_ibge = mr.codigo_ibge::integer AND m.estado_id = e.id
    WHERE m.id IS NULL
      AND mr.codigo_ibge ~ '^[0-9]+$';
    
    -- Check for duplicate codigo_ibge (should be 0)
    SELECT COUNT(*) INTO duplicate_count
    FROM (
        SELECT codigo_ibge, COUNT(*) 
        FROM municipios 
        GROUP BY codigo_ibge 
        HAVING COUNT(*) > 1
    ) duplicates;
    
    -- Check for invalid data
    SELECT COUNT(*) INTO invalid_count
    FROM municipios 
    WHERE codigo_ibge IS NULL OR nome IS NULL OR nome = '' OR estado_id IS NULL;
    
    -- Check for orphaned municipios (estado_id not in estados)
    SELECT COUNT(*) INTO orphaned_count
    FROM municipios m
    LEFT JOIN estados e ON m.estado_id = e.id
    WHERE e.id IS NULL;
    
    -- Log validation results
    INSERT INTO migration_log (step, status, message) 
    VALUES (
        'municipio_validation', 
        CASE 
            WHEN missing_count > 0 THEN 'warning'
            WHEN duplicate_count > 0 THEN 'error'
            WHEN invalid_count > 0 THEN 'error'
            WHEN orphaned_count > 0 THEN 'error'
            ELSE 'success'
        END,
        FORMAT('Validation: Missing=%s, Duplicates=%s, Invalid=%s, Orphaned=%s', 
               missing_count, duplicate_count, invalid_count, orphaned_count)
    );
    
    -- Raise error if critical issues found
    IF duplicate_count > 0 OR invalid_count > 0 OR orphaned_count > 0 THEN
        RAISE EXCEPTION 'Municipio unification validation failed: Duplicates=%, Invalid=%, Orphaned=%', 
                       duplicate_count, invalid_count, orphaned_count;
    END IF;
END $$;

-- Step 5: Create mapping table for foreign key updates
-- =====================================================
CREATE TEMPORARY TABLE municipio_id_mapping AS
SELECT 
    mr.id as old_id,
    m.id as new_id,
    mr.codigo_ibge,
    mr.nome as old_nome,
    m.nome as new_nome,
    er.uf,
    e.id as estado_id
FROM municipios_referencia mr
INNER JOIN estados_referencia er ON mr.uf_id = er.id
INNER JOIN estados e ON e.uf = er.uf
INNER JOIN municipios m ON m.codigo_ibge = mr.codigo_ibge::integer AND m.estado_id = e.id
WHERE mr.codigo_ibge ~ '^[0-9]+$';

-- Verify mapping completeness
SELECT 
    'Municipio Mapping' as step,
    'Total mappings created' as metric,
    COUNT(*) as count
FROM municipio_id_mapping;

-- Show sample mappings for verification
SELECT 
    'Municipio Mapping Sample' as step,
    old_id,
    new_id,
    codigo_ibge,
    old_nome,
    new_nome,
    uf
FROM municipio_id_mapping
ORDER BY uf, codigo_ibge
LIMIT 10;

-- Step 6: Handle invalid municipios_referencia records
-- =====================================================
INSERT INTO migration_log (step, status, message, error_details) 
SELECT 
    'municipio_invalid_records',
    'warning',
    FORMAT('Skipped %s invalid municipio records', COUNT(*)),
    STRING_AGG(FORMAT('ID: %s, Nome: %s, Codigo: %s', id, nome, codigo_ibge), '; ')
FROM invalid_municipios;

-- Log completion of municipio migration
INSERT INTO migration_log (step, status, message) 
VALUES ('municipio_unification', 'completed', 'Municipio unification process completed successfully');

-- Final municipio statistics
SELECT 
    'Final Municipio Statistics' as report_type,
    'Total municipios after migration' as metric,
    COUNT(*) as count
FROM municipios
UNION ALL
SELECT 
    'Final Municipio Statistics',
    'Active municipios',
    COUNT(*)
FROM municipios
WHERE ativo = true
UNION ALL
SELECT 
    'Final Municipio Statistics',
    'Municipios with complete data',
    COUNT(*)
FROM municipios
WHERE nome IS NOT NULL 
  AND nome != ''
  AND codigo_ibge IS NOT NULL
  AND estado_id IS NOT NULL
UNION ALL
SELECT 
    'Final Municipio Statistics',
    'Municipios with geographic coordinates',
    COUNT(*)
FROM municipios
WHERE latitude IS NOT NULL AND longitude IS NOT NULL;

-- Verify no data loss (excluding invalid records)
SELECT 
    'Data Loss Check' as check_type,
    CASE 
        WHEN (SELECT COUNT(*) FROM municipios) >= 
             (SELECT COUNT(*) FROM municipios_backup) +
             (SELECT COUNT(*) FROM municipios_referencia WHERE codigo_ibge ~ '^[0-9]+$') -
             (SELECT COUNT(*) FROM municipios_backup m 
              INNER JOIN municipios_referencia mr ON m.codigo_ibge = mr.codigo_ibge::integer)
        THEN 'PASS - No data loss detected'
        ELSE 'FAIL - Data loss detected'
    END as result;

-- Clean up temporary tables
DROP TABLE IF EXISTS invalid_municipios;