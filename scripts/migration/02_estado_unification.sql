-- =====================================================
-- Geographic Tables Unification - Estado Migration
-- =====================================================
-- This script merges estados_referencia data into estados table
-- =====================================================

-- Log start of estado migration
INSERT INTO migration_log (step, status, message) 
VALUES ('estado_unification', 'started', 'Starting estado unification process');

-- Step 1: Add missing estados from estados_referencia
-- =====================================================
INSERT INTO estados (nome, uf, codigo_ibge, regiao, pais_id, data_criacao, data_atualizacao, ativo)
SELECT DISTINCT 
    er.nome,
    er.uf,
    generate_codigo_ibge(er.uf) as codigo_ibge,
    map_uf_to_regiao(er.uf) as regiao,
    COALESCE(er.pais_id, 1) as pais_id,
    er.data_criacao,
    er.data_atualizacao,
    er.ativo
FROM estados_referencia er
LEFT JOIN estados e ON e.uf = er.uf
WHERE e.id IS NULL
  AND er.uf IS NOT NULL 
  AND er.uf != ''
  AND er.nome IS NOT NULL
  AND er.nome != '';

-- Get count of inserted records
SELECT 
    'Estado Migration' as step,
    'Records inserted' as metric,
    COUNT(*) as count
FROM estados e
INNER JOIN estados_referencia er ON e.uf = er.uf
WHERE e.data_criacao >= (SELECT MAX(created_at) FROM migration_log WHERE step = 'estado_unification');

-- Step 2: Update existing estados with missing data from estados_referencia
-- =====================================================
UPDATE estados 
SET 
    nome = COALESCE(estados.nome, er.nome),
    pais_id = COALESCE(estados.pais_id, er.pais_id, 1),
    data_atualizacao = CURRENT_TIMESTAMP
FROM estados_referencia er
WHERE estados.uf = er.uf
  AND ((estados.nome IS NULL OR estados.nome = '') OR estados.pais_id IS NULL)
  AND er.nome IS NOT NULL 
  AND er.nome != '';

-- Step 3: Validate estado unification results
-- =====================================================
DO $$
DECLARE
    missing_count INTEGER;
    duplicate_count INTEGER;
    invalid_count INTEGER;
BEGIN
    -- Check for missing UFs
    SELECT COUNT(*) INTO missing_count
    FROM estados_referencia er
    LEFT JOIN estados e ON e.uf = er.uf
    WHERE e.id IS NULL;
    
    -- Check for duplicate UFs (should be 0)
    SELECT COUNT(*) INTO duplicate_count
    FROM (
        SELECT uf, COUNT(*) 
        FROM estados 
        GROUP BY uf 
        HAVING COUNT(*) > 1
    ) duplicates;
    
    -- Check for invalid data
    SELECT COUNT(*) INTO invalid_count
    FROM estados 
    WHERE uf IS NULL OR uf = '' OR nome IS NULL OR nome = '';
    
    -- Log validation results
    INSERT INTO migration_log (step, status, message) 
    VALUES (
        'estado_validation', 
        CASE 
            WHEN missing_count > 0 THEN 'warning'
            WHEN duplicate_count > 0 THEN 'error'
            WHEN invalid_count > 0 THEN 'error'
            ELSE 'success'
        END,
        FORMAT('Validation: Missing=%s, Duplicates=%s, Invalid=%s', 
               missing_count, duplicate_count, invalid_count)
    );
    
    -- Raise error if critical issues found
    IF duplicate_count > 0 OR invalid_count > 0 THEN
        RAISE EXCEPTION 'Estado unification validation failed: Duplicates=%, Invalid=%', 
                       duplicate_count, invalid_count;
    END IF;
END $$;

-- Step 4: Create mapping table for foreign key updates
-- =====================================================
CREATE TEMPORARY TABLE estado_id_mapping AS
SELECT 
    er.id as old_id,
    e.id as new_id,
    er.uf,
    er.nome as old_nome,
    e.nome as new_nome
FROM estados_referencia er
INNER JOIN estados e ON e.uf = er.uf;

-- Verify mapping completeness
SELECT 
    'Estado Mapping' as step,
    'Total mappings created' as metric,
    COUNT(*) as count
FROM estado_id_mapping;

-- Show sample mappings for verification
SELECT 
    'Estado Mapping Sample' as step,
    old_id,
    new_id,
    uf,
    old_nome,
    new_nome
FROM estado_id_mapping
ORDER BY uf
LIMIT 10;

-- Log completion of estado migration
INSERT INTO migration_log (step, status, message) 
VALUES ('estado_unification', 'completed', 'Estado unification process completed successfully');

-- Final estado statistics
SELECT 
    'Final Estado Statistics' as report_type,
    'Total estados after migration' as metric,
    COUNT(*) as count
FROM estados
UNION ALL
SELECT 
    'Final Estado Statistics',
    'Active estados',
    COUNT(*)
FROM estados
WHERE ativo = true
UNION ALL
SELECT 
    'Final Estado Statistics',
    'Estados with complete data',
    COUNT(*)
FROM estados
WHERE nome IS NOT NULL 
  AND nome != ''
  AND uf IS NOT NULL 
  AND uf != ''
  AND codigo_ibge IS NOT NULL
  AND regiao IS NOT NULL
  AND regiao != '';

-- Verify no data loss
SELECT 
    'Data Loss Check' as check_type,
    CASE 
        WHEN (SELECT COUNT(*) FROM estados) >= (SELECT COUNT(*) FROM estados_backup)
        THEN 'PASS - No data loss detected'
        ELSE 'FAIL - Data loss detected'
    END as result;