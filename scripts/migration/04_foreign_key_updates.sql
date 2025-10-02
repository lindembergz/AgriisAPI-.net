-- =====================================================
-- Geographic Tables Unification - Foreign Key Updates
-- =====================================================
-- This script updates all foreign key references to use unified tables
-- =====================================================

-- Log start of foreign key updates
INSERT INTO migration_log (step, status, message) 
VALUES ('foreign_key_updates', 'started', 'Starting foreign key updates process');

-- Step 1: Update Fornecedor UfId references
-- =====================================================

-- First, identify Fornecedor records that need UfId updates
CREATE TEMPORARY TABLE fornecedor_uf_updates AS
SELECT 
    f."Id" as fornecedor_id,
    f."UfId" as old_uf_id,
    e.id as new_uf_id,
    er.uf,
    f."Nome" as fornecedor_nome
FROM "Fornecedor" f
INNER JOIN estados_referencia er ON f."UfId" = er.id
INNER JOIN estados e ON e.uf = er.uf;

-- Log count of records to update
INSERT INTO migration_log (step, status, message) 
VALUES ('fornecedor_uf_analysis', 'info', 
        FORMAT('Found %s Fornecedor records with UfId references to estados_referencia', 
               (SELECT COUNT(*) FROM fornecedor_uf_updates)));

-- Update Fornecedor UfId references
UPDATE "Fornecedor" 
SET "UfId" = fuu.new_uf_id,
    "DataAtualizacao" = CURRENT_TIMESTAMP
FROM fornecedor_uf_updates fuu
WHERE "Fornecedor"."Id" = fuu.fornecedor_id;

-- Verify UfId updates
SELECT 
    'Fornecedor UfId Updates' as step,
    'Records updated' as metric,
    COUNT(*) as count
FROM fornecedor_uf_updates;

-- Step 2: Update Fornecedor MunicipioId references
-- =====================================================

-- Identify Fornecedor records that need MunicipioId updates
CREATE TEMPORARY TABLE fornecedor_municipio_updates AS
SELECT 
    f."Id" as fornecedor_id,
    f."MunicipioId" as old_municipio_id,
    m.id as new_municipio_id,
    mr.codigo_ibge,
    f."Nome" as fornecedor_nome,
    er.uf
FROM "Fornecedor" f
INNER JOIN municipios_referencia mr ON f."MunicipioId" = mr.id
INNER JOIN estados_referencia er ON mr.uf_id = er.id
INNER JOIN estados e ON e.uf = er.uf
INNER JOIN municipios m ON m.codigo_ibge = mr.codigo_ibge::integer AND m.estado_id = e.id
WHERE mr.codigo_ibge ~ '^[0-9]+$';

-- Log count of records to update
INSERT INTO migration_log (step, status, message) 
VALUES ('fornecedor_municipio_analysis', 'info', 
        FORMAT('Found %s Fornecedor records with MunicipioId references to municipios_referencia', 
               (SELECT COUNT(*) FROM fornecedor_municipio_updates)));

-- Update Fornecedor MunicipioId references
UPDATE "Fornecedor" 
SET "MunicipioId" = fmu.new_municipio_id,
    "DataAtualizacao" = CURRENT_TIMESTAMP
FROM fornecedor_municipio_updates fmu
WHERE "Fornecedor"."Id" = fmu.fornecedor_id;

-- Verify MunicipioId updates
SELECT 
    'Fornecedor MunicipioId Updates' as step,
    'Records updated' as metric,
    COUNT(*) as count
FROM fornecedor_municipio_updates;

-- Step 3: Check for other tables with geographic references
-- =====================================================

-- Check PontoDistribuicao Estado field (varchar, not FK)
SELECT 
    'PontoDistribuicao Estado Check' as step,
    'Records with Estado field' as metric,
    COUNT(*) as count
FROM "PontoDistribuicao"
WHERE "Estado" IS NOT NULL AND "Estado" != '';

-- Update PontoDistribuicao Estado field to use proper UF codes
UPDATE "PontoDistribuicao" 
SET "Estado" = e.uf,
    "DataAtualizacao" = CURRENT_TIMESTAMP
FROM estados e
WHERE "PontoDistribuicao"."Estado" = e.nome
  AND "PontoDistribuicao"."Estado" IS NOT NULL
  AND "PontoDistribuicao"."Estado" != '';

-- Step 4: Validate foreign key integrity
-- =====================================================

-- Check for orphaned Fornecedor UfId references
CREATE TEMPORARY TABLE orphaned_fornecedor_uf AS
SELECT f."Id", f."Nome", f."UfId"
FROM "Fornecedor" f
LEFT JOIN estados e ON f."UfId" = e.id
WHERE f."UfId" IS NOT NULL AND e.id IS NULL;

-- Check for orphaned Fornecedor MunicipioId references
CREATE TEMPORARY TABLE orphaned_fornecedor_municipio AS
SELECT f."Id", f."Nome", f."MunicipioId"
FROM "Fornecedor" f
LEFT JOIN municipios m ON f."MunicipioId" = m.id
WHERE f."MunicipioId" IS NOT NULL AND m.id IS NULL;

-- Check for orphaned enderecos references (should be none)
CREATE TEMPORARY TABLE orphaned_enderecos AS
SELECT e.id, e.logradouro, e.estado_id, e.municipio_id
FROM enderecos e
LEFT JOIN estados est ON e.estado_id = est.id
LEFT JOIN municipios mun ON e.municipio_id = mun.id
WHERE (e.estado_id IS NOT NULL AND est.id IS NULL)
   OR (e.municipio_id IS NOT NULL AND mun.id IS NULL);

-- Log orphaned records
INSERT INTO migration_log (step, status, message) 
VALUES ('orphaned_records_check', 
        CASE 
            WHEN (SELECT COUNT(*) FROM orphaned_fornecedor_uf) > 0 OR
                 (SELECT COUNT(*) FROM orphaned_fornecedor_municipio) > 0 OR
                 (SELECT COUNT(*) FROM orphaned_enderecos) > 0
            THEN 'warning'
            ELSE 'success'
        END,
        FORMAT('Orphaned records: Fornecedor UF=%s, Fornecedor Municipio=%s, Enderecos=%s',
               (SELECT COUNT(*) FROM orphaned_fornecedor_uf),
               (SELECT COUNT(*) FROM orphaned_fornecedor_municipio),
               (SELECT COUNT(*) FROM orphaned_enderecos)));

-- Step 5: Update foreign key constraints
-- =====================================================

-- Drop existing foreign key constraints that reference old tables
DO $$
BEGIN
    -- Check if FK constraint exists before dropping
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Fornecedor_Estados_UfId' 
        AND table_name = 'Fornecedor'
    ) THEN
        ALTER TABLE "Fornecedor" DROP CONSTRAINT "FK_Fornecedor_Estados_UfId";
    END IF;
    
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Fornecedor_Municipios_MunicipioId' 
        AND table_name = 'Fornecedor'
    ) THEN
        ALTER TABLE "Fornecedor" DROP CONSTRAINT "FK_Fornecedor_Municipios_MunicipioId";
    END IF;
END $$;

-- Add proper foreign key constraints to unified tables
ALTER TABLE "Fornecedor" 
ADD CONSTRAINT "FK_Fornecedor_Estados_UfId" 
FOREIGN KEY ("UfId") REFERENCES estados(id) ON DELETE RESTRICT;

ALTER TABLE "Fornecedor" 
ADD CONSTRAINT "FK_Fornecedor_Municipios_MunicipioId" 
FOREIGN KEY ("MunicipioId") REFERENCES municipios(id) ON DELETE RESTRICT;

-- Step 6: Final validation
-- =====================================================

-- Validate all foreign key relationships
DO $$
DECLARE
    invalid_uf_count INTEGER;
    invalid_municipio_count INTEGER;
    invalid_endereco_count INTEGER;
BEGIN
    -- Check Fornecedor UfId relationships
    SELECT COUNT(*) INTO invalid_uf_count
    FROM "Fornecedor" f
    LEFT JOIN estados e ON f."UfId" = e.id
    WHERE f."UfId" IS NOT NULL AND e.id IS NULL;
    
    -- Check Fornecedor MunicipioId relationships
    SELECT COUNT(*) INTO invalid_municipio_count
    FROM "Fornecedor" f
    LEFT JOIN municipios m ON f."MunicipioId" = m.id
    WHERE f."MunicipioId" IS NOT NULL AND m.id IS NULL;
    
    -- Check enderecos relationships
    SELECT COUNT(*) INTO invalid_endereco_count
    FROM enderecos e
    LEFT JOIN estados est ON e.estado_id = est.id
    LEFT JOIN municipios mun ON e.municipio_id = mun.id
    WHERE (e.estado_id IS NOT NULL AND est.id IS NULL)
       OR (e.municipio_id IS NOT NULL AND mun.id IS NULL);
    
    -- Log final validation
    INSERT INTO migration_log (step, status, message) 
    VALUES (
        'foreign_key_validation', 
        CASE 
            WHEN invalid_uf_count > 0 OR invalid_municipio_count > 0 OR invalid_endereco_count > 0
            THEN 'error'
            ELSE 'success'
        END,
        FORMAT('Final FK validation: Invalid UF=%s, Invalid Municipio=%s, Invalid Endereco=%s', 
               invalid_uf_count, invalid_municipio_count, invalid_endereco_count)
    );
    
    -- Raise error if validation fails
    IF invalid_uf_count > 0 OR invalid_municipio_count > 0 OR invalid_endereco_count > 0 THEN
        RAISE EXCEPTION 'Foreign key validation failed: UF=%, Municipio=%, Endereco=%', 
                       invalid_uf_count, invalid_municipio_count, invalid_endereco_count;
    END IF;
END $$;

-- Log completion of foreign key updates
INSERT INTO migration_log (step, status, message) 
VALUES ('foreign_key_updates', 'completed', 'Foreign key updates completed successfully');

-- Final statistics
SELECT 
    'Foreign Key Update Statistics' as report_type,
    'Fornecedor UfId updates' as metric,
    COUNT(*) as count
FROM fornecedor_uf_updates
UNION ALL
SELECT 
    'Foreign Key Update Statistics',
    'Fornecedor MunicipioId updates',
    COUNT(*)
FROM fornecedor_municipio_updates
UNION ALL
SELECT 
    'Foreign Key Update Statistics',
    'Total Fornecedor records with valid UfId',
    COUNT(*)
FROM "Fornecedor" f
INNER JOIN estados e ON f."UfId" = e.id
UNION ALL
SELECT 
    'Foreign Key Update Statistics',
    'Total Fornecedor records with valid MunicipioId',
    COUNT(*)
FROM "Fornecedor" f
INNER JOIN municipios m ON f."MunicipioId" = m.id;

-- Clean up temporary tables
DROP TABLE IF EXISTS fornecedor_uf_updates;
DROP TABLE IF EXISTS fornecedor_municipio_updates;
DROP TABLE IF EXISTS orphaned_fornecedor_uf;
DROP TABLE IF EXISTS orphaned_fornecedor_municipio;
DROP TABLE IF EXISTS orphaned_enderecos;