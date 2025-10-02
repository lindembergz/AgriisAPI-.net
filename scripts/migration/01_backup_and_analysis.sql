-- =====================================================
-- Geographic Tables Unification - Backup and Analysis
-- =====================================================
-- This script creates backups and analyzes data conflicts
-- between duplicate geographic tables before migration
-- =====================================================

-- Create backup tables for rollback purposes
-- =====================================================

-- Backup estados_referencia
CREATE TABLE IF NOT EXISTS estados_referencia_backup AS 
SELECT * FROM estados_referencia;

-- Backup municipios_referencia  
CREATE TABLE IF NOT EXISTS municipios_referencia_backup AS 
SELECT * FROM municipios_referencia;

-- Backup current estados (in case of conflicts)
CREATE TABLE IF NOT EXISTS estados_backup AS 
SELECT * FROM estados;

-- Backup current municipios (in case of conflicts)
CREATE TABLE IF NOT EXISTS municipios_backup AS 
SELECT * FROM municipios;

-- Backup affected foreign key tables
CREATE TABLE IF NOT EXISTS "Fornecedor_backup" AS 
SELECT * FROM "Fornecedor";

-- Data Analysis Queries
-- =====================================================

-- 1. Analyze estados conflicts
SELECT 
    'Estados Analysis' as analysis_type,
    'Total estados records' as metric,
    COUNT(*) as count
FROM estados
UNION ALL
SELECT 
    'Estados Analysis',
    'Total estados_referencia records',
    COUNT(*)
FROM estados_referencia
UNION ALL
SELECT 
    'Estados Analysis',
    'UF conflicts (same UF in both tables)',
    COUNT(*)
FROM estados e
INNER JOIN estados_referencia er ON e.uf = er.uf
UNION ALL
SELECT 
    'Estados Analysis',
    'UFs only in estados',
    COUNT(*)
FROM estados e
LEFT JOIN estados_referencia er ON e.uf = er.uf
WHERE er.uf IS NULL
UNION ALL
SELECT 
    'Estados Analysis',
    'UFs only in estados_referencia',
    COUNT(*)
FROM estados_referencia er
LEFT JOIN estados e ON e.uf = er.uf
WHERE e.uf IS NULL;

-- 2. Analyze municipios conflicts
SELECT 
    'Municipios Analysis' as analysis_type,
    'Total municipios records' as metric,
    COUNT(*) as count
FROM municipios
UNION ALL
SELECT 
    'Municipios Analysis',
    'Total municipios_referencia records',
    COUNT(*)
FROM municipios_referencia
UNION ALL
SELECT 
    'Municipios Analysis',
    'IBGE code conflicts (same code in both tables)',
    COUNT(*)
FROM municipios m
INNER JOIN municipios_referencia mr ON m.codigo_ibge::varchar = mr.codigo_ibge
UNION ALL
SELECT 
    'Municipios Analysis',
    'Codes only in municipios',
    COUNT(*)
FROM municipios m
LEFT JOIN municipios_referencia mr ON m.codigo_ibge::varchar = mr.codigo_ibge
WHERE mr.codigo_ibge IS NULL
UNION ALL
SELECT 
    'Municipios Analysis',
    'Codes only in municipios_referencia',
    COUNT(*)
FROM municipios_referencia mr
LEFT JOIN municipios m ON m.codigo_ibge::varchar = mr.codigo_ibge
WHERE m.codigo_ibge IS NULL;

-- 3. Analyze data type conversion issues
SELECT 
    'Data Type Analysis' as analysis_type,
    'Invalid codigo_ibge (non-numeric)' as metric,
    COUNT(*) as count
FROM municipios_referencia 
WHERE codigo_ibge !~ '^[0-9]+$';

-- 4. Analyze foreign key dependencies
SELECT 
    'Foreign Key Analysis' as analysis_type,
    'Fornecedor records with UfId from estados_referencia' as metric,
    COUNT(*) as count
FROM "Fornecedor" f
INNER JOIN estados_referencia er ON f."UfId" = er.id
UNION ALL
SELECT 
    'Foreign Key Analysis',
    'Fornecedor records with MunicipioId from municipios_referencia',
    COUNT(*)
FROM "Fornecedor" f
INNER JOIN municipios_referencia mr ON f."MunicipioId" = mr.id;

-- Detailed conflict analysis
-- =====================================================

-- Show UF conflicts in detail
SELECT 
    'UF Conflict Detail' as report_type,
    e.uf,
    e.nome as estados_nome,
    er.nome as estados_referencia_nome,
    e.regiao as estados_regiao,
    'N/A' as estados_referencia_regiao,
    e.codigo_ibge as estados_codigo_ibge,
    'N/A' as estados_referencia_codigo_ibge
FROM estados e
INNER JOIN estados_referencia er ON e.uf = er.uf
WHERE e.nome != er.nome;

-- Show municipios with conversion issues
SELECT 
    'Municipio Conversion Issues' as report_type,
    mr.nome,
    mr.codigo_ibge as original_codigo,
    'Cannot convert to integer' as issue
FROM municipios_referencia mr
WHERE codigo_ibge !~ '^[0-9]+$'
LIMIT 10;

-- Show orphaned foreign key references
SELECT 
    'Orphaned FK References' as report_type,
    f."Id" as fornecedor_id,
    f."Nome" as fornecedor_nome,
    f."UfId" as uf_id_referencia,
    er.uf,
    er.nome as estado_nome
FROM "Fornecedor" f
INNER JOIN estados_referencia er ON f."UfId" = er.id
LEFT JOIN estados e ON e.uf = er.uf
WHERE e.id IS NULL
LIMIT 10;

-- Data integrity validation
-- =====================================================

-- Check for NULL values that could cause issues
SELECT 
    'Data Integrity Check' as check_type,
    'estados_referencia NULL UF' as check_name,
    COUNT(*) as count
FROM estados_referencia 
WHERE uf IS NULL OR uf = ''
UNION ALL
SELECT 
    'Data Integrity Check',
    'municipios_referencia NULL codigo_ibge',
    COUNT(*)
FROM municipios_referencia 
WHERE codigo_ibge IS NULL OR codigo_ibge = ''
UNION ALL
SELECT 
    'Data Integrity Check',
    'municipios_referencia NULL uf_id',
    COUNT(*)
FROM municipios_referencia 
WHERE uf_id IS NULL;

-- Generate migration readiness report
-- =====================================================
SELECT 
    'Migration Readiness' as report_type,
    CASE 
        WHEN (SELECT COUNT(*) FROM municipios_referencia WHERE codigo_ibge !~ '^[0-9]+$') > 0 
        THEN 'NOT READY - Invalid codigo_ibge values found'
        WHEN (SELECT COUNT(*) FROM estados_referencia WHERE uf IS NULL OR uf = '') > 0
        THEN 'NOT READY - NULL UF values found'
        WHEN (SELECT COUNT(*) FROM municipios_referencia WHERE uf_id IS NULL) > 0
        THEN 'NOT READY - NULL uf_id values found'
        ELSE 'READY - No blocking issues found'
    END as status;

-- Create indexes for migration performance
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_estados_referencia_uf ON estados_referencia(uf);
CREATE INDEX IF NOT EXISTS idx_municipios_referencia_codigo_ibge ON municipios_referencia(codigo_ibge);
CREATE INDEX IF NOT EXISTS idx_municipios_referencia_uf_id ON municipios_referencia(uf_id);

-- Log backup completion
INSERT INTO migration_log (step, status, message, created_at) 
VALUES ('backup_and_analysis', 'completed', 'Backup tables created and data analysis completed', NOW())
ON CONFLICT DO NOTHING;